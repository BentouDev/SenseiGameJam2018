using System.Collections;
using System.Collections.Generic;
using Data.Scripts.AI.Modules;
using Framework;
using UnityEngine;
using UnityEngine.AI;

public class ModuleFollow : AIModule
{
    public bool DrawDebug;
    public Transform Destination;
    public string TargetTag = "";
    public float DestinationThreshold = 2.0f;

    public Vector3 Checkpoint;

    public DestinationTarget.TargetClass TargetClassToFind;

    public AIModule OnDestinationReached;

    public float TriggerRange = 5;
    
    [Range(0, 100)]
    public float MaxPriority = 100;

    public float ThinkLag = 0.5f;
    public float CornerProximity = 0.25f;

    public bool Interruptable;

    private NavMeshPath Path;
    private int PathProgress;
    private float LastThinkingTime;
    
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, TriggerRange);
    }
    
    private bool CalcPath(Vector3 destination, NavMeshPath path)
    {
        return NavMesh.CalculatePath(Pawn.transform.position, destination, NavMesh.AllAreas, path);
    }

    void ObtainDestination()
    {
        // Find one yourself
        if (Destination == null && !string.IsNullOrEmpty(TargetTag))
        {
            var go = GameObject.FindGameObjectWithTag(TargetTag);
            Destination = go != null ? go.transform : null;
        }

        if (Destination == null)
        {
            foreach (var target in FindObjectsOfType<DestinationTarget>())
            {
                var pawn = target.GetComponent<BasePawn>();
                if (pawn && !pawn.IsAlive())
                    continue;
                
                if (target.Class == TargetClassToFind)
                {
                    Destination = target.transform;
                    break;
                }
            }
        }

        // Get current enemy
        if (!Destination)
            Destination = Driver.CurrentEnemy ? Driver.CurrentEnemy.transform : null;
    }

    protected override float CalcPriority()
    {
        ObtainDestination();

        if (!Destination)
            return 0;
        
        var diff = Destination.transform.position - Pawn.transform.position;
        if (diff.magnitude < TriggerRange && !HasReachedDestination())
            return MaxPriority;

        return 0;
    }

    public bool HasReachedDestination()
    {   
        if (!Destination)
            return false;

        return Mathf.Abs((Pawn.transform.position - Destination.position).magnitude) < DestinationThreshold;
    }

    protected override void OnBegin()
    {
        ObtainDestination();

        LastThinkingTime = Time.time;

        if (Destination)
        {
            var pawn = Destination.GetComponent<Damageable>();
            if (pawn)
                Driver.ObtainEnemy(pawn);

            PathProgress = 0;
            Path = new NavMeshPath();
            CalcPath(Destination.position, Path);
        }
    }

    protected override void OnEnd()
    {
        Destination = null;
    }

    protected override void OnLateTick()
    {
        // var dir = -(Pawn.transform.position - Destination.position).normalized;
        
        // Pawn.FaceMovementDirection(RotateSpeed);
    }

    protected override Vector3 OnProcessMovement()
    {
        Vector3 direction = Vector3.zero;
        if (HasReachedDestination())
        {
            Driver.SwitchModule(OnDestinationReached);
        }
        else
        {
            if (Time.time - LastThinkingTime > ThinkLag)
            {
                LastThinkingTime = Time.time;
            
                if (CalcPath(Destination.position, Path))
                {
                    PathProgress = 0;
                }
            }

            if (PathProgress + 1 >= Path.corners.Length)
            {
                Driver.SwitchToBestModule();
                return direction;
            }

            if (DrawDebug)
            {
                Debug.DrawLine(transform.position, Path.corners[PathProgress], Color.yellow);

                for (int i = PathProgress; i < Path.corners.Length - 1; i++)
                {
                    Debug.DrawLine(Path.corners[i], Path.corners[i + 1], Color.white);
                }
            }
            
            var diff = transform.position - Path.corners[PathProgress + 1];

            if (diff.magnitude < CornerProximity)
                PathProgress++;
            
            // var diff = Destination.position - transform.position;
            direction = -diff.normalized;

            Debug.DrawRay(transform.position, -diff.normalized, Color.red);
            
            if (Interruptable)
                Driver.SwitchToBestModule();
        }

        return direction;
    }
}

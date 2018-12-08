using System.Collections;
using System.Collections.Generic;
using Data.Scripts.AI.Modules;
using Framework;
using UnityEngine;

public class ModuleFollow : AIModule
{
    public bool DrawDebug;
    public Transform Destination;
    public string TargetTag = "";
    public float DestinationThreshold = 2.0f;

    public DestinationTarget.TargetClass TargetClassToFind;

    public AIModule OnDestinationReached;

    public float TriggerRange = 5;
    
    [Range(0, 100)]
    public float MaxPriority = 100;

    public bool Interruptable;
    
    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, TriggerRange);
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

        if (Destination)
        {
            var pawn = Destination.GetComponent<Damageable>();
            if (pawn)
                Driver.ObtainEnemy(pawn);
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
            var diff = Destination.position - transform.position;
            direction = diff.normalized;
                
            if (Interruptable)
                Driver.SwitchToBestModule();
        }

        return direction;
    }
}

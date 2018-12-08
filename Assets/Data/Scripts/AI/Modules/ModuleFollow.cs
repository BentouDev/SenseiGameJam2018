using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleFollow : AIModule
{
    public bool DrawDebug;
    public Transform Destination;
    public string TargetTag = "";
    public float DestinationThreshold = 2.0f;

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

    protected override float CalcPriority()
    {
        // Find one yourself
        if (Destination == null && !string.IsNullOrEmpty(TargetTag))
        {
            var go = GameObject.FindGameObjectWithTag(TargetTag);
            Destination = go?.transform;
        }

        // Get current enemy
        if (!Destination)
            Destination = Driver.CurrentEnemy?.transform;

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
        if (Destination == null)
        {
            var go = GameObject.FindGameObjectWithTag(TargetTag);
            Destination = go != null ? go.transform : null;
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

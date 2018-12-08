using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public class PawnThrown : PawnState
{
    public float StandingUpTime;
    public LayerMask GroundCheckMask;
    private bool WasGrounded;
    
    protected override void OnStart()
    {
        base.OnStart();
        Pawn.Body.freezeRotation = false;
        Pawn.Body.useGravity = true;
    }

    protected override void OnTick()
    {
        base.OnTick();

        if (Physics.Raycast(Pawn.transform.position, Vector3.down, 1.2f, GroundCheckMask))
        {
            StartCoroutine(ProcessExit());
        }

        WasGrounded = Pawn.IsGrounded;
    }

    IEnumerator ProcessExit()
    {
        yield return new WaitForSeconds(StandingUpTime);
        
        Pawn.SwitchState<PawnIdle>();
    }

    protected override void OnEnd()
    {
        base.OnEnd();
        Pawn.Body.freezeRotation = true;
        Pawn.Body.useGravity = false;
        Pawn.transform.rotation = Quaternion.identity;
    }
}

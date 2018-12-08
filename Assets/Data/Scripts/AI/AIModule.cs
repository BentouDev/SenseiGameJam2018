using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public abstract class AIModule : MonoBehaviour
{
    public bool DebugDisable;
    protected AIDriver Driver;
    protected StatePawn Pawn;
    protected float LastPriorityCalc;

    private float Priority;

    public float GetPriority()
    {
        if (Time.time - LastPriorityCalc > Mathf.Epsilon)
        {
            LastPriorityCalc = Time.time;
            Priority = Mathf.Clamp(CalcPriority(), 0, 100);
        }

        return Priority;
    }

    protected virtual float CalcPriority()
    {
        return 0;
    }

    public void Init(AIDriver driver)
    {
        LastPriorityCalc = 0;
        Driver = driver;
        Pawn = driver.Pawn as StatePawn;

        OnInit();
    }

    public void Begin()
    {
        // Driver.SwitchModule(this);
        OnBegin();
    }

    public void End()
    {
        // Driver.Deactivate(this);
        OnEnd();
    }

    public Vector3 ProcessMovement()
    {
        return OnProcessMovement();
    }

    public void FixedTick()
    {
        OnFixedTick();
    }

    public void LateTick()
    {
        OnLateTick();
    }

    protected virtual void OnInit()
    { }

    protected virtual void OnBegin()
    { }
    
    protected virtual void OnEnd()
    { }

    protected virtual Vector3 OnProcessMovement()
    { return Vector3.zero; }

    protected virtual void OnFixedTick()
    { }

    protected virtual void OnLateTick()
    { }
}

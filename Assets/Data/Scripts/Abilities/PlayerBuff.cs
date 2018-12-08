using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Framework;

public abstract class PlayerBuff : MonoBehaviour, ITickable, System.IComparable<PlayerBuff>
{
    public float Duration;

    public float Elapsed => Time.time - LastBegin;
    public float ToEnd => Duration - Elapsed;
    public float Progress => Elapsed / Duration;

    public float LastBegin { get; private set; }

    protected AbilityContainer Controller;
    
    protected BasePawn Pawn;

    protected virtual void OnInit()
    { }

    protected virtual void OnBegin()
    { }

    protected virtual void OnEnd()
    { }

    protected virtual void OnTick()
    { }
    
    protected virtual void OnFixedTick()
    { }
       
    protected virtual void OnLateTick()
    { }

    public void Init(AbilityContainer controller)
    {
        Controller = controller;
        Pawn = Controller.GetComponent<BasePawn>();

        LastBegin = 0;
        OnInit();
    }

    public void Begin()
    {
        LastBegin = Time.time;
        OnBegin();
    }

    public void End()
    {
        OnEnd();
    }

    public void Tick()
    {
        OnTick();
    }

    public void FixedTick()
    {
        OnFixedTick();
    }

    public void LateTick()
    {
        OnLateTick();
    }

    public int CompareTo(PlayerBuff other)
    {
        // If other is not a valid object reference, this instance is greater.
        if (other == null) return 1;

        // The temperature comparison depends on the comparison of 
        // the underlying Double values. 
        return ToEnd.CompareTo(other.ToEnd);
    }
}

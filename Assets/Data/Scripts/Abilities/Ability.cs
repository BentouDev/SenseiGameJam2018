using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public abstract class Ability : MonoBehaviour, ITickable
{
    [Header("Main")]
    public bool Enabled;
    public string Name = "Ability";
    public Sprite Icon;

    protected BasePawn Pawn;
    protected AbilityContainer Controller;
    
    [Header("Input")]
    [Tooltip("Generic ability if empty")]
    public string Button = "";
    
    [Header("Gameplay")]
    public float CooldownTime = 10;
    public int StaminaCost = 30;
    public int StaminaRequirement = 5;

    [Header("Timing")]
    public float Duration;

    [Range(0,1)]
    public float ActiveBegin;

    [Range(0, 1)]
    public float ActiveEnd;

    [Header("Invincibility")]
    public bool InvincibilityFrames;

    [Range(0, 1)]
    public float InvBegin;

    [Range(0, 1)]
    public float InvEnd;

    public Vector3 DesiredDirection { get; private set; }

    protected float Elapsed => Controller.Elapsed;
    protected float Progress => Elapsed / Duration;

    private float LastStartTime;

    public bool IsCooledDown => Time.time - LastStartTime > CooldownTime;
    public float CooldownProgress => Mathf.Clamp01((Time.time - LastStartTime) / CooldownTime);

    protected bool DoAbort;

    public void EnableThis()
    {
        if (!Controller)
            Controller = GetComponentInChildren<AbilityContainer>() ?? GetComponentInParent<AbilityContainer>();

        Controller.EnableAbility(this);
    }

    public void DisableThis()
    {
        if (!Controller)
            Controller = GetComponentInChildren<AbilityContainer>() ?? GetComponentInParent<AbilityContainer>();

        Controller.DisableAbility(this);
    }

    public void Abort()
    {
        if (Controller.CurrentAbility != this)
            return;

        DoAbort = true;
        Controller.AbortAbility(this);
    }

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

    protected virtual Vector3 OnProcessMovement(Vector3 dir)
    { return dir; }

    public void Init(AbilityContainer owner)
    {
        LastStartTime = 0;
        Controller = owner;
        Pawn = owner.GetComponent<BasePawn>();
        OnInit();
    }

    public void Begin()
    {
        DoAbort = false;
        LastStartTime = Time.time;
        OnBegin();
    }

    public void End()
    {
        Pawn.Damageable.Invincible = false;

        OnEnd();
    }

    public void Tick()
    {
        Pawn.Damageable.Invincible = InvincibilityFrames && Progress > InvBegin && Progress < InvEnd;

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

    public Vector3 ProcessMovement(Vector3 direction)
    {
        return OnProcessMovement(direction);
    }
}
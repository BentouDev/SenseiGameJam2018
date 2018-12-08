using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Stamina))]
public class AbilityContainer : MonoBehaviour, ITickable
{
	[HideInInspector]
    public List<Ability> ButtonAbilities;

	[HideInInspector]
    public List<Ability> GenericAbilities;

	public Stamina Stamina;
    
    public Ability CurrentAbility { get; private set; }
    public Ability NextAbility { get; private set; }

	private HashSet<Ability> ToEnable = new HashSet<Ability>();
    private HashSet<Ability> ToDisable = new HashSet<Ability>();
    private HashSet<PlayerBuff> ToBuff = new HashSet<PlayerBuff>();

    private HashSet<PlayerBuff> BuffsToAbort = new HashSet<PlayerBuff>();
    private HashSet<Ability> AbilitiesToAbort = new HashSet<Ability>();

    private SortedSet<PlayerBuff> Buffs = new SortedSet<PlayerBuff>();

	private float LastAbilityStart;
    private int CurrentIndex;

    public float Elapsed { get; private set; }

	public bool CanSwitchAbility()
	{
		return CurrentAbility == null || Elapsed / CurrentAbility.Duration > CurrentAbility.ActiveEnd;
	}

	public void PushEffect(PlayerBuff buff)
    {
        if (buff) ToBuff.Add(buff);
        else Debug.LogError("Tried to push null buff!", this);
    }

    public void AbortAbility(Ability abs)
    {
        if (abs) AbilitiesToAbort.Add(abs);
        else Debug.LogError("Tried to abort null ability!", this);
    }

    public void AbortBuff(PlayerBuff buff)
    {
        if (buff) BuffsToAbort.Add(buff);
        else Debug.LogError("Tried to abort null buff!", this);
    }

    public void EnableAbility(Ability ability)
    {
        if (ability) ToEnable.Add(ability);
        else Debug.LogError("Tried to enable null ability!", this);
    }

    public void DisableAbility(Ability ability)
    {
        if (ability) ToDisable.Add(ability);
        else Debug.LogError("Tried to disable null ability!", this);
    }

    private void PushBuffInternal(PlayerBuff buff)
    {
        buff.Init(this);
        buff.Begin();
        Buffs.Add(buff);
    }

    private void EnableAbilityInternal(Ability ability)
    {
        ability.Enabled = true;
        ability.Init(this);

        if (string.IsNullOrEmpty(ability.Button))
            GenericAbilities.Add(ability);
        else
            ButtonAbilities.Add(ability);
    }

    private void DisableAbilityInternal(Ability ability)
    {
        ability.Enabled = false;
        GenericAbilities.Remove(ability);
        ButtonAbilities.Remove(ability);
    }

    public void CinematicChangeAbility(Ability abs)
    {
        AbortAbilityInternal(CurrentAbility);
        CurrentAbility = abs;
        abs.Begin();
        LastAbilityStart = Time.time;
        CurrentAbility.Tick();
    }

    public bool ChangeAbility(Ability abs)
    {
        if (abs.IsCooledDown && Stamina.CurrentAmount >= abs.StaminaRequirement)
        {
            NextAbility = abs;
            return true;
        }

        return false;
    }

    private void ChangeAbilityInternal(Ability abs)
    {
        Stamina.TakeStamina(abs.StaminaCost);

        AbortAbilityInternal(CurrentAbility);

        CurrentAbility = abs;
        LastAbilityStart = Time.time;
        Elapsed = 0;
        abs.Begin();
    }

    private void AbortAbilityInternal(Ability abs)
    {
        if (CurrentAbility != null && CurrentAbility == abs)
        {
            CurrentAbility.End();
            CurrentAbility = null;
        }
    }

    private void AbortBuffInternal(PlayerBuff buff)
    {
        if (Buffs.Contains(buff))
        {
            buff.End();
            Buffs.Remove(buff);
        }
    }

    public void Init()
	{
		LastAbilityStart = 0;

		this.TryInit(ref Stamina);
		this.InitList(ref GenericAbilities);
        this.InitList(ref ButtonAbilities);

        var abs = GetComponentsInChildren<Ability>().Where(c => c.Enabled);
        var playerAbilities = abs as IList<Ability> ?? abs.ToList();

        foreach (Ability ability in playerAbilities)
        {
            ability.Init(this);
        }

        GenericAbilities.AddRange(playerAbilities.Where(c => string.IsNullOrEmpty(c.Button)));
        ButtonAbilities.AddRange(playerAbilities.Except(GenericAbilities));
	}

    private void HandleList<T>(ref HashSet<T> list, System.Action<T> act)
    {
        foreach (var element in list)
        {
            act(element);
        }

        list.Clear();
    }
	    
	private void ProcessBuffs()
    {
        PlayerBuff buff = null;
        while (Buffs.Any() && (buff = Buffs.Last()).ToEnd < Mathf.Epsilon)
        {
            buff.End();
            Buffs.Remove(buff);
        }
    }

	public void Preprocess()
	{
	    HandleList(ref AbilitiesToAbort, AbortAbilityInternal);
	    HandleList(ref BuffsToAbort, AbortBuffInternal);
        HandleList(ref ToDisable, DisableAbilityInternal);
        HandleList(ref ToEnable, EnableAbilityInternal);
        HandleList(ref ToBuff, PushBuffInternal);

        ProcessBuffs();
        
	    if (NextAbility != null)
	    {
	        ChangeAbilityInternal(NextAbility);
	        NextAbility = null;
	    }
	    else
	    {
	        Elapsed = Time.time - LastAbilityStart;
	        if (CurrentAbility && Elapsed > CurrentAbility.Duration)
	        {
	            AbortAbilityInternal(CurrentAbility);
	        }
        }
	}

    public void Tick()
    {
        foreach (PlayerBuff buff in Buffs)
        {
            buff.Tick();
        }
    }

    public void FixedTick()
	{
        foreach (PlayerBuff buff in Buffs)
        {
            buff.FixedTick();
        }
	}

	public void LateTick()
	{
		foreach (PlayerBuff buff in Buffs)
        {
            buff.LateTick();
        }
	}

    public Vector3 ProcessAbilities(Vector3 direction)
    {     
        if (CurrentAbility)
        {
            CurrentAbility.Tick();
            direction = CurrentAbility.ProcessMovement(direction);
        }
		
        return direction;
    }
}
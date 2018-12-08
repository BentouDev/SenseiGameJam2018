using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using UnityEditor;
using UnityEngine;

public class AIDriver : Controller
{
    private AIModule ActiveModule;
    private AIModule PreviousModule;
    private AIModule NextModule;

    protected HashSet<AIModule> ModuleHistory = new HashSet<AIModule>();
    protected List<AIModule>    AllModules    = new List<AIModule>();
    public    Damageable        CurrentEnemy { get; private set; }
    
    public Stamina Stamina { get; private set; }
    public AbilityContainer Abilities { get; private set; }

    private bool WasAlive;
    
    protected override void OnInit()
    {
        WasAlive = true;

        Stamina = Pawn.GetComponent<Stamina>();
        Abilities = Pawn.GetComponent<AbilityContainer>();

        Abilities.Init();

        this.InitList(ref AllModules);

        var foundModules = new List<AIModule>();

        foundModules.AddRange(GetComponentsInChildren<AIModule>());

        // Add Pawn modules, if they are present
        if (!Pawn.transform.IsChildOf(transform) && !transform.IsChildOf(Pawn.transform))
        {
            foundModules.AddRange(Pawn.GetComponentsInChildren<AIModule>());
        }
        
        foreach (var module in foundModules)
        {
            module.Init(this);
        }

        AllModules.AddRange(foundModules.Where(m => !m.DebugDisable));

        SwitchModule(PickBestModule());
    }
    
    public void ObtainEnemy(Damageable pawn)
    {
        if (pawn != Pawn && (!pawn || pawn.IsAlive))
            CurrentEnemy = pawn;
    }

    public void Kill()
    {
        DisableInput();
        // MainGame.Instance.AIDirector.OnAIDestroyed(this);

        // Odpal animacje znikania
        // Skasuj po animacji
    }

    private Vector3 ProcessAbilities(Vector3 direction)
    {
        Abilities.Preprocess();

        return Abilities.ProcessAbilities(direction);
    }

    protected override void OnProcessControll()
    {
        if (!Pawn)
            return;

        if (Pawn.Damageable && WasAlive && !Pawn.Damageable.IsAlive)
        {
            Kill();
        }

        if (CurrentEnemy && !CurrentEnemy.IsAlive)
        {
            CurrentEnemy = null;
            
            SwitchToBestModule();
        }

        if (NextModule)
        {
            SwitchModuleInternal(NextModule);
            NextModule = null;
        }

        WasAlive = Pawn.Damageable.IsAlive;

        Vector3 direction = Vector3.zero;
        if (Enabled && Pawn.IsAlive())
        {
            if (ActiveModule)
                direction = ActiveModule.ProcessMovement();

            direction = ProcessAbilities(direction);
        }
        else
        {
            direction = Vector3.zero;
            Pawn.ResetBody();
        }

        Abilities.Tick();
        
        Pawn.ProcessMovement(direction);
        Pawn.Tick();
    }

    protected override void OnFixedTick()
    {
        if (Enabled)
        {
            if (ActiveModule)
                ActiveModule.FixedTick();
        }

        if (Pawn)
        {
            Pawn.FixedTick();
        }
    }

    protected override void OnLateTick()
    {
        if (Enabled)
        {
            if (ActiveModule)
                ActiveModule.LateTick();
        }

        if (Pawn)
            Pawn.LateTick();
    }

    public void SwitchModule(AIModule newModule)
    {
        NextModule = newModule;
    }

    private void SwitchModuleInternal(AIModule newModule)
    {
        if (ActiveModule == newModule)
            return;

        PreviousModule = ActiveModule;

        if (ActiveModule)
            ActiveModule.End();

        ActiveModule = newModule;

        if (ActiveModule)
        {
            ActiveModule.Begin();
            ModuleHistory.Add(ActiveModule);
        }
    }

    public AIModule PickBestModule()
    {
        return AllModules.OrderByDescending(m => m.GetPriority()).FirstOrDefault();
    }

    public AIModule PickShuffleModule()
    {
        if (ModuleHistory.Count == AllModules.Count)
            ModuleHistory.Clear();

        var possibleModules = AllModules.Where(m => m.GetPriority() > Mathf.Epsilon).ToList();
        AIModule nextModule = possibleModules[Random.Range(0, possibleModules.Count)];
        if (AllModules.Count != 1)
        {
            int steps = 0;
            while (steps != possibleModules.Count && (ModuleHistory.Contains(nextModule) || nextModule == PreviousModule))
            {
                nextModule = possibleModules[Random.Range(0, possibleModules.Count)];
                steps++;
            }

            // Unable to pick shuffled module, pick best one
            if (steps == possibleModules.Count)
                nextModule = null;
        }

        return nextModule;
    }

    public void SwitchToBestModule()
    {
        var best = PickBestModule();
        if (best == null)
            return;

        SwitchModule(best);
    }

    protected override void OnDrawDebug()
    {
        PrintDebug();
    }

    protected void PrintDebug()
    {
        Print("Alive : " + Pawn?.IsAlive() + ", Enabled : " + Enabled);
        Print("Enemy : " + CurrentEnemy);
        Print(NextModule != null ? ("Module : " + ActiveModule + ", Next : " + NextModule) : ("Module : " + ActiveModule));

        int i = 1;
        foreach (var module in AllModules)
        {
            Print(i + ": '" + module + "' : " + module.GetPriority());
            i++;
        }
    }
}
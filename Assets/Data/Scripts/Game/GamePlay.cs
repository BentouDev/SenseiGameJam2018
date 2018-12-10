using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Data.Scripts.Game;
using Framework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GamePlay : GameState
{
    [Header("Master")] 
    public PotController Pot;

    public BasePlayerController Player;
    
    [Header("Wave")]
    [RequireValue]
    public EventScheduler Scheduler;
    
    [Validate("ValidateStart")]
    public string WaveStartEvent;

    [Validate("ValidateEnd")]
    public string WaveEndEvent;
    public float WaveDelay = 2;
    private int CurrentWave = 0;
    
    [Header("Enemy Spawn")]
    public GameObject EnemyPrefab;
    public int StartingEnemyCount = 5;
    public float EnemyCountCoefficient = 0.25f;
    public float WorldRadius = 20;
    public float SpawnHeight = 1.5f;

    [Header("UI")] 
    public List<TextMeshProUGUI> WaveText = new List<TextMeshProUGUI>();

    public UnityEvent OnGameBegin;

    public string WavePrefix = "Wave ";
    
    private List<StatePawn> CurrentWaveEnemies = new List<StatePawn>();
    private bool InLimbo;
    
    protected override void OnStart()
    {
        MainGame.Instance.Controllers.Init();
        MainGame.Instance.Controllers.Enable();
        
        OnGameBegin.Invoke();

        foreach (var ai in FindObjectsOfType<AIDriver>())
        {
            ai.EnableInput();
        }
        
        StartWave();
    }

    protected override void OnEnd()
    {
        MainGame.Instance.Controllers.Disable();

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    protected override void OnTick()
    {
        if (Application.isFocused)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;            
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;            
        }

        if (!Pot.IsAlive)
            MainGame.Instance.SwitchState<GameLose>();
        
        if (!Player.Pawn.IsAlive())
            MainGame.Instance.SwitchState<GameLose>();

        if (InLimbo)
            return;

        if (CurrentWaveEnemies.All(e => !e || !e.IsAlive()))
        {
            StartCoroutine(EndWave());
        }
    }

    protected IEnumerator EndWave()
    {
        InLimbo = true;
        {
            foreach (var enemy in CurrentWaveEnemies)
            {
                if (enemy)
                    MainGame.Instance.Controllers.Unregister(enemy.GetComponent<Controller>());
            }
        
            CurrentWaveEnemies.Clear();
        
            if (Scheduler)
                Scheduler.RaiseEvent(WaveEndEvent);
        
            yield return new WaitForSeconds(WaveDelay);

            CurrentWave++;

            StartWave();   
        }
        InLimbo = false;
    }

    protected void StartWave()
    {
        foreach (var proUgui in WaveText)
        {
            proUgui.text = WavePrefix + (CurrentWave + 1);
        }
        
        if (Scheduler)
            Scheduler.RaiseEvent(WaveStartEvent);
        
        SpawnEnemies();
    }

    protected void SpawnEnemies()
    {
        int enemyCount = Mathf.CeilToInt(StartingEnemyCount + CurrentWave * (StartingEnemyCount * EnemyCountCoefficient));
        for (int i = 0; i < enemyCount; i++)
        {
            float angle = Random.Range(0, 360);
            Vector3 pos = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward * WorldRadius;
            pos.y += SpawnHeight;

            var go = Instantiate(EnemyPrefab, pos, Quaternion.identity);
            go.GetComponent<Controller>().EnableInput();
            CurrentWaveEnemies.Add(go.GetComponent<StatePawn>());
        }
    }

    public ValidationResult ValidateStart()
    {
        if (Scheduler && Scheduler.Events.Any(e=>e.Name == WaveStartEvent))
            return ValidationResult.Ok;
        return new ValidationResult(ValidationStatus.Error, $"No event {WaveStartEvent} in scheduler!");
    }
    
    public ValidationResult ValidateEnd()
    {
        if (Scheduler && Scheduler.Events.Any(e=>e.Name == WaveEndEvent))
            return ValidationResult.Ok;
        return new ValidationResult(ValidationStatus.Error, $"No event {WaveEndEvent} in scheduler!");
    }

    public void OnAIKilled(AIDriver driver)
    {
        MainGame.Instance.Controllers.Unregister(driver);
        foreach (var collider in driver.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        driver.GetComponent<DeadBody>().JustDied();
    }
}

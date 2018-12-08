using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Framework;
using UnityEngine;

public class GamePlay : GameState
{
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

    private List<StatePawn> CurrentWaveEnemies = new List<StatePawn>();
    private bool InLimbo;
    
    protected override void OnStart()
    {
        MainGame.Instance.Controllers.Init();
        MainGame.Instance.Controllers.Enable();

        foreach (var ai in FindObjectsOfType<AIDriver>())
        {
            ai.EnableInput();
        }
        
        StartWave();
    }

    protected override void OnEnd()
    {
        MainGame.Instance.Controllers.Disable();
    }

    protected override void OnTick()
    {
        if (InLimbo)
            return;
        
        if (CurrentWaveEnemies.All(e => !e.IsAlive()))
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
                MainGame.Instance.Controllers.Unregister(enemy.GetComponent<Controller>());
                Destroy(enemy.gameObject);
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
}

using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public class GameStart : GameState, ILevelDependable
{
    [Header("Hero")] 
    public string HeroSpawnTag = "PlayerSpawn";
    
    [RequireValue]
    public GameObject HeroPrefab;
    
    [RequireValue]
    public BasePlayerController Controller;

    protected override void OnEnd()
    {
        if (Controller.InitOnStart)
            Debug.LogError("Controller is init on start!");
        
        Controller.Init();
    }

    protected override void OnTick()
    {
        MainGame.Instance.SwitchState<GamePlay>();
    }

    public void OnLevelCleanUp()
    {
        
    }

    public void OnLevelLoaded()
    {
        var spawn = GameObject.FindWithTag(HeroSpawnTag);
        if (!spawn)
        {
            Debug.LogError("No spawn point!");
        }
        else
        {
            var go = Instantiate(HeroPrefab, spawn.transform.position, spawn.transform.rotation);
        }
    }
}

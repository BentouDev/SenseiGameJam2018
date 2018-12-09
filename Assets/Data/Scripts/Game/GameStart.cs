using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

public class GameStart : GameState, ILevelDependable
{
    public AudioSource GameplayMusic;
    
    [Header("Hero")] 
    public string HeroSpawnTag = "PlayerSpawn";
    
    [RequireValue]
    public GameObject HeroPrefab;
    
    [RequireValue]
    public BasePlayerController Controller;

    IEnumerator VolumeDown(AudioSource source)
    {
        GameplayMusic.volume = 0;
        GameplayMusic.Play();
        
        float begin = Time.time;
        float duration = 1;
        while (Time.time - begin < duration)
        {
            var coeff = (Time.time - begin) / duration;
            source.volume = 1- coeff;
            GameplayMusic.volume = coeff;
            yield return null;
        }
        
        source.Stop();
    }

    protected override void OnStart()
    {
        var go = GameObject.FindWithTag("MusicMaster");
        StartCoroutine(VolumeDown(go.GetComponent<AudioSource>()));
    }

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

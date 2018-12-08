using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class MainGame : Game<MainGame>
{
    public PotController Pot;

    protected override void OnSceneLoad()
    {
        base.OnSceneLoad();

        if (!Pot)
            Pot = FindObjectOfType<PotController>();
    }

    public override bool IsPlaying()
    {
        return CurrentState is GamePlay;
    }

    public void OnAIKilled(AIDriver driver)
    {
        var asPlay = CurrentState as GamePlay;
        if (asPlay)
        {
            asPlay.OnAIKilled(driver);
        }
    }
}

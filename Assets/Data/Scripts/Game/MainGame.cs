using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

public class MainGame : Game<MainGame>
{
    public override bool IsPlaying()
    {
        return CurrentState is GamePlay;
    }
}

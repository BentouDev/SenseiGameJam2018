using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Framework;

public class GameLoader : GameState
{
    public SceneReference SceneToLoad;

    private bool ShouldLoadScene()
    {
        if (SceneManager.sceneCount == 1)
            return true;

        return false;
    }

    protected override void OnStart()
    {
        if (ShouldLoadScene())
        {
            BaseGame.Instance.GetLoader().StartLoadScene(SceneToLoad);
        }
    }
}

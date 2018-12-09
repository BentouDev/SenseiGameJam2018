using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using UnityEngine.Playables;

public class GameMenu : GameState, ILevelDependable
{
    public PlayableDirector MenuAnim;
    public SceneReference ToLoad;
    private bool Clicked;

    public void Play()
    {
        if (!Clicked)
        {
            Clicked = true;
            BaseGame.Instance.GetLoader().StartLoadScene(ToLoad);
        }
    }

    public void Quit()
    {
        if (!Clicked)
        {
            Clicked = true;
            BaseGame.Instance.QuitGame();
        }
    }

    public void OnLevelCleanUp()
    {
        
    }

    public void OnLevelLoaded()
    {
        MenuAnim.Play();
    }
}

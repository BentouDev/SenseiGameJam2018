using Framework;
using UnityEngine;

public class GamePlay : GameState
{
    protected override void OnStart()
    {
        MainGame.Instance.Controllers.Init();
        MainGame.Instance.Controllers.Enable();
    }

    protected override void OnEnd()
    {
        MainGame.Instance.Controllers.Disable();
    }
}

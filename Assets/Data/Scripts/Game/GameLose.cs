using Framework;

namespace Data.Scripts.Game
{
    public class GameLose : GameState
    {
        public SceneReference SceneToLoad;

        protected override void OnStart()
        {
            MainGame.Instance.Loader.StartLoadScene(SceneToLoad);
        }
    }
}
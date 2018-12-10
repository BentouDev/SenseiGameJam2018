using System.Collections;
using System.Timers;
using Framework;
using UnityEngine;
using UnityEngine.Events;

namespace Data.Scripts.Game
{
    public class GameLose : GameState
    {
        public SceneReference SceneToLoad;
        public SceneReference SceneToMenu;

        public float AnimStopDuration = 0.5f;
        public UnityEvent OnShowMenu;
        private bool Clicked;

        public void Reload()
        {
            if (Clicked)
                return;
            MainGame.Instance.Loader.StartLoadScene(SceneToLoad, true);
            Clicked = true;
        }

        public void Menu()
        {
            if (Clicked)
                return;
            MainGame.Instance.Loader.StartLoadScene(SceneToMenu, true);
            Clicked = true;
        }

        public void Quit()
        {
            if (Clicked)
                return;
            MainGame.Instance.QuitGame();
            Clicked = true;
        }

        protected override void OnStart()
        {
            // StartCoroutine(ProcessTween());
            OnShowMenu.Invoke();
            FindObjectOfType<GamePlayerController>().Pawn.ResetBody();
        }

        private bool isAnimating;

        IEnumerator ProcessTween()
        {
            float startTime = Time.time;

            isAnimating = true;

            while (Time.time - startTime < AnimStopDuration)
            {
                Time.timeScale = Mathf.Lerp(1, 0, (Time.time - startTime) / AnimStopDuration);
                yield return null;
            }

            isAnimating = false;
            Time.timeScale = 0.0f;
            
            OnShowMenu.Invoke();
        }
    }
}
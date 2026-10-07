using UI.Screens;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.UI.Controllers
{
    public class GameSceneScreenController : MonoBehaviour
    {
        [SerializeField] private GameOverScreen _gameOverScreen;

        private void Awake()
        {
            _gameOverScreen.OnReplayButtonClick += HandleReplayButtonClick;
            _gameOverScreen.OnHomeButtonClick += HandleHomeButtonClick;
        }

        private void OnDestroy()
        {
            _gameOverScreen.OnReplayButtonClick -= HandleReplayButtonClick;
            _gameOverScreen.OnHomeButtonClick -= HandleHomeButtonClick;
        }

        public void SetupGameOverScreen(int score, int earnedCoins) => _gameOverScreen.SetupScreen(score, earnedCoins);
        public void OpenGameOverScreen() => _gameOverScreen.Open();

        private void HandleReplayButtonClick() => SceneManager.LoadSceneAsync(GameConstants.GAME);
        private void HandleHomeButtonClick() => SceneManager.LoadSceneAsync(GameConstants.MAIN_MENU);
    }
}
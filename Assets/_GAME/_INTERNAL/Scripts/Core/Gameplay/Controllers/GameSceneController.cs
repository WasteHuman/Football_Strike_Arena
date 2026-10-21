using Core.Common;
using Core.Data;
using Core.Gameplay.Game.Player;
using Core.Gameplay.Game.TargetSystem;
using Core.Services;
using Core.Services.AdsService;
using Core.Services.Analytics;
using Core.UI.Controllers;
using UI.Other;
using UI.Player;
using UnityEngine;

namespace Core.Gameplay.Controllers
{
    public class GameSceneController : SceneController
    {
        [Header("Controllers (View & Gmae)")]
        [SerializeField] private PlayerInfoPanelView _playerInfoPanelView;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private TargetSystemController _targetSystemController;
        [SerializeField] private GameplayTutorialView _gameplayTutorialView;

        [Space(5), Header("Screens")]
        [SerializeField] private GameSceneScreenController _screenController;

        private const string  Player_Lose_Count = "Player_Lose_Count";
        private static int _currentPlayerLoseCount;

        public override void Enter()
        {
            _playerController.OnPlayerLose += HandlePlayerLose;
            _gameplayTutorialView.OnTutorialSkipped += HandleGameplayStarted;
        }

        public override void Initialize()
        {
            _playerInfoPanelView.Init();
            _playerController.Initialize();

            _currentPlayerLoseCount = PlayerPrefs.GetInt(Player_Lose_Count, 0);

            if(!GameServices.PlayerService.IsTutorialCompleted)
                _gameplayTutorialView.Initialize();
            else
                HandleGameplayStarted();
        }

        public override void Exit()
        {
            _playerController.OnPlayerLose -= HandlePlayerLose;
            _gameplayTutorialView.OnTutorialSkipped -= HandleGameplayStarted;

            _playerController.Dispose();
            _playerInfoPanelView.Dispose();

            PlayerPrefs.SetInt(Player_Lose_Count, _currentPlayerLoseCount);
        }

        private void HandlePlayerLose()
        {
            var earnedCoins = GameServices.PlayerService.SessionEarnedCoins;
            var sessionScore = GameServices.PlayerService.SessionPlayerScore;

            GameResult sessionResult = new(false, earnedCoins, sessionScore);
             
            _targetSystemController.FreezeAllTargets();
            _screenController.SetupGameOverScreen(sessionResult.Score, Mathf.RoundToInt(sessionResult.RewardCoins));
            _screenController.OpenGameOverScreen();

            GameServices.GameSessionService.HandleEndedGame(sessionResult);
            AnalyticsService.Instance.ReportGameLoss();

            if(_currentPlayerLoseCount < 5)
            {
                _currentPlayerLoseCount++;
                #if UNITY_EDITOR
                Debug.Log($"[Game Scene Controller] Current Player Lose Count: {_currentPlayerLoseCount}");
                #endif
            }
            else
            {
                AdsService.Instance.ShowInterstitialAd(() =>
                {
                    _currentPlayerLoseCount = 0;
                });
            }
        }

        private void HandleGameplayStarted()
        {
            if (_gameplayTutorialView != null && !GameServices.PlayerService.IsTutorialCompleted)
            {
                _gameplayTutorialView.Hide();
                GameServices.PlayerService.MarkTutorialAsCompleted();
            }

            _gameplayTutorialView.gameObject.SetActive(false);

            _targetSystemController.Initialize();
            _playerController.StartGameplay();
        }
    }
}
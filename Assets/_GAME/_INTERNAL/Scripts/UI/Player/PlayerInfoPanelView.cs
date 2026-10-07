using Core.Services;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI.Player
{
    public class PlayerInfoPanelView : MonoBehaviour
    {
        [Header("Lables Setup")]
        [SerializeField] private TextMeshProUGUI _playerBalanceLabel;
        [SerializeField] private TextMeshProUGUI _currentPlayerScoreLabel;

        [Space(5), Header("Animation Durations Setup")]
        [SerializeField] private float _coinsBalanceChangedAnimationDuration = 0.5f;

        private float _displayedCoinsBalance;

        public void Init()
        {
            GameServices.EconomyService.OnCoinsBalanceChanged += HandleChangedCoinsBalance;
            GameServices.PlayerService.OnCurrentPlayerScoreChanged += HandleUpdatedCurrentPlayerScore;
            _displayedCoinsBalance = GameServices.EconomyService.GetCoinsBalance();
            
            HandleUpdatedCurrentPlayerScore(GameServices.PlayerService.SessionPlayerScore);

            GameServices.EconomyService.RequestCoinsBalance();
        }

        void OnDestroy() => Dispose();

        public void Dispose()
        {
            GameServices.EconomyService.OnCoinsBalanceChanged -= HandleChangedCoinsBalance;
            GameServices.PlayerService.OnCurrentPlayerScoreChanged -= HandleUpdatedCurrentPlayerScore;
        }

        public void HandleUpdatedCurrentPlayerScore(int score)
        {
            if(_currentPlayerScoreLabel != null)
                _currentPlayerScoreLabel.text = $"Score: {score}";
        }

        private void HandleChangedCoinsBalance(float amount)
        {
            float oldValue = _displayedCoinsBalance;
            _displayedCoinsBalance = amount;

            DOVirtual.Float(oldValue, _displayedCoinsBalance, _coinsBalanceChangedAnimationDuration, value =>
            {
                _playerBalanceLabel.text = $"{value:N0}";
            });
        }
    }
}
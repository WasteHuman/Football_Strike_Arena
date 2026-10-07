using Screen = UI.Other.Screen;
using UnityEngine;
using UI.Other;
using System;
using DG.Tweening;
using TMPro;
using Core.Services.Audio;

namespace UI.Screens
{
    public class GameOverScreen : Screen
    {
        [Header("Refs")]
        [SerializeField] private RectTransform _panelRect;
        [SerializeField] private TextMeshProUGUI _scoreLabel;
        [SerializeField] private TextMeshProUGUI _earnedCoinsLabel;

        [Space(5), Header("Animation Setup")]
        [SerializeField] private float _animationDuration = 0.55f;
        [SerializeField] private float _targetY;

        [Space(5), Header("Buttons")]
        [SerializeField] private ActionButton _replayButton;
        [SerializeField] private ActionButton _homeButton;

        private Tween _openTween;

        public event Action OnHomeButtonClick;
        public event Action OnReplayButtonClick;

        private void Awake()
        {
            _replayButton.OnButtonClick += HandleReplayButtonClick;
            _homeButton.OnButtonClick += HandleHomeButtonClick;
        }

        private void OnDestroy()
        {
            _replayButton.OnButtonClick -= HandleReplayButtonClick;
            _homeButton.OnButtonClick -= HandleHomeButtonClick;
        }

        public override void Open()
        {
            _panelRect.gameObject.SetActive(true);
            _openTween?.Kill();

            _openTween = _panelRect
                .DOAnchorPosY(_targetY, _animationDuration)
                .SetEase(Ease.InOutBack)
                .OnComplete(() => _panelRect.anchoredPosition = new(_panelRect.anchoredPosition.x, _targetY));

            AudioService.Instance.PlaySfx(SoundType.Player_Lose);
        }

        public void SetupScreen(int score, int earnedCoins)
        {
            if(_scoreLabel != null)
                _scoreLabel.text = $"Score: {score}";
            
            if(_earnedCoinsLabel != null)
                _earnedCoinsLabel.text = $"+{earnedCoins}";
        }

        private void HandleReplayButtonClick()
        {
            OnReplayButtonClick?.Invoke();
        }

        private void HandleHomeButtonClick()
        {
            OnHomeButtonClick?.Invoke();
        }
    }
}
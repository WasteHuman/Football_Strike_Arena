using Screen = UI.Other.Screen;
using UnityEngine;
using UI.Other;
using System;

namespace UI.Screens
{
    public class WelcomeScreenView : Screen
    {
        [Header("Action Buttons Setup")]
        [SerializeField] private ActionButton _letsPlayButton;

        public event Action OnLetsPlayButtonClick;

        private void Awake()
        {
            _letsPlayButton.OnButtonClick += HandleLetsPlayButtonClick;
        }

        private void OnDestroy()
        {
            _letsPlayButton.OnButtonClick -= HandleLetsPlayButtonClick;
        }

        private void HandleLetsPlayButtonClick() => OnLetsPlayButtonClick?.Invoke();
    }
}
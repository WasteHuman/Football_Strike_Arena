using UI.Other;
using UnityEngine;
using Screen = UI.Other.Screen;

namespace UI.Screens
{
    public class MainMenuScreenView : Screen
    {
        [SerializeField] private ActionButtonsAnimationsService _animationService;

        private void Awake()
        {
            _animationService.StartAsyncWaveAnimation().Forget();
        }

    }
}
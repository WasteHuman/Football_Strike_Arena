using UI.Other;
using UnityEngine;
using Screen = UI.Other.Screen;

namespace UI.Screens
{
    public class GameScreenView : Screen
    {
        [SerializeField] private ActionButtonsAnimationsService _animationService;

        void Awake() => _animationService.StartAsyncWaveAnimation().Forget();
    }
}
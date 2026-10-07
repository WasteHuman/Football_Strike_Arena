using UI.Other;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.UI.Controllers
{
    public class MainMenuButtonsController : MonoBehaviour
    {
        [SerializeField] private ActionButton _playButton;
        [SerializeField] private ActionButton _wheelOfLuckButton;
        [SerializeField] private ActionButton _shopButton;
        [SerializeField] private ActionButton _settingsButton;

        private void Awake()
        {
            _playButton.OnButtonClick += HandlePlayButtonClick;
            _wheelOfLuckButton.OnButtonClick += HandleWheelOfLuckButtonClick;
            _shopButton.OnButtonClick += HandleShopButtonClick;
            _settingsButton.OnButtonClick += HandleSettignsButtonClick;
        }

        private void OnDestroy()
        {
            _playButton.OnButtonClick -= HandlePlayButtonClick;
            _wheelOfLuckButton.OnButtonClick -= HandleWheelOfLuckButtonClick;
            _shopButton.OnButtonClick -= HandleShopButtonClick;
            _settingsButton.OnButtonClick -= HandleSettignsButtonClick;
        }

        private void HandleSettignsButtonClick() => SceneManager.LoadSceneAsync(GameConstants.SETTINGS);

        private void HandleShopButtonClick() => SceneManager.LoadSceneAsync(GameConstants.SHOP_MENU);

        private void HandleWheelOfLuckButtonClick() => SceneManager.LoadSceneAsync(GameConstants.WHEEL_OF_LUCK);

        private void HandlePlayButtonClick() => SceneManager.LoadSceneAsync(GameConstants.GAME);
    }
}
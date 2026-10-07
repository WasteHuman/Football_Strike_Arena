using UI.Other;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.UI.Controllers
{
    public class GameNavigationButtonsController : MonoBehaviour
    {
        [SerializeField] private ActionButton _wheelOfLuckButton;
        [SerializeField] private ActionButton _shopButton;
        [SerializeField] private ActionButton _settingsButton;

        private void Awake()
        {
            _wheelOfLuckButton.OnButtonClick += HandleWheelOfLuckButtonClick;
            _shopButton.OnButtonClick += HandleShopButtonClick;
            _settingsButton.OnButtonClick += HandleSettignsButtonClick;
        }

        private void OnDestroy()
        {
            _wheelOfLuckButton.OnButtonClick -= HandleWheelOfLuckButtonClick;
            _shopButton.OnButtonClick -= HandleShopButtonClick;
            _settingsButton.OnButtonClick -= HandleSettignsButtonClick;
        }

        private void HandleSettignsButtonClick() => SceneManager.LoadSceneAsync(GameConstants.SETTINGS);

        private void HandleShopButtonClick() => SceneManager.LoadSceneAsync(GameConstants.SHOP_MENU);

        private void HandleWheelOfLuckButtonClick() => SceneManager.LoadSceneAsync(GameConstants.WHEEL_OF_LUCK);
    }
}
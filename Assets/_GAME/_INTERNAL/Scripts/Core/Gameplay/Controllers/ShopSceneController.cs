using Core.Common;
using UI.Player;
using UI.Shop;
using UnityEngine;

namespace Core.Gameplay.Controllers
{
    public class ShopSceneController : SceneController
    {
        [SerializeField] private PlayerInfoPanelView _playerInfoView;
        [SerializeField] private PlayerSkinsView _playerSkinsView;
        [SerializeField] private ShopController _shopController;

        public override void Enter()
        {
            _shopController.Enter();
        }
        
        public override void Initialize()
        {
            _playerInfoView.Init();
            _playerSkinsView.Init();
            _shopController.Initialize();
        }

        public override void Exit()
        {
            _playerInfoView.Dispose();
            _shopController.Exit();
        }
    }
}
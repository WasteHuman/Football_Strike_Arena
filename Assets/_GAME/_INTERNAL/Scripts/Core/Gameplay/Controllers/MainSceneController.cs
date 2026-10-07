using Core.Common;
using UI.Player;
using UnityEngine;

namespace Core.Gameplay.Controllers
{
    public class MainSceneController : SceneController
    {
        [SerializeField] private PlayerInfoPanelView _playerInfoPanelView;
        [SerializeField] private PlayerSkinsView _playerSkinsView;

        public override void Enter() {}
        
        public override void Initialize()
        {
            _playerInfoPanelView.Init();
            _playerSkinsView.Init();
        }

        public override void Exit()
        {
            _playerInfoPanelView.Dispose();
        }
    }
}
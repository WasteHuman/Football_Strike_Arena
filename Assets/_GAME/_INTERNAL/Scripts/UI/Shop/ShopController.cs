using System.Collections.Generic;
using Core.Services;
using Core.Services.Shop;
using Core.SO;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Shop
{
    public class ShopController : MonoBehaviour
    {
        [Header("Ball Skin Views Setup")]
        [SerializeField] private ShopItemView _ballCardPrefab;
        [SerializeField] private RectTransform _container;

        [Space(5), Header("Player Skin Views")]
        [SerializeField] private List<ShopItemView> _playerSkinViews = new();

        [Space(5), Header("Ball Skin Views")]
        [SerializeField] private List<ShopItemView> _ballSkinViews = new();

        [Space(5), Header("Upgrade Views")]
        [SerializeField] private List<ShopItemView> _upgradeViews = new();

        [Space(5), Header("Sprites Config")]
        [SerializeField] private ShopEntitiesConfig _config;

        private ShopService _shopService;

        public void  Enter()
        {
            _shopService = GameServices.ShopService;

            InitBallViewCards();
            InitPlayerSkinViews();
            InitPlayerUpgradeViews();
        }

        public void Initialize()
        {
            for (int i = 0; i < _ballSkinViews.Count; i++)
            {
                var ballView = _ballSkinViews[i];
                ballView.OnItemPurchased += HandlePurchasedItem;
                ballView.OnItemSelected += HandleSelectedItem;
            }

            for (int i = 0; i < _playerSkinViews.Count; i++)
            {
                var playerSkinView = _playerSkinViews[i];
                playerSkinView.OnItemPurchased += HandlePurchasedItem;
                playerSkinView.OnItemSelected += HandleSelectedItem;
            }

            for (int i = 0; i < _upgradeViews.Count; i++)
            {
                var upgradeView = _upgradeViews[i];
                upgradeView.OnItemPurchased += HandlePurchasedItem;
            }
        }

        public void Exit()
        {
            for (int i = 0; i < _ballSkinViews.Count; i++)
            {
                var ballView = _ballSkinViews[i];
                ballView.OnItemPurchased -= HandlePurchasedItem;
                ballView.OnItemSelected -= HandleSelectedItem;
            }

            for (int i = 0; i < _playerSkinViews.Count; i++)
            {
                var playerSkinView = _playerSkinViews[i];
                playerSkinView.OnItemPurchased -= HandlePurchasedItem;
                playerSkinView.OnItemSelected -= HandleSelectedItem;
            }

            for (int i = 0; i < _upgradeViews.Count; i++)
            {
                var upgradeView = _upgradeViews[i];
                upgradeView.OnItemPurchased -= HandlePurchasedItem;
            }
        }

        private void InitBallViewCards()
        {
            var ballEntities = _shopService.BallsData;

            for (int i = 0; i < ballEntities.Count; i++)
            {
                var ballView = Instantiate(_ballCardPrefab, _container);
                var ballData = ballEntities[i];
                bool isSelected = GameServices.PlayerService.CurrentPlayerBallSkinId == ballData.EntityID;

                ballView.Init(
                    _config.GetPlayerBallSkinById(ballData.EntityID), 
                    ballData.EntityID, 
                    ballData.EntityDescription, 
                    ballData.EntityCost, 
                    ballData.Type,
                    ballData.IsPurchased,
                    isSelected);

                _ballSkinViews.Add(ballView);
            }
        }

        private void InitPlayerSkinViews()
        {
            var skinEntities = _shopService.SkinsData;

            for (int i = 0; i < skinEntities.Count; i++)
            {
                var skinView = _playerSkinViews[i];
                var skinData = skinEntities[i];
                bool isSelected = GameServices.PlayerService.CurrentPlayerSkinId == skinData.EntityID;

                skinView.Init(
                    _config.GetItemSpriteById(skinData.EntityID), 
                    skinData.EntityID, 
                    skinData.EntityDescription, 
                    skinData.EntityCost, 
                    skinData.Type,
                    skinData.IsPurchased,
                    isSelected);
            }
        }

        private void InitPlayerUpgradeViews()
        {
            var upgradeEntities = _shopService.UpgradesData;

            for (int i = 0; i < upgradeEntities.Count; i++)
            {
                var upgradeView = _upgradeViews[i];
                var upgradeData = upgradeEntities[i];

                upgradeView.Init(
                    _config.GetPlayerUpgradeSpriteById(upgradeData.EntityID), 
                    upgradeData.EntityID, 
                    upgradeData.EntityDescription, 
                    upgradeData.EntityCost, 
                    upgradeData.Type,
                    upgradeData.IsPurchased);
            }
        }

        private void HandlePurchasedItem(string id)
        {
            if (id.Contains("Skin") && !id.Contains("Ball"))
            {
                var selectedItem = _playerSkinViews.Find(skin => skin.ItemId == id);
                if (selectedItem == null)
                    return;

                if (_shopService.BuyPlayerSkin(id))
                    selectedItem.UpdateToPurchasedItemView(true);
                return;
            }

            if (id.Contains("Ball"))
            {
                var selectedItem = _ballSkinViews.Find(skin => skin.ItemId == id);
                if (selectedItem == null)
                    return;

                if (_shopService.BuyPlayerBallSkin(id))
                    selectedItem.UpdateToPurchasedItemView(true);
                return;
            }

            if (id.Contains("Upgrade"))
            {
                var selectedItem = _upgradeViews.Find(skin => skin.ItemId == id);
                if (selectedItem == null)
                    return;

                if (_shopService.BuyPlayerUpgrade(id))
                    selectedItem.UpdateToPurchasedItemView(true);
            }
        }

        private void HandleSelectedItem(string id)
        {
            if (id.Contains("Skin"))
            {
                _shopService.SelectPlayerSkin(id);
                var prevSelectedItem = _playerSkinViews.Find(skin => skin.IsSelected);
                prevSelectedItem.UpdateToUnselectedItemView(prevSelectedItem.Type);

                var selectedItem = _playerSkinViews.Find(skin => skin.ItemId == id);
                selectedItem.UpdateToSelectedItemView(true);
            }

            if (id.Contains("Ball"))
            {
                _shopService.SelectPlayerBallSkin(id);
                var prevSelectedItem = _ballSkinViews.Find(skin => skin.IsSelected);
                prevSelectedItem.UpdateToUnselectedItemView(prevSelectedItem.Type);

                var selectedItem = _ballSkinViews.Find(skin => skin.ItemId == id);
                selectedItem.UpdateToSelectedItemView(true);
            }
        }
    }
}
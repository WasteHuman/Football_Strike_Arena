using System.Collections.Generic;
using Core.Data;
using Core.Gameplay;
using Core.Services.Player;
using Core.SO;
using UnityEngine;

namespace Core.Services.Shop
{
    public class ShopService
    {
        private readonly List<ShopEntityDataRuntime> _ballsData = new();
        private readonly List<ShopEntityDataRuntime> _skinsData = new();
        private readonly List<ShopEntityDataRuntime> _upgradesData = new();

        private PlayerService _playerService;
        private EconomyService _economyService;

        public IReadOnlyList<ShopEntityDataRuntime> BallsData => _ballsData.AsReadOnly();
        public IReadOnlyList<ShopEntityDataRuntime> SkinsData => _skinsData.AsReadOnly();
        public IReadOnlyList<ShopEntityDataRuntime> UpgradesData => _upgradesData.AsReadOnly();

        public void Init(PlayerService playerService, EconomyService economyService, ShopEntitiesConfig entitiesConfig)
        {
            _playerService = playerService;
            _economyService = economyService;

            InitPlayerSkinDatas(entitiesConfig.PlayerSkinsData.AsReadOnly());
            InitPlayerBallSkinDatas(entitiesConfig.PlayerBallSkinsData.AsReadOnly());
            InitPlayerUpgradedatas(entitiesConfig.PlayerUpgradesData.AsReadOnly());
        }

        public bool BuyPlayerSkin(string skinId)
        {
            var skin = _skinsData.Find(skin => skin.EntityID == skinId);

            if(skin == null)
            {
                Debug.LogWarning($"[Shop Service] Skin to bought is null!");
                return false;
            }

            if (skin.IsPurchased)
            {
                return false;
            }

            if (!_economyService.HasEnoughBalance(skin.EntityCost))
            {
                Debug.LogWarning($"[Shop Service] Player not enough coins for buy this skin {skinId}!");
                return false;
            }

            _economyService.SpendCoins(skin.EntityCost);
            skin.IsPurchased = true;
            _playerService.AddSkinToPurchased(skin.EntityID);
            GameServices.SaveAll();
            return true;
        }

        public bool BuyPlayerBallSkin(string skinId)
        {
            var skin = _ballsData.Find(skin => skin.EntityID == skinId);

            if(skin == null)
            {
                Debug.LogWarning($"[Shop Service] Skin to bought is null!");
                return false;
            }

            if (skin.IsPurchased)
            {
                return false;
            }

            if (!_economyService.HasEnoughBalance(skin.EntityCost))
            {
                Debug.LogWarning($"[Shop Service] Player not enough coins for buy this skin {skinId}!");
                return false;
            }

            _economyService.SpendCoins(skin.EntityCost);
            skin.IsPurchased = true;
            _playerService.AddBallSkinToPurchased(skin.EntityID);
            GameServices.SaveAll();
            return true;
        }

        public bool BuyPlayerUpgrade(string upgradeId)
        {
            var upgrade = _upgradesData.Find(skin => skin.EntityID == upgradeId);

            if(upgrade == null)
            {
                Debug.LogWarning($"[Shop Service] Upgrade to bought is null!");
                return false;
            }

            if (upgrade.IsPurchased)
            {
                return false;
            }

            if (!_economyService.HasEnoughBalance(upgrade.EntityCost))
            {
                Debug.LogWarning($"[Shop Service] Player not enough coins for buy this upgrade {upgradeId}!");
                return false;
            }

            _economyService.SpendCoins(upgrade.EntityCost);
            upgrade.IsPurchased = true;
            _playerService.AddUpgradeToPurchased(upgrade.EntityID);

            if(upgradeId.Contains("Damage"))
                _playerService.DoublePlayerDamage();
             
            if(upgradeId.Contains("Reload"))
                _playerService.ReducePlayerReload();

            GameServices.SaveAll();
            return true;
        }

        public void SelectPlayerSkin(string skinId)
        {
            var skin = _skinsData.Find(skin => skin.EntityID == skinId);

            if(skin == null)
            {
                Debug.LogWarning($"[Shop Service] Selected skin is null!");
                return;
            }

            _playerService.ChangePlayerSkin(skinId);
            GameServices.SaveAll();
        }

        public void SelectPlayerBallSkin(string skinId)
        {
            var skin = _ballsData.Find(skin => skin.EntityID == skinId);

            if(skin == null)
            {
                Debug.LogWarning($"[Shop Service] Selected ball skin is null!");
                return;
            }

            _playerService.ChangePlayerBallSkin(skinId);
            GameServices.SaveAll();
        }

        private void InitPlayerSkinDatas(IReadOnlyList<ShopEntityData> playerSkinDatas)
        {
            for (int i = 0; i < playerSkinDatas.Count; i++)
            {
                var data = playerSkinDatas[i];
                var runtimeData = new ShopEntityDataRuntime(data.EntityID, data.EntityCost, data.Type, data.EntityDescription, data.IsPurchased);
                _skinsData.Add(runtimeData);
            }

            _skinsData.Sort((a, b) => a.EntityCost.CompareTo(b.EntityCost));
            var purchasedSkins = _playerService.GetData().PurchasedPlayerSkins;
            for (int i = 0; i < purchasedSkins.Count; i++)
            {
                var purchasedSkin = purchasedSkins[i];
                var skinDef = _skinsData.Find(skin => skin.EntityID == purchasedSkin);
                if (skinDef != null)
                {
                    skinDef.IsPurchased = true;
                }
            }
        }

        private void InitPlayerBallSkinDatas(IReadOnlyList<ShopEntityData> playerBallSkinDatas)
        {
            for (int i = 0; i < playerBallSkinDatas.Count; i++)
            {
                var data = playerBallSkinDatas[i];
                var runtimeData = new ShopEntityDataRuntime(data.EntityID, data.EntityCost, data.Type, data.EntityDescription, data.IsPurchased);
                _ballsData.Add(runtimeData);
            }

            _ballsData.Sort((a, b) => a.EntityCost.CompareTo(b.EntityCost));
            var purchasedSkins = _playerService.GetData().PurchasedPlayerBallSkins;
            for (int i = 0; i < purchasedSkins.Count; i++)
            {
                var purchasedSkin = purchasedSkins[i];
                var skinDef = _ballsData.Find(skin => skin.EntityID == purchasedSkin);
                if (skinDef != null)
                {
                    skinDef.IsPurchased = true;
                }
            }
        }

        private void InitPlayerUpgradedatas(IReadOnlyList<ShopEntityData> upgradeDatas)
        {
            for (int i = 0; i < upgradeDatas.Count; i++)
            {
                var data = upgradeDatas[i];
                var runtimeData = new ShopEntityDataRuntime(data.EntityID, data.EntityCost, data.Type, data.EntityDescription, data.IsPurchased);
                _upgradesData.Add(runtimeData);
            }

            _upgradesData.Sort((a, b) => a.EntityCost.CompareTo(b.EntityCost));
            var purchasedSkins = _playerService.GetData().PurchasedUpgrades;
            for (int i = 0; i < purchasedSkins.Count; i++)
            {
                var purchasedSkin = purchasedSkins[i];
                var upgradeDef = _upgradesData.Find(upgrade => upgrade.EntityID == purchasedSkin);
                if (upgradeDef != null)
                {
                    upgradeDef.IsPurchased = true;
                }
            }
        }
    }
}
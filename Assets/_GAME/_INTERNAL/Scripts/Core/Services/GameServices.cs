using System.Collections.Generic;
using Core.Gameplay;
using Core.Services.Player;
using Core.Services.SaveSystem;
using Core.Services.Shop;
using Core.SO;
using UnityEngine;

namespace Core.Services
{
    public static class GameServices
    {
        public static PlayerService PlayerService { get; private set; }
        public static SaveService SaveService { get; private set; }
        public static EconomyService EconomyService { get; private set; }
        public static ShopService ShopService { get; private set; }
        public static GameSessionService GameSessionService { get; private set; }

        public static void InitializeAll(bool isDebug = false)
        {
            SaveService = new();
            SaveService.Init(isDebug);

            PlayerService = new();
            PlayerService.Init(SaveService.PlayerData);

            EconomyService = new();
            EconomyService.Init(PlayerService.PlayerCoins);

            var shopEntitiesConfig = Resources.Load<ShopEntitiesConfig>("Configs/Shop/ShopEntitiesConfig");
            if(shopEntitiesConfig == null)
            {
                Debug.LogError($"[Game Services] Shop Entities Config is null!");
                return;
            }

            ShopService = new();
            ShopService.Init(PlayerService, EconomyService, shopEntitiesConfig);

            GameSessionService = new();
            GameSessionService.Init(EconomyService, PlayerService);
        }

        public static void SaveAll()
        {
            if (SaveService == null || SaveService.PlayerData == null)
                return;

            if (PlayerService != null)
            {
                var playerData = PlayerService.GetData();
                
                SaveService.PlayerData.CurrentPlayerSkinId = playerData.CurrentPlayerSkinId;
                SaveService.PlayerData.CurrentPlayerBallSkinId = playerData.CurrentPlayerBallSkinId;
                SaveService.PlayerData.CurrentPlayerDamage = playerData.CurrentPlayerDamage;
                SaveService.PlayerData.CurrentPlayerReload = playerData.CurrentPlayerReload;
                SaveService.PlayerData.IsTutorialCompleted = playerData.IsTutorialCompleted;

                SaveService.PlayerData.PurchasedPlayerSkins = new(playerData.PurchasedPlayerSkins);

                SaveService.PlayerData.PurchasedPlayerBallSkins = new(playerData.PurchasedPlayerBallSkins);

                SaveService.PlayerData.PurchasedUpgrades = new(playerData.PurchasedUpgrades);
            }

            if (EconomyService != null)
                SaveService.PlayerData.Coins = EconomyService.GetCoinsBalance();

            SaveService.PlayerData.EnsureValidState();
            SaveService.SavePlayerData();
        }
    }
}
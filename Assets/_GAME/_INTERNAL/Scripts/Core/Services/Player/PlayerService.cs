using System;
using Core.Data;
using UnityEngine;

namespace Core.Services.Player
{
    public class PlayerService
    {
        private PlayerData _currentPlayerData;

        private int _sessionPlayerScore;
        private int _sessionEarnedCoins;

        public float PlayerCoins => _currentPlayerData.Coins;
        public string CurrentPlayerSkinId => _currentPlayerData.CurrentPlayerSkinId;
        public string CurrentPlayerBallSkinId => _currentPlayerData.CurrentPlayerBallSkinId;
        public int CurrentPlayerDamage => _currentPlayerData.CurrentPlayerDamage;
        public float CurrentPlayerReload => _currentPlayerData.CurrentPlayerReload;
        public int SessionPlayerScore => _sessionPlayerScore;
        public int SessionEarnedCoins => _sessionEarnedCoins;
        public bool IsTutorialCompleted => _currentPlayerData.IsTutorialCompleted;

        public event Action<string> OnPlayerBallSkinChanged;
        public event Action<string> OnPlayerSkinChanged;
        public event Action<int> OnCurrentPlayerScoreChanged;

        public void Init(PlayerData playerData)
        {
            _currentPlayerData = playerData;
        }

        public void DoublePlayerDamage() => _currentPlayerData.CurrentPlayerDamage *= 2;
        public void ReducePlayerReload() => _currentPlayerData.CurrentPlayerReload *= 0.85f;
        public void MarkTutorialAsCompleted()
        {
            _currentPlayerData.IsTutorialCompleted = true;
            PlayerPrefs.SetInt(GameConstants.KEY_TUTORIAL_COMPLETED, 1);
            PlayerPrefs.Save();
        }

        public void ChangePlayerSkin(string skinId)
        {
            _currentPlayerData.CurrentPlayerSkinId = skinId;
            OnPlayerSkinChanged?.Invoke(skinId);
        }

        public void ChangePlayerBallSkin(string skinId)
        {
            _currentPlayerData.CurrentPlayerBallSkinId = skinId;
            OnPlayerBallSkinChanged?.Invoke(skinId);
        }

        public void AddSkinToPurchased(string skinId)
        {
            if (!_currentPlayerData.PurchasedPlayerSkins.Contains(skinId))
                _currentPlayerData.PurchasedPlayerSkins.Add(skinId);
        }

        public void AddBallSkinToPurchased(string skinId)
        {
            if (!_currentPlayerData.PurchasedPlayerBallSkins.Contains(skinId))
                _currentPlayerData.PurchasedPlayerBallSkins.Add(skinId);
        }

        public void AddUpgradeToPurchased(string upgradeId)
        {
            if (!_currentPlayerData.PurchasedUpgrades.Contains(upgradeId))
                _currentPlayerData.PurchasedUpgrades.Add(upgradeId);
        }

        public void ResetSessionScore() => _sessionPlayerScore = 0;
        public void ResetEarnedSessionCoins() => _sessionEarnedCoins = 0;
        public void IncreasePlayerSessionScore()
        {
            _sessionPlayerScore++;
            OnCurrentPlayerScoreChanged?.Invoke(_sessionPlayerScore);
        }
        public void AddEarnedCoins(int amount) => _sessionEarnedCoins += amount;

        /// <summary>
        /// Получить прямой доступ к PlayerData для сложных операций
        /// Использовать осторожно, только для внутренних сервисов
        /// </summary>
        internal PlayerData GetData() => _currentPlayerData;
    }
}
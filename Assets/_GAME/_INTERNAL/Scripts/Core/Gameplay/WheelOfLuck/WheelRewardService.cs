using System;
using Core.Data;
using Core.Services;
using Core.Services.Analytics;
using UnityEngine;

namespace Core.WheelOfLuck
{
    public sealed class WheelRewardService
    {
        private readonly WheelState _state;
        private readonly WheelConfig _config;
        private readonly WheelPersistence _persistence;

        public WheelRewardService(
            WheelState state,
            WheelConfig config,
            WheelPersistence persistence)
        {
            _state = state;
            _config = config;
            _persistence = persistence;
        }

        public GameResult Apply(WheelReward reward, int bonusMultiplier = 1)
        {
            if (reward == null)
                return default;

            switch (reward.Type)
            {
                case WheelReward.RewardType.Coins:
                    return HandleGameResult(reward, reward.Amount);

                case WheelReward.RewardType.XP:
                    return HandleGameResult(reward, 0f);

                case WheelReward.RewardType.FreeSpin:
                {
                    int spins = Mathf.RoundToInt(reward.Amount) * Mathf.Max(1, bonusMultiplier);

                    _state.FreeSpins += spins;
                    _state.NextAvailableUtc = DateTimeOffset.UtcNow;

                    _persistence.Save(_state);

                    return HandleGameResult(reward, 0f);
                }

                case WheelReward.RewardType.Nothing:
                    return HandleGameResult(reward, 0f);

                case WheelReward.RewardType.Sector:
                    Debug.LogWarning(
                        "[Wheel] Sector reward is not supported by the generic wheel. " +
                        "The old Neon Wheel flow was intentionally removed.");

                    return HandleGameResult(reward, 0f, false);

                default:
                    Debug.LogError($"[Wheel] Unsupported reward type: {reward.Type}");
                    return default;
            }
        }

        private GameResult HandleGameResult(
            WheelReward reward,
            float rewardCoins,
            bool? overrideIsWin = null)
        {
            if (string.IsNullOrWhiteSpace(_config.GameId))
            {
                Debug.LogWarning(
                    "[Wheel] GameId is empty. GameResult will use an empty game ID.");
            }

            bool isWin = overrideIsWin ?? reward.IsWin;

            GameResult result = new(
                isWin: isWin,
                rewardCoins: rewardCoins);

            if(result.RewardCoins > 0f)
                GameServices.EconomyService.AddCoins(result.RewardCoins);

            if (reward.ReportAnalytics)
            {
                if (isWin)
                    AnalyticsService.Instance.ReportGameWin(_config.GameId);
                else
                    AnalyticsService.Instance.ReportGameLoss(_config.GameId);
            }

            return result;
        }
    }
}

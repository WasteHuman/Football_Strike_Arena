using System;
using UnityEngine;

namespace Core.WheelOfLuck
{
    [Serializable]
    public class WheelReward
    {
        public enum RewardType
        {
            FreeSpin,
            Coins,
            Nothing,
            Multiplier,
            Energy,
            XP,
            Sector
        }

        [Tooltip("Тип награды")]
        public RewardType Type = RewardType.Nothing;

        [Tooltip("Количество награды")]
        public float Amount;

        [Tooltip("Вес при случайном выборе. Чем больше вес, тем выше шанс.")]
        [Min(0f)]
        public float Weight = 1f;

        [Tooltip("Считается ли результат победой.")]
        public bool IsWin = true;

        [Tooltip("Базовый XP, который будет добавлен в GameResult.")]
        [Min(0)]
        public int BaseXP;

        [Tooltip("Отправлять ли win/loss analytics для этой награды.")]
        public bool ReportAnalytics = true;

        public override string ToString() => $"{Type} x{Amount}";
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Core.WheelOfLuck
{
    public sealed class WheelRewardSelector
    {
        public int SelectIndex(IReadOnlyList<RewardView> rewards)
        {
            if (rewards == null || rewards.Count == 0)
                return -1;

            float totalWeight = 0f;

            for (int i = 0; i < rewards.Count; i++)
                totalWeight += Mathf.Max(0f, rewards[i].Reward.Weight);

            if (totalWeight <= 0f)
                return Random.Range(0, rewards.Count);

            float randomValue = Random.value * totalWeight;
            float accumulated = 0f;

            for (int i = 0; i < rewards.Count; i++)
            {
                accumulated += Mathf.Max(0f, rewards[i].Reward.Weight);

                if (randomValue <= accumulated)
                    return i;
            }

            return rewards.Count - 1;
        }
    }
}

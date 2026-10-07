using System;
using UnityEngine;

namespace Core.WheelOfLuck
{
    public sealed class WheelPersistence
    {
        private const string FreeSpinsSuffix = "_FreeSpins";
        private const string NextAvailableSuffix = "_NextAvailable";

        private readonly string _prefix;

        public WheelPersistence(string stateId)
        {
            _prefix = $"Wheel_{stateId}";
        }

        public void Load(WheelState state, WheelConfig config)
        {
            state.FreeSpins = PlayerPrefs.GetInt(
                _prefix + FreeSpinsSuffix,
                config.InitialFreeSpins);

            long unixTime = Convert.ToInt64(
                PlayerPrefs.GetString(_prefix + NextAvailableSuffix, "0"));

            state.NextAvailableUtc = unixTime == 0
                ? DateTimeOffset.MinValue
                : DateTimeOffset.FromUnixTimeSeconds(unixTime);
        }

        public void Save(WheelState state)
        {
            PlayerPrefs.SetInt(_prefix + FreeSpinsSuffix, state.FreeSpins);

            long unixTime = state.NextAvailableUtc == DateTimeOffset.MinValue
                ? 0
                : state.NextAvailableUtc.ToUnixTimeSeconds();

            PlayerPrefs.SetString(
                _prefix + NextAvailableSuffix,
                unixTime.ToString());

            PlayerPrefs.Save();
        }

        public void Reset(WheelState state, WheelConfig config)
        {
            state.FreeSpins = config.InitialFreeSpins;
            state.NextAvailableUtc = DateTimeOffset.MinValue;
            state.PendingReward = null;
            state.IsSpinning = false;

            Save(state);
        }
    }
}

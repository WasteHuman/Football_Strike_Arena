using System;
using UnityEngine;

namespace Core.WheelOfLuck
{
    [CreateAssetMenu(
        fileName = "WheelConfig",
        menuName = "Wheel Of Luck/Wheel Config")]
    public class WheelConfig : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Уникальный ID состояния этого колеса. Не меняйте после релиза.")]
        [SerializeField] private string _stateId = "wheel";

        [Tooltip("ID игры для GameResult, аналитики и статистики.")]
        [SerializeField] private string _gameId;

        [Header("Spin")]
        [SerializeField, Min(0.1f)] private float _spinDuration = 4f;
        [SerializeField, Min(0)] private int _minFullRotations = 4;

        [Header("Free Spins")]
        [SerializeField, Min(0)] private int _initialFreeSpins = 1;
        [Tooltip("Если включено, когда колесо доступно и бесплатных спинов нет, будет выдан 1 спин.")]
        [SerializeField] private bool _autoGrantFreeSpinWhenAvailable = true;

        [Header("Cooldown")]
        [SerializeField] private bool _hasCooldown = true;
        [SerializeField, Min(0f)] private float _cooldownHours = 12f;

        [Header("Cost")]
        [SerializeField] private bool _requiresEnergy;
        [SerializeField, Min(0)] private int _energyCost;

        public string StateId => string.IsNullOrWhiteSpace(_stateId) ? name : _stateId;
        public string GameId => _gameId;
        public float SpinDuration => _spinDuration;
        public int MinFullRotations => _minFullRotations;
        public int InitialFreeSpins => _initialFreeSpins;
        public bool AutoGrantFreeSpinWhenAvailable => _autoGrantFreeSpinWhenAvailable;
        public bool HasCooldown => _hasCooldown;
        public TimeSpan Cooldown => TimeSpan.FromHours(_cooldownHours);
        public bool RequiresEnergy => _requiresEnergy;
        public int EnergyCost => _energyCost;

        private void OnValidate()
        {
            _initialFreeSpins = Mathf.Max(0, _initialFreeSpins);
            _minFullRotations = Mathf.Max(0, _minFullRotations);
            _spinDuration = Mathf.Max(0.1f, _spinDuration);
            _cooldownHours = Mathf.Max(0f, _cooldownHours);
            _energyCost = Mathf.Max(0, _energyCost);
        }
    }
}

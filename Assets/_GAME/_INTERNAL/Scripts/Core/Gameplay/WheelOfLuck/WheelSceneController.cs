using System;
using System.Collections;
using Core.Common;
using UI.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.WheelOfLuck
{
    public class WheelSceneController : SceneController
    {
        [SerializeField] private WheelConfig _config;
        [SerializeField] private WheelView _view;
        [SerializeField] private PlayerInfoPanelView _playerInfoView;

        [Header("Debug")]
        [SerializeField] private bool _isDebug;

        private WheelState _state;
        private WheelPersistence _persistence;
        private WheelRewardSelector _rewardSelector;
        private WheelRewardService _rewardService;

        private Coroutine _cooldownCoroutine;

        public event Action<WheelReward> OnSpinStarted;
        public event Action<WheelReward> OnSpinFinished;
        public event Action OnStateChanged;

        public WheelConfig Config => _config;
        public int GetFreeSpins() => _state?.FreeSpins ?? 0;

        public override void Enter()
        {
            if (_config == null)
            {
                Debug.LogError("[Wheel] WheelConfig is not assigned.", this);
                enabled = false;
                return;
            }

            if (_view == null)
            {
                Debug.LogError("[Wheel] WheelView is not assigned.", this);
                enabled = false;
                return;
            }

            _state = new WheelState();
            _persistence = new WheelPersistence(_config.StateId);
            _rewardSelector = new WheelRewardSelector();
            _rewardService = new WheelRewardService(
                _state,
                _config,
                _persistence);

            _persistence.Load(_state, _config);
            _view.Enter();

            TryAutoGrantFreeSpin();
            RefreshView();

            _view.OnSpinClicked += HandleSpinClicked;
        }

        public override void Initialize()
        {
            StartCooldownUpdater();
            _playerInfoView.Init();
        }

        public override void Exit()
        {
            if (_view != null)
                _view.OnSpinClicked -= HandleSpinClicked;

            StopCooldownUpdater();
            _playerInfoView.Dispose();
        }

        private void OnDisable()
        {
            if(_view != null)
                _view.SetPulse(false);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (_isDebug && Keyboard.current.rKey.wasPressedThisFrame)
                ResetStateForDebug();
#endif
        }

        public bool IsAvailable()
        {
            return !_config.HasCooldown ||
                   DateTimeOffset.UtcNow >= _state.NextAvailableUtc;
        }

        public TimeSpan GetRemainingCooldown()
        {
            if (!_config.HasCooldown || IsAvailable())
                return TimeSpan.Zero;

            return _state.NextAvailableUtc - DateTimeOffset.UtcNow;
        }

        public bool CanSpin()
        {
            if (_state.IsSpinning)
                return false;

            if (_state.FreeSpins <= 0)
                return false;

            if (!IsAvailable())
                return false;

            if (_view.RewardViews == null || _view.RewardViews.Count == 0)
                return false;

            if (_view.RewardViews.Count != _view.RewardViews.Count)
            {
                Debug.LogWarning(
                    $"[Wheel] Reward count mismatch. Config: {_view.RewardViews.Count}, " +
                    $"View: {_view.RewardViews.Count}.",
                    this);

                return false;
            }

            return true;
        }

        /// <summary>
        /// Starts a regular free spin.
        /// </summary>
        public void PrepareAndStartSpin(Action onComplete = null)
        {
            if (!CanSpin())
            {
                Debug.LogWarning("[Wheel] Cannot start spin. Check CanSpin().");
                return;
            }

            if (!TrySpendEnergy())
                return;

            int targetIndex = _rewardSelector.SelectIndex(_view.RewardViews);

            if (targetIndex < 0)
            {
                Debug.LogError("[Wheel] Failed to select reward.");
                return;
            }

            WheelReward reward = _view.RewardViews[targetIndex].Reward;

            _state.PendingReward = reward;
            _state.IsSpinning = true;

            OnSpinStarted?.Invoke(reward);

            _view.SetSpinInteractable(false);
            _view.SetPulse(false);

            _view.SpinTo(
                targetIndex,
                _config.SpinDuration,
                _config.MinFullRotations,
                () => FinishSpin(onComplete));
        }

        /// <summary>
        /// Claims the reward selected by the latest completed spin.
        /// </summary>
        public void ClaimWithoutAd(int bonusMultiplier = 1)
        {
            if (_state.PendingReward == null)
            {
                Debug.LogWarning("[Wheel] There is no pending reward to claim.");
                return;
            }

            WheelReward reward = _state.PendingReward;

            _rewardService.Apply(reward, bonusMultiplier);

            _state.PendingReward = null;

            RefreshView();
            OnStateChanged?.Invoke();
        }

        private void HandleSpinClicked()
        {
            PrepareAndStartSpin(() => ClaimWithoutAd());
            _state.FreeSpins = Mathf.Max(0, _state.FreeSpins - 1);
        }

        private void FinishSpin(Action onComplete)
        {
            _state.IsSpinning = false;

            if (_config.HasCooldown && _state.FreeSpins == 0)
                _state.NextAvailableUtc = DateTimeOffset.UtcNow + _config.Cooldown;

            _persistence.Save(_state);

            WheelReward reward = _state.PendingReward;

            Debug.Log($"[Wheel] Spin finished. Reward: {reward}");

            OnSpinFinished?.Invoke(reward);
            OnStateChanged?.Invoke();

            RefreshView();

            onComplete?.Invoke();
        }

        private bool TrySpendEnergy()
        {
            if (!_config.RequiresEnergy)
                return true;

            Debug.LogWarning(
                $"[Wheel] Not enough energy. Required: {_config.EnergyCost}");

            return false;
        }

        private void TryAutoGrantFreeSpin()
        {
            if (!_config.AutoGrantFreeSpinWhenAvailable)
                return;

            if (!_config.HasCooldown)
                return;

            if (!IsAvailable() || _state.FreeSpins > 0)
                return;

            _state.FreeSpins = 1;
            _state.NextAvailableUtc = DateTimeOffset.MinValue;

            _persistence.Save(_state);
        }

        private void RefreshView()
        {
            if (_view == null || _state == null)
                return;

            _view.SetFreeSpins(_state.FreeSpins);
            _view.SetCooldown(GetRemainingCooldown());

            bool canSpin = CanSpin();

            _view.SetSpinInteractable(canSpin);
            _view.SetPulse(canSpin);
        }

        private void StartCooldownUpdater()
        {
            StopCooldownUpdater();
            _cooldownCoroutine = StartCoroutine(CooldownRoutine());
        }

        private void StopCooldownUpdater()
        {
            if (_cooldownCoroutine == null || this == null)
                return;

            StopCoroutine(_cooldownCoroutine);
            _cooldownCoroutine = null;
        }

        private IEnumerator CooldownRoutine()
        {
            while (true)
            {
                if (_state != null)
                {
                    _view.SetCooldown(GetRemainingCooldown());

                    if (!_state.IsSpinning &&
                        _config.HasCooldown &&
                        IsAvailable() &&
                        _state.FreeSpins > 0)
                    {
                        _view.SetSpinInteractable(CanSpin());
                        _view.SetPulse(CanSpin());
                    }
                }

                yield return new WaitForSeconds(1f);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("DEBUG: Reset Wheel State")]
        private void ResetStateForDebug()
        {
            _persistence.Reset(_state, _config);
            RefreshView();
            OnStateChanged?.Invoke();

            Debug.Log("[Wheel] DEBUG: state reset.");
        }
#endif
    }
}
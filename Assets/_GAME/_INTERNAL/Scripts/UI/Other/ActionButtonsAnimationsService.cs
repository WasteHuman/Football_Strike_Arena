using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace UI.Other
{
    public class ActionButtonsAnimationsService : MonoBehaviour
    {
        [Header("Buttons references")]
        [SerializeField] private List<ActionButton> _buttonsToWaveAnimation = new();

        [Space(5), Header("Wave Setup")]
        [SerializeField] private Vector2 _waveTargetScale;
        [SerializeField] private float _waveDelay = 0.1f;
        [SerializeField] private float _waveDuration = 0.5f;
        [SerializeField] private float _delayBetweenWaves = 1.25f;

        private CancellationTokenSource _cts;

        private Sequence _waveSequence;

        private void OnDestroy() => StopWaveAnimation();

        public async UniTaskVoid StartAsyncWaveAnimation()
        {
            _cts = new();

            CancellationToken token = _cts.Token;

            while (!token.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenWaves));

                if (this == null)
                    return;

                WaveAnimation();

                if (_waveSequence == null)
                    continue;

                await _waveSequence.AsyncWaitForCompletion().AsUniTask();
            }
        }

        private void WaveAnimation()
        {
            _waveSequence?.Kill();

            _waveSequence = DOTween.Sequence();

            for (int i = 0; i < _buttonsToWaveAnimation.Count; i++)
            {
                ActionButton button = _buttonsToWaveAnimation[i];

                if (button == null || button.Animations == null)
                    continue;

                Tween buttonWave = button.Animations.GetWaveTween(_waveTargetScale, _waveDuration);

                _waveSequence.Insert(i * _waveDelay, buttonWave);
            }
        }

        public void ForceStopAnimationWithComplete() => StopWaveAnimation(true);

        public void StopWaveAnimation(bool complete = false)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _waveSequence?.Kill(complete);
            _waveSequence = null;
        }
    }
}
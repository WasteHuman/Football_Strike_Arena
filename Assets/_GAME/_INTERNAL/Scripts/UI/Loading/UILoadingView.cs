using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Loading
{
    public class UILoadingView : MonoBehaviour
    {
        [SerializeField] private Slider _progressBar;
        [SerializeField] private float _animDuration;

        private Tween _progressTween;

        public void ResetProgress()
        {
            if (_progressBar != null)
            {
                _progressTween?.Kill();
                _progressBar.value = 0f;
            }
        }

        public void SetLoadingProgress(float progress)
        {
            var normalizedProgress = Mathf.Clamp01(progress);

            _progressTween?.Kill();
            _progressTween = _progressBar.DOValue(normalizedProgress, _animDuration).SetEase(Ease.Linear);
        }
    }
}
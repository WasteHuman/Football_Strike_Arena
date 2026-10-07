using Core.Services.Audio;
using DG.Tweening;
using UnityEngine;

namespace Core.Gameplay.Game.TargetSystem
{
    public class CoinPickupView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _dropHeight = 0.35f;
        [SerializeField] private float _dropDuration = 0.22f;
        [SerializeField] private float _travelDuration = 0.42f;
        [SerializeField] private float _minScaleOffset = 0.5f;
        [SerializeField] private float _maxScaleOffset = 1f;
        [SerializeField] private Vector3 _originalScale;

        private Tween _currentTween;

        void OnDestroy()
        {
            _currentTween?.Kill();
        }

        public void Initialize(Sprite sprite, Vector3 startPosition, Vector3 destination)
        {
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_spriteRenderer == null)
                return;

            float randomScaleOffset = Random.Range(_minScaleOffset, _maxScaleOffset);
            float randomTravelDuration = Random.Range(-0.1f, 0.1f);
            float travelDuration = _travelDuration + randomTravelDuration;
            Vector3 randomScale = _originalScale * randomScaleOffset;

            _currentTween?.Kill();

            _spriteRenderer.sprite = sprite;
            _spriteRenderer.color = Color.white;
            transform.position = startPosition;
            transform.localScale = randomScale;

            var dropTarget = startPosition + new Vector3(Random.Range(-0.12f, 0.12f), -_dropHeight, 0f);

            var sequence = DOTween.Sequence();
            sequence.Append(transform.DOScale(randomScale, 0.12f).SetEase(Ease.OutBack));
            sequence.Append(transform.DOJump(dropTarget, 0.26f, 1, _dropDuration).SetEase(Ease.OutQuad));
            sequence.Append(transform.DOMove(destination, travelDuration).SetEase(Ease.InOutCubic));
            sequence.Join(_spriteRenderer.DOFade(0.1f, travelDuration * 0.75f).SetEase(Ease.InCubic));
            sequence.OnComplete(() =>
            {
                _currentTween = transform.DOScale(Vector3.zero, 0.18f).SetEase(Ease.InBack);
                _currentTween.OnComplete(() =>
                {
                    _spriteRenderer.color = Color.white;
                    _spriteRenderer.DOFade(1f, 0f);
                    gameObject.SetActive(false);
                });
            });

            _currentTween = sequence;
        }
    }
}
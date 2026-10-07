using DG.Tweening;
using UnityEngine;

namespace Core.Extensions.RectTransform
{
    public static class RectTransformExtensions
    {
        public static Tween DORollTo(this UnityEngine.RectTransform rect, Vector2 target, float duration, float rotations = 1f)
        {
            Sequence sequence = DOTween.Sequence();

            sequence
                .Join(rect.DOAnchorPos(target, duration).SetEase(Ease.InOutCubic));
            sequence
                .Join(rect.DORotate(new(0f, 0f, 360f * rotations), duration, RotateMode.FastBeyond360));

            return sequence;
        }
    }
}
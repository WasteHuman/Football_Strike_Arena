using System;
using Core.Gameplay.Game.Player;
using Core.Services.Audio;
using TMPro;
using UI.Animations.Game;
using UnityEngine;

namespace Core.Gameplay.Game.TargetSystem
{
    public class TargetBallView : ObjectAnimations
    {
        [Header("Visual Setup")]
        [SerializeField] private SpriteRenderer _targetSprite;
        [SerializeField] private TextMeshPro _hpLabel;
        [SerializeField] private ParticleSystem _destroyVfx;

        [Space(5), Header("Physics Setup")]
        [SerializeField] private float _minImpulseForce = 12.5f;
        [SerializeField] private float _maxImpulseForce = 20f;
        [SerializeField] private float _speed = 2.5f;

        private Rigidbody2D _rb;
        private CircleCollider2D _collider;

        private int _currentHp;
        private int _initialHp;
        private bool _isDestroyed;

        public bool CanSplit => _initialHp >= 4;
        public int InitialHp => _initialHp;
        public Vector3 OriginalScale => _originalScale;

        public event Action<TargetBallView> OnTargetDestroyed;
        public event Action<TargetBallView> OnTargetSplitted;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _collider = GetComponent<CircleCollider2D>();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if(_isDestroyed || !collision.gameObject.TryGetComponent<PlayerProjectile>(out var projectile))
                return;

            ApplyDamage(projectile.CurrentDamage);
        }

        void FixedUpdate()
        {
            if(_rb != null && _rb.linearVelocity.sqrMagnitude > 0f)
                _rb.linearVelocity = _rb.linearVelocity.normalized * _speed;
        }

        public void Init(int initialHp, Vector2 position, Vector2 impulseDirection)
        {
            _originalScale = transform.localScale;
            _initialHp = initialHp;
            _currentHp = initialHp;
            _isDestroyed = false;
            _hpLabel.text = $"{_currentHp}";

            _rb.bodyType = RigidbodyType2D.Dynamic;

            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _collider.enabled = true;

            transform.position = position;
            float endImpulseForce = UnityEngine.Random.Range(_minImpulseForce, _maxImpulseForce);

            _rb.AddForce(impulseDirection * endImpulseForce, ForceMode2D.Impulse);
        }

        public void SetSprite(Sprite sprite) => _targetSprite.sprite = sprite;

        public void SetScale(Vector3 scale)
        {
            transform.localScale = scale;
            _originalScale = scale;
        }

        public void FreezeTarget()
        {
            _rb.bodyType = RigidbodyType2D.Static;

            CollapseAnimation();
            PlayDestroyVfx();
        }

        private void ApplyDamage(int damage)
        {
            if(_hpLabel == null)
                return;

            _currentHp -= damage;

            if(_currentHp <= 0 && !CanSplit)
                HandleGoneHealth(OnTargetDestroyed);
            else if(_currentHp <= 0 && CanSplit)
                HandleGoneHealth(OnTargetSplitted);

            _hpLabel.text = $"{_currentHp}";
        }

        private void HandleGoneHealth(Action<TargetBallView> onComplete)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.bodyType = RigidbodyType2D.Static;
            _collider.enabled = false;

            _currentHp = 0;
            _isDestroyed = true;
            CollapseAnimation(() => onComplete?.Invoke(this));

            PlayDestroyVfx();
        }

        private void PlayDestroyVfx()
        {
            if (_destroyVfx == null)
                return;

            _destroyVfx.transform.position = transform.position;
            _destroyVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            _destroyVfx.Clear();
            _destroyVfx.Play();

            AudioService.Instance.PlaySfx(SoundType.Target_Ball_Explosion);
        }
    }
}
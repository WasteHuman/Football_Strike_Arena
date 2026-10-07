using Core.Gameplay.Game.DifficultSystem;
using Core.Gameplay.Game.Player;
using Core.Services;
using Core.Services.Audio;
using SO.Game;
using SO.Game.DifficultSystem;
using UnityEngine;
using Utils;

namespace Core.Gameplay.Game.TargetSystem
{
    public class TargetSystemController : MonoBehaviour
    {
        [Header("Configs")]
        [SerializeField] private TargetBallSpritesConfig _spritesConfig;
        [SerializeField] private DifficultSystemConfig _difficultConfig;
        [SerializeField] private float _totalEarnedCoinsMultiplier = 2f;

        [Space(5), Header("Coin Visuals")]
        [SerializeField] private CoinPickupView _coinPrefab;
        [SerializeField] private Transform _coinsContainer;
        [SerializeField] private Sprite _coinSprite;
        [SerializeField] private int _coinPoolInitialCount = 50;

        [Space(5), Header("Spawn System")]
        [SerializeField] private TargetBallSpawner _spawner;

        [Space(5), Header("Refs")]
        [SerializeField] private PlayerController _playerController;

        private bool _isInitialized;

        private GameDifficultSystem _gameDifficultSystem;
        private ObjectPool<CoinPickupView> _coinPool;
        private Transform _playerCoinTarget;

        void OnDestroy()
        {
            if (_spawner != null)
                _spawner.OnTargetDestroyed -= HandleDestroyedTarget;
        }

        public void Initialize()
        {
            if(_isInitialized)
                return;

            _isInitialized = true;

            _gameDifficultSystem = new(_difficultConfig);

            if (_coinPrefab != null)
            {
                _coinPool = new ObjectPool<CoinPickupView>(_coinPrefab, _coinPoolInitialCount, _coinsContainer)
                {
                    AutoExpand = true
                };
                _playerCoinTarget = _playerController.transform;
            }

            _spawner.Init(_spritesConfig, _gameDifficultSystem);
            _spawner.OnTargetDestroyed += HandleDestroyedTarget;
        }

        public void FreezeAllTargets() => _spawner.StopSpawnAndFreezeActiveTargets();

        private void HandleDestroyedTarget(TargetBallView view)
        {
            if (view == null)
                return;

            int earnedCoins = Mathf.RoundToInt(view.InitialHp * _totalEarnedCoinsMultiplier);

            GameServices.PlayerService.IncreasePlayerSessionScore();
            GameServices.PlayerService.AddEarnedCoins(earnedCoins);
            GameServices.GameSessionService.HandleDestroyedTarget(earnedCoins);
            
            SpawnCoinBurst(view.transform.position, earnedCoins);
        }

        private void SpawnCoinBurst(Vector3 startPosition, int coinCount)
        {
            if (_coinPool == null || _coinPrefab == null || _coinSprite == null)
                return;

            if (_playerCoinTarget == null)
                _playerCoinTarget = _playerController.transform;

            if (_playerCoinTarget == null)
                return;

            int visualCoinsCount = Mathf.Clamp(coinCount, 1, _coinPoolInitialCount);
            Vector3 playerPosition = _playerCoinTarget.position;

            for (int i = 0; i < visualCoinsCount; i++)
            {
                var coin = _coinPool.GetFreeElement();
                coin.Initialize(
                    _coinSprite,
                    startPosition + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.25f, 0.15f), 0f),
                    playerPosition + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.15f, 0.2f), 0f));
            }
            AudioService.Instance.PlaySfx(SoundType.Coins_Take);
        }
    }
}
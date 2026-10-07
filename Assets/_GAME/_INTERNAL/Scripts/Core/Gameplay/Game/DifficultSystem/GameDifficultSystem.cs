using SO.Game.DifficultSystem;
using UnityEngine;

namespace Core.Gameplay.Game.DifficultSystem
{
    public class GameDifficultSystem
    {
        private readonly int _minTargetHp;
        private readonly int _maxTargetHp;
        private readonly int _maxDifficultyDestroyedTargets;
        private readonly int _randomHpRange;

        private readonly float _minScale;
        private readonly float _maxScale;

        private int _currentTargetsHp;
        private Vector3 _currentScale;
        private int _currentDestroyedTargets = 0;

        private readonly AnimationCurve _difficultCurve;

        public GameDifficultSystem(DifficultSystemConfig config)
        {
            _difficultCurve = config.DifficultCurve;

            _currentTargetsHp = config.MinTargetsHp;
            _currentScale = new(config.MinTargetScale, config.MinTargetScale, config.MinTargetScale);
            _minTargetHp = config.MinTargetsHp;
            _maxTargetHp = config.MaxTargetsHp;
            _randomHpRange = config.HpRandomRangeOffset;

            _maxDifficultyDestroyedTargets = config.MaxDifficultyDestroyedTargets;

            _minScale = config.MinTargetScale;
            _maxScale = config.MaxTargetScale;
        }

        public void RecalculateDifficult()
        {
            _currentDestroyedTargets++;

            float progress = Mathf.Clamp01((float)_currentDestroyedTargets / _maxDifficultyDestroyedTargets);
            float difficulty = _difficultCurve.Evaluate(progress);

            int baseHp = Mathf.RoundToInt(Mathf.Lerp(_minTargetHp, _maxTargetHp, difficulty));
            int hp = baseHp + Random.Range(-_randomHpRange, _randomHpRange);
            
            _currentTargetsHp = Mathf.Clamp(hp, _minTargetHp, _maxTargetHp);

            float scale = Mathf.Lerp(_minScale, _maxScale, difficulty);
            float normalizedScale = Mathf.Clamp(scale, _minScale, _maxScale);
            _currentScale = new(normalizedScale, normalizedScale, normalizedScale);
        }

        public int GetCurrentTargetsHP() => _currentTargetsHp; 
        public Vector3 GetCurrentScale() => _currentScale;
    }
}
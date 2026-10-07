using UnityEngine;

namespace SO.Game.DifficultSystem
{
    [CreateAssetMenu(menuName = "Game/Difficult System", fileName = "DifficultSystemConfig")]
    public class DifficultSystemConfig : ScriptableObject
    {
        [field: SerializeField] public AnimationCurve DifficultCurve { get; private set; }
        [field: SerializeField] public int MinTargetsHp { get; private set; }
        [field: SerializeField] public int MaxTargetsHp { get; private set; }
        [field: SerializeField] public int HpRandomRangeOffset { get; private set; }
        [field: SerializeField] public int MaxDifficultyDestroyedTargets { get; private set; }
        [field: SerializeField] public float MinTargetScale { get; private set; }
        [field: SerializeField] public float MaxTargetScale { get; private set; }
    }
}
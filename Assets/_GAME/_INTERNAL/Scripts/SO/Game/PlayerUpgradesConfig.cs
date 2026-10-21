using UnityEngine;

namespace SO.Game
{
    [CreateAssetMenu(menuName = "Game/Shop/Player Upgrades Config", fileName = "PlayerUpgradesConfig")]
    public class PlayerUpgradesConfig : ScriptableObject
    {
        [field: SerializeField] public int PlayerDamageIncreaseMultiplier { get; private set; }

        [field: SerializeField, Tooltip("Variable must be in 0-1 range")] 
        [Range(0f, 1f)] public float PlayerReloadReduceMultilplier { get; private set; }
    }
}
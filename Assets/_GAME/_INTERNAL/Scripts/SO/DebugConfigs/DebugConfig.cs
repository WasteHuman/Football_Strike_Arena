using UnityEngine;

namespace Core.SO.DebugConfigs
{
    [CreateAssetMenu(menuName = "Configs/Debug/Debug Config", fileName = "DebugConfig")]
    public class DebugConfig : ScriptableObject
    {
        [field: SerializeField] public bool IsDebug { get; private set; } = false;
    }
}
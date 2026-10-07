using UnityEngine;

namespace Extensions.GameObject
{
    public static class GameObjectExtensions
    {
        public static bool GetComponentOrNull<T>(this UnityEngine.GameObject gameObject) where T : class
        {
            gameObject.TryGetComponent<T>(out var component);

            if(component == null)
                return false;

            return true;
        }
    }
}
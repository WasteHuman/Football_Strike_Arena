using UnityEngine;

namespace UI.Other
{
    public abstract class Screen : MonoBehaviour
    {
        public bool IsActive => gameObject.activeSelf;

        public virtual void Open() => gameObject.SetActive(true);
        public virtual void Close() => gameObject.SetActive(false);
    }
}
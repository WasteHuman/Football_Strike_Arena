using DG.Tweening;
using UnityEngine;

namespace Core.Common
{
    public abstract class BaseSceneEntryPoint : MonoBehaviour
    {
        [SerializeField] protected SceneController _controller;

        protected virtual void Start()
        {
            _controller.Enter();
            _controller.Initialize();
        }

        protected virtual void OnDestroy()
        {
            _controller.Exit();
            DOTween.KillAll();
        }
    }
}
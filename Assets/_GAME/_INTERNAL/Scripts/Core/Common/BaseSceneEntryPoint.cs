using DG.Tweening;
using UnityEngine;

namespace Core.Common
{
    public abstract class BaseSceneEntryPoint : MonoBehaviour
    {
        [SerializeField] protected SceneController _controller;

        private void Start()
        {
            _controller.Enter();
            _controller.Initialize();
        }

        private void OnDestroy()
        {
            _controller.Exit();
            DOTween.KillAll();
        }
    }
}
using UnityEngine;
using UnityEngine.Video;

namespace UI.Other
{
    public class TutorialVideoPlayer : MonoBehaviour
    {
        [SerializeField] private VideoPlayer _videoPlayer;

        void OnEnable()
        {
            _videoPlayer.errorReceived += OnVideoError;
            _videoPlayer.prepareCompleted += OnVideoPrepared;
            _videoPlayer.Prepare();
        }

        void OnDisable()
        {
            _videoPlayer.errorReceived -= OnVideoError;
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            _videoPlayer.Stop();
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            source.Play();
        }

        private void OnVideoError(VideoPlayer source, string message)
        {
            Debug.LogError($"Tutorial video failed to play: {message}");
        }
    }
}
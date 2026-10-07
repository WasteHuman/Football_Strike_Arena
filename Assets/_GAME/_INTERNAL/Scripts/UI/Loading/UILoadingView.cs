using UI.Other;
using UnityEngine;

namespace UI.Loading
{
    public class UILoadingView : MonoBehaviour
    {
        [SerializeField] private CustomProgressBar _progressBar;

        public void ResetProgress()
        {
            if (_progressBar != null)
                _progressBar.SetProgress(0f);
        }

        public void SetLoadingProgress(float progress) =>  _progressBar.SetProgress(progress);
    }
}
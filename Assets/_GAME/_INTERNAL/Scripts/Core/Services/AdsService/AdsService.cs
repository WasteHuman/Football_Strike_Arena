using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using YandexMobileAds;
using YandexMobileAds.Base;

namespace Core.Services.AdsService
{
    public class AdsService : MonoBehaviour
    {
        [Header("Service Settings")]
        [SerializeField] private bool _isEnabled = false;

        [Space(5), Header("Ad Placements Setup")]
        [SerializeField] private string _interstitialAdPlacement;
        [SerializeField] private string _rewardedAdPlacement;

        [Space(5), Header("Interstitial Ad Showrate Setup")]
        [SerializeField] private float _interstitialAdShowrate = 0.5f;

        private bool _isInitialized = false;

        private static AdsService _instance;
        private bool _isRewardedVideoLoading;

        private InterstitialAdLoader _interstitialAdLoader;
        private RewardedAdLoader _rewardedAdLoader;

        private Interstitial _interstitialAd;
        private RewardedAd _rewardedAd;

        private Action _onRewardCallback;

        public static AdsService Instance
        {
            get => _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (!_isEnabled)
                return;

            InitializeAds().Forget();
        }

        private void OnDestroy()
        {
            DestroyInterstitial();
            DestroyRewarded();
        }

        private async UniTask InitializeAds()
        {
            try
            {
                Debug.Log("[Ads] Initialization Yandex Ads...");

                _interstitialAdLoader = new();
                _rewardedAdLoader = new();

                bool currentConsent = PlayerPrefs.GetInt("gdpr_consent_granted", 1) == 1;
                long timestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;

                await AsyncLoadInterstitialAd();
                await AsyncLoadRewardedAd();

                _isInitialized = true;
                Debug.Log("[Ads] Yandex SDK successfully initialized.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Ads] SDK initialization failed: {ex.Message}");
            }
        }

        public void ShowInterstitialAd()
        {
            float randRoll = UnityEngine.Random.Range(0f, 1f);

            if(randRoll > _interstitialAdShowrate)
                _interstitialAd?.Show();
        }

        public void ShowRewardedAd(Action onRewardCallback)
        {
            if (!_isEnabled)
            {
                Debug.LogWarning($"[Ads] Cannot show video. SDK is not enabled.");
                return;
            }

            if (!_isInitialized)
            {
                Debug.LogWarning("[Ads] Cannot show video. SDK is not initialized.");
                return;
            }

            if (_isRewardedVideoLoading)
            {
                Debug.LogWarning("[Ads] Rewarded Video is already loading. Please wait.");
                return;
            }

            _onRewardCallback = onRewardCallback;

            if(!_isRewardedVideoLoading)
                _rewardedAd?.Show();
            else
                AsyncLoadRewardedAd().Forget();
        }

        public bool IsRewardedAdLoaded() => _rewardedAd != null;

        private async UniTask AsyncLoadRewardedAd()
        {
            SetRewardedLoadingState(true);

            string adUnitId = _rewardedAdPlacement;

            try
            {
                var loadedAd = await _rewardedAdLoader.LoadAd(new(adUnitId));
                HandleRewardedLoaded(loadedAd);
            }
            catch (AdLoadingException e)
            {
                HandleRewardedFailedToLoad(e);
            }
        }

        private async UniTask AsyncLoadInterstitialAd()
        {
            string adUnitId = _interstitialAdPlacement;

            try
            {
                var loadedAd = await _interstitialAdLoader.LoadAd(new(adUnitId));
                HandleInterstitialLoaded(loadedAd);
                
            }
            catch (AdLoadingException e)
            {
                HandleInterstitialFailedToLoad(e);
            }
        }

        private void SetRewardedLoadingState(bool isLoading) => _isRewardedVideoLoading = isLoading;

        private void DestroyRewarded()
        {
            _rewardedAd?.Destroy();
            _rewardedAd = null;
        }

        private void DestroyInterstitial()
        {
            _interstitialAd?.Destroy();
            _interstitialAd = null;
        }

        private void HandleRewardedLoaded(RewardedAd loadedAd)
        {
            SetRewardedLoadingState(false);
            _rewardedAd = loadedAd;

            _rewardedAd.OnRewarded += HandleRewarded;
            _rewardedAd.OnAdShown += HandleRewardedAdShown;
            _rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
            _rewardedAd.OnAdDismissed += HandleRewardedAdDismissed;

            if(_isRewardedVideoLoading)
                _rewardedAd.Show();
        }

        private void HandleInterstitialLoaded(Interstitial loadedAd)
        {
            _interstitialAd = loadedAd;

            _interstitialAd.OnAdDismissed += HandleInterstitialAdDismissed;
            _interstitialAd.OnAdFailedToShow += HandleInterstitialAdFailedToShow;
            _interstitialAd.OnAdShown += HandleInterstitialAdShown;
        }

        private void HandleInterstitialAdShown(object sender, EventArgs e)
        {
            DestroyInterstitial();
            AsyncLoadInterstitialAd().Forget();
        }

        private void HandleInterstitialAdFailedToShow(object sender, AdFailureEventArgs e)
        {
            DestroyInterstitial();
            AsyncLoadInterstitialAd().Forget();
        }

        private void HandleInterstitialAdDismissed(object sender, EventArgs e)
        {
            DestroyInterstitial();
            AsyncLoadInterstitialAd().Forget();
        }

        private void HandleInterstitialFailedToLoad(AdLoadingException e)
        {
            DestroyInterstitial();
            AsyncLoadInterstitialAd().Forget();
        }

        private void HandleRewardedFailedToLoad(AdLoadingException e)
        {
            DestroyRewarded();
            AsyncLoadRewardedAd().Forget();

            _onRewardCallback = null;
        }

        private void HandleRewarded(object sender, EventArgs e)
        {
            Debug.Log("[Ads] The user has finished viewing the ad");

            _onRewardCallback?.Invoke();
            _onRewardCallback = null;
        }

        private void HandleRewardedAdFailedToShow(object sender, AdFailureEventArgs e)
        {
            SetRewardedLoadingState(false);

            DestroyRewarded();
            AsyncLoadRewardedAd().Forget();
        }

        private void HandleRewardedAdShown(object sender, EventArgs e)
        {
            Debug.Log($"YandexAds: ad shown {e}");
        }

        private void HandleRewardedAdDismissed(object sender, EventArgs e)
        {
            Debug.Log("[Ads] Rewarded ad is closed");
            SetRewardedLoadingState(false);

            if (_onRewardCallback != null)
            {
                Debug.Log("[Ads] Reward conditions met. Triggering callback.");

                _onRewardCallback?.Invoke();
                _onRewardCallback = null;
            }

            if (_rewardedAd != null)
            {
                _rewardedAd.OnRewarded -= HandleRewarded;
                _rewardedAd.OnAdShown -= HandleRewardedAdShown;
                _rewardedAd.OnAdFailedToShow -= HandleRewardedAdFailedToShow;
                _rewardedAd.OnAdDismissed -= HandleRewardedAdDismissed;
                _rewardedAd = null;
            }
        }

        
    }
}
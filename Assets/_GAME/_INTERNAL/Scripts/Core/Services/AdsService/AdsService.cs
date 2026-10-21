using System;
using System.Collections.Generic;
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

        [Space(5), Header("Interstitial Ad Showrate Setup")]
        [SerializeField] private float _interstitialAdShowrate = 0.5f;

        private Dictionary<AdType, string> _placementsDict;

        private bool _isInitialized = false;

        private static AdsService _instance;
        private bool _isRewardedVideoLoading;
        private bool _isReadyToShow;

        private InterstitialAdLoader _interstitialAdLoader;
        private RewardedAdLoader _rewardedAdLoader;

        private Interstitial _interstitialAd;
        private RewardedAd _rewardedAd;

        private Action _onRewardCallback;
        private Action _onInterstitialCallback;

        public static AdsService Instance => _instance;
        public bool IsReadyToShow => _isReadyToShow;

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

                _placementsDict = new()
                {
                    {AdType.Lose_Interstitial, "R-M-20200744-1"},
                    {AdType.Claim_Wheel_Of_Luck_Pending_Reward_Rewarded, "R-M-20200744-2"},
                    {AdType.Double_Score_Per_Session_Rewarded, "R-M-20200744-3"},
                    {AdType.Double_Coins_Per_Sessoin_Rewarded, "R-M-20200744-4"}
                };

                _interstitialAdLoader = new();
                _rewardedAdLoader = new();

                _isReadyToShow = false;

                await AsyncLoadInterstitialAd();
                await AsyncLoadRewardedAd();

                _isInitialized = true;
                Debug.Log("[Ads] Yandex SDK successfully initialized.");
                Debug.Log($"[Ads] Rewarded ad loaded: {_rewardedAd != null}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Ads] SDK initialization failed: {ex.Message}");
            }
        }

        public void ShowInterstitialAd(Action onComplete = null, bool useRandomFactor = false)
        {
            float randRoll = UnityEngine.Random.Range(0f, 1f);

            _onInterstitialCallback = onComplete;

            if(useRandomFactor && randRoll > _interstitialAdShowrate)
            {
                _interstitialAd?.Show();
                return;
            }

            _interstitialAd?.Show();
        }

        public void ShowRewardedAd(AdType type, Action onRewardCallback)
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
                AsyncLoadRewardedAd(type).Forget();
        }

        public bool IsRewardedAdLoaded() => _rewardedAd != null;

        private async UniTask AsyncLoadRewardedAd(AdType type = AdType.Claim_Wheel_Of_Luck_Pending_Reward_Rewarded)
        {
            SetRewardedLoadingState(true);
            string adUnitId;

            if(_placementsDict.TryGetValue(type, out string placementId))
                adUnitId = placementId;
            else
            {
                Debug.LogWarning($"[Ads] Placement by type {type} not found!");
                return;
            }

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
            string adUnitId;

            if(_placementsDict.TryGetValue(AdType.Lose_Interstitial, out string placementId))
                adUnitId = placementId;
            else
            {
                Debug.LogWarning($"[Ads] Placement by type {AdType.Lose_Interstitial} not found!");
                return;
            }

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
            _isReadyToShow = true;

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
            _interstitialAd.OnAdImpression += HandleInterstitialAdImpression;
        }

        private void HandleInterstitialAdImpression(object sender, ImpressionData e)
        {
            DestroyInterstitial();
            AsyncLoadInterstitialAd().Forget();

            _onInterstitialCallback?.Invoke();
            _onInterstitialCallback = null;
        }

        private void HandleInterstitialAdShown(object sender, EventArgs e)
        {
            DestroyInterstitial();
            AsyncLoadInterstitialAd().Forget();

            _onInterstitialCallback?.Invoke();
            _onInterstitialCallback = null;
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

            _isReadyToShow = false;
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
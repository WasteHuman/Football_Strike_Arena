using System;
using System.Collections.Generic;
using System.Linq;
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

        private Dictionary<AdType, AdEntity> _placementsDict;

        private Dictionary<AdType, RewardedAd> _rewardedAdsDict;
        private Dictionary<AdType, Interstitial> _interstitialAdsDict;
        private Dictionary<Interstitial, AdType> _interstitialTypeByInstanceDict;
        private Dictionary<RewardedAd, AdType> _rewardedAdTypeByInstanceDict;

        private HashSet<AdType> _pendingRewardedLoads;

        private bool _isInitialized = false;

        private static AdsService _instance;
        private bool _isRewardedShowRequested;
        private bool _isReadyToShow;

        private InterstitialAdLoader _interstitialAdLoader;
        private RewardedAdLoader _rewardedAdLoader;

        private Action _onRewardCallback;
        private Action _onRewardDismissedCallback;
        private Action _onInterstitialCallback;

        public static AdsService Instance => _instance;
        public bool IsReadyToShow => _isReadyToShow;

        public event Action<AdType> OnRewardedAvailabilityChanged;

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

                _interstitialTypeByInstanceDict = new();
                _rewardedAdTypeByInstanceDict = new();
                _pendingRewardedLoads = new();

                _placementsDict = new()
                {
                    {AdType.Lose_Interstitial, new(AdEntityType.Interstitial, AdType.Lose_Interstitial, "R-M-20200744-1")},
                    {AdType.Claim_Wheel_Of_Luck_Pending_Reward_Rewarded, new(AdEntityType.Rewarded, AdType.Claim_Wheel_Of_Luck_Pending_Reward_Rewarded, "R-M-20200744-2")},
                    {AdType.Double_Score_Per_Session_Rewarded, new(AdEntityType.Rewarded, AdType.Double_Score_Per_Session_Rewarded, "R-M-20200744-3")},
                    {AdType.Double_Coins_Per_Sessoin_Rewarded, new(AdEntityType.Rewarded, AdType.Double_Coins_Per_Sessoin_Rewarded, "R-M-20200744-4")},
                    {AdType.Transition_Interstitial, new(AdEntityType.Interstitial, AdType.Transition_Interstitial, "R-M-20200744-5")},
                };

                _rewardedAdsDict = new();
                _interstitialAdsDict = new();

                _interstitialAdLoader = new();
                _rewardedAdLoader = new();

                _isReadyToShow = false;

                await AsyncLoadInterstitialAds();
                await AsyncLoadRewardedAds();

                _isInitialized = true;
                Debug.Log("[Ads] Yandex SDK successfully initialized.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Ads] SDK initialization failed: {ex.Message}");
            }
        }

        public void ShowInterstitialAd(AdType type, Action onComplete = null, bool useRandomFactor = false)
        {
            float randRoll = UnityEngine.Random.Range(0f, 1f);

            _onInterstitialCallback = onComplete;

            if(_interstitialAdsDict.TryGetValue(type, out Interstitial interstitialAd))
            {
                if(useRandomFactor && randRoll > _interstitialAdShowrate)
                {
                    interstitialAd?.Show();
                    return;
                }

                interstitialAd?.Show();
            }
            else
                AsyncLoadInterstitialAd(type).Forget();
        }

        public void ShowRewardedAd(AdType type, Action onRewardCallback, Action onDismissedCallback = null)
        {
            if (!_isEnabled)
            {
                Debug.LogWarning($"[Ads] Cannot show video. SDK is not enabled.");
                onDismissedCallback?.Invoke();
                return;
            }

            if (!_isInitialized)
            {
                Debug.LogWarning("[Ads] Cannot show video. SDK is not initialized.");
                onDismissedCallback?.Invoke();
                return;
            }

            if (_isRewardedShowRequested)
            {
                Debug.LogWarning("[Ads] Rewarded Video is already loading. Please wait.");
                return;
            }

            _onRewardCallback = onRewardCallback;
            _onRewardDismissedCallback = onDismissedCallback;

            if(_rewardedAdsDict.TryGetValue(type, out RewardedAd rewardedAd))
            {
                _isRewardedShowRequested = false;
                rewardedAd?.Show();
            }
            else
            {
                _isRewardedShowRequested = true;
                AsyncLoadRewardedAd(type).Forget();
            }
        }

        public bool IsRewardedAdLoaded(AdType type)
        {
            if(_rewardedAdsDict == null)
                return false;

            if(_rewardedAdsDict.TryGetValue(type, out RewardedAd rewardedAd))
                return rewardedAd != null;

            return false;
        }

        private async UniTask AsyncLoadInterstitialAds()
        {
            var placements = _placementsDict
                .Where(x => x.Value.PlacementType == AdEntityType.Interstitial)
                .ToArray();

            foreach(var placement in placements)
                await AsyncLoadInterstitialAd(placement.Key);
        }

        private async UniTask AsyncLoadRewardedAds()
        {
            var placements = _placementsDict
                .Where(x => x.Value.PlacementType == AdEntityType.Rewarded)
                .ToArray();

            foreach(var placement in placements)
                await AsyncLoadRewardedAd(placement.Key);
        }

        private async UniTask AsyncLoadRewardedAd(AdType type)
        {
            if(_rewardedAdsDict.ContainsKey(type))
                return;

            if (_pendingRewardedLoads.Contains(type))
                return;
            
            if (!_placementsDict.TryGetValue(type, out AdEntity placement))
            {
                Debug.LogWarning($"[Ads] Placement by type {type} not found!");
                return;
            }

            if (placement.PlacementType != AdEntityType.Rewarded)
                return;

            _pendingRewardedLoads.Add(type);

            try
            {
                var loadedAd = await _rewardedAdLoader.LoadAd(new(placement.PlacementId));

                HandleRewardedLoaded(type, loadedAd);
            }
            catch (AdLoadingException e)
            {
                HandleRewardedFailedToLoad(type, e);
            }
            finally
            {
                _pendingRewardedLoads.Remove(type);
            }
        }

        private async UniTask AsyncLoadInterstitialAd(AdType type)
        {
            if (_interstitialAdsDict.ContainsKey(type))
                return;

            if (!_placementsDict.TryGetValue(type, out AdEntity placement))
            {
                Debug.LogWarning($"[Ads] Placement by type {type} not found!");
                return;
            }

            if (placement.PlacementType != AdEntityType.Interstitial)
                return;

            try
            {
                var loadedAd = await _interstitialAdLoader.LoadAd(new(placement.PlacementId));

                HandleInterstitialLoaded(type, loadedAd);
            }
            catch (AdLoadingException e)
            {
                HandleInterstitialFailedToLoad(type, e);
            }
        }

        private void DestroyRewarded(AdType type = AdType.None)
        {
            if (_rewardedAdsDict == null)
                return;

            if(type == AdType.None)
            {
                foreach (var kpv in _rewardedAdsDict.Keys)
                    _rewardedAdsDict[kpv]?.Destroy();

                _rewardedAdsDict.Clear();
                return;
            }

            if(!_rewardedAdsDict.TryGetValue(type, out RewardedAd rewardedAd))
                return;

            _rewardedAdTypeByInstanceDict?.Remove(rewardedAd);

            rewardedAd?.Destroy();
            _rewardedAdsDict.Remove(type);
        }

        private void DestroyInterstitial(AdType type = AdType.None)
        {
            if(type == AdType.None)
            {
                foreach (var kpv in _interstitialAdsDict.Keys)
                    _interstitialAdsDict[kpv]?.Destroy();

                _interstitialAdsDict.Clear();
                return;
            }

            if(!_interstitialAdsDict.TryGetValue(type, out Interstitial interstitialAd))    
                return;

            _interstitialTypeByInstanceDict.Remove(interstitialAd);

            interstitialAd?.Destroy();
            _interstitialAdsDict.Remove(type);
        }

        private void InvokeRewardDismissedCallback()
        {
            var callback = _onRewardDismissedCallback;
            _onRewardDismissedCallback = null;
            callback?.Invoke();
        }

        private void HandleRewardedLoaded(AdType type, RewardedAd loadedAd)
        {
            _rewardedAdsDict[type] = loadedAd;
            _rewardedAdTypeByInstanceDict[loadedAd] = type;

            var rewardedAd = _rewardedAdsDict[type];

            _isReadyToShow = true;

            rewardedAd.OnRewarded += HandleRewarded;
            rewardedAd.OnAdShown += HandleRewardedAdShown;
            rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
            rewardedAd.OnAdDismissed += HandleRewardedAdDismissed;

            OnRewardedAvailabilityChanged?.Invoke(type);

            if (_isRewardedShowRequested)
            {
                _isRewardedShowRequested = false;
                rewardedAd.Show();
            }
        }

        private void HandleInterstitialLoaded(AdType type, Interstitial loadedAd)
        {
            _interstitialAdsDict[type] = loadedAd;
            _interstitialTypeByInstanceDict[loadedAd] = type;

            var interstitialAd = _interstitialAdsDict[type];

            interstitialAd.OnAdDismissed += HandleInterstitialAdDismissed;
            interstitialAd.OnAdFailedToShow += HandleInterstitialAdFailedToShow;
            interstitialAd.OnAdShown += HandleInterstitialAdShown;
            interstitialAd.OnAdImpression += HandleInterstitialAdImpression;
        }

        private void HandleInterstitialAdImpression(object sender, ImpressionData e)
        {
            if(sender is not Interstitial interstitial)
                return;

            if(!_interstitialTypeByInstanceDict.TryGetValue(interstitial, out AdType type))
                return;

            DestroyInterstitial(type);
            AsyncLoadInterstitialAd(type).Forget();

            _onInterstitialCallback?.Invoke();
            _onInterstitialCallback = null;
        }

        private void HandleInterstitialAdShown(object sender, EventArgs e)
        {
            if(sender is not Interstitial interstitial)
                return;

            if(!_interstitialTypeByInstanceDict.TryGetValue(interstitial, out AdType type))
                return;

            DestroyInterstitial(type);
            AsyncLoadInterstitialAd(type).Forget();

            _onInterstitialCallback?.Invoke();
            _onInterstitialCallback = null;
        }

        private void HandleInterstitialAdFailedToShow(object sender, AdFailureEventArgs e)
        {
            if(sender is not Interstitial interstitial)
                return;

            if(!_interstitialTypeByInstanceDict.TryGetValue(interstitial, out AdType type))
                return;

            DestroyInterstitial(type);
            AsyncLoadInterstitialAd(type).Forget();
        }

        private void HandleInterstitialAdDismissed(object sender, EventArgs e)
        {
            if(sender is not Interstitial interstitial)
                return;

            if(!_interstitialTypeByInstanceDict.TryGetValue(interstitial, out AdType type))
                return;

            DestroyInterstitial(type);
            AsyncLoadInterstitialAd(type).Forget();
        }

        private void HandleInterstitialFailedToLoad(AdType type, AdLoadingException e)
        {
            DestroyInterstitial(type);
            AsyncLoadInterstitialAd(type).Forget();
        }

        private void HandleRewardedFailedToLoad(AdType type, AdLoadingException e)
        {
            Debug.LogWarning($"[Ads] Rewarded ad failed to load ({type}): {e.Message}");

            DestroyRewarded(type);

            InvokeRewardDismissedCallback();
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
            Debug.LogWarning($"[Ads] Rewarded ad failed to show: {e.Message}");

            if(sender is not RewardedAd rewardedAd)
            {
                InvokeRewardDismissedCallback();
                return;
            }

            if(_rewardedAdTypeByInstanceDict.TryGetValue(rewardedAd, out AdType type))
            {
                DestroyRewarded(type);
                AsyncLoadRewardedAd(type).Forget();
            }

            InvokeRewardDismissedCallback();
        }

        private void HandleRewardedAdShown(object sender, EventArgs e)
        {
            Debug.Log($"YandexAds: ad shown {e}");
        }

        private void HandleRewardedAdDismissed(object sender, EventArgs e)
        {
            if(sender is not RewardedAd rewardedAd)
                return;

            Debug.Log("[Ads] Rewarded ad is closed");

            if(!_rewardedAdTypeByInstanceDict.TryGetValue(rewardedAd, out AdType type))
                return;

            DestroyRewarded(type);

            AsyncLoadRewardedAd(type).Forget();

            InvokeRewardDismissedCallback();
        }
    }
}
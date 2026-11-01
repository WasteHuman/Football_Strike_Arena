using Core.Common;
using Core.Services.AdsService;

namespace Core.Boot.SceneEntryPoints
{
    public class ShopSceneEntryPoint : BaseSceneEntryPoint
    {
        protected override void Start() => AdsService.Instance.ShowInterstitialAd(type: AdType.Transition_Interstitial, onComplete: () => base.Start(), useRandomFactor: true);
    }
}
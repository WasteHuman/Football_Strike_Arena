using Core;
using Core.Services;
using System;
using UI.Screens;
using UnityEngine;

namespace UI.Controllers
{
    public class MainSceneScreenController : MonoBehaviour
    {
        [SerializeField] private MainMenuScreenView _mainMenuScreenView;
        [SerializeField] private DailyFreeBonusScreen _dailyFreeBonusScreen;
        [SerializeField] private WelcomeScreenView _welcomeScreenView;

        private void Awake()
        {
            _welcomeScreenView.OnLetsPlayButtonClick += HandlePlayerReady;
            _dailyFreeBonusScreen.OnBonusClaimed += HandleBonusClaimed;
        }

        private void Start()
        {
            if (PlayerPrefs.GetInt(GameConstants.KEY_WELCOME_SCREEN_SHOWN) == 1)
                HandlePlayerReady();
            else
                _welcomeScreenView.Open();
        }

        private void OnDestroy()
        {
            _welcomeScreenView.OnLetsPlayButtonClick -= HandlePlayerReady;
            _dailyFreeBonusScreen.OnBonusClaimed -= HandleBonusClaimed;
        }

        private void HandleBonusClaimed()
        {
            GameServices.PlayerService.GetData().LastDailyBonusTime = DateTime.Now;
            string todayUtc = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            PlayerPrefs.SetString(GameConstants.KEY_LAST_FREE_DAILY_BONUS_CLAIM, todayUtc);

            _dailyFreeBonusScreen.Close();
            
            _mainMenuScreenView.Open();
        }

        private void HandlePlayerReady()
        {
            PlayerPrefs.SetInt(GameConstants.KEY_WELCOME_SCREEN_SHOWN, 1);

            _welcomeScreenView.Close();

            if (GameServices.EconomyService.RequestDailyFreeBonusAvailable())
            {
                _dailyFreeBonusScreen.Open();
                _mainMenuScreenView.Open();
            }
            else
            {
                _dailyFreeBonusScreen.Close();
                _mainMenuScreenView.Open();
            }
        }
    }
}
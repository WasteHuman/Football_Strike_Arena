namespace Core
{
    public static class GameConstants
    {
        #region Main Scene Names
        public const string MAIN_MENU = "Main_Menu";
        public const string GAME = "Game";
        public const string SETTINGS = "Settings";
        public const string SHOP_MENU = "Shop";
        public const string WHEEL_OF_LUCK = "Wheel_Of_Luck";
        #endregion

        #region Settings Prefs
        public const string KEY_NOTIFICATIONS = "Notifications";
        public const string KEY_VIBRATIONS = "Vibrations";
        #endregion

        #region Player Prefs
        public const string KEY_WELCOME_SCREEN_SHOWN = "Welcome_Screen_Shown";
        public const string KEY_PLAYER_DATA = "Player_Data_JSON";
        public const string KEY_LAST_FREE_DAILY_BONUS_CLAIM = "Last_Daily_Free_Bonus";
        public const string KEY_TUTORIAL_COMPLETED = "Tutorial_Completed";
        #endregion

        #region Economy & Player Data
        public const float INITIAL_COINS = 1000f;
        public const int INITIAL_PLAYER_DAMAGE = 2;
        public const string INITIAL_PLAYER_SKIN_ID = "Skin_0";
        public const string INITIAL_PLAYER_BALL_SKIN_ID = "Ball_0";
        public const float INITIAL_PLAYER_RELOAD = 0.15f;
        #endregion
    }
}
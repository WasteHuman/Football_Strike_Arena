namespace Core.Data
{
    public readonly struct GameResult
    {
        public readonly bool IsWin;
        public readonly int Score;
        public readonly float RewardCoins;

        public GameResult(bool isWin, float rewardCoins, int score = 0)
        {
            IsWin = isWin;
            RewardCoins = rewardCoins;
            Score = score;
        }
    }
}
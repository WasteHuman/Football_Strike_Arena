using Core.Data;
using Core.Gameplay;

namespace Core.Services.Player
{
    public class GameSessionService
    {
        private EconomyService _economyService;
        private PlayerService _playerService;

        public void Init(EconomyService economyService, PlayerService playerService)
        {
            _economyService = economyService;
            _playerService = playerService;
        }

        public void HandleEndedGame(GameResult sessionResult)
        {
            _playerService.ResetSessionScore();
            _playerService.ResetEarnedSessionCoins();
            _economyService.AddCoins(sessionResult.RewardCoins);
        }
    }
}
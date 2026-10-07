using System;
using Core.Data;
using Core.Gameplay;

namespace Core.Services.Player
{
    public class GameSessionService
    {
        private EconomyService _economyService;
        private PlayerService _playerService;

        public event Action<GameResult> OnGameEnded;

        public void Init(EconomyService economyService, PlayerService playerService)
        {
            _economyService = economyService;
            _playerService = playerService;
        }

        public void HandleDestroyedTarget(int earnedCoins) => _economyService.AddCoins(earnedCoins);

        public void HandleEndedGame(GameResult sessionResult)
        {
            _playerService.ResetSessionScore();
            _playerService.ResetEarnedSessionCoins();
            OnGameEnded?.Invoke(sessionResult);
        }
    }
}
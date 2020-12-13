using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateGameOver : IGameState
{
    public GameStateGameOver(GameSession gameSession) : base(gameSession)
    { }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.GAME_OVER;
    }
}

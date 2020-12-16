using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateGameStart : IGameState
{
    float delayBetweenCards = 0.5f;
    float elapsedTime;

    int cardCount;
    int totalCards;

    int cardsDrawn;

    public GameStateGameStart(GameSession gameSession) : base(gameSession)
    {
    }

    public override void OnEnter()
    {
        if (gameSession.isServer)
        {
            gameSession.SetActivePlayerIndex((Random.value < 0.5f) ? 0 : 1);
        }
        totalCards = GameConstants.STARTING_HAND_SIZE;
        elapsedTime = 0.0f;
        cardsDrawn = 0;
    }
    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            elapsedTime += frameDelta;
            while (elapsedTime > delayBetweenCards)
            {
                elapsedTime -= delayBetweenCards;

                if (cardCount < totalCards)
                {
                    cardCount++;
                    foreach (PlayerController player in gameSession.GetPlayerList())
                    {
                        gameSession.ServerPlayerDrawCard(player, player, CardGenerationFlags.NONE, true);
                    }
                }
            }
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (eventInfo is CardDrawnEvent cardEvent)
        {
            cardsDrawn++;
            if (cardsDrawn == (gameSession.GetMaxPlayers() * totalCards))
            {
                ChangeState(GameSession.GameState.TURN_START);
            }
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.GAME_START;
    }
}

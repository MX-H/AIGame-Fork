using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateTurnStart : IGameState
{
    PlayerController activePlayer;
    public GameStateTurnStart(GameSession session) : base(session)
    {

    }

    public override void OnEnter()
    {
        if (gameSession.isServer)
        {
            activePlayer = gameSession.GetActivePlayer();
            activePlayer.ServerStartTurn();

            gameSession.ServerPlayerDrawCard(activePlayer, activePlayer);
            gameSession.SetCombatDeclared(false);
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (eventInfo is CardDrawnEvent cardEvent)
        {
            if (cardEvent.playerId == activePlayer.netIdentity)
            {
                ChangeState(GameSession.GameState.WAIT_ACTIVE);
            }
        }
    }

    public override void OnExit()
    {
        if (gameSession.isServer)
        {
            gameSession.ResetPriorityPasses();

            TurnTimer turnTimer = GameUtils.GetTurnTimer();
            turnTimer.ResetTimer(true);
            turnTimer.StartTimer();
            turnTimer.StoreTimer();

            activePlayer = null;
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.TURN_START;
    }
}

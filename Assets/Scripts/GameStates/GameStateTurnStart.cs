using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateTurnStart : IGameState
{
    PlayerController activePlayer;
    bool cardDrawn;
    public GameStateTurnStart(GameSession session) : base(session)
    {

    }

    public override void OnEnter()
    {
        if (gameSession.isServer)
        {
            gameSession.SetWaitingPlayerIndex(gameSession.GetActivePlayerIndex());
            activePlayer = gameSession.GetActivePlayer();
            activePlayer.ServerStartTurn();

            cardDrawn = false;
            gameSession.SetCombatDeclared(false);
        }
    }

    public override void Update(float frameDelta)
    {
        base.Update(frameDelta);

        if (gameSession.isServer && !cardDrawn)
        {
            gameSession.ServerPlayerDrawCard(activePlayer, activePlayer);
            cardDrawn = true;
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (eventInfo is CardDrawnEvent cardEvent && cardDrawn)
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
            turnTimer.StoreTimer();

            activePlayer = null;
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.TURN_START;
    }
}

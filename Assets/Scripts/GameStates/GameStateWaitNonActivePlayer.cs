using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateWaitNonActivePlayer : IGameStateWaitPlayer
{
    public GameStateWaitNonActivePlayer(GameSession gameSession) : base(gameSession)
    {
    }

    public override void OnEnter()
    {
        if (gameSession.isServer)
        {
            gameSession.SetWaitingPlayerIndex(gameSession.GetNonActivePlayerIndex());
            GameUtils.GetTurnTimer().ResetTimer(false);
        }
    }

    protected override void GoToNextState()
    {
        gameSession.ServerPassPriority();

        if (gameSession.GetPriorityPasses() < gameSession.GetMaxPlayers())
        {
            ChangeState(GameSession.GameState.WAIT_ACTIVE);
        }
        else
        {
            if (!gameSession.IsStackEmpty())
            {
                ChangeState(GameSession.GameState.RESOLVING_EFFECTS);
            }
            else
            {
                ChangeState(GameSession.GameState.TURN_END);
            }
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (gameSession.isServer && eventInfo.playerId == gameSession.GetNonActivePlayer().netIdentity)
        {
            if (eventInfo is ConfirmationEvent confirmEvent)
            {
                HandleConfirmEvent(confirmEvent);
            }

            if (eventInfo is UseTrapEvent trapEvent)
            {
                HandleTrapEvent(trapEvent);
            }
        }
    }
    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.WAIT_NON_ACTIVE;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IGameStateWaitPlayer : IGameState
{
    public IGameStateWaitPlayer(GameSession gameSession) : base(gameSession)
    { }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            TurnTimer timer = GameUtils.GetTurnTimer();
            if (timer.IsTimeUp())
            {
                gameSession.GetWaitingOnPlayer().ServerForceConfirmation();
            }
        }
    }

    protected abstract void GoToNextState();

    protected void HandleConfirmEvent(ConfirmationEvent confirmEvent)
    {
        GoToNextState();
    }

    protected void HandleTrapEvent(UseTrapEvent trapEvent)
    {
        PlayerController player = trapEvent.playerId.GetComponent<PlayerController>();
        Card card = trapEvent.cardId.GetComponent<Card>();

        if (trapEvent.flattenedTargets == null)
        {
            if (player.CanUseTrap(card))
            {
                List<ITargettingDescription> targets = card.cardData.GetSelectableTargets(TriggerCondition.NONE);

                if (targets.Count > 0)
                {
                    if (card.HasValidTargets(targets))
                    {
                        gameSession.StartSelectingTargets(card, player, TriggerCondition.NONE);
                        player.ServerRemoveTrapFromArena(card);
                    }
                }
                else
                {
                    player.ServerRemoveTrapFromArena(card);
                    player.ServerActivateTrap(card);
                    gameSession.ResetPriorityPasses();
                }
            }
        }
        else
        {
            player.ServerActivateTrap(card, trapEvent.flattenedTargets, trapEvent.indexes);
            gameSession.ResetPriorityPasses();
        }
        gameSession.ServerUpdateGameState();
    }
}

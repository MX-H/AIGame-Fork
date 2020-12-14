using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameStateSelectTargets : IGameState
{
    private bool hasTimedOut;

    public GameStateSelectTargets(GameSession gameSession) : base(gameSession)
    { }

    public override void OnEnter()
    {
        hasTimedOut = false;

        if (gameSession.isServer)
        {
            if (gameSession.GetWaitingPlayerIndex() == gameSession.GetActivePlayerIndex())
            {
                GameUtils.GetTurnTimer().RestoreTimer();
            }
            else
            {
                GameUtils.GetTurnTimer().ResetTimer(false);
            }
        }
    }

    public override void OnExit()
    {
        if (gameSession.isServer && gameSession.GetWaitingPlayerIndex() == gameSession.GetActivePlayerIndex())
        {
            GameUtils.GetTurnTimer().StoreTimer();
        }

        if (gameSession.GetLocalPlayer() == gameSession.GetWaitingOnPlayer())
        {
            gameSession.GetLocalPlayer().StopSelectingTargets();
        }
    }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer && !hasTimedOut)
        {
            if (GameUtils.GetTurnTimer().IsTimeUp())
            {
                PlayerController waitingPlayer = gameSession.GetWaitingOnPlayer();
                switch (gameSession.GetPendingActionType())
                {
                    case GameSession.PendingType.PLAY_CARD:
                        CancelPlayCard();
                        break;
                    case GameSession.PendingType.USE_TRAP:
                        CancelPlayCard();
                        break;
                    case GameSession.PendingType.TRIGGER_EFFECT:
                        waitingPlayer.TargetSelectRandomTargets(waitingPlayer.connectionToClient);
                        break;
                }
                hasTimedOut = true;
            }
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (gameSession.isServer && eventInfo.playerId == gameSession.GetWaitingOnPlayer().netIdentity)
        {
            if (eventInfo is TargetSelectionEvent targetEvent)
            {
                HandleTargetEvent(targetEvent);
            }

            if (eventInfo is CancelPlayCardEvent)
            {
                CancelPlayCard();
            }
        }
    }

    private void HandleTargetEvent(TargetSelectionEvent targetEvent)
    {
        // Validate all targets
        Card card = gameSession.GetPendingCard(targetEvent.playerId.GetComponent<PlayerController>());

        bool isValid = false;
        if (card != null && targetEvent.playerId == gameSession.GetWaitingOnPlayer().netIdentity)
        {
            PlayerController player = gameSession.GetWaitingOnPlayer();
            TriggerCondition triggerCondition = gameSession.GetPendingTriggerCondition();
            NetworkIdentity[][] targets = targetEvent.ReconstructTargets();

            List<ITargettingDescription> selectableTargetDescriptions = null;
            switch (card.cardData.GetCardType())
            {
                case CardType.CREATURE:
                    selectableTargetDescriptions = card.cardData.GetSelectableTargets(triggerCondition);
                    break;
                case CardType.SPELL:
                case CardType.TRAP:
                    selectableTargetDescriptions = card.cardData.GetSelectableTargets(TriggerCondition.NONE);
                    break;
            }

            if (selectableTargetDescriptions != null && selectableTargetDescriptions.Count == targets.Length)
            {
                isValid = true;
                for (int i = 0; i < selectableTargetDescriptions.Count; i++)
                {
                    ITargettingDescription desc = selectableTargetDescriptions[i];
                    if (desc.targettingType == TargettingType.EXCEPT)
                    {
                        ExceptTargetDescription exceptDesc = (ExceptTargetDescription)desc;
                        desc = exceptDesc.targetDescription;
                    }

                    switch (desc.targettingType)
                    {
                        case TargettingType.TARGET:
                            {
                                TargetXDescription targetDesc = (TargetXDescription)desc;
                                if (targetDesc.amount == targets[i].Length)
                                {
                                    TargettingQuery query = new TargettingQuery(targetDesc, player);
                                    for (int j = 0; j < targets[i].Length; j++)
                                    {
                                        Targettable targettable = targets[i][j].GetComponent<Targettable>();
                                        if (!targettable.IsTargettable(query))
                                        {
                                            isValid = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    isValid = false;
                                }
                                break;
                            }
                        case TargettingType.UP_TO_TARGET:
                            {
                                UpToXTargetDescription targetDesc = (UpToXTargetDescription)desc;
                                if (targetDesc.amount >= targets[i].Length)
                                {
                                    TargettingQuery query = new TargettingQuery(targetDesc, player);
                                    for (int j = 0; j < targets[i].Length; j++)
                                    {
                                        Targettable targettable = targets[i][j].GetComponent<Targettable>();
                                        if (!targettable.IsTargettable(query))
                                        {
                                            isValid = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    isValid = false;
                                }
                                break;
                            }
                    }

                    if (!isValid)
                    {
                        break;
                    }
                }
            }

            if (isValid)
            {
                switch (gameSession.GetPendingActionType())
                {
                    case GameSession.PendingType.PLAY_CARD:
                        PlayCardEvent playCardEvent = new PlayCardEvent(player, card, targetEvent.flattenedTargets, targetEvent.indexes);
                        gameSession.HandleEvent(playCardEvent);
                        gameSession.ServerPopState();
                        break;
                    case GameSession.PendingType.TRIGGER_EFFECT:
                        gameSession.ServerAddEffectToStack(card, triggerCondition, targetEvent.flattenedTargets, targetEvent.indexes);
                        gameSession.ServerPopState();
                        break;
                    case GameSession.PendingType.USE_TRAP:
                        UseTrapEvent trapEvent  = new UseTrapEvent(player, card, targetEvent.flattenedTargets, targetEvent.indexes);
                        gameSession.HandleEvent(trapEvent);
                        gameSession.ServerPopState();
                        break;
                }
            }
            else
            {
                CancelPlayCard();
            }
        }
    }

    private void CancelPlayCard()
    {
        PlayerController player = gameSession.GetWaitingOnPlayer();
        Card card = gameSession.GetPendingCard(player);

        switch (gameSession.GetPendingActionType())
        {
            case GameSession.PendingType.PLAY_CARD:
                player.ServerAddExistingCardToHand(card);
                player.TargetCancelPlayCard(player.connectionToClient);

                gameSession.ServerPopState();

                break;
            case GameSession.PendingType.USE_TRAP:
                player.ServerAddTrapBackToArena(card, gameSession.GetPendingTrapPosition());
                player.TargetCancelPlayCard(player.connectionToClient);

                gameSession.ServerPopState();

                break;
            default:
                // We can't do anything about triggered effects, these are mandatory so do nothing
                break;
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.SELECTING_TARGETS;
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameStateWaitActivePlayer : IGameStateWaitPlayer
{
    public GameStateWaitActivePlayer(GameSession session) : base(session)
    {
    }

    public override void OnEnter()
    {
        if (gameSession.isServer)
        {
            GameUtils.GetTurnTimer().RestoreTimer();
            gameSession.SetWaitingPlayerIndex(gameSession.GetActivePlayerIndex());
        }
    }

    public override void OnResume()
    {
        if (gameSession.isServer)
        {
            GameUtils.GetTurnTimer().RestoreTimer();
            gameSession.SetWaitingPlayerIndex(gameSession.GetActivePlayerIndex());
        }
    }

    public override void OnSuspend()
    {
        if (gameSession.isServer)
        {
            GameUtils.GetTurnTimer().StoreTimer();
        }
    }

    protected override void GoToNextState()
    {
        gameSession.ServerPassPriority();

        if (gameSession.GetPriorityPasses() < gameSession.GetMaxPlayers())
        {
            ChangeState(GameSession.GameState.WAIT_NON_ACTIVE);
        }
        else
        {
            if (!gameSession.IsStackEmpty())
            {
                ChangeState(GameSession.GameState.RESOLVING_EFFECTS);
            }
            else if (gameSession.GetActivePlayer().IsInCombat())
            {
                // Return to combat
                ExitState();
            }
            else
            {
                ChangeState(GameSession.GameState.TURN_END);
            }
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (gameSession.isServer && eventInfo.playerId == gameSession.GetActivePlayer().netIdentity)
        {
            PlayerController player = gameSession.GetActivePlayer();

            if (eventInfo is ConfirmationEvent confirmEvent)
            {
                HandleConfirmEvent(confirmEvent);
            }

            if (eventInfo is CreatureMoveToCombatEvent combatEvent)
            {
                Creature creature = combatEvent.creatureId.GetComponent<Creature>();
                if (!gameSession.WasCombatDeclared() && !creature.GetCreatureState().IsSummoningSick())
                {
                    gameSession.GetActivePlayer().ServerMoveToCombat(combatEvent.creatureId, combatEvent.arenaPosition, true);
                    ChangeState(GameSession.GameState.DECLARE_ATTACKS);
                }
            }

            if (eventInfo is PlayCardEvent playCardEvent)
            {
                Card card = playCardEvent.cardId.GetComponent<Card>();

                switch (card.cardData.GetCardType())
                {
                    case CardType.CREATURE:
                        Creature pendingCreature = gameSession.GetPendingCreature(player);
                        bool replacingCreatureSelected = pendingCreature != null;

                        if (playCardEvent.flattenedTargets == null)
                        {
                            if (!replacingCreatureSelected)
                            {
                                if (player.CanPlayCard(card))
                                {
                                    player.ServerRemoveCardFromHand(card);

                                    if (player.arena.IsFull() && !replacingCreatureSelected)
                                    {
                                        gameSession.SelectReplaceCreature(card, player);
                                    }
                                    else
                                    {
                                        List<ITargettingDescription> targets = card.cardData.GetSelectableTargets(TriggerCondition.ON_SELF_ENTER);
                                        if (targets.Count > 0 && card.HasValidTargets(targets))
                                        {
                                            gameSession.StartSelectingTargets(card, card, player, TriggerCondition.ON_SELF_ENTER);
                                        }
                                        else
                                        {
                                            player.ServerPayCost(card);
                                            Creature creature = gameSession.ServerCreateCreature(player, card);
                                            player.ServerPlayCreature(creature, card);
                                            gameSession.ResetPriorityPasses();
                                            gameSession.ServerUpdateGameState();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Don't check CanPlayCard, we already did and the card has already been removed from hand so it would fail

                                List<ITargettingDescription> targets = card.cardData.GetSelectableTargets(TriggerCondition.ON_SELF_ENTER);
                                if (targets.Count > 0 && card.HasValidTargets(targets))
                                {
                                    gameSession.StartSelectingTargets(card, card, player, TriggerCondition.ON_SELF_ENTER, true);
                                }
                                else
                                {
                                    // Destroy creature to be replaced now, note that we do not trigger on die effects when replaced
                                    player.ServerDestroyCreature(pendingCreature);
                                    gameSession.ResetPendingCreature();

                                    player.ServerPayCost(card);
                                    Creature creature = gameSession.ServerCreateCreature(player, card);
                                    player.ServerPlayCreature(creature, card);
                                    gameSession.ResetPriorityPasses();
                                    gameSession.ServerUpdateGameState();
                                }
                            }
                        }
                        else
                        {
                            if (replacingCreatureSelected)
                            {
                                // Destroy creature to be replaced now, note that we do not trigger on die effects when replaced
                                player.ServerDestroyCreature(pendingCreature);
                                gameSession.ResetPendingCreature();
                            }
                            player.ServerPayCost(card);
                            Creature creature = gameSession.ServerCreateCreature(player, card);
                            player.ServerPlayCreature(creature, card, playCardEvent.flattenedTargets, playCardEvent.indexes);
                            gameSession.ResetPriorityPasses();
                            gameSession.ServerUpdateGameState();
                        }

                        break;
                    case CardType.SPELL:
                        if (playCardEvent.flattenedTargets == null)
                        {
                            if (player.CanPlayCard(card))
                            {
                                List<ITargettingDescription> targets = card.cardData.GetSelectableTargets(TriggerCondition.ON_SELF_ENTER);
                                if (targets.Count > 0)
                                {
                                    if (card.HasValidTargets(targets))
                                    {
                                        player.ServerRemoveCardFromHand(card);
                                        gameSession.StartSelectingTargets(card, card, player, TriggerCondition.NONE);
                                    }
                                }
                                else
                                {
                                    player.ServerRemoveCardFromHand(card);
                                    player.ServerPayCost(card);
                                    player.ServerPlaySpell(card);
                                    gameSession.ResetPriorityPasses();
                                    gameSession.ServerUpdateGameState();
                                }
                            }
                        }
                        else
                        {
                            player.ServerPayCost(card);
                            player.ServerPlaySpell(card, playCardEvent.flattenedTargets, playCardEvent.indexes);
                            gameSession.ResetPriorityPasses();
                            gameSession.ServerUpdateGameState();
                        }
                        break;
                    case CardType.TRAP:
                        if (player.CanPlayCard(card))
                        {
                            player.ServerRemoveCardFromHand(card);
                            player.ServerPayCost(card);
                            player.ServerPlayTrap(card);
                            gameSession.ResetPriorityPasses();
                            gameSession.ServerUpdateGameState();
                        }
                        break;
                }
            }

            if (eventInfo is UseTrapEvent trapEvent)
            {
                HandleTrapEvent(trapEvent);
            }
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.WAIT_ACTIVE;
    }
}

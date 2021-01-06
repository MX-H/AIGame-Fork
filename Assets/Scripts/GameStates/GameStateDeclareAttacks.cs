using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateDeclareAttacks : IGameState
{
    public GameStateDeclareAttacks(GameSession gameSession) : base(gameSession)
    { }

    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
        if (gameSession.isServer)
        {
            GameUtils.GetTurnTimer().StoreTimer();
        }
    }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            PlayerController activePlayer = gameSession.GetActivePlayer();
            if (activePlayer.arena.InCombatCount() == 0)
            {
                activePlayer.ServerLeaveCombat();
                ChangeState(GameSession.GameState.WAIT_ACTIVE);
            }
            else if (GameUtils.GetTurnTimer().IsTimeUp())
            {
                activePlayer.ServerForceConfirmation();
            }
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (gameSession.isServer && eventInfo.playerId == gameSession.GetActivePlayer().netIdentity)
        {
            if (eventInfo is ConfirmationEvent)
            {
                gameSession.SetCombatDeclared(true);
                ChangeState(GameSession.GameState.DECLARE_BLOCKS);
            }

            if (eventInfo is CreatureMoveToCombatEvent combatAddEvent)
            {
                Creature creature = combatAddEvent.creatureId.GetComponent<Creature>();
                if (!creature.GetCreatureState().IsSummoningSick())
                {
                    gameSession.GetActivePlayer().ServerMoveToCombat(combatAddEvent.creatureId, combatAddEvent.arenaPosition, false);
                }
            }

            if (eventInfo is CreatureRemoveFromCombatEvent combatRemoveEvent)
            {
                gameSession.GetActivePlayer().ServerRemoveFromCombat(combatRemoveEvent.creatureId);
            }
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.DECLARE_ATTACKS;
    }
}

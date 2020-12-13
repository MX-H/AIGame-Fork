using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameStateDeclareBlocks : IGameState
{
    public GameStateDeclareBlocks(GameSession gameSession) : base(gameSession)
    { }

    public override void OnEnter()
    {
        if (gameSession.isServer)
        {
            List<Creature> creatures = gameSession.GetActivePlayer().arena.GetCombatCreatures();
            NetworkIdentity[] attackerIds = new NetworkIdentity[creatures.Count];
            for (int i = 0; i < attackerIds.Length; i++)
            {
                attackerIds[i] = creatures[i].GetComponent<NetworkIdentity>();
            }

            gameSession.SetWaitingPlayerIndex(gameSession.GetNonActivePlayerIndex());
            gameSession.GetNonActivePlayer().ServerReceiveAttackers(attackerIds);

            GameUtils.GetTurnTimer().ResetTimer(false);
        }
    }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            if (GameUtils.GetTurnTimer().IsTimeUp())
            {
                gameSession.GetNonActivePlayer().ServerForceConfirmation();
            }
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (gameSession.isServer && eventInfo.playerId == gameSession.GetNonActivePlayer().netIdentity)
        {
            PlayerController nonActivePlayer = gameSession.GetNonActivePlayer();

            if (eventInfo is ConfirmationEvent)
            {
                ChangeState(GameSession.GameState.RESOLVING_COMBAT);
            }

            if (eventInfo is CreatureMoveToCombatEvent combatAddEvent)
            {
                Creature creature = combatAddEvent.creatureId.GetComponent<Creature>();
                if (!creature.creatureState.IsSummoningSick() && nonActivePlayer.arena.IsValidBlock(creature, combatAddEvent.arenaPosition))
                {
                    nonActivePlayer.ServerMoveToCombat(combatAddEvent.creatureId, combatAddEvent.arenaPosition, false);
                    ChangeState(GameSession.GameState.DECLARE_ATTACKS);
                }
            }

            if (eventInfo is CreatureRemoveFromCombatEvent combatRemoveEvent)
            {
                nonActivePlayer.ServerRemoveFromCombat(combatRemoveEvent.creatureId);
            }
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.DECLARE_BLOCKS;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateTriggerEffects : IGameState
{
    private Queue<(Creature, Creature, TriggerCondition)> pendingTriggers;
    public GameStateTriggerEffects(GameSession gameSession) : base(gameSession)
    {
    }

    public override void OnEnter()
    {
        if (gameSession.isServer)
        {
            pendingTriggers = gameSession.GetPendingTriggers();
        }
    }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            if (gameSession.IsStackFull())
            {
                // TODO: Burn through the remaining effects, show some visual indication
                // for now we'll just empty the remaining effects
                pendingTriggers.Clear();
            }
            else if (pendingTriggers.Count > 0)
            {
                (Creature creature, Creature source, TriggerCondition trigger) = pendingTriggers.Dequeue();

                List<ITargettingDescription> targets = creature.card.cardData.GetSelectableTargets(trigger);

                if (targets.Count > 0)
                {
                    if (creature.card.HasValidTargets(targets))
                    {
                        gameSession.StartSelectingTargets(creature, creature.card, creature.controller, trigger);
                    }
                }
                else
                {
                    gameSession.ServerAddEffectToStack(creature, creature.card, trigger);
                }
            }

            if (pendingTriggers.Count == 0)
            {
                gameSession.ResetPriorityPasses();
                ExitState();
            }
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.TRIGGERING_EFFECTS;
    }
}

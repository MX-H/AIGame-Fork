using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateResolveEffects : IGameState
{
    Queue<EffectResolutionTask> effectTasks;
    bool receivedAcknowledgement;
    bool stateUpdated;
    System.Type acknowledgementType;
    public GameStateResolveEffects(GameSession gameSession) : base(gameSession)
    { }

    public override void OnEnter()
    {
        receivedAcknowledgement = true;
        stateUpdated = true;
    }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            if (receivedAcknowledgement)
            {
                if (effectTasks == null || effectTasks.Count == 0)
                {
                    if (!stateUpdated)
                    {
                        // This might trigger a state push of trigger effects
                        // Don't make an exit request on the same frame so only do this for the whole frame
                        gameSession.ServerUpdateGameState();
                        stateUpdated = true;
                    }
                    else
                    {
                        stateUpdated = false;
                        if (gameSession.IsStackEmpty())
                        {
                            if (gameSession.GetLocalPlayer().IsInCombat())
                            {
                                ExitState();
                            }
                            else
                            {
                                ChangeState(GameSession.GameState.WAIT_ACTIVE);
                            }
                        }
                        else
                        {
                            Effect effect = gameSession.ServerPopStack();
                            effectTasks = effect.GetEffectTasks();
                        }
                    }
                }
                else
                {
                    EffectResolutionTask task = effectTasks.Dequeue();
                    task.effect.ApplyToTarget(task.target, task.player, task.source);
                    acknowledgementType = task.effect.GetAcknowlegementType();
                    if (acknowledgementType != null)
                    {
                        receivedAcknowledgement = false;
                    }
                }
            }
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (gameSession.isServer)
        {
            if (eventInfo.GetType() == acknowledgementType)
            {
                receivedAcknowledgement = true;
            }
        }
    }

    public override void OnExit()
    {
        if (gameSession.isServer)
        {
            gameSession.ResetPriorityPasses();
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.RESOLVING_EFFECTS;
    }
}

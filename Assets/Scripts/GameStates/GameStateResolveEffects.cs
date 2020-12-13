using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateResolveEffects : IGameState
{
    public GameStateResolveEffects(GameSession gameSession) : base(gameSession)
    { }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            gameSession.ServerResolveStack();

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
            gameSession.ServerUpdateGameState();
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

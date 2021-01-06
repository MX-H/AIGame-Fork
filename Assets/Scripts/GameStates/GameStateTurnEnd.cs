using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateTurnEnd : IGameState
{
    public GameStateTurnEnd(GameSession gameSession) : base(gameSession)
    {
    }

    public override void OnEnter()
    {
        foreach (PlayerController player in gameSession.GetPlayerList())
        {
            foreach (Creature creature in player.arena.GetAllCreatures())
            {
                creature.GetCreatureState().RemoveEndOfTurnModifiers();
            }

            foreach (Targettable target in player.hand.GetTargettables())
            {
                Card card = target as Card;
                card.cardData.RemoveEndOfTurnModifiers();
            }
        }
    }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            ChangeState(GameSession.GameState.TURN_START);
        }
    }

    public override void OnExit()
    {
        if (gameSession.isServer)
        {
            gameSession.SetActivePlayerIndex(gameSession.GetNonActivePlayerIndex());
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.TURN_END;
    }
}

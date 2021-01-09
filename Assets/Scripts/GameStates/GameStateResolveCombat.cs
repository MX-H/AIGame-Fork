using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateResolveCombat : IGameState
{
    private List<Creature> attackers;
    private Creature[] defenders;
    private int attackerIndex;

    private float timeElapsed;
    private float delayBetweenAttacks = 0.5f;
    public GameStateResolveCombat(GameSession gameSession) : base(gameSession)
    { }

    public override void OnEnter()
    {
        attackers = gameSession.GetActivePlayer().arena.GetCombatCreatures();
        defenders = gameSession.GetNonActivePlayer().arena.GetDefenders();
        attackerIndex = 0;
        timeElapsed = 0.0f;
        SoundLibrary.PlaySound("combat");
    }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            timeElapsed += frameDelta;
            while (timeElapsed > delayBetweenAttacks)
            {
                timeElapsed -= delayBetweenAttacks;
                if (attackerIndex < attackers.Count)
                {
                    CreatureState attackerState = attackers[attackerIndex].gameObject.GetComponent<CreatureState>();

                    if (defenders[attackerIndex] != null)
                    {
                        CreatureState defenderState = defenders[attackerIndex].gameObject.GetComponent<CreatureState>();
            
                        gameSession.ServerCreatureDoDamage(attackers[attackerIndex], attackerState.GetAttack(), defenders[attackerIndex]);

                        // Fast strike hits first, if it kills defender they can't do damage back
                        if (!attackers[attackerIndex].HasKeyword(KeywordAttribute.FAST_STRIKE) || !defenders[attackerIndex].GetCreatureState().IsDead())
                        {
                            gameSession.ServerCreatureDoDamage(defenders[attackerIndex], defenderState.GetAttack(), attackers[attackerIndex]);
                        }
                    }
                    else
                    {
                        gameSession.ServerCreatureDoDamage(attackers[attackerIndex], attackerState.GetAttack(), gameSession.GetNonActivePlayer());
                    }
                    attackerIndex++;
                }
                else
                {
                    ChangeState(GameSession.GameState.WAIT_ACTIVE);
                }
                gameSession.ServerUpdateGameState();
            }
        }
    }

    public override void OnExit()
    {
        if (gameSession.isServer)
        {
            foreach (PlayerController players in gameSession.GetPlayerList())
            {
                players.ServerLeaveCombat();
            }
        }
        attackers = null;
        defenders = null;
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.RESOLVING_COMBAT;
    }
}

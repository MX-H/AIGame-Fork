using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateCardSelect : IGameState
{
    private int[] seeds = new int[3];
    private PlayerController srcPlayer;
    private CardGenerationFlags flags;
    private bool started;
    private int oldWaitingIndex;

    public GameStateCardSelect(GameSession session) : base(session)
    { }

    public override void OnEnter()
    {
        base.OnEnter();
        started = false;

        if (gameSession.isServer)
        {
            oldWaitingIndex = gameSession.GetWaitingPlayerIndex();
            GameUtils.GetTurnTimer().ResetTimer(false);
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        GameUtils.GetCardSelector().HideSelections();
        if (gameSession.isServer)
        {
            gameSession.SetWaitingPlayerIndex(oldWaitingIndex);
        }
    }

    public override void Update(float frameDelta)
    {
        if (gameSession.isServer)
        {
            if (GameUtils.GetTurnTimer().IsTimeUp() && started)
            {
                int seed = seeds[Random.Range(0, 3)];
                CardSelectionEvent cardEvent = new CardSelectionEvent(gameSession.GetWaitingOnPlayer(), srcPlayer, seed, flags);

                // Send event to game session so if there is an incoming event this frame we choose that event
                gameSession.HandleEvent(cardEvent);
            }
        }
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        base.HandleEvent(eventInfo);

        PlayerController player = eventInfo.playerId.GetComponent<PlayerController>();

        if (gameSession.isServer)
        {
            if (eventInfo is StartCardSelectionEvent startEvent)
            {
                gameSession.SetWaitingPlayerIndex(gameSession.GetPlayerIndex(player));

                seeds[0] = startEvent.seed1;
                seeds[1] = startEvent.seed2;
                seeds[2] = startEvent.seed3;
                srcPlayer = startEvent.srcPlayer.GetComponent<PlayerController>();
                flags = startEvent.flags;
                started = true;

                player.ServerStartCardSelection(srcPlayer, seeds[0], seeds[1], seeds[2], flags);
            }

            if (player == gameSession.GetWaitingOnPlayer() && eventInfo is CardSelectionEvent selectedEvent)
            {
                if((selectedEvent.seed == seeds[0] || selectedEvent.seed == seeds[1] || selectedEvent.seed == seeds[2]) 
                    && selectedEvent.srcPlayer == srcPlayer.netIdentity && selectedEvent.flags == flags && started)
                {
                    started = false;
                    gameSession.ServerPlayerDrawCard(gameSession.GetWaitingOnPlayer(), selectedEvent.srcPlayer.GetComponent<PlayerController>(), selectedEvent.seed, selectedEvent.flags);
                    ExitState();
                }
            }
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.CARD_SELECT;
    }
}

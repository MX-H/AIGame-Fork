using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameStateInitialize : IGameState
{
    private enum Substate
    {
        INITIALIZE,
        WAITING
    }

    private int playerAcknowledgements;
    private Substate substate;
    private List<NetworkIdentity> playerList;
    private bool hasSentConfirmation;

    // Start is called before the first frame update
    public GameStateInitialize(GameSession session) : base(session)
    {
    }

    public override void OnEnter()
    {
        playerList = new List<NetworkIdentity>();
        playerAcknowledgements = 0;
        substate = Substate.INITIALIZE;
        hasSentConfirmation = false;
    }

    public override void OnExit()
    {
        playerList = null;
    }

    public override void Update(float frameDelta)
    {
        switch (substate)
        {
            case Substate.INITIALIZE:
                {
                    if (gameSession.isServer && playerAcknowledgements == gameSession.GetMaxPlayers())
                    {
                        EnterSubstate((int)Substate.WAITING);
                        gameSession.ServerInitializePlayers(playerList.ToArray());
                    }

                    if (!hasSentConfirmation && gameSession.GetLocalPlayer() != null)
                    {
                        gameSession.GetLocalPlayer().ClientRequestSendConfirmation();
                        hasSentConfirmation = true;
                    }

                    break;
                }
            case Substate.WAITING:
                {
                    if (gameSession.isServer && playerAcknowledgements == gameSession.GetMaxPlayers())
                    {
                        ChangeState(GameSession.GameState.GAME_START);
                    }

                    break;
                }
        }
    }

    public override void SetSubstate(int index)
    {
        substate = (Substate)index;
        playerAcknowledgements = 0;
        hasSentConfirmation = false;
    }

    public override int GetSubstateIndex()
    {
        return (int)substate;
    }

    public override void HandleEvent(IEvent eventInfo)
    {
        if (eventInfo is ConfirmationEvent confirmEvent)
        {
            playerAcknowledgements++;

            if (substate == Substate.INITIALIZE)
            {
                playerList.Add(confirmEvent.playerId);
            }
        }
    }

    public override GameSession.GameState GetStateType()
    {
        return GameSession.GameState.INITIALIZE;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConfirmButton : Targettable
{
    public GameSession gameSession;
    public PlayerController localPlayer;
    public TextMeshProUGUI confirmText;
    private enum State
    {
        AWAITING_CONFIRMATION,
        AWAITING_SELECTION,
        INACTIVE,
        END_TURN
    }

    State state;
    protected override void Start()
    {
        base.Start();
        state = State.INACTIVE;
    }

    public override bool IsTargettable()
    {
        return (state != State.INACTIVE) && (state != State.AWAITING_SELECTION);
    }

    // Update is called once per frame
    protected override void Update()
    {
        DetermineState();
        switch (state)
        {
            case State.AWAITING_SELECTION:
                confirmText.text = "CHOOSE";
                break;
            case State.INACTIVE:
                confirmText.text = "WAIT";
                break;
            case State.AWAITING_CONFIRMATION:
                confirmText.text = "OK";
                break;
            case State.END_TURN:
                confirmText.text = "END TURN";
                break;
        }

        base.Update();
        
    }

    private void OnMouseDown()
    {
        if (localPlayer)
        {
            switch (state)
            {
                case State.AWAITING_CONFIRMATION:
                    localPlayer.ClientRequestSendConfirmation();
                    break;
                case State.END_TURN:
                    localPlayer.ClientRequestEndTurn();
                    break;
            }
        }
    }

    private void DetermineState()
    {
        if (gameSession && localPlayer)
        {
            if (!gameSession.IsWaitingOnPlayer(localPlayer))
            {
                state = State.INACTIVE;
            }
            else if (localPlayer.IsSelectingTargets())
            {
                state = State.AWAITING_SELECTION;
            }
            else if (!localPlayer.IsInCombat() && !localPlayer.IsResolving() && gameSession.IsActivePlayer(localPlayer))
            {
                state = State.END_TURN;
            }
            else
            {
                state = State.AWAITING_CONFIRMATION;
            }
        }
    }
}

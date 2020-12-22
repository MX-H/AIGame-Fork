using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ConfirmButton : MonoBehaviour
{
    public PlayerController localPlayer;
    private TextMeshProUGUI confirmText;
    private Button button;

    private enum State
    {
        AWAITING_CONFIRMATION,
        AWAITING_SELECTION,
        INACTIVE,
        END_TURN
    }

    State state;
    void Start()
    {
        state = State.INACTIVE;
        confirmText = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
        DetermineState();
        switch (state)
        {
            case State.AWAITING_SELECTION:
                confirmText.text = "CHOOSE";
                button.interactable = false;
                break;
            case State.INACTIVE:
                confirmText.text = "WAIT";
                button.interactable = false;
                break;
            case State.AWAITING_CONFIRMATION:
                confirmText.text = "OK";
                button.interactable = true;
                break;
            case State.END_TURN:
                button.interactable = true;
                if (localPlayer.IsActivePlayer())
                {
                    confirmText.text = "END TURN";
                }
                else
                {
                    confirmText.text = "START TURN";
                }
                break;
        }        
    }

    public void OnClick()
    {
        if (localPlayer)
        {
            switch (state)
            {
                case State.AWAITING_CONFIRMATION:
                    if (localPlayer.IsSelectingTargets())
                    {
                        localPlayer.ConfirmSelectedTargets();
                    }
                    else
                    {
                        localPlayer.ClientRequestSendConfirmation();
                    }
                    break;
                case State.END_TURN:
                    localPlayer.ClientRequestSendConfirmation();
                    break;
            }
        }
    }

    private void DetermineState()
    {
        GameSession gameSession = FindObjectOfType<GameSession>();
        if (gameSession && localPlayer)
        {
            if (!gameSession.IsWaitingOnPlayer(localPlayer))
            {
                state = State.INACTIVE;
            }
            else if (localPlayer.IsSelectingTargets())
            {
                if (localPlayer.HasValidSelectedTargets())
                {
                    state = State.AWAITING_CONFIRMATION;
                }
                else
                {
                    state = State.AWAITING_SELECTION;
                }
            }
            else if (!localPlayer.IsInCombat() && !localPlayer.IsResolving())
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

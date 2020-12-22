using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

[RequireComponent(typeof(Button))]
public class GameOverButton : MonoBehaviour
{
    public PlayerController localPlayer;
    private TextMeshProUGUI gameOverMsg;
    private Button button;

    private enum State
    {
        ON_GOING,
        OVER
    }

    State state;
    protected void Start()
    {
        state = State.ON_GOING;
        GetComponent<Image>().enabled = false;
        gameOverMsg = GetComponentInChildren<TextMeshProUGUI>();
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    // Update is called once per frame
    void Update()
    {
        DetermineState();
        switch (state)
        {
            case State.ON_GOING:
                gameOverMsg.text = "";
                button.interactable = false;
                break;
            case State.OVER:
                gameOverMsg.text = "CONFIRM";
                button.interactable = true;
                break;
        }

    }

// TODO: This should be updated to return to the game client when that is complete
// Currently it just uses the logic in the game lobby to return to that and if the host presses on it, both players return to the lobby
    void OnClick()
    {
        if (localPlayer && state == State.OVER)
        {
            NetworkRoomManager networkRoomManager = FindObjectOfType<NetworkRoomManager>();
            networkRoomManager.ServerChangeScene(networkRoomManager.RoomScene);
        }
    }
    private void DetermineState()
    {
        GameSession gameSession = FindObjectOfType<GameSession>();
        if (gameSession && localPlayer)
        {
            if (gameSession.IsGameOverState())
            {
                state = State.OVER;
                GetComponent<Image>().enabled = true;
            }
            else
            {
                state = State.ON_GOING;
            }
        }
    }
}

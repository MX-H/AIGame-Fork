using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class GameOverButton : Targettable
{
    public PlayerController localPlayer;
    public TextMeshProUGUI gameOverMsg;

    private enum State
    {
        ON_GOING,
        OVER
    }

    State state;
    protected override void Start()
    {
        base.Start();
        state = State.ON_GOING;
        GetComponent<Renderer>().enabled = false;
        gameOverMsg = GetComponentInChildren<TextMeshProUGUI>();
    }

    public override bool IsTargettable()
    {
        return state == State.OVER;
    }

    public override bool IsTargettable(TargettingQuery targetQuery)
    {
        return false;
    }

    public void InvokeUpdate()
    {
        Update();
    }

    // Update is called once per frame
    protected override void Update()
    {
        DetermineState();
        switch (state)
        {
            case State.ON_GOING:
                gameOverMsg.text = "";
                break;
            case State.OVER:
                gameOverMsg.text = "CONFIRM";
                break;
        }

        base.Update();
    }

// TODO: This should be updated to return to the game client when that is complete
// Currently it just uses the logic in the game lobby to return to that and if the host presses on it, both players return to the lobby
    protected override void OnMouseDown()
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
            if (gameSession.isGameOverState())
            {
                state = State.OVER;
                GetComponent<Renderer>().enabled = true;
            }
            else
            {
                state = State.ON_GOING;
            }
        }
    }
}

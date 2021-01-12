using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RoomPlayer : NetworkRoomPlayer
{
    [SyncVar]
    MatchSettings matchSettings;

    public GameLobby gameLobby;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        gameLobby = GameObject.Find("GameLobby").GetComponent<GameLobby>();
    }

    public override void OnClientEnterRoom()
    {
        base.OnClientEnterRoom();

        if (isLocalPlayer)
        {
            CmdSetMatchSettings(gameLobby.GetMatchSettings());
            gameLobby.SetEnableMatchGUI(false);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (isLocalPlayer)
        {
            gameLobby.SetEnableMatchGUI(true);
        }
    }

    [Command]
    private void CmdSetMatchSettings(MatchSettings settings)
    {
        matchSettings = settings;
    }

    public MatchSettings GetMatchSettings()
    {
        return matchSettings;
    }

}

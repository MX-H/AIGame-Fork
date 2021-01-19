using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameLobbyNetworkManager : NetworkRoomManager
{
    [SerializeField]
    private GlobalDatabase database;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        GameUtils.SetDatabase(database);
    }

    // Update is called once per frame
    void Update() 
    {
    }


    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection connection, GameObject roomPlayerObj)
    {
        RoomPlayer roomPlayer = roomPlayerObj.GetComponent<RoomPlayer>();

        GameObject gameObject = Instantiate(playerPrefab);

        PlayerController player = gameObject.GetComponent<PlayerController>();
        player.ServerSetupPlayer(roomPlayer.GetMatchSettings());

        return gameObject;
    }

}

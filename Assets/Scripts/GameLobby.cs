using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class GameLobby : NetworkRoomManager
{
    [SerializeField]
    private Text lobbyText;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update() 
    {
        if (lobbyText != null)
        {
            lobbyText.text = "Players Joined (" + roomSlots.Count + "/" + maxConnections + ")";
        }
    }
}

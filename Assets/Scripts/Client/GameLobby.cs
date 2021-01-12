using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    public GameObject selectionPanel;

    // Start is called before the first frame update
    MatchSettings matchSettings;

    void Start()
    {
        matchSettings = new MatchSettings();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEnableMatchGUI(bool enable)
    {
        selectionPanel.gameObject.SetActive(enable);
    }

    public void SetAvatarIndex(int avatarIndex)
    {
        matchSettings.avatarIndex = avatarIndex;
    }

    public void SetDeckModelIndex(int deckModelIndex)
    {
        matchSettings.deckModelIndex = deckModelIndex;
    }

    public MatchSettings GetMatchSettings()
    {
        return matchSettings;
    }
}

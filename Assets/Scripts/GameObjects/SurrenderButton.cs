using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]

public class SurrenderButton : MonoBehaviour
{
    public PlayerController localPlayer;
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        if (localPlayer)
        {
            localPlayer.ClientRequestSurrender();
        }
    }
}

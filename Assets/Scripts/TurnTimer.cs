using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Mirror;

public class TurnTimer : NetworkBehaviour
{
    public float turnTime;
    public float currTurnTime;

    public TextMeshProUGUI timerText;
    
    void Start()
    {
        turnTime = GameConstants.TURN_DURATION;
        currTurnTime = turnTime;
        GameUtils.SetTurnTimer(this);
    }

    void Update()
    {
        if (turnTime > 0)
        {
            turnTime -= Time.deltaTime;
        }
        else
        {
            turnTime = 0;
        }
        timerText.text = Math.Round(turnTime).ToString();
    }

    public void ResetTimer(bool isStartTurn)
    {
        turnTime = isStartTurn ? GameConstants.TURN_DURATION : GameConstants.CONFIRM_DURATION;
        Debug.Log("RESET TIMER");
    }

    public void StoreTimer()
    {
        currTurnTime = turnTime;
        Debug.Log("STORING TIME: " + currTurnTime);
    }

    public void RestoreTimer()
    {
        turnTime = currTurnTime;
        Debug.Log("RESTORED TIME: " + turnTime);
    }

    public bool IsTimeUp()
    {
        return turnTime <= 0;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurnTimer : MonoBehaviour
{
    public float turnTime;
    public float currTurnTime;
    public bool runTimer;
    void Start() 
    {
        turnTime = GameConstants.TURN_DURATION;
        currTurnTime = turnTime;
        runTimer = false;
        GameUtils.SetTurnTimer(this);
    }

    void Update() 
    {
        if (runTimer) 
        {
            if (turnTime > 0)
            {
                turnTime -= Time.deltaTime;
                Debug.Log("Time: " + turnTime.ToString());
            }
            else
            {
                turnTime = 0;
                Debug.Log("TURN END TIME");
            }
        }
    }

    public void ResetTimer(bool isStartTurn)
    {
        turnTime = isStartTurn ? GameConstants.TURN_DURATION : GameConstants.CONFIRM_DURATION;
        Debug.Log("RESET TIMER");
    }

    public void StartTimer()
    {
        runTimer = true;
        Debug.Log("START TIMER");
    }

    public void StopTimer()
    {
        runTimer = false;
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
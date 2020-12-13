using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IGameState
{
    protected GameSession gameSession;
    public IGameState(GameSession session)
    {
        gameSession = session;
    }

    public virtual void OnEnter()
    {   
    }

    public virtual void OnResume()
    {
    }

    public virtual void OnSuspend()
    {
    }

    public virtual void Update(float frameDelta)
    {
    }

    public virtual void OnExit()
    {
    }

    public virtual void HandleEvent(IEvent eventInfo)
    {
    }

    public virtual void SetSubstate(int index)
    { }

    public virtual int GetSubstateIndex()
    {
        return 0;
    }

    public abstract GameSession.GameState GetStateType();

    protected void ChangeState(GameSession.GameState gameState)
    {
        if (gameSession.isServer)
        {
            gameSession.ServerChangeState(gameState);
        }
    }

    protected void EnterState(GameSession.GameState gameState)
    {
        if (gameSession.isServer)
        {
            gameSession.ServerPushState(gameState);
        }
    }

    protected void ExitState()
    {
        if (gameSession.isServer)
        {
            gameSession.ServerPopState();
        }
    }

    protected void EnterSubstate(int substateIndex)
    {
        if (gameSession.isServer)
        {
            SetSubstate(substateIndex);
            gameSession.ServerChangeSubstate(substateIndex);
        }
    }
}

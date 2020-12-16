using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtils
{
    private static GameSession gameSession;
    private static CreatureModelIndex creatureModelIndex;
    private static TurnTimer turnTimer;
    private static CardSelector cardSelector;
    public static void SetGameSession(GameSession game)
    {
        gameSession = game;
    }

    public static GameSession GetGameSession()
    {
        return gameSession;
    }

    public static void SetCreatureModelIndex(CreatureModelIndex creatureModels)
    {
        creatureModelIndex = creatureModels;
    }

    public static CreatureModelIndex GetCreatureModelIndex()
    {
        return creatureModelIndex;
    }
    
    public static void SetTurnTimer(TurnTimer timer)
    {
        turnTimer = timer;
    }

    public static TurnTimer GetTurnTimer()
    {
        return turnTimer;
    }

    public static void SetCardSelector(CardSelector selector)
    {
        cardSelector = selector;
    }

    public static CardSelector GetCardSelector()
    {
        return cardSelector;
    }
}

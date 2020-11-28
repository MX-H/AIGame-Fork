using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtils
{
    private static GameSession gameSession;
    private static CreatureModelIndex creatureModelIndex;
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
}

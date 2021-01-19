using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtils
{
    private static GameSession gameSession;
    private static CreatureModelIndex creatureModelIndex;
    private static TurnTimer turnTimer;
    private static CardSelector cardSelector;
    private static GlobalDatabase database;
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

    public static void SetDatabase(GlobalDatabase data)
    {
        database = data;
    }

    public static GlobalDatabase GetDatabase()
    {
        return database;
    }

    public static List<ITargettingDescription> ReplaceCreatureTargetDescriptions()
    {
        TargetXDescription targetDesc = new TargetXDescription(TargetType.CREATURES, Alignment.POSITIVE);
        targetDesc.amount = 1;
        targetDesc.qualifier = null;

        List<ITargettingDescription> list = new List<ITargettingDescription>();
        list.Add(targetDesc);

        return list;
    }

    public static List<CardEffectDescription> ReplaceCreatureEffectDescriptions()
    {
        List<CardEffectDescription> list = new List<CardEffectDescription>();

        TargetXDescription targetDesc = new TargetXDescription(TargetType.CREATURES, Alignment.POSITIVE);
        targetDesc.amount = 1;
        targetDesc.qualifier = null;

        ReplaceCreatureEffectDescription effectDesc = new ReplaceCreatureEffectDescription();

        CardEffectDescription cardEffectDesc = new CardEffectDescription();

        cardEffectDesc.targettingType = targetDesc;
        cardEffectDesc.effectType = effectDesc;

        list.Add(cardEffectDesc);
        return list;
    }
}

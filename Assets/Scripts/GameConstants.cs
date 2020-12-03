using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameConstants
{
    public static readonly int MAX_HAND_SIZE = 10;
    public static readonly int START_DECK_SIZE = 30;
    public static readonly int MAX_PLAYER_HEALTH = 10;
    public static readonly int MAX_MANA = 10;
    public static readonly int MAX_CREATURES = 5;
    public static readonly int MAX_TRAPS = 5;
    public static readonly int STARTING_HAND_SIZE = 4;
    public static readonly int MAX_STACK_SIZE = 5;
    public static readonly float TURN_DURATION = 60.0f;
    public static readonly float CONFIRM_DURATION = 30.0f;
    public static class Z_LAYERS
    {
        public static readonly float HOVER_LAYER = -40;
        public static readonly float CREATURE_DRAG_LAYER = -10;
    }

    public static class Y_LAYERS
    {
        public static readonly float PLAY_LEVEL = -2f;
    }

    public static class PATHS
    {
        public static readonly string CARD_IMAGES = "Sprites\\CardSprites\\";
    }
}
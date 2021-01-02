using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatModifier : IModifier
{
    public int atkModifier;
    public int defModifier;

    public StatModifier()
    {
        atkModifier = 0;
        defModifier = 0;
    }

    public StatModifier(int atk, int def, DurationType duration) : base(duration)
    {
        atkModifier = atk;
        defModifier = def;
    }

    public StatModifier(int atk, int def, Creature source) : base(source)
    {
        atkModifier = atk;
        defModifier = def;
    }
}

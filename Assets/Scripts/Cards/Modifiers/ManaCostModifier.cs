using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaCostModifier : IModifier
{
    public int manaCostChange;

    public ManaCostModifier()
    {
        manaCostChange = 0;
    }

    public ManaCostModifier(int manaCost, DurationType duration) : base(duration)
    {
        manaCostChange = manaCost;
    }

    public ManaCostModifier(int manaCost, Creature source) : base(source)
    {
        manaCostChange = manaCost;
    }
}

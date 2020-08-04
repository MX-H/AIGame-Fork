using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IHistogram : ScriptableObject
{
    public abstract int GetValue<T>(T key) where T : System.Enum;

    public abstract int GetTotal<T>() where T : System.Enum;

    public abstract int GetTotal<T>(IEnumerable<T> keys) where T : System.Enum;
}

public class NullHistogram : IHistogram
{
    public override int GetValue<T>(T key)
    {
        return 0;
    }

    public override int GetTotal<T>()
    {
        return 0;
    }

    public override int GetTotal<T>(IEnumerable<T> keys)
    {
        return 0;
    }
}
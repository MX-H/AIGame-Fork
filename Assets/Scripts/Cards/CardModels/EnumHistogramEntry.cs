using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnumHistogramEntry
{
    public EnumHistogramEntry(int ind, int val, System.Type type)
    {
        enumVal = ind;
        value = val;
        enumType = type.AssemblyQualifiedName;
    }

    [ReadOnly]
    public int enumVal;

    [Range(0, 30)]
    public int value;

    public string enumType;

    public int total;
}
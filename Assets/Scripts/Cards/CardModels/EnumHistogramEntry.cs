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

    public int enumVal;

    public int value;

    public string enumType;
}
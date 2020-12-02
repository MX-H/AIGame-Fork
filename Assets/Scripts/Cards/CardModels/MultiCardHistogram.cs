using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiCardHistogram : IHistogram
{
    List<IHistogram> histograms;

    MultiCardHistogram(IEnumerable<IHistogram> models)
    {
        histograms = new List<IHistogram>(models);
    }

    public override int GetTotal<T>()
    {
        return GetTotal((T[]) System.Enum.GetValues(typeof(T)));
    }

    public override int GetTotal<T>(IEnumerable<T> keys)
    {
        int total = 0;

        // We have to add up each element value since we multiply across all the histograms
        foreach (T val in keys)
        {
            total += GetValue(val);
        }

        return total;
    }

    public override int GetValue<T>(T key)
    {
        int value = 1;

        foreach (IHistogram histogram in histograms)
        {
            value *= histogram.GetValue(key);
        }

        return value;
    }

}

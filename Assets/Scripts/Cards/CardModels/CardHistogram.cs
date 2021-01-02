using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Model", menuName = "CardHistogram", order = 51)]
public class CardHistogram : IHistogram
{
    [System.Serializable]
    private class EnumHistogram
    {
        [ReadOnly]
        public string id;

        [SerializeField]
        private List<EnumHistogramEntry> values;

        private void UpdateTotal()
        {
            int total = GetTotal();
            foreach (EnumHistogramEntry entry in values)
            {
                entry.total = total;
            }
        }

        public EnumHistogram(System.Type t)
        {
            id = t.Name;
            values = new List<EnumHistogramEntry>();
            int i = 0;
            foreach (int val in System.Enum.GetValues(t))
            {
                values.Add(new EnumHistogramEntry(val, 1, t));
            }

            UpdateTotal();
        }

        public void Validate()
        {
            System.Type type = System.Type.GetType(id);

            for (int i = values.Count; i < System.Enum.GetValues(type).Length; i++)
            {
                values.Add(new EnumHistogramEntry(i, 1, type));
            }

            UpdateTotal();
        }

        public void AddValue(int key, int value)
        {
            int prevValue = values[key].value;
            if (prevValue + value < 0)
            {
                value = prevValue;
            }
            values[key].value += value;

            UpdateTotal();
        }

        public void SetValue(int key, int value)
        {
            if (value < 0)
            {
                value = 0;
            }
            int prevValue = values[key].value;
            values[key].value = value;

            UpdateTotal();
        }

        public int GetTotal()
        {
            int total = 0;
            foreach (EnumHistogramEntry i in values)
            {
                total += i.value;
            }
            return total;
        }

        public int GetTotal(IEnumerable<int> keys)
        {
            int total = 0;
            foreach (int key in keys)
            {
                total += values[key].value;
            }
            return total;
        }

        public int GetValue(int key)
        {
            return values[key].value;
        }
    }

    [SerializeField]
    public Dictionary<System.Type, int> typeIndexMap;

    [SerializeField]
    private List<EnumHistogram> histograms;

    public CardHistogram()
    {
        typeIndexMap = new Dictionary<System.Type, int>();
        histograms = new List<EnumHistogram>();
        foreach (System.Type t in CardEnums.EnumTypes)
        {
            typeIndexMap[t] = histograms.Count;
            histograms.Add(new EnumHistogram(t));
        }
    }

    public void OnEnable()
    {
        foreach (System.Type t in CardEnums.EnumTypes)
        {
            if (typeIndexMap.ContainsKey(t) && typeIndexMap[t] < histograms.Count)
            {
                histograms[typeIndexMap[t]].Validate();
            }
            else
            {
                typeIndexMap[t] = histograms.Count;
                histograms.Add(new EnumHistogram(t));
            }
        }
    }

    public void OnValidate()
    {
        OnEnable();
    }

    public void AddValue<T>(T key, int value) where T : System.Enum
    {
        if (typeIndexMap.ContainsKey(typeof(T)))
        {
            histograms[typeIndexMap[typeof(T)]].AddValue(System.Convert.ToInt32(key), value);
        }
    }

    public void SetValue<T>(T key, int value) where T : System.Enum
    {
        if (typeIndexMap.ContainsKey(typeof(T)))
        {
            histograms[typeIndexMap[typeof(T)]].SetValue(System.Convert.ToInt32(key), value);
        }
    }

    public override int GetValue<T>(T key)
    {
        if (typeIndexMap.ContainsKey(typeof(T)))
        {
            return histograms[typeIndexMap[typeof(T)]].GetValue(System.Convert.ToInt32(key));
        }
        return 0;
    }

    public override int GetTotal<T>()
    {
        if (typeIndexMap.ContainsKey(typeof(T)))
        {
            return histograms[typeIndexMap[typeof(T)]].GetTotal();
        }
        return 0;
    }

    public override int GetTotal<T>(IEnumerable<T> keys)
    {
        if (typeIndexMap.ContainsKey(typeof(T)))
        {
            return histograms[typeIndexMap[typeof(T)]].GetTotal(keys.Cast<int>());
        }
        return 0;
    }

}

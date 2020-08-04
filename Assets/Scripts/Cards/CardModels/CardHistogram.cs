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
#if UNITY_EDITOR
        [SerializeField]
        public readonly string id;
#endif
        [SerializeField]
        public readonly System.Type type;
        private class IndexMap : Dictionary<int, int> { }

        [SerializeField]
        private IndexMap keyIndexMap;

        [SerializeField]
        private List<int> values;

        private void ValidateKey(int key)
        {
            if (!keyIndexMap.ContainsKey(key))
            {
                keyIndexMap[key] = values.Count;
                values.Add(1);
            }
        }

        public EnumHistogram(System.Type t)
        {
#if UNITY_EDITOR
            id = t.Name;
#endif
            type = t;
            keyIndexMap = new IndexMap();
            values = new List<int>();
            int i = 0;
            foreach (int val in System.Enum.GetValues(t))
            {
                keyIndexMap[val] = i++;
                values.Add(1);
            }
        }

        public void Update()
        {
            foreach (int val in System.Enum.GetValues(type))
            {
                ValidateKey(val);
            }
        }

        public void AddValue(int key, int value)
        {
            ValidateKey(key);

            int prevValue = values[keyIndexMap[key]];
            if (prevValue + value < 0)
            {
                value = prevValue;
            }
            values[keyIndexMap[key]] += value;
        }

        public void SetValue(int key, int value)
        {
            ValidateKey(key);

            if (value < 0)
            {
                value = 0;
            }
            int prevValue = values[keyIndexMap[key]];
            values[keyIndexMap[key]] = value;
        }

        public int GetTotal()
        {
            int total = 0;
            foreach (int i in values)
            {
                total += i;
            }
            return total;
        }

        public int GetTotal(IEnumerable<int> keys)
        {
            int total = 0;
            foreach (int key in keys)
            {
                ValidateKey(key);
                total += values[keyIndexMap[key]];
            }
            return total;
        }

        public int GetValue(int key)
        {
            ValidateKey(key);

            return values[keyIndexMap[key]];
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

    public void Update()
    {
        foreach (System.Type t in CardEnums.EnumTypes)
        {
            if (typeIndexMap.ContainsKey(t))
            {
                histograms[typeIndexMap[t]].Update();
            }
            else
            {
                typeIndexMap[t] = histograms.Count;
                histograms.Add(new EnumHistogram(t));
            }
        }
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

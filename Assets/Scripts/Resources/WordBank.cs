using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New WordBank", menuName = "WordBank", order = 51)]
public class WordBank : ScriptableObject
{
    [System.Serializable]
    public struct WordBankEntry
    {
        public string word;

        public CardTags requiredTags;
        public CardTags categoryTags;

        public WordTags wordTags;
    }

    [SerializeField]
    private List<WordBankEntry> words = new List<WordBankEntry>();

    public void OnEnable()
    {
        words.Sort((w1, w2) => w1.word.CompareTo(w2.word));
    }

    public void OnValidate()
    {
    }

    public List<string> GetWordsFromTags(CardTags cardTags, WordTags wordTags, List<string> excludeWords = null)
    {
        List<string> list = new List<string>();
        foreach (WordBankEntry entry in words)
        {
            if (excludeWords != null && excludeWords.Contains(entry.word))
            {
                continue;
            }

            bool valid = (cardTags & entry.requiredTags) == entry.requiredTags;
            valid &= (entry.categoryTags == CardTags.NONE || (cardTags & entry.categoryTags) != CardTags.NONE);
            valid &= (wordTags & entry.wordTags) == wordTags;

            if (valid)
            {
                list.Add(entry.word);
            }
        }
        return list;

    }
}

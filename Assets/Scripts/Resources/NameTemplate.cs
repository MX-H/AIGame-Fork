using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System;

[System.Serializable]
public class NameTemplate
{
    public string template;
    public CardTags requiredTags;
    public CardTags categoryTags;

    // Used for combining as subphrase
    public WordTags outputTags;

    private string regexPattern = @"\[([^\]]*?)\]";

    public NameTemplate Combine(NameTemplate innerTemplate)
    {
        // the inner template should be a subset of the outer template
        if ((categoryTags & innerTemplate.categoryTags) == innerTemplate.categoryTags &&
            (requiredTags & innerTemplate.requiredTags) == innerTemplate.requiredTags)
        {
            List<WordTags> wordTags = GetWordTags();
            MatchCollection matches = Regex.Matches(template, regexPattern);

            for (int i = 0; i < wordTags.Count; i++)
            {
                if ((wordTags[i] & innerTemplate.outputTags) == wordTags[i])
                {
                    NameTemplate newTemplate = new NameTemplate();
                    newTemplate.template = template.Replace(matches[i].Groups[0].Value, innerTemplate.template);
                    newTemplate.categoryTags = categoryTags;
                    newTemplate.requiredTags = requiredTags;
                    newTemplate.requiredTags |= CardTags.MID_COST | CardTags.HIGH_COST;
                    return newTemplate;
                }
            }
        }
        return null;
    }

    public void Validate()
    {
        MatchCollection matches = Regex.Matches(template, regexPattern);

        foreach (Match m in matches)
        {
            WordTags tags = (WordTags)System.Enum.Parse(typeof(WordTags), m.Groups[1].Value, true);
            if (!(System.Enum.IsDefined(typeof(WordTags), tags) || tags.ToString().Contains(",")))
            {
                Debug.LogError("{" + template + "} could not parse " + m.Groups[1].Value);
            }
        }

    }
    public List<WordTags> GetWordTags()
    {
        MatchCollection matches = Regex.Matches(template, regexPattern);

        List<WordTags> tags = new List<WordTags>();
        foreach (Match m in matches)
        {
            tags.Add((WordTags)System.Enum.Parse(typeof(WordTags), m.Groups[1].Value, true));
        }

        return tags;
    }

    public string GetName(List<string> replacements)
    {
        if (replacements.Count != GetWordTags().Count)
        {
            Debug.LogError("The number of words generated does not match the template");
        }

        Regex regex = new Regex(regexPattern);

        string replacedText = template;
        foreach (string replace in replacements)
        {
            replacedText = regex.Replace(replacedText, replace, 1);
        }

        return replacedText;
    }

}

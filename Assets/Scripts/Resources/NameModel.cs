using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New NameModel", menuName = "NameModel", order = 51)]
public class NameModel : ScriptableObject
{
    public WordBank wordBank;
    [SerializeField]
    private List<NameTemplate> nameTemplates = new List<NameTemplate>();
    [SerializeField]
    private List<NameTemplate> combinedTemplates;

    public void OnEnable()
    {
        wordBank.OnEnable();
        nameTemplates.Sort((w1, w2) => w1.template.CompareTo(w2.template));
    }

    public void OnValidate()
    {
        List<string> strings = new List<string>();

        foreach (NameTemplate template in nameTemplates)
        {
            template.Validate();
            strings.Add(template.template);
        }

        combinedTemplates = new List<NameTemplate>();

        foreach (NameTemplate outerTemplate in nameTemplates)
        {
            foreach (NameTemplate innerTemplate in nameTemplates)
            {
                // Nesting templates within itself is kind of awkward
                if (outerTemplate == innerTemplate)
                {
                    continue;
                }
                NameTemplate newTemplate = outerTemplate.Combine(innerTemplate);
                if (newTemplate != null && !strings.Contains(newTemplate.template))
                {
                    combinedTemplates.Add(newTemplate);
                    strings.Add(newTemplate.template);
                }
            }
        }

        combinedTemplates.Sort((w1, w2) => w1.template.CompareTo(w2.template));
    }

    private List<NameTemplate> GetTemplates(CardTags tags)
    {
        List<NameTemplate> templates = new List<NameTemplate>();

        foreach (NameTemplate template in nameTemplates)
        {
            if (((template.requiredTags & tags) == template.requiredTags) &&
                (template.categoryTags == CardTags.NONE || ((tags & template.categoryTags) != CardTags.NONE)))
            {
                templates.Add(template);
            }
        }

        foreach (NameTemplate template in combinedTemplates)
        {
            if (((template.requiredTags & tags) == template.requiredTags) &&
                (template.categoryTags == CardTags.NONE || ((tags & template.categoryTags) != CardTags.NONE)))
            {
                templates.Add(template);
            }
        }

        return templates;
    }

    public string GenerateName(System.Random random, CardTags tags)
    {
        List<NameTemplate> templates = GetTemplates(tags);


        while (templates.Count > 0)
        {
            NameTemplate template = templates[random.Next(templates.Count)];
            List<WordTags> wordTags = template.GetWordTags();
            List<string> replacements = new List<string>();
            bool valid = true;

            foreach (WordTags wTags in wordTags)
            {
                List<string> possibleWords = wordBank.GetWordsFromTags(tags, wTags, replacements);
                if (possibleWords.Count > 0)
                {
                    replacements.Add(possibleWords[random.Next(possibleWords.Count)]);
                }
                else
                {
                    Debug.LogWarning("Could not generate words to fill out template: " + template.template);
                    valid = false;
                    break;
                }
            }

            if (valid)
            {
                return template.GetName(replacements);
            }

            templates.Remove(template);
        }
        return "Untitled";
    }
}

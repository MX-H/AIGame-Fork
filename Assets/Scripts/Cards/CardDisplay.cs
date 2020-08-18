using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    private CardDescription cardDesc;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI mana;
    public TextMeshProUGUI textBox;

    public GameObject creatureFields;
    public TextMeshProUGUI creatureType;
    public TextMeshProUGUI atk;
    public TextMeshProUGUI def;
    public TextMeshProUGUI typeText;
    public Image templateImage;
    public Sprite[] cardTemplates;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (cardDesc != null)
        {
            cardName.text = cardDesc.name;
            mana.text = cardDesc.manaCost.ToString();
            typeText.text = CardParsing.Parse(cardDesc.cardType).ToUpper();

            string effectText = "";

            creatureFields.SetActive(false);

            switch (cardDesc.cardType)
            {
                case CardType.CREATURE:
                    if (cardDesc is CreatureCardDescription creatureDesc)
                    {
                        creatureFields.SetActive(true);
                        creatureType.text = CardParsing.Parse(creatureDesc.creatureType).ToUpper();
                        atk.text = creatureDesc.attack.ToString();
                        def.text = creatureDesc.health.ToString();
                        bool first = true;
                        foreach (KeywordAttribute a in creatureDesc.attributes)
                        {
                            if (first)
                            {
                                first = !first;
                            }
                            else
                            {
                                effectText += ", ";
                            }
                            effectText += CardParsing.Parse(a);
                        }
                        if (!first)
                        {
                            effectText += "\n\n";
                        }
                    }
                    templateImage.sprite = cardTemplates[0];

                    break;
                case CardType.SPELL:
                    templateImage.sprite = cardTemplates[1];

                    break;
                case CardType.TRAP:
                    templateImage.sprite = cardTemplates[2];

                    break;
            }

            foreach (CardEffectDescription effect in cardDesc.cardEffects)
            {
                string effectString = effect.CardText() + "(";
                if (effect.GetAlignment() == Alignment.NEGATIVE)
                {
                    effectString += (-(PowerBudget.DOWNSIDE_WEIGHT * effect.PowerLevel())).ToString();

                }
                else
                {
                    effectString += effect.PowerLevel().ToString();
                }
                effectString += ").\n";
                effectText += effectString;
            }

            textBox.text = effectText;

        }
    }

    public void SetCardDescription(CardDescription desc)
    {
        cardDesc = desc;
    }
}

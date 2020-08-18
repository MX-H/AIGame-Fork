using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    private CardDescription cardDesc;
    public Image cardBack;
    public Text cardName;
    public Text mana;
    public Text textBox;

    public GameObject creatureFields;
    public Text creatureType;
    public Text atk;
    public Text def;
    public Text typeText;
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
                    cardBack.color = new Color(1.0f, 0.5f, 0.5f);
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
                    cardBack.color = new Color(0.5f, 0.8f, 0.8f);
                    templateImage.sprite = cardTemplates[1];

                    break;
                case CardType.TRAP:
                    cardBack.color = new Color(0.5f, 0.5f, 1.0f);
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

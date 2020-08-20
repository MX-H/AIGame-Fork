using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    // Start is called before the first frame update
    private CardInstance cardDesc;
    public TextMeshProUGUI cardName;
    public TextMeshProUGUI mana;
    public TextMeshProUGUI textBox;

    public GameObject creatureFields;
    public TextMeshProUGUI creatureType;
    public TextMeshProUGUI atk;
    public TextMeshProUGUI def;
    public TextMeshProUGUI typeText;
    public Image templateImage;
    public Image cardImage;

    public Sprite[] cardTemplates;
    public Card referenceCard;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (referenceCard != null)
        {
            cardDesc = referenceCard.cardData;
        }

        if (cardDesc != null)
        {
            cardName.text = cardDesc.GetCardName();
            mana.text = cardDesc.GetManaCost().ToString();
            typeText.text = CardParsing.Parse(cardDesc.GetCardType()).ToUpper();

            string effectText = "";

            creatureFields.SetActive(false);

            switch (cardDesc.GetCardType())
            {
                case CardType.CREATURE:

                    creatureFields.SetActive(true);
                    creatureType.text = CardParsing.Parse(cardDesc.GetCreatureType()).ToUpper();
                    atk.text = cardDesc.GetAttackVal().ToString();
                    def.text = cardDesc.GetHealthVal().ToString();
                    bool first = true;
                    foreach (KeywordAttribute a in cardDesc.GetAttributes())
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
                    
                    templateImage.sprite = cardTemplates[0];

                    break;
                case CardType.SPELL:
                    templateImage.sprite = cardTemplates[1];

                    break;
                case CardType.TRAP:
                    templateImage.sprite = cardTemplates[2];

                    break;
            }

            foreach (CardEffectDescription effect in cardDesc.GetCardEffects())
            {
                string effectString = effect.CardText();
                /*
                effectString += "(";
                if (effect.GetAlignment() == Alignment.NEGATIVE)
                {
                    effectString += (-(PowerBudget.DOWNSIDE_WEIGHT * effect.PowerLevel())).ToString();

                }
                else
                {
                    effectString += effect.PowerLevel().ToString();
                }
                effectString += ").";
                */
                effectString += '\n';
                effectText += effectString;
            }

            textBox.text = effectText;
            if (cardDesc.GetImage())
            {
                Sprite[] sprites = Resources.LoadAll<Sprite>(GameConstants.PATHS.CARD_IMAGES + cardDesc.GetImage().name);
                cardImage.sprite = sprites[1];
            }
        }
    }

    public void SetCard(CardInstance card)
    {
        cardDesc = card;
    }
}

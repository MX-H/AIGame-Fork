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
            typeText.text = CardParsing.Parse(cardDesc.GetCardType()).ToUpper();

            int manaVal = cardDesc.GetManaCost();
            int baseManaVal = cardDesc.GetBaseManaCost();

            mana.text = manaVal.ToString();

            if (manaVal < baseManaVal)
            {
                mana.text = "<color=green>" + mana.text + "</color>";
            }
            else if (manaVal > baseManaVal)
            {
                mana.text = "<color=red>" + mana.text + "</color>";
            }

            string effectText = "";

            creatureFields.SetActive(false);

            switch (cardDesc.GetCardType())
            {
                case CardType.CREATURE:

                    creatureFields.SetActive(true);
                    creatureType.text = CardParsing.Parse(cardDesc.GetCreatureType()).ToUpper();

                    int atkVal = cardDesc.GetAttackVal();
                    int baseAtkVal = cardDesc.GetBaseAttackVal();

                    int hpVal = cardDesc.GetHealthVal();
                    int hpBaseVal = cardDesc.GetBaseHealthVal();

                    atk.text = atkVal.ToString();
                    def.text = hpVal.ToString();

                    if (atkVal > baseAtkVal)
                    {
                        atk.text = "<color=green>" + atk.text + "</color>";
                    }
                    else if (atkVal < baseAtkVal)
                    {
                        atk.text = "<color=red>" + atk.text + "</color>";
                    }

                    if (hpVal > hpBaseVal)
                    {
                        def.text = "<color=green>" + def.text + "</color>";
                    }
                    else if (hpVal < hpBaseVal)
                    {
                        def.text = "<color=red>" + def.text + "</color>";
                    }

                    bool first = true;

                    SortedSet<KeywordAttribute> baseAttributes = cardDesc.GetBaseAttributes();

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

                        if (baseAttributes.Contains(a))
                        {
                            effectText += CardParsing.Parse(a);
                        }
                        else
                        {
                            effectText += "<color=#009A35ff>" + CardParsing.Parse(a) + "</color>";
                        }
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

                effectString += '\n';
                effectText += effectString;
            }

            textBox.text = effectText;
            if (cardDesc.GetImage())
            {
                Sprite[] sprites = Resources.LoadAll<Sprite>(GameConstants.PATHS.CARD_IMAGES + cardDesc.GetImage().name);
                cardImage.sprite = sprites[0];
            }
        }
    }

    public void SetCard(CardInstance card)
    {
        cardDesc = card;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    private CardDescription cardDesc;
    public Image cardBack;
    public Text name;
    public Text mana;
    public Text textBox;

    public GameObject creatureFields;
    public Text creatureType;
    public Text atk;
    public Text def;
    public Text typeText;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (cardDesc != null)
        {
            name.text = cardDesc.name;
            mana.text = cardDesc.manaCost.ToString();
            typeText.text = cardDesc.cardType.ToString();

            string effectText = "";

            creatureFields.SetActive(false);

            switch (cardDesc.cardType)
            {
                case CardType.CREATURE:
                    cardBack.color = new Color(1.0f, 0.5f, 0.5f);
                    if (cardDesc is CreatureCardDescription creatureDesc)
                    {
                        creatureFields.SetActive(true);
                        creatureType.text = creatureDesc.creatureType.ToString();
                        atk.text = creatureDesc.attack.ToString();
                        def.text = creatureDesc.health.ToString();
                        bool first = true;
                        foreach (KeywordAttributes a in creatureDesc.attributes)
                        {
                            if (first)
                            {
                                first = !first;
                            }
                            else
                            {
                                effectText += ", ";
                            }
                            effectText += a.ToString();
                        }
                        if (!first)
                        {
                            effectText += '\n';
                        }
                    }

                    break;
                case CardType.SPELL:
                    cardBack.color = new Color(0.5f, 0.8f, 0.8f);
                    break;
                case CardType.TRAP:
                    cardBack.color = new Color(0.5f, 0.5f, 1.0f);
                    break;
            }

            foreach (CardEffectDescription effect in cardDesc.cardEffects)
            {
                effectText += effect.CardText() + '\n';
            }

            textBox.text = effectText;

        }
    }

    public void SetCardDescription(CardDescription desc)
    {
        cardDesc = desc;
    }
}

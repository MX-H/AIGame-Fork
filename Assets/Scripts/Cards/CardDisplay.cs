using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    // Start is called before the first frame update
    private CardDescription cardDesc;
    public Text name;
    public Text mana;
    public Text textBox;

    public GameObject creatureFields;
    public Text creatureType;
    public Text atk;
    public Text def;


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

            if (cardDesc is CreatureCardDescription creatureDesc)
            {
                creatureFields.SetActive(true);
                creatureType.text = creatureDesc.creatureType.ToString();
                atk.text = creatureDesc.attack.ToString();
                def.text = creatureDesc.health.ToString();
            }
            else
            {
                creatureFields.SetActive(false);
            }
        }
    }

    public void SetCardDescription(CardDescription desc)
    {
        cardDesc = desc;
    }
}

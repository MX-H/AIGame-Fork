using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSelector : MonoBehaviour
{
    [SerializeField]
    private Card[] cardSelection = new Card[3];

    // Start is called before the first frame update
    void Start()
    {
        foreach (Card card in cardSelection)
        {
            card.selectorOption = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCardSelection(PlayerController owner, PlayerController src, int seed1, int seed2, int seed3, CardGenerationFlags flags = CardGenerationFlags.NONE)
    {
        cardSelection[0].cardData = new CardInstance(src, seed1, flags);
        cardSelection[0].owner = owner;
        cardSelection[1].cardData = new CardInstance(src, seed2, flags);
        cardSelection[1].owner = owner;
        cardSelection[2].cardData = new CardInstance(src, seed3, flags);
        cardSelection[2].owner = owner;
    }

    public void ShowSelections(PlayerController selector)
    {
        bool revealed = selector.isLocalPlayer || GameUtils.GetGameSession().isServerOnly;
        foreach (Card card in cardSelection)
        {
            card.isRevealed = revealed;
            card.enabled = revealed;
            card.transform.eulerAngles = new Vector3(0, (revealed ? 0 : 180), 0);
            card.gameObject.SetActive(true);
        }
    }

    public void HideSelections()
    {
        foreach (Card card in cardSelection)
        {
            card.gameObject.SetActive(false);
            card.enabled = false;
        }
    }
}

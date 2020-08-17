using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Hand : MonoBehaviour
{
    public static readonly int MAX_HAND_SIZE = 10;
    public List<Card> cards = new List<Card>();

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        int inHand = 0;
        bool cardIsDragged = false;
        foreach (Card c in cards)
        {
            if (!c.dragging)
            {
                inHand++;
            }
            else {
                cardIsDragged = true;
            }
        }
        if (inHand > 0)
        {
            float center = (float)(cards.Count - 1) / 2.0f;
            int count = 0;
            for (int i = 0; i < cards.Count; i++)
            {
                if (!cards[i].dragging)
                {
                    cards[i].transform.localPosition = new Vector3((i - center) * 3.2f, 0, Mathf.Abs((center - i )) * 0.3f);
                    count++;
                }
            }
        }

        // Want to resort the cards in hand if you are dragging cards
        if (cardIsDragged)
        {
            cards = cards.OrderBy(x => x.transform.position.x).ToList();
        }

    }

    void AddCard(Card c)
    {
        cards.Add(c);
        c.transform.parent = transform;
    }

}

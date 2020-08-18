using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Hand : MonoBehaviour
{
    public static readonly int MAX_HAND_SIZE = 10;
    public List<Card> cards = new List<Card>();
    public bool localPlayer = true;

    // Start is called before the first frame update
    void Start()
    {
        if (localPlayer)
        {
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, transform.up);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, transform.up);
            foreach (Card c in cards)
            {
                c.isDraggable = false;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        int inHand = 0;
        bool cardIsDragged = false;
        float fanRadius = 20.0f;

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
                if (!cards[i].IsInteracting())
                {

                    if (localPlayer)
                    {
                        cards[i].transform.rotation = Quaternion.LookRotation(cards[i].transform.position - Camera.main.transform.position, transform.up);
                        cards[i].transform.localPosition = new Vector3(0, 0, (center - i) * 0.35f);
                        cards[i].transform.Rotate(new Vector3(0, 0, (i - center) * -5.0f));

                    }
                    else
                    {
                        cards[i].transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - cards[i].transform.position, transform.up);
                        cards[i].transform.localPosition = new Vector3(0, 0, (center - i) * 0.35f);
                        cards[i].transform.Rotate(new Vector3(0, 0, (i - center) * -5.0f));
                    }


                    Vector3 translation = cards[i].transform.up * fanRadius - transform.up * fanRadius;
                    cards[i].transform.Translate(translation, Space.World);

                    count++;
                }
            }
        }

        // Want to resort the cards in hand if you are dragging cards
        if (cardIsDragged)
        {
            cards = cards.OrderBy(x => Camera.main.WorldToScreenPoint(x.transform.position).x).ToList();
        }

    }

    void AddCard(Card c)
    {
        cards.Add(c);
        c.transform.parent = transform;
    }

}

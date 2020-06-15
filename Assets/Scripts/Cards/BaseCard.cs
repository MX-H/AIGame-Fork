using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCard : MonoBehaviour
{
    public string Name { get; set; }

    private int manaCost;
    public int ManaCost
    {
        get { return manaCost; }
        set { if (value >= 0) { manaCost = value; } }
    }

    [SerializeField]
    private List<CardEffect> cardEffects;

    BaseCard()
    {
        cardEffects = new List<CardEffect>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void AddCardEffect(CardEffect effect)
    {
        cardEffects.Add(effect);
    }
}

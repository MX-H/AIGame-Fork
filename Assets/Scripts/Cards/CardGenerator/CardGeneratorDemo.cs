using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGeneratorDemo : MonoBehaviour
{
    // Start is called before the first frame update

    public CardDisplay display;
    public CardHistogram model;

    private ICardGenerator cardGenerator;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            model.Update();
            ICardGenerator cardGenerator = new ProceduralCardGenerator(model);
            display.SetCardDescription(cardGenerator.GenerateCard((int)Random.Range(0, 10000)));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGeneratorDemo : MonoBehaviour
{
    // Start is called before the first frame update

    public CardUI display;
    public CardHistogram model;
    public ImageGlossary images;

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
            ICardGenerator cardGenerator = new ProceduralCardGenerator(model, images);
            display.SetCard(new CardInstance(cardGenerator.GenerateCard(Random.Range(0, 10000))));
        }
    }
}

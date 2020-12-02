using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardGeneratorDemo : MonoBehaviour
{
    // Start is called before the first frame update

    public CardUI display;
    public CardHistogram model;
    public ImageGlossary images;
    public CardDescription desc;
    public CreatureModelIndex creatureModels;

    private ICardGenerator cardGenerator;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (desc == null)
            {
                ICardGenerator cardGenerator = new ProceduralCardGenerator(model, images, creatureModels);
                display.SetCard(new CardInstance(cardGenerator.GenerateCard(Random.Range(0, 10000))));
            }
            else
            {
                display.SetCard(new CardInstance(desc));
            }
        }
    }
}

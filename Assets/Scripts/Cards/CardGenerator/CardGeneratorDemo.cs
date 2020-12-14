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
    public CardGenerationFlags flags;
    public NameModel nameModel;

    private ICardGenerator cardGenerator;

    public CardInstance instance;
    public int providedSeed;
    public int generatedSeed;

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
                ICardGenerator cardGenerator = new ProceduralCardGenerator(model, images, creatureModels, nameModel);

                if (providedSeed == 0)
                {
                    generatedSeed = Random.Range(0, 10000);
                }
                else
                {
                    generatedSeed = providedSeed;
                }
                instance = new CardInstance(cardGenerator.GenerateCard(generatedSeed, flags));
            }
            else
            {
                display.SetCard(new CardInstance(desc));
            }
            display.SetCard(instance);
        }
        else if (Input.GetKeyDown("q"))
        {
            if (instance != null)
            {
                double power = instance.baseCard.PowerLevel();
                double mana = PowerBudget.PowerLevelToMana(power);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ICardGenerator
{
    public abstract CardDescription GenerateCard(int seed, CardGenerationFlags flags);
}

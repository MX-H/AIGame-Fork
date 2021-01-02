using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveModifierEffectDescription : IEffectDescription
{
    public ModifierType modifierType;
    public DurationType durationType;
    public IModifierDescription modifierDescription;

    public GiveModifierEffectDescription(EffectType effectType, ModifierType modifier) : base(effectType)
    {
        modifierType = modifier;
    }
    public override void ApplyToTarget(Targettable target, PlayerController player)
    {
        GameSession gameSession = GameUtils.GetGameSession();
        gameSession.ServerApplyModifier(target.GetComponent<Creature>(), modifierDescription.GetModifier(durationType));
    }

    public override string CardText(bool plural)
    {
        return ((durationType == DurationType.END_OF_TURN) ? "for this turn, grant ": "grant ") + modifierDescription.CardText() + " to";
    }

    public override Alignment GetAlignment()
    {
        return modifierDescription.GetAlignment();
    }

    public override double PowerLevel()
    {
        return modifierDescription.PowerLevel();
    }
}

public class GiveModifierEffectProceduralGenerator : IProceduralEffectGenerator
{
    EffectType effectType;
    ModifierType modifierType;
    IProceduralModifierGenerator modifierGenerator;
    public GiveModifierEffectProceduralGenerator(EffectType effect)
    {
        effectType = effect;
        switch (effectType)
        {
            case EffectType.GIVE_POSITIVE_STATS:
                modifierGenerator = new StatModifierProceduralGenerator(true);
                modifierType = ModifierType.STAT;
                break;
            case EffectType.GIVE_NEGATIVE_STATS:
                modifierGenerator = new StatModifierProceduralGenerator(false);
                modifierType = ModifierType.STAT;
                break;
            case EffectType.GIVE_KEYWORD:
                modifierGenerator = new KeywordModifierProceduralGenerator();
                modifierType = ModifierType.KEYWORD;
                break;
        }
    }
    public override IEffectDescription Generate()
    {
        GiveModifierEffectDescription desc = new GiveModifierEffectDescription(effectType, modifierType);

        desc.durationType = (random.NextDouble() > 0.5) ? DurationType.END_OF_TURN : DurationType.FOREVER;

        double durationMod = PowerBudget.GetDurationTypeModifier(desc.durationType);
        modifierGenerator.SetupParameters(random, model, minAllocatedBudget / durationMod, maxAllocatedBudget / durationMod);
        desc.modifierDescription = modifierGenerator.Generate();

        return desc;
    }

    public override IEffectDescription GetDescriptionType()
    {
        GiveModifierEffectDescription desc = new GiveModifierEffectDescription(effectType, modifierType);
        desc.modifierDescription = modifierGenerator.GetDescriptionType();
        return desc;
    }

    public override double GetMinCost()
    {
        return modifierGenerator.GetMinCost() * PowerBudget.GetDurationTypeModifier(DurationType.END_OF_TURN);
    }
}

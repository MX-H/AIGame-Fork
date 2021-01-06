using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuraModifierEffectDescription : IEffectDescription
{
    public ModifierType modifierType;
    public IModifierDescription modifierDescription;

    public AuraModifierEffectDescription(EffectType effectType, ModifierType modifier) : base(effectType)
    {
        modifierType = modifier;
    }
    public override void ApplyToTarget(Targettable target, PlayerController player, Targettable source)
    {
        GameSession gameSession = GameUtils.GetGameSession();
        gameSession.ServerApplyModifier(target, modifierDescription.GetModifier(source as Creature));
    }

    public override string CardText(bool plural)
    {
        return "have " + modifierDescription.CardText();
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

public class AuraModifierEffectProceduralGenerator : IProceduralEffectGenerator
{
    EffectType effectType;
    ModifierType modifierType;
    IProceduralModifierGenerator modifierGenerator;
    public AuraModifierEffectProceduralGenerator(EffectType effect)
    {
        effectType = effect;
        switch (effectType)
        {
            case EffectType.AURA_POSITIVE_STATS:
                modifierGenerator = new StatModifierProceduralGenerator(true);
                modifierType = ModifierType.STAT;
                break;
            case EffectType.AURA_NEGATIVE_STATS:
                modifierGenerator = new StatModifierProceduralGenerator(false);
                modifierType = ModifierType.STAT;
                break;
            case EffectType.AURA_KEYWORD:
                modifierGenerator = new KeywordModifierProceduralGenerator();
                modifierType = ModifierType.KEYWORD;
                break;
            case EffectType.AURA_MANA_COST_REDUCTION:
                modifierGenerator = new ManaCostModifierProceduralGenerator(true);
                modifierType = ModifierType.MANA_COST;
                break;
            case EffectType.AURA_MANA_COST_TAX:
                modifierGenerator = new ManaCostModifierProceduralGenerator(false);
                modifierType = ModifierType.MANA_COST;
                break;
        }
    }
    public override IEffectDescription Generate()
    {
        AuraModifierEffectDescription desc = new AuraModifierEffectDescription(effectType, modifierType);

        double durationMod = PowerBudget.GetDurationTypeModifier(DurationType.AURA);
        modifierGenerator.SetupParameters(random, model, minAllocatedBudget / durationMod, maxAllocatedBudget / durationMod);
        desc.modifierDescription = modifierGenerator.Generate();

        return desc;
    }

    public override IEffectDescription GetDescriptionType()
    {
        AuraModifierEffectDescription desc = new AuraModifierEffectDescription(effectType, modifierType);
        desc.modifierDescription = modifierGenerator.GetDescriptionType();
        return desc;
    }

    public override double GetMinCost()
    {
        return modifierGenerator.GetMinCost() * PowerBudget.GetDurationTypeModifier(DurationType.AURA);
    }
}

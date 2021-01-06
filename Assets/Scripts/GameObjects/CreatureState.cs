using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Creature))]
public class CreatureState : NetworkBehaviour
{
    Creature creature;

    [SyncVar]
    int currHealthVal;

    [SyncVar]
    bool summoningSick;

    // Start is called before the first frame update

    public CreatureState()
    {
    }

    void Start()
    {
        creature = gameObject.GetComponent<Creature>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Server]
    public void ServerInitialize()
    {
        creature = gameObject.GetComponent<Creature>();
        currHealthVal = GetMaxHealth();
        summoningSick = !creature.HasKeyword(KeywordAttribute.EAGER);
    }

    [Server]
    public void ServerDoDamage(int damage)
    {
        currHealthVal -= damage;
    }

    [Server]
    public void ServerHealDamage(int heal)
    {
        currHealthVal += heal;
        if (currHealthVal > GetMaxHealth())
        {
            currHealthVal = GetMaxHealth();
        }
    }

    [Server]
    public void SetSummoningSick(bool sick)
    {
        summoningSick = sick;
    }

    [Server]
    public void ServerAddModifier(IModifier modifier)
    {
        if (modifier is StatModifier statMod)
        {
            creature.card.cardData.AddModifier(modifier);
            currHealthVal += statMod.defModifier;

            RpcAddStatModifier(statMod);
        }
        else if (modifier is KeywordModifier keywordMod)
        {
            creature.card.cardData.AddModifier(modifier);

            if (keywordMod.keywordAttribute == KeywordAttribute.EAGER)
            {
                summoningSick = false;
            }

            RpcAddKeywordModifier(keywordMod);
        }
    }

    [ClientRpc]
    private void RpcAddStatModifier(StatModifier modifier)
    {
        if (!isServer)
        {
            creature.card.cardData.AddModifier(modifier);
        }
    }

    [ClientRpc]
    private void RpcAddKeywordModifier(KeywordModifier modifier)
    {
        if (!isServer)
        {
            creature.card.cardData.AddModifier(modifier);
        }
    }

    public void RemoveEndOfTurnModifiers()
    {
        creature.card.cardData.RemoveEndOfTurnModifiers();
        if (isServer)
        {
            if (GetHealth() > GetMaxHealth())
            {
                currHealthVal = GetMaxHealth();
            }
        }
    }

    public void RemoveAuraModifiers(Creature source)
    {
        creature.card.cardData.RemoveAuraModifiers(source);
        if (isServer)
        {
            if (GetHealth() > GetMaxHealth())
            {
                currHealthVal = GetMaxHealth();
            }
        }
    }

    public bool IsSummoningSick()
    {
        return summoningSick;
    }

    public int GetMaxHealth()
    {
        return creature.card.cardData.GetHealthVal();
    }

    public int GetHealth()
    {
        return currHealthVal;
    }

    public int GetAttack()
    {
        return creature.card.cardData.GetAttackVal();
    }

    public int GetBaseHealth()
    {
        return creature.card.cardData.GetBaseHealthVal();
    }

    public int GetBaseAttack()
    {
        return creature.card.cardData.GetBaseAttackVal();
    }

    public bool IsDead()
    {
        return currHealthVal <= 0;
    }

}

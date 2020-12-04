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
        summoningSick = creature.HasKeyword(KeywordAttribute.EAGER);
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

    public bool IsDead()
    {
        return currHealthVal <= 0;
    }

}

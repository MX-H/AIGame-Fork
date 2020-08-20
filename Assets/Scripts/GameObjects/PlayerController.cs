using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    public Hand hand;
    public Deck deck;
    public Arena arena;
    public DiscardPile discard;

    [SyncVar]
    public int health;

    [SyncVar]
    public int currMana;

    [SyncVar]
    public int totalMana;

    public ICardGenerator cardGenerator;
    public IHistogram model;
    public ImageGlossary imageGlossary;
    public Card cardPrefab;
    public Creature creaturePrefab;
    GameSession gameSession;

    public override void OnStartServer()
    {

    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        gameSession = FindObjectOfType<GameSession>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ClientScene.RegisterPrefab(cardPrefab.gameObject);
        ClientScene.RegisterPrefab(creaturePrefab.gameObject);

        cardGenerator = new ProceduralCardGenerator(model, imageGlossary);
        if (isLocalPlayer)
        {
            ConfirmButton confirmButton = FindObjectOfType<ConfirmButton>();
            confirmButton.localPlayer = this;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    [ClientRpc]
    public void RpcAddCardToHand(NetworkIdentity playerId, int seed, NetworkIdentity id)
    {
        Card c = id.gameObject.GetComponent<Card>();

        c.cardData = new CardInstance(playerId.GetComponent<PlayerController>(), seed);

        c.owner = this;
        c.controller = this;
        c.isRevealed = isLocalPlayer;
        c.isDraggable = isLocalPlayer;

        hand.AddCard(c);
    }

    [ClientRpc]
    public void RpcPlayCreature(NetworkIdentity creatureId, NetworkIdentity cardId)
    {
        Creature creature = creatureId.gameObject.GetComponent<Creature>();
        Card card = cardId.gameObject.GetComponent<Card>();
        creature.controller = this;
        creature.owner = this;
        creature.SetCard(card);
        hand.RemoveCard(card);
        arena.AddCreature(creature);
    }

    [ClientRpc]
    public void RpcPlaySpell(NetworkIdentity cardId)
    {
        Card card = cardId.gameObject.GetComponent<Card>();

        hand.RemoveCard(card);
        card.gameObject.SetActive(false);
    }

    [ClientRpc]
    public void RpcPlayTrap(NetworkIdentity cardId)
    {
        Card card = cardId.gameObject.GetComponent<Card>();

        hand.RemoveCard(card);
        arena.AddTrap(card);
    }


    [Server]
    public void ServerStartTurn()
    {
        if (totalMana < GameConstants.MAX_MANA)
        {
            totalMana++;
        }
        currMana = totalMana;
    }

    [Server]
    public void ServerPayCost(Card c)
    {
        currMana -= c.cardData.GetManaCost();
    }

    [Command]
    private void CmdResetPlayerResources()
    {
        health = GameConstants.MAX_PLAYER_HEALTH;
        currMana = totalMana = 0;
    }

    [Command]
    public void CmdSendAcknowledgement()
    {
        gameSession.ServerReceiveAcknowledge();
    }

    public void Reset()
    {
        if (isLocalPlayer)
        {
            CmdResetPlayerResources();
        }

        if (deck)
        {
            deck.Reset();
        }
        if (hand)
        {
            hand.Reset();
        }
        if (arena)
        {
            arena.Reset();
        }
    }

    [Client]
    public void ClientRequestEndTurn()
    {
        if (isLocalPlayer)
        {
            CmdEndTurn();
        }
    }

    [Client]
    public void ClientRequestSendConfirmation()
    {
        if (isLocalPlayer)
        {
            CmdSendConfirmation();
        }
    }

    [Client]
    public void ClientRequestPlayCard(NetworkIdentity card)
    {
        if (isLocalPlayer)
        {
            CmdPlayCard(card);
        }
    }

    [Client]
    public void ClientRequestRemoveFromCombat(NetworkIdentity creature)
    {
        if (isLocalPlayer && CanMoveCreatures())
        {
            CmdRemoveFromCombat(creature);
        }
    }

    [Command]
    private void CmdRemoveFromCombat(NetworkIdentity creature)
    {
        gameSession.ServerRemoveFromCombat(netIdentity, creature);
    }

    [ClientRpc]
    public void RpcRemoveFromCombat(NetworkIdentity creature)
    {
        arena.RemoveFromCombat(creature.gameObject.GetComponent<Creature>());

        // End combat
        if (isLocalPlayer && gameSession.CanChooseAttackers(this) && arena.InCombatCount() == 0)
        {
            CmdLeaveCombat();
        }
    }

    [ClientRpc]
    public void RpcReceiveAttackers(NetworkIdentity[] attackers)
    {
        arena.SetState(Arena.State.BLOCKING);
        List<Creature> creatures = new List<Creature>();
        foreach (NetworkIdentity id in attackers)
        {
            creatures.Add(id.gameObject.GetComponent<Creature>());
        }
        arena.ReceiveAttackers(creatures);
    }

    [Client]
    public void ClientRequestMoveToCombat(NetworkIdentity creature, int ind)
    {
        if (isLocalPlayer &&  CanMoveCreatures())
        {
            CmdMoveToCombat(creature, ind);
        }
    }

    [Command]
    private void CmdMoveToCombat(NetworkIdentity creature, int ind)
    {
        gameSession.ServerMoveToCombat(netIdentity, creature, ind);
    }

    [ClientRpc]
    public void RpcMoveToCombat(NetworkIdentity creature, int ind, bool declaringAttack)
    {
        if (declaringAttack)
        {
            arena.SetState(Arena.State.ATTACKING);
        }
        arena.MoveToCombat(creature.gameObject.GetComponent<Creature>(), ind);
    }

    [ClientRpc]
    public void RpcLeaveCombat()
    {
        arena.SetState(Arena.State.NONE);
    }

    [Command]
    private void CmdLeaveCombat()
    {
        if (gameSession.CanChooseAttackers(this) && arena.InCombatCount() == 0)
        {
            gameSession.ServerLeaveCombat(netIdentity);
        }
    }

    [Command]
    private void CmdEndTurn()
    {
        gameSession.ServerEndTurn(netIdentity);
    }

    [Command]
    private void CmdSendConfirmation()
    {
        gameSession.ServerSendConfirmation(netIdentity);
    }

    [Command]
    private void CmdPlayCard(NetworkIdentity card)
    {
        Card c = card.gameObject.GetComponent<Card>();
        if (c != null && c.cardData != null)
        {
            if (hand.HasCard(c) && c.cardData.GetManaCost() <= currMana && CanPlayCards())
            {
                gameSession.ServerPlayCard(netIdentity, card);
            }
        }
    }

    [Server]
    public void ServerDoDamage(int damage)
    {
        health -= damage;
    }

    [Server]
    public void ServerHealDamage(int heal)
    {
        health += heal;
        if (health > GameConstants.MAX_PLAYER_HEALTH)
        {
            health = GameConstants.MAX_PLAYER_HEALTH;
        }
    }

    [ClientRpc]
    public void RpcDestroyCreature(NetworkIdentity creatureId)
    {
        Creature creature = creatureId.gameObject.GetComponent<Creature>();
        arena.RemoveCreature(creature);
        discard.AddCreature(creature);
    }

    [TargetRpc]
    public void TargetEndGame(NetworkConnection target, bool winner)
    {
        TextMeshProUGUI endGameText = GameObject.Find("GameOverText").GetComponent<TextMeshProUGUI>();
        endGameText.text = winner ? "VICTORY" : "DEFEAT";
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    public bool CanMoveCreatures()
    {
        return gameSession.CanChooseAttackers(this) || gameSession.CanChooseBlocks(this) || gameSession.CanDeclareCombat(this);
    }
    public bool CanPlayCards()
    {
        return gameSession.IsActivePlayer(this) && gameSession.IsWaitingOnPlayer(this) && !IsInCombat();
    }

    public bool IsSelectingTargets()
    {
        return false;
    }

    public bool IsResolving()
    {
        return false;
    }

    public bool IsInCombat()
    {
        return gameSession.CanChooseAttackers(this) || gameSession.CanChooseBlocks(this);
    }
}

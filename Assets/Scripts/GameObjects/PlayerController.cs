using System;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class PlayerController : Targettable
{
    public Hand hand;
    public Deck deck;
    public Arena arena;
    public DiscardPile discard;
    public PlayerUI playerUI;
    public bool hasSentTargets;

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

    List<Targettable> selectedTargets; // Targets for current condition
    List<List<Targettable>> allSelectedTargets;
    List<ITargettingDescription> selectableTargetDescriptions;
    List<CardEffectDescription> selectableEffectDescriptions;

    private TextPrompt selectingTextPrompt;

    private TurnTimer turnTimer;

    public override void OnStartServer()
    {
        base.OnStartServer();
        gameSession = FindObjectOfType<GameSession>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        gameSession = FindObjectOfType<GameSession>();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        ClientScene.RegisterPrefab(cardPrefab.gameObject);
        ClientScene.RegisterPrefab(creaturePrefab.gameObject);
        if (isLocalPlayer)
        {
            selectingTextPrompt = GameObject.Find("Player Selection Prompt").GetComponent<TextPrompt>();
            selectingTextPrompt.gameObject.SetActive(false);
        }
        cardGenerator = new ProceduralCardGenerator(model, imageGlossary, gameSession.creatureModelIndex);
        if (isLocalPlayer)
        {
            ConfirmButton confirmButton = FindObjectOfType<ConfirmButton>();
            confirmButton.localPlayer = this;
            GameOverButton gameOverButton = FindObjectOfType<GameOverButton>();
            gameOverButton.localPlayer = this;
            SurrenderButton surrenderButton = FindObjectOfType<SurrenderButton>();
            surrenderButton.localPlayer = this;
        }
        turnTimer = FindObjectOfType<TurnTimer>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (gameSession.CanSelectTargets(this) && isLocalPlayer)
        {
            // Start target selections
            if (selectableTargetDescriptions == null && !hasSentTargets)
            {
                Card card = gameSession.GetPendingCard(this);
                if (card != null)
                {
                    switch (card.cardData.GetCardType())
                    {
                        case CardType.CREATURE:
                            selectableTargetDescriptions = card.cardData.GetSelectableTargets(gameSession.GetPendingTriggerCondition());
                            selectableEffectDescriptions = card.cardData.GetSelectableEffectsOnTrigger(gameSession.GetPendingTriggerCondition());
                            break;
                        case CardType.SPELL:
                        case CardType.TRAP:
                            selectableTargetDescriptions = card.cardData.GetSelectableTargets(TriggerCondition.NONE);
                            selectableEffectDescriptions = card.cardData.GetSelectableEffectsOnTrigger(TriggerCondition.NONE);
                            break;
                    }

                    selectedTargets = new List<Targettable>();
                    allSelectedTargets = new List<List<Targettable>>();

                    SetTargettingQuery(selectableTargetDescriptions[0]);
                    SetSelectionPrompt(selectableEffectDescriptions[0], 0);
                }

            }

            // Cancel actions when right click detected (undo target select, then playing card)
            if (Input.GetMouseButtonDown(1))
            {
                // Back out of target select to the previous selection
                if (allSelectedTargets.Count > 0)
                {
                    int lastInd = allSelectedTargets.Count - 1;
                    allSelectedTargets.RemoveAt(lastInd);

                    foreach (Targettable t in gameSession.GetPotentialTargets())
                    {
                        t.Deselect();
                    }

                    selectedTargets = new List<Targettable>();

                    SetTargettingQuery(selectableTargetDescriptions[lastInd]);
                    SetSelectionPrompt(selectableEffectDescriptions[lastInd]);

                }
                // Send a request to cancel playing card, note: triggered effects cannot be cancelled so this request will do nothing
                else
                {
                    ClientRequestCancelPlayCard();
                }
            }
        }
    }

    public void SetSelectionPrompt(CardEffectDescription effectDescription, int index = -1)
    {
        if (selectingTextPrompt != null)
        {
            if (effectDescription != null)
            {
                string selectMessage = ((index < 0) ? "" : "<sprite=" + index + "> ") + "Select ";
                if (effectDescription.targettingType.targettingType != TargettingType.EXCEPT)
                {
                    selectMessage += effectDescription.targettingType.CardText() + " that ";
                }
                else
                {
                    ExceptTargetDescription exceptDesc = effectDescription.targettingType as ExceptTargetDescription;
                    ITargettingDescription targetDesc = exceptDesc.targetDescription;
                    selectMessage += exceptDesc.targetDescription.CardText() + " that " + (targetDesc.RequiresPluralEffect() ? "does not " : "do not "); 
                }
                selectMessage += effectDescription.effectType.CardText(effectDescription.targettingType.RequiresPluralEffect());
                selectingTextPrompt.SetText(selectMessage);
                selectingTextPrompt.gameObject.SetActive(true);
            }
        }
    }

    public void HideSelectionPrompt()
    {
        if (selectingTextPrompt != null)
        {
            selectingTextPrompt.gameObject.SetActive(false);
        }
    }

    public override bool IsTargettable()
    {
        return IsActivePlayer();
    }

    public override bool IsTargettable(TargettingQuery targetQuery)
    {
        bool valid = false;

        ITargettingDescription desc = targetQuery.targettingDesc;
        if (desc.targettingType == TargettingType.EXCEPT)
        {
            ExceptTargetDescription exceptDesc = (ExceptTargetDescription)desc;
            desc = exceptDesc.targetDescription;
        }
        switch (desc.targetType)
        {
            case TargetType.PLAYERS:
            case TargetType.DAMAGEABLE:
                valid = true;
                break;
        }

        if (valid)
        {
            IQualifiableTargettingDescription qualifiableDesc = (IQualifiableTargettingDescription)desc;
            if (qualifiableDesc != null)
            {
                IQualifierDescription qualifier = qualifiableDesc.qualifier;
                if (qualifier != null)
                {
                    switch (qualifier.qualifierType)
                    {
                        case QualifierType.NONE:
                            break;
                        default:
                            valid = false;
                            break;
                    }
                }
            }
        }

        return valid;
    }

    [Server]
    public void ServerAddCardToHand(NetworkIdentity playerId, NetworkIdentity sourcePlayerId, NetworkIdentity cardId, int seed, CardGenerationFlags flags = CardGenerationFlags.NONE)
    {
        if (isServerOnly)
        {
            Card c = cardId.gameObject.GetComponent<Card>();

            c.cardData = new CardInstance(sourcePlayerId.GetComponent<PlayerController>(), seed, flags);

            c.owner = this;
            c.controller = this;
            c.isRevealed = true;
            c.isDraggable = false;

            hand.AddCard(c);
        }

        RpcAddCardToHand(playerId, cardId, seed, flags);
    }

    [Server]
    public void ServerAddExistingCardToHand(NetworkIdentity cardId)
    {
        if (isServerOnly)
        {
            Card c = cardId.GetComponent<Card>();
            hand.AddCard(c);
        }
        RpcAddExistingCardToHand(cardId);
    }

    [ClientRpc]
    public void RpcAddExistingCardToHand(NetworkIdentity cardId)
    {
        Card c = cardId.GetComponent<Card>();
        hand.AddCard(c);
    }

    [Server]
    public void ServerAddTrapBackToArena(NetworkIdentity cardId, int index)
    {
        if (isServerOnly)
        {
            Card c = cardId.GetComponent<Card>();
            arena.AddTrap(c, index);
        }
        RpcAddTrapBackToArena(cardId, index);
    }

    [ClientRpc]
    public void RpcAddTrapBackToArena(NetworkIdentity cardId, int index)
    {
        Card c = cardId.GetComponent<Card>();
        arena.AddTrap(c, index);
    }

    [ClientRpc]
    public void RpcAddCardToHand(NetworkIdentity playerId, NetworkIdentity cardId, int seed, CardGenerationFlags flags)
    {
        Card c = cardId.gameObject.GetComponent<Card>();

        c.cardData = new CardInstance(playerId.GetComponent<PlayerController>(), seed, flags);

        c.owner = this;
        c.controller = this;
        c.isRevealed = isLocalPlayer;
        c.isDraggable = isLocalPlayer;

        hand.AddCard(c);
    }

    [Server]
    public void ServerPlayToken(NetworkIdentity cardId, NetworkIdentity creatureId, CreatureType tokenType)
    {
        Card card = cardId.gameObject.GetComponent<Card>();
        card.cardData = new CardInstance(GameUtils.GetCreatureModelIndex().GetToken(tokenType));
        card.owner = this;
        card.controller = this;
        card.isRevealed = isLocalPlayer;
        card.isDraggable = isLocalPlayer;

        ServerPlayCreature(creatureId, cardId, false);

        RpcPlayToken(cardId, creatureId, tokenType);
    }

    [ClientRpc]
    public void RpcPlayToken(NetworkIdentity cardId, NetworkIdentity creatureId, CreatureType tokenType)
    {
        if (!isServer)
        {
            Card card = cardId.gameObject.GetComponent<Card>();
            card.cardData = new CardInstance(GameUtils.GetCreatureModelIndex().GetToken(tokenType));
            card.owner = this;
            card.controller = this;
            card.isRevealed = isLocalPlayer;
            card.isDraggable = isLocalPlayer;

            Creature creature = creatureId.gameObject.GetComponent<Creature>();
            creature.controller = this;
            creature.owner = this;
            creature.SetCard(card);
            arena.AddCreature(creature);
        }
    }

    [Server]
    public void ServerPlayCreature(NetworkIdentity creatureId, NetworkIdentity cardId, bool rpc = true)
    {
        ServerPlayCreature(creatureId, cardId, null, null, rpc);
    }

    [Server]
    public void ServerPlayCreature(NetworkIdentity creatureId, NetworkIdentity cardId, NetworkIdentity[] targets, int[] indexes, bool rpc = true)
    {
        Card card = cardId.gameObject.GetComponent<Card>();
        Creature creature = creatureId.gameObject.GetComponent<Creature>();

        creature.controller = this;
        creature.owner = this;
        creature.SetCard(card);
        arena.AddCreature(creature);

        if (targets != null && indexes != null)
        {
            gameSession.ServerAddEffectToStack(cardId, TriggerCondition.ON_SELF_ENTER, targets, indexes);
        }
        // If creature has an ETB effect with no targets add the effect to the stack
        else if (card.cardData.HasEffectsOnTrigger(TriggerCondition.ON_SELF_ENTER) && card.cardData.GetSelectableTargets(TriggerCondition.ON_SELF_ENTER).Count == 0)
        {
            gameSession.ServerAddEffectToStack(cardId, TriggerCondition.ON_SELF_ENTER);
        }
        gameSession.ServerTriggerEffects(creature, TriggerCondition.ON_CREATURE_ENTER);

        if (rpc)
        {
            RpcPlayCreature(creatureId, cardId);
        }
    }

    [ClientRpc]
    public void RpcPlayCreature(NetworkIdentity creatureId, NetworkIdentity cardId)
    {
        if (!isServer)
        {
            Creature creature = creatureId.gameObject.GetComponent<Creature>();
            Card card = cardId.gameObject.GetComponent<Card>();
            creature.controller = this;
            creature.owner = this;
            creature.SetCard(card);
            arena.AddCreature(creature);
        }
    }

    [Server]
    public void ServerPlaySpell(NetworkIdentity cardId)
    {
        ServerPlaySpell(cardId, null, null);
    }

    [Server]
    public void ServerPlaySpell(NetworkIdentity cardId, NetworkIdentity[] targets, int[] indexes)
    {
        if (targets != null && indexes != null)
        {
            gameSession.ServerAddEffectToStack(cardId, TriggerCondition.NONE, targets, indexes);
        }
        else
        {
            gameSession.ServerAddEffectToStack(cardId, TriggerCondition.NONE);
        }
        RpcPlaySpell(cardId);
    }


    [ClientRpc]
    public void RpcPlaySpell(NetworkIdentity cardId)
    {
    }

    [Server]
    public void ServerPlayTrap(NetworkIdentity cardId)
    {
        if (isServerOnly)
        {
            Card card = cardId.gameObject.GetComponent<Card>();
            arena.AddTrap(card);
        }
        RpcPlayTrap(cardId);
    }

    [ClientRpc]
    public void RpcPlayTrap(NetworkIdentity cardId)
    {
        Card card = cardId.gameObject.GetComponent<Card>();
        arena.AddTrap(card);
    }

    [Server]
    public void ServerActivateTrap(NetworkIdentity cardId)
    {
        // Activating a trap should be the same as playing a spell
        ServerPlaySpell(cardId);
    }

    [Server]
    public void ServerActivateTrap(NetworkIdentity cardId, NetworkIdentity[] targets, int[] indexes)
    {
        ServerPlaySpell(cardId, targets, indexes);
    }

    [Server]
    public void ServerRemoveCardFromHand(NetworkIdentity cardId)
    {
        Card card = cardId.gameObject.GetComponent<Card>();
        hand.RemoveCard(card);
        card.gameObject.SetActive(false);
        RpcRemoveCardFromHand(cardId);
    }

    [ClientRpc]
    public void RpcRemoveCardFromHand(NetworkIdentity cardId)
    {
        if (!isServer)
        {
            Card card = cardId.gameObject.GetComponent<Card>();
            hand.RemoveCard(card);
            card.gameObject.SetActive(false);
        }
    }

    [Server]
    public void ServerRemoveTrapFromArena(NetworkIdentity cardId)
    {
        Card card = cardId.GetComponent<Card>();
        arena.RemoveTrap(card);
        card.gameObject.SetActive(false);
        RpcRemoveTrapFromArena(cardId);
    }

    [ClientRpc]
    public void RpcRemoveTrapFromArena(NetworkIdentity cardId)
    {
        if (!isServer)
        {
            Card card = cardId.GetComponent<Card>();
            arena.RemoveTrap(card);
            card.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    public void RpcSyncTimer(float turnTime, float currTurnTime, String timerText)
    {
        if (!isServer)
        {
            turnTimer.turnTime = turnTime;
            turnTimer.currTurnTime = currTurnTime;
            turnTimer.timerText.text = timerText;
        }
    }

    [Server]
    public void ServerStartTurn()
    {
        if (totalMana < GameConstants.MAX_MANA)
        {
            totalMana++;
        }
        currMana = totalMana;

        foreach (Creature creature in arena.GetAllCreatures())
        {
            creature.creatureState.SetSummoningSick(false);
        }
    }

    [Server]
    public void ServerPayCost(Card c)
    {
        currMana -= c.cardData.GetManaCost();
    }

    [Server]
    public void ServerRefundCost(Card c)
    {
        currMana += c.cardData.GetManaCost();
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

    [Server]
    public void ServerForceEndTurn()
    {
        RpcForceEndTurn();
    }

    [ClientRpc]
    public void RpcForceEndTurn()
    {
        if (isLocalPlayer)
        {
            CmdEndTurn();
        }
    }

    [Server]
    public void ServerForceConfirmation()
    {
        RpcForceConfirmation();
    }

    [ClientRpc]
    public void RpcForceConfirmation()
    {
        if (isLocalPlayer)
        {
            CmdSendConfirmation();
        }
    }

    [Server]
    public void ServerForceCancelPlayCard()
    {
        RpcForceCancelPlayCard();
    }

    [ClientRpc]
    public void RpcForceCancelPlayCard()
    {
        if (isLocalPlayer)
        {
            CmdCancelPlayCard();
        }
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
    public void ClientRequestActivateTrap(NetworkIdentity cardId)
    {
        if (isLocalPlayer)
        {
            CmdActivateTrap(cardId);
        }
    }

    [Client]
    public void ClientRequestCancelPlayCard()
    {
        if (isLocalPlayer)
        {
            CmdCancelPlayCard();
        }
    }

    [TargetRpc]
    public void TargetCancelPlayCard(NetworkConnection target)
    {
        foreach (Targettable t in gameSession.GetPotentialTargets())
        {
            t.Deselect();
        }

        RemoveTargettingQuery();
        HideSelectionPrompt();

        allSelectedTargets = null;
        selectedTargets = null;
        selectableTargetDescriptions = null;
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

    [Server]
    public void ServerRemoveFromCombat(NetworkIdentity creature)
    {
        if (isServerOnly)
        {
            arena.RemoveFromCombat(creature.gameObject.GetComponent<Creature>());
        }
        RpcRemoveFromCombat(creature);
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

    [Server]
    public void ServerReceiveAttackers(NetworkIdentity[] attackers)
    {
        if (isServerOnly)
        {
            arena.SetState(Arena.State.BLOCKING);
            List<Creature> creatures = new List<Creature>();
            foreach (NetworkIdentity id in attackers)
            {
                creatures.Add(id.gameObject.GetComponent<Creature>());
            }
            arena.ReceiveAttackers(creatures);
        }
        RpcReceiveAttackers(attackers);
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
    public void ClientRequestMoveToCombat(Creature creature, int ind)
    {
        if (isLocalPlayer && CanMoveCreatures())
        {
            bool validPos = true;
            if (arena.GetState() == Arena.State.BLOCKING)
            {
                if (!arena.IsValidBlock(creature, ind))
                {
                    validPos = false;
                }
            }

            if (validPos)
            {
                CmdMoveToCombat(creature.netIdentity, ind);
            }
        }
    }

    [Command]
    private void CmdMoveToCombat(NetworkIdentity creature, int ind)
    {
        gameSession.ServerMoveToCombat(netIdentity, creature, ind);
    }

    [Server]
    public void ServerMoveToCombat(NetworkIdentity creature, int ind, bool declaringAttack)
    {
        if (isServerOnly)
        {
            if (declaringAttack)
            {
                arena.SetState(Arena.State.ATTACKING);
            }
            arena.MoveToCombat(creature.gameObject.GetComponent<Creature>(), ind);
        }
        RpcMoveToCombat(creature, ind, declaringAttack);
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

    [Server]
    public void ServerLeaveCombat()
    {
        if (isServerOnly)
        {
            arena.SetState(Arena.State.NONE);
        }
        RpcLeaveCombat();
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

    [Client]
    public void ClientRequestSurrender() 
    {
        CmdSurrender();
    }

    [Command]
    private void CmdSurrender()
    {
        gameSession.ServerSurrender(netIdentity);
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
                TriggerCondition trigger = (c.cardData.GetCardType() == CardType.CREATURE) ? TriggerCondition.ON_SELF_ENTER : TriggerCondition.NONE;

                List<ITargettingDescription> targets = new List<ITargettingDescription>();

                // Traps do not need conditions to be played from hand
                if (c.cardData.GetCardType() != CardType.TRAP)
                {
                    targets = c.cardData.GetSelectableTargets(trigger);
                }

                // If need to select targets select targets first
                if (targets.Count > 0)
                {
                    if (c.HasValidTargets(targets))
                    {
                        gameSession.ServerPlayCardAndSelectTargets(netIdentity, card);
                    }
                    else if (c.cardData.GetCardType() == CardType.CREATURE)
                    {
                        // Creatures do not need to have valid targets to be played, the enter effect just doesn't occur
                        gameSession.ServerPlayCard(netIdentity, card);
                    }
                }
                else
                {
                    // Resolve card
                    gameSession.ServerPlayCard(netIdentity, card);
                }
            }
        }
    }

    [Command]
    private void CmdActivateTrap(NetworkIdentity cardId)
    {
        Card card = cardId.GetComponent<Card>();
        if (arena.IsTrap(card) && CanActivateTraps())
        {
            List<ITargettingDescription> targets = card.cardData.GetSelectableTargets(TriggerCondition.NONE);

            if (targets.Count > 0)
            {
                if (card.HasValidTargets(targets))
                {
                    gameSession.ServerActivateTrapAndSelectTargets(netIdentity, cardId);
                }
            }
            else
            {
                gameSession.ServerActivateTrap(netIdentity, cardId);
            }
        }
    }

    [Command]
    private void CmdCancelPlayCard()
    {
        gameSession.ServerCancelPlayCard(netIdentity);
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

    [Server]
    public void ServerDestroyCreature(NetworkIdentity creatureId)
    {
        if (isServerOnly)
        {
            Creature creature = creatureId.gameObject.GetComponent<Creature>();
            arena.RemoveCreature(creature);
            discard.AddCreature(creature);
        }
        RpcDestroyCreature(creatureId);
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

    [TargetRpc]
    public void TargetSelectRandomTargets(NetworkConnection target)
    {
        var random = new System.Random();
        bool hasMoreSelections = false;
        do
        {
            List<Targettable> targets = gameSession.GetPotentialTargets();
            while (CanSelectMoreTargets() && !HasValidSelectedTargets())
            {
                int index = random.Next(targets.Count);
                targets[index].Select();
                AddTarget(targets[index]);
                targets.RemoveAt(index);
            }
            hasMoreSelections = ConfirmSelectedTargets();
        }  while(hasMoreSelections);
    }

    [TargetRpc]
    public void TargetNotifySelectTargets(NetworkConnection target)
    {
        hasSentTargets = false;
    }

    public bool ConfirmSelectedTargets()
    {
        if (isLocalPlayer && HasValidSelectedTargets())
        {
            allSelectedTargets.Add(selectedTargets);
            foreach (Targettable t in gameSession.GetPotentialTargets())
            {
                t.Deselect();
            }

            if (allSelectedTargets.Count == selectableTargetDescriptions.Count)
            {
                // Send all selected targets to the server
                RemoveTargettingQuery();
                HideSelectionPrompt();

                int[] indexes = new int[allSelectedTargets.Count];
                int totalSize = 0;
                for (int i = 0; i < allSelectedTargets.Count; i++)
                {
                    totalSize += allSelectedTargets[i].Count;
                    indexes[i] = allSelectedTargets[i].Count;
                }

                NetworkIdentity[] targets = new NetworkIdentity[totalSize];
                int ind = 0;
                for (int i = 0; i < allSelectedTargets.Count; i++)
                {
                    for (int j = 0; j < allSelectedTargets[i].Count; j++, ind++)
                    {
                        targets[ind] = allSelectedTargets[i][j].GetComponent<NetworkIdentity>();
                    }
                }

                CmdSendTargets(targets, indexes);

                allSelectedTargets = null;
                selectedTargets = null;
                selectableTargetDescriptions = null;

                hasSentTargets = true;
                
                return false;
            }
            else
            {
                selectedTargets = new List<Targettable>();
                SetTargettingQuery(selectableTargetDescriptions[allSelectedTargets.Count]);
                SetSelectionPrompt(selectableEffectDescriptions[allSelectedTargets.Count], allSelectedTargets.Count);
                return true;
            }
        }
        return false;
    }

    [Command]
    private void CmdSendTargets(NetworkIdentity[] targets, int[] indexes)
    {
        gameSession.ServerSendTargets(netIdentity, targets, indexes);
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
        return gameSession.IsActivePlayer(this) && gameSession.IsWaitingOnPlayer(this) && gameSession.IsStackEmpty() && !IsInCombat() && !IsSelectingTargets();
    }

    public bool CanActivateTraps()
    {
        return gameSession.IsWaitingOnPlayer(this) && !gameSession.IsStackFull() && !IsInCombat() && !IsSelectingTargets();
    }

    public bool IsSelectingTargets()
    {
        return gameSession.CanSelectTargets(this);
    }

    public void SetTargettingQuery(ITargettingDescription desc)
    {
        TargettingQuery query = new TargettingQuery(desc, this);
        foreach (Targettable t in gameSession.GetPotentialTargets())
        {
            t.SetTargettingQuery(query);
        }
    }

    public void RemoveTargettingQuery()
    {
        foreach (Targettable t in gameSession.GetPotentialTargets())
        {
            t.ResetTargettingQuery();
        }
    }

    public bool HasValidSelectedTargets()
    {
        if (selectableTargetDescriptions != null && allSelectedTargets.Count < selectableTargetDescriptions.Count)
        {
            ITargettingDescription desc = selectableTargetDescriptions[allSelectedTargets.Count];
            if (desc.targettingType == TargettingType.EXCEPT)
            {
                ExceptTargetDescription exceptDesc = (ExceptTargetDescription)desc;
                desc = exceptDesc.targetDescription;
            }

            switch (desc.targettingType)
            {
                case TargettingType.TARGET:
                    TargetXDescription targetDesc = (TargetXDescription)desc;
                    return selectedTargets.Count == targetDesc.amount;
                case TargettingType.UP_TO_TARGET:
                    // 0 is valid for up to so selected targets is always valid
                    return true;
            }
        }
        return false;
    }

    public bool CanSelectMoreTargets()
    {
        if (selectableTargetDescriptions != null)
        {
            ITargettingDescription desc = selectableTargetDescriptions[allSelectedTargets.Count];
            if (desc.targettingType == TargettingType.EXCEPT)
            {
                ExceptTargetDescription exceptDesc = (ExceptTargetDescription)desc;
                desc = exceptDesc.targetDescription;
            }

            switch (desc.targettingType)
            {
                case TargettingType.TARGET:
                    TargetXDescription targetDesc = (TargetXDescription)desc;
                    return selectedTargets.Count < targetDesc.amount;
                case TargettingType.UP_TO_TARGET:
                    UpToXTargetDescription upToTargetDesc = (UpToXTargetDescription)desc;
                    return selectedTargets.Count < upToTargetDesc.amount;
            }
        }
        return false;
    }

    public void AddTarget(Targettable target)
    {
        if (CanSelectMoreTargets())
        {
            selectedTargets.Add(target);
        }
    }

    public void RemoveTarget(Targettable target)
    {
        if (selectedTargets != null)
        {
            selectedTargets.Remove(target);
        }
    }

    public bool IsResolving()
    {
        return !gameSession.effectStack.IsEmpty();
    }

    public bool IsInCombat()
    {
        return gameSession.CanChooseAttackers(this) || gameSession.CanChooseBlocks(this);
    }

    public bool IsActivePlayer()
    {
        return gameSession.IsActivePlayer(this);
    }

    public List<PlayerController> GetOpponents()
    {
        return gameSession.GetOpponents(this);
    }

    public bool IsAnOpponent(PlayerController c)
    {
        return GetOpponents().Contains(c);
    }

    public override Targettable GetTargettableUI()
    {
        return playerUI;
    }
}

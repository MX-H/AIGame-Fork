using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class GameSession : NetworkBehaviour
{
    private enum GameState
    {
        INITIALIZE,
        WAIT_FOR_INITIALIZE,
        GAME_START,
        DECLARE_ATTACKS,
        DECLARE_BLOCKS,
        TURN_START,
        WAIT_ACTIVE,
        WAIT_NON_ACTIVE,
        SELECTING_TARGETS,
        GAME_OVER
    }

    [SyncVar]
    GameState currState;

    [SyncVar]
    GameState prevState;

    [SyncVar]
    int activeIndex = 0;

    [SyncVar]
    int waitingIndex = 0;

    [SyncVar]
    bool combatWasDeclared = false;

    PlayerController[] playerList;

    int playerAcknowledgements;

    [SyncVar]
    NetworkIdentity pendingCard;

    [Serializable]
    public struct AssignableAreas
    {
        public PlayerUI playerUI;
        public Hand hand;
        public Deck deck;
        public Arena arena;
        public DiscardPile discard;
    }

    public AssignableAreas[] playerAreas;

    public override void OnStartServer()
    {
        base.OnStartServer();
        currState = GameState.INITIALIZE;
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create 2 players

    }

    public bool IsGameReady()
    {
        return currState != GameState.INITIALIZE && currState != GameState.WAIT_FOR_INITIALIZE;
    }

    [Server]
    private void ChangeState(GameState state)
    {
        prevState = currState;
        currState = state;
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            switch (currState)
            {
                case GameState.INITIALIZE:
                    // Players are loaded
                    playerList = FindObjectsOfType<PlayerController>();

                    if (playerList.Length == playerAreas.Length)
                    {
                        NetworkIdentity[] playerIds = new NetworkIdentity[playerList.Length];
                        for (int i = 0; i < playerList.Length; i++)
                        {
                            playerIds[i] = playerList[i].GetComponent<NetworkIdentity>();
                        }

                        playerAcknowledgements = 0;

                        ServerInitializePlayers(playerIds);

                        ChangeState(GameState.WAIT_FOR_INITIALIZE);
                    }
                    break;
                case GameState.WAIT_FOR_INITIALIZE:
                    if (playerAcknowledgements == playerList.Length)
                    {
                        ChangeState(GameState.GAME_START);
                    }
                    break;
                case GameState.GAME_START:
                    {
                        // Draw starting Hands
                        for (int i = 0; i < GameConstants.STARTING_HAND_SIZE; i++)
                        {
                            for (int j = 0; j < playerList.Length; j++)
                            {
                                NetworkIdentity cardId = ServerCreateCard(playerList[j]);
                                playerList[j].ServerAddCardToHand(playerList[j].netIdentity, (int)(UnityEngine.Random.value * Int32.MaxValue), cardId);
                            }
                        }
                        activeIndex = (UnityEngine.Random.value < 0.5f) ? 0 : 1;
                        ChangeState(GameState.TURN_START);
                    }
                    break;
                case GameState.TURN_START:
                    {
                        playerList[activeIndex].ServerStartTurn();

                        NetworkIdentity cardId = ServerCreateCard(playerList[activeIndex]);
                        playerList[activeIndex].ServerAddCardToHand(playerList[activeIndex].netIdentity, (int)(UnityEngine.Random.value * Int32.MaxValue), cardId);

                        waitingIndex = activeIndex;
                        combatWasDeclared = false;

                        ChangeState(GameState.WAIT_ACTIVE);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    [Server]
    public void ServerReceiveAcknowledge()
    {
        playerAcknowledgements++;
    }

    [Server]
    private NetworkIdentity ServerCreateCard(PlayerController p)
    {
        if (IsGameReady())
        {
            Card c = Instantiate(p.cardPrefab);
            NetworkServer.Spawn(c.gameObject);
            return c.GetComponent<NetworkIdentity>();
        }
        return null;
    }

    [Server]
    private NetworkIdentity ServerCreateCreature(PlayerController p, Card card)
    {
        if (IsGameReady())
        {
            Creature c = Instantiate(p.creaturePrefab);
            NetworkServer.Spawn(c.gameObject);
            c.SetCard(card);
            c.gameObject.GetComponent<CreatureState>().ServerInitialize();
            return c.GetComponent<NetworkIdentity>();
        }
        return null;
    }

    [Server]
    void ServerInitializePlayers(NetworkIdentity[] players)
    {
        if (isServerOnly)
        {
            PlayerController local = null;
            for (int i = 0; i < players.Length; i++)
            {
                PlayerController p = playerList[i];
                playerAreas[i].playerUI.player = p;
                playerAreas[i].hand.AssignPlayer(p);
                playerAreas[i].deck.AssignPlayer(p);
                playerAreas[i].arena.AssignPlayer(p);
                playerAreas[i].discard.AssignPlayer(p);

                p.Reset();
            }
        }
        RpcInitializePlayers(players);
    }

    [ClientRpc]
    void RpcInitializePlayers(NetworkIdentity[] players)
    {
        playerList = new PlayerController[players.Length];
        for (int i = 0; i < playerList.Length; i++)
        {
            playerList[i] = players[i].GetComponent<PlayerController>();
        }

        int startInd = 0;
        for (int i = 0; i < playerList.Length; i++)
        {
            if (playerList[i].isLocalPlayer)
            {
                startInd = i;
                break;
            }
        }

        PlayerController local = null;
        for (int i = 0; i < players.Length; i++)
        {
            PlayerController p = playerList[(startInd + i) % players.Length];
            playerAreas[i].playerUI.player = p;
            playerAreas[i].hand.AssignPlayer(p);
            playerAreas[i].deck.AssignPlayer(p);
            playerAreas[i].arena.AssignPlayer(p);
            playerAreas[i].discard.AssignPlayer(p);

            p.Reset();

            if (p.isLocalPlayer)
            {
                local = p;
            }
        }

        local.CmdSendAcknowledgement();
    }

    [Server]
    public void ServerSendConfirmation(NetworkIdentity id)
    {
        if (IsGameReady() && id == playerList[waitingIndex].netIdentity)
        {
            Debug.Log("Received Confirmation");
            switch (currState)
            {
                case GameState.DECLARE_ATTACKS:
                    {
                        waitingIndex = (activeIndex + 1) % playerList.Length;

                        List<Creature> creatures = playerList[activeIndex].arena.GetCombatCreatures();
                        NetworkIdentity[] attackerIds = new NetworkIdentity[creatures.Count];
                        for (int i = 0; i < attackerIds.Length; i++)
                        {
                            attackerIds[i] = creatures[i].GetComponent<NetworkIdentity>();
                        }

                        playerList[waitingIndex].ServerReceiveAttackers(attackerIds);
                        ChangeState(GameState.DECLARE_BLOCKS);
                    }
                    break;
                case GameState.DECLARE_BLOCKS:
                    {
                        List<Creature> attackers = playerList[activeIndex].arena.GetCombatCreatures();
                        Creature[] defenders = playerList[waitingIndex].arena.GetDefenders();

                        for (int i = 0; i < attackers.Count; i++)
                        {
                            CreatureState attackerState = attackers[i].gameObject.GetComponent<CreatureState>();

                            if (defenders[i] != null)
                            {
                                CreatureState defenderState = defenders[i].gameObject.GetComponent<CreatureState>();

                                attackerState.ServerDoDamage(defenderState.GetAttack());
                                defenderState.ServerDoDamage(attackerState.GetAttack());

                                if (attackerState.IsDead())
                                {
                                    playerList[activeIndex].ServerDestroyCreature(attackerState.netIdentity);
                                }

                                if (defenderState.IsDead())
                                {
                                    playerList[waitingIndex].ServerDestroyCreature(defenderState.netIdentity);
                                }
                            }
                            else
                            {
                                playerList[waitingIndex].ServerDoDamage(attackerState.GetAttack());
                            }
                        }

                        waitingIndex = activeIndex;

                        foreach (PlayerController p in playerList)
                        {
                            p.ServerLeaveCombat();
                        }

                        ChangeState(GameState.WAIT_ACTIVE);
                    }
                    break;
            }
        }

        ServerCheckGameState();
    }

    [Server]
    public void ServerSendTargets(NetworkIdentity playerId, NetworkIdentity[][] targets)
    {
        // Validate all targets
        Card card = pendingCard.GetComponent<Card>();

        bool isValid = false;
        if (card != null && playerList[waitingIndex].netIdentity && currState == GameState.SELECTING_TARGETS)
        {
            PlayerController player = playerId.GetComponent<PlayerController>();

            List<ITargettingDescription> selectableTargetDescriptions = null;
            switch (card.cardData.GetCardType())
            {
                case CardType.CREATURE:
                    selectableTargetDescriptions = card.cardData.GetSelectableTargets(TriggerCondition.ON_CREATURE_ENTER);
                    break;
                case CardType.SPELL:
                case CardType.TRAP:
                    selectableTargetDescriptions = card.cardData.GetSelectableTargets(TriggerCondition.NONE);
                    break;
            }

            if (selectableTargetDescriptions != null && selectableTargetDescriptions.Count == targets.Length)
            {
                isValid = true;
                for (int i = 0; i < selectableTargetDescriptions.Count; i++)
                {
                    ITargettingDescription desc = selectableTargetDescriptions[i];
                    if (desc.targettingType == TargettingType.EXCEPT)
                    {
                        ExceptTargetDescription exceptDesc = (ExceptTargetDescription)desc;
                        desc = exceptDesc.targetDescription;
                    }

                    
                    switch (desc.targettingType)
                    {
                        case TargettingType.TARGET:
                            {
                                TargetXDescription targetDesc = (TargetXDescription)desc;
                                if (targetDesc.amount == targets[i].Length)
                                {
                                    TargettingQuery query = new TargettingQuery(targetDesc, player);
                                    for (int j = 0; j < targets[i].Length; j++)
                                    {
                                        Targettable targettable = targets[i][j].GetComponent<Targettable>();
                                        if (!targettable.IsTargettable(query))
                                        {
                                            isValid = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    isValid = false;
                                }
                                break;
                            }
                        case TargettingType.UP_TO_TARGET:
                            {
                                UpToXTargetDescription targetDesc = (UpToXTargetDescription)desc;
                                if (targetDesc.amount >= targets[i].Length)
                                {
                                    TargettingQuery query = new TargettingQuery(targetDesc, player);
                                    for (int j = 0; j < targets[i].Length; j++)
                                    {
                                        Targettable targettable = targets[i][j].GetComponent<Targettable>();
                                        if (!targettable.IsTargettable(query))
                                        {
                                            isValid = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    isValid = false;
                                }
                                break;
                            }
                    }

                    if (!isValid)
                    {
                        break;
                    }
                }
            }

            if (isValid)
            {
                ServerPlayCardWithTargets(playerId, targets);
            }
            else
            {
                ServerCancelPlayCard(playerId);
            }
        }
    }

    [Server]
    public void ServerCheckGameState()
    {
        PlayerController loser = null;
        foreach (PlayerController p in playerList)
        {
            if (p.IsDead())
            {
                loser = p;
            }
        }
        if (loser != null)
        {
            foreach (PlayerController p in playerList)
            {
                if (loser == p)
                {
                    p.TargetEndGame(p.connectionToClient, false);
                }
                else
                {
                    p.TargetEndGame(p.connectionToClient, true);
                }
            }
            ChangeState(GameState.GAME_OVER);
        }
    }

    [Server]
    public void ServerEndTurn(NetworkIdentity id)
    {
        if (IsGameReady() && id == playerList[activeIndex].netIdentity && id == playerList[waitingIndex].netIdentity)
        {
            activeIndex = (activeIndex + 1) % (playerList.Length);
            waitingIndex = activeIndex;
            ChangeState(GameState.TURN_START);
        }
    }

    [Server]
    public void ServerPlayCardAndSelectTargets(NetworkIdentity playerId, NetworkIdentity card)
    {
        if (IsGameReady() && playerId == playerList[activeIndex].netIdentity && currState == GameState.WAIT_ACTIVE)
        {
            // Remove the card from the players hand
            PlayerController p = playerList[activeIndex];
            Card c = card.gameObject.GetComponent<Card>();
            p.ServerRemoveCardFromHand(card);

            // Go into target selection state and wait for response
            pendingCard = card;
            ChangeState(GameState.SELECTING_TARGETS);
        }
    }

    [Server]
    public void ServerPlayCardWithTargets(NetworkIdentity playerId, NetworkIdentity[][] targets)
    {
        if (IsGameReady() && playerId == playerList[activeIndex].netIdentity && currState == GameState.SELECTING_TARGETS)
        {
            PlayerController player = playerList[activeIndex];
            Card card = pendingCard.GetComponent<Card>();
            player.ServerPayCost(card);

            switch (card.cardData.GetCardType())
            {
                case CardType.CREATURE:
                    NetworkIdentity creature = ServerCreateCreature(player, card);
                    player.ServerPlayCreature(creature, pendingCard);
                    break;
                case CardType.SPELL:
                    player.ServerPlaySpell(pendingCard);
                    break;
                case CardType.TRAP:
                    player.ServerPlayTrap(pendingCard);
                    break;
            }

            ChangeState(GameState.WAIT_ACTIVE);
            pendingCard = null;
        }
    }

    [Server]
    public void ServerCancelPlayCard(NetworkIdentity playerId)
    {
        if(IsGameReady() && playerId == playerList[waitingIndex].netIdentity && currState == GameState.SELECTING_TARGETS)
        {
            Card card = pendingCard.GetComponent<Card>();
            PlayerController player = playerList[waitingIndex];
            player.ServerAddExistingCardToHand(pendingCard);
        }
    }

    [Server]
    public void ServerRemoveCardFromHand(NetworkIdentity playerId, NetworkIdentity card)
    {
        PlayerController p = playerId.GetComponent<PlayerController>();
        p.ServerRemoveCardFromHand(card);
    }

    [Server]
    public void ServerPlayCard(NetworkIdentity playerId, NetworkIdentity card)
    {
        if (IsGameReady() && playerId == playerList[activeIndex].netIdentity && currState == GameState.WAIT_ACTIVE)
        {
            PlayerController p = playerList[activeIndex];
            Card c = card.gameObject.GetComponent<Card>();
            p.ServerPayCost(c);
            p.ServerRemoveCardFromHand(card);

            switch (c.cardData.GetCardType())
            {
                case CardType.CREATURE:
                    NetworkIdentity creature = ServerCreateCreature(p, c);
                    p.ServerPlayCreature(creature, card);
                    break;
                case CardType.SPELL:
                    p.ServerPlaySpell(card);
                    break;
                case CardType.TRAP:
                    p.ServerPlayTrap(card);
                    break;
            }
        }
    }

    [Server]
    public void ServerLeaveCombat(NetworkIdentity playerId)
    {
        if (IsGameReady() && playerId == playerList[activeIndex].netIdentity)
        {
            combatWasDeclared = false;
            ChangeState(GameState.WAIT_ACTIVE);
            playerList[activeIndex].ServerLeaveCombat();
        }
    }

    [Server]
    public void ServerRemoveFromCombat(NetworkIdentity playerId, NetworkIdentity creature)
    {
        PlayerController p = playerId.gameObject.GetComponent<PlayerController>();

        if (IsGameReady() && (CanChooseAttackers(p) || CanChooseBlocks(p)))
        {
            p.ServerRemoveFromCombat(creature);
        }
    }

    [Server]
    public void ServerMoveToCombat(NetworkIdentity playerId, NetworkIdentity creature, int ind)
    {
        PlayerController p = playerId.gameObject.GetComponent<PlayerController>();

        if (IsGameReady() && p.CanMoveCreatures())
        {
            if (CanDeclareCombat(p))
            {
                combatWasDeclared = true;
                ChangeState(GameState.DECLARE_ATTACKS);
                p.ServerMoveToCombat(creature, ind, true);
            }
            else
            {
                p.ServerMoveToCombat(creature, ind, false);
            }
        }
    }

    public PlayerController GetActivePlayer()
    {
        if (IsGameReady())
        {
            return playerList[activeIndex];
        }
        return null;
    }

    public PlayerController GetWaitingOnPlayer()
    {
        if (IsGameReady())
        {
            return playerList[waitingIndex];
        }
        return null;
    }

    public Card GetPendingCard(PlayerController c)
    {
        if (IsGameReady() && pendingCard != null)
        {
            Card card = pendingCard.GetComponent<Card>();
            if (card.controller == c)
            {
                return card;
            }
        }
        return null;
    }

    public bool CanSelectTargets(PlayerController c)
    {
        if (IsGameReady())
        {
            return currState == GameState.SELECTING_TARGETS && IsWaitingOnPlayer(c);
        }
        return false;
    }
    public bool CanDeclareCombat(PlayerController c)
    {
        if (IsGameReady())
        {
            return !combatWasDeclared && currState == GameState.WAIT_ACTIVE && IsActivePlayer(c);
        }
        return false;
    }

    public bool CanChooseAttackers(PlayerController c)
    {
        if (IsGameReady())
        {
            return currState == GameState.DECLARE_ATTACKS && IsActivePlayer(c);
        }
        return false;
    }

    public bool CanChooseBlocks(PlayerController c)
    {
        if (IsGameReady())
        {
            return currState == GameState.DECLARE_BLOCKS && !IsActivePlayer(c);
        }
        return false;
    }

    public bool IsActivePlayer(PlayerController c)
    {
        if (IsGameReady())
        {
            return playerList[activeIndex] == c;
        }
        return false;
    }

    public bool IsWaitingOnPlayer(PlayerController c)
    {
        if (IsGameReady())
        {
            return playerList[waitingIndex] == c;
        }
        return false;
    }

    public PlayerController[] GetPlayerList()
    {
        return (PlayerController[]) playerList.Clone();
    }

    public PlayerController GetLocalPlayer()
    {
        foreach (PlayerController player in playerList)
        {
            if (player.isLocalPlayer)
            {
                return player;
            }
        }

        return null;
    }
    public List<PlayerController> GetOpponents(PlayerController c)
    {
        List<PlayerController> list = new List<PlayerController>(playerList);
        if (list.Contains(c))
        {
            list.Remove(c);
            return list;
        }

        return new List<PlayerController>();
    }

    public List<Targettable> GetPotentialTargets()
    {
        List<Targettable> targets = new List<Targettable>();
        foreach (AssignableAreas area in playerAreas)
        {
            targets.Add(area.playerUI);
            targets.AddRange(area.hand.GetTargettables());
            targets.AddRange(area.arena.GetTargettables());
        }

        return targets;
    }

}

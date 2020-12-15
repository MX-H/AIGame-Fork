using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class GameSession : NetworkBehaviour
{
    public enum GameState
    {
        NONE,
        INITIALIZE,
        WAIT_FOR_INITIALIZE,
        GAME_START,
        DECLARE_ATTACKS,
        DECLARE_BLOCKS,
        RESOLVING_COMBAT,
        TURN_START,
        TURN_END,
        WAIT_ACTIVE,
        WAIT_NON_ACTIVE,
        SELECTING_TARGETS,
        RESOLVING_EFFECTS,
        TRIGGERING_EFFECTS,
        GAME_OVER
    }

    public enum PendingType
    {
        PLAY_CARD,
        USE_TRAP,
        TRIGGER_EFFECT
    }

    private enum StateStatus
    {
        NONE,
        POP_STATE,
        PUSH_STATE,
        CHANGE_STATE,
        CHANGE_SUBSTATE
    }

    // Refactor
    Dictionary<GameState, IGameState> gameStates;
    Stack<IGameState> stateStack;

    GameState nextState;
    int nextSubstateIndex;
    StateStatus stateStatus;
    PlayerController localPlayer;
    Queue<IEvent> receivedEvents;

    [SyncVar]
    GameState currState;

    [SyncVar]
    GameState prevState;    // useful for debugging which state we came from

    [SyncVar]
    int activeIndex = 0;

    [SyncVar]
    int waitingIndex = 0;

    [SyncVar]
    bool combatWasDeclared = false;

    PlayerController[] playerList;

    int playerAcknowledgements;
    int playerPasses;

    [SyncVar]
    NetworkIdentity pendingCard;

    [SyncVar]
    PendingType pendingAction;

    [SyncVar]
    TriggerCondition pendingTriggerCondition;

    [SyncVar]
    int pendingTrapIndex;

    Queue<(Creature, Creature, TriggerCondition)> pendingTriggers;

    // Used to queue up triggers, triggers should be added on a game state update
    Queue<(Creature, TriggerCondition)> delayedTriggers;

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
    public EffectStack effectStack;
    public Effect effectPrefab;
    public CreatureModelIndex creatureModelIndex;

    public override void OnStartServer()
    {
        base.OnStartServer();
        currState = GameState.NONE;
        pendingTriggers = new Queue<(Creature, Creature, TriggerCondition)>();
        delayedTriggers = new Queue<(Creature, TriggerCondition)>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Create 2 players
        ClientScene.RegisterPrefab(effectPrefab.gameObject);
        GameUtils.SetGameSession(this);
        GameUtils.SetCreatureModelIndex(creatureModelIndex);
        SetupGameStates();

        receivedEvents = new Queue<IEvent>();
        stateStack = new Stack<IGameState>();
    }

    #region StateSwitching
    private void SetupGameStates()
    {
        gameStates = new Dictionary<GameState, IGameState>();
        gameStates.Add(GameState.INITIALIZE,            new GameStateInitialize(this));
        gameStates.Add(GameState.GAME_START,            new GameStateGameStart(this));
        gameStates.Add(GameState.TURN_START,            new GameStateTurnStart(this));
        gameStates.Add(GameState.WAIT_ACTIVE,           new GameStateWaitActivePlayer(this));
        gameStates.Add(GameState.WAIT_NON_ACTIVE,       new GameStateWaitNonActivePlayer(this));
        gameStates.Add(GameState.TURN_END,              new GameStateTurnEnd(this));
        gameStates.Add(GameState.DECLARE_ATTACKS,       new GameStateDeclareAttacks(this));
        gameStates.Add(GameState.DECLARE_BLOCKS,        new GameStateDeclareBlocks(this));
        gameStates.Add(GameState.RESOLVING_COMBAT,      new GameStateResolveCombat(this));
        gameStates.Add(GameState.SELECTING_TARGETS,     new GameStateSelectTargets(this));
        gameStates.Add(GameState.TRIGGERING_EFFECTS,    new GameStateTriggerEffects(this));
        gameStates.Add(GameState.RESOLVING_EFFECTS,     new GameStateResolveEffects(this));
        gameStates.Add(GameState.GAME_OVER,             new GameStateGameOver(this));
    }

    [Server]
    public void ServerPushState(GameState state)
    {
        nextState = state;
        stateStatus = StateStatus.PUSH_STATE;
    }

    [Server]
    public void ServerPopState()
    {
        stateStatus = StateStatus.POP_STATE;
    }

    [Server]
    public void ServerChangeState(GameState state)
    {
        nextState = state;
        stateStatus = StateStatus.CHANGE_STATE;
    }

    [Server]
    public void ServerChangeSubstate(int substate)
    {
        nextSubstateIndex = substate;
        stateStatus = StateStatus.CHANGE_SUBSTATE;
    }

    [ClientRpc]
    private void RpcStateChange(StateStatus status, GameState state, int substate)
    {
        Debug.Log("State Change: " + status.ToString() + " " + state.ToString() + " - " + substate);
        if (!isServer)
        {
            stateStatus = status;
            nextState = state;
            nextSubstateIndex = substate;
        }
    }

    #endregion

    #region Players
    [Server]
    public void ServerInitializePlayers(NetworkIdentity[] players)
    {
        if (isServerOnly)
        {
            playerList = new PlayerController[players.Length];
            for (int i = 0; i < playerList.Length; i++)
            {
                playerList[i] = players[i].GetComponent<PlayerController>();
            }

            for (int i = 0; i < players.Length; i++)
            {
                PlayerController p = playerList[i];
                playerAreas[i].playerUI.AssignPlayer(p);
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
    private void RpcInitializePlayers(NetworkIdentity[] players)
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
            playerAreas[i].playerUI.AssignPlayer(p);
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

        local.ClientRequestSendConfirmation();
    }
    public void SetLocalPlayer(PlayerController playerController)
    {
        localPlayer = playerController;
    }

    [Server]
    public void SetActivePlayerIndex(int index)
    {
        activeIndex = index;
    }

    public int GetActivePlayerIndex()
    {
        return activeIndex;
    }

    public int GetNonActivePlayerIndex()
    {
        return (activeIndex + 1) % GetMaxPlayers();
    }

    [Server]
    public void SetWaitingPlayerIndex(int index)
    {
        waitingIndex = index;
    }

    public int GetWaitingPlayerIndex()
    {
        return waitingIndex;
    }

    public PlayerController[] GetPlayerList()
    {
        return (PlayerController[])playerList.Clone();
    }

    public PlayerController GetLocalPlayer()
    {
        return localPlayer;
    }

    public PlayerController GetActivePlayer()
    {
        if (IsGameReady())
        {
            return playerList[GetActivePlayerIndex()];
        }
        return null;
    }

    public PlayerController GetNonActivePlayer()
    {
        if (IsGameReady())
        {
            return playerList[GetNonActivePlayerIndex()];
        }
        return null;
    }

    public PlayerController GetWaitingOnPlayer()
    {
        if (IsGameReady())
        {
            return playerList[GetWaitingPlayerIndex()];
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

    #endregion


    // Don't handle events immediately, we don't want to process events during state updates or other logic
    [Server]
    public void HandleEvent(IEvent eventInfo)
    {
        receivedEvents.Enqueue(eventInfo);
    }

    public bool IsGameReady()
    {
        return currState != GameState.INITIALIZE && currState != GameState.WAIT_FOR_INITIALIZE && currState != GameState.NONE;
    }

    [Server]
    public void SetCombatDeclared(bool declared)
    {
        combatWasDeclared = declared;
    }

    public bool WasCombatDeclared()
    {
        return combatWasDeclared;
    }

    [Server]
    public void ServerPassPriority()
    {
        playerPasses++;
    }

    public int GetPriorityPasses()
    {
        return playerPasses;
    }

    [Server]
    public void ResetPriorityPasses()
    {
        playerPasses = 0;
    }

    public PendingType GetPendingActionType()
    {
        return pendingAction;
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

    public int GetPendingTrapPosition()
    {
        return pendingTrapIndex;
    }

    public TriggerCondition GetPendingTriggerCondition()
    {
        return pendingTriggerCondition;
    }


    [Server]
    public Queue<(Creature, Creature, TriggerCondition)> GetPendingTriggers()
    {
        return pendingTriggers;
    }

    [Server]
    public void ServerTriggerEffects(Creature source, TriggerCondition triggerCondition)
    {
        delayedTriggers.Enqueue((source, triggerCondition));
    }

    [Server]
    private void ServerProcessTriggerEffects(Creature source, TriggerCondition trigger)
    {
        // Creature effects trigger, so poll available creatures

        List<Creature> creaturesToTrigger = new List<Creature>();

        // Active triggers first
        for (int i = 0; i < playerList.Length; i++)
        {
            PlayerController player = playerList[(activeIndex + i) % playerList.Length];
            creaturesToTrigger.AddRange(player.arena.GetAllCreatures());
        }

        switch (trigger)
        {
            case TriggerCondition.ON_CREATURE_ENTER:
                // Look at all creatures other than the source
                foreach (Creature creature in creaturesToTrigger)
                {
                    if (creature != source)
                    {
                        if (creature.card.cardData.HasEffectsOnTrigger(trigger) && creature.card.HasValidTargets(creature.card.cardData.GetSelectableTargets(trigger)))
                        {
                            pendingTriggers.Enqueue((creature, source, trigger));
                        }
                    }
                }
                break;
            case TriggerCondition.ON_SELF_DIES:
                // Add self trigger first
                if (source.card.cardData.HasEffectsOnTrigger(trigger) && source.card.HasValidTargets(source.card.cardData.GetSelectableTargets(trigger)))
                {
                    pendingTriggers.Enqueue((source, source, trigger));
                }

                // Then add triggers from others
                foreach (Creature creature in creaturesToTrigger)
                {
                    if (creature.card != source)
                    {
                        if (creature.card.cardData.HasEffectsOnTrigger(TriggerCondition.ON_CREATURE_DIES) &&
                            creature.card.HasValidTargets(creature.card.cardData.GetSelectableTargets(TriggerCondition.ON_CREATURE_DIES)))
                        {
                            pendingTriggers.Enqueue((creature, source, TriggerCondition.ON_CREATURE_DIES));
                        }
                    }
                }
                break;
            case TriggerCondition.ON_SELF_DAMAGE_DEALT_TO_PLAYER:
            case TriggerCondition.ON_SELF_DAMAGE_TAKEN:
                if (source.card.cardData.HasEffectsOnTrigger(trigger) && source.card.HasValidTargets(source.card.cardData.GetSelectableTargets(trigger)))
                {
                    pendingTriggers.Enqueue((source, source, trigger));
                }
                break;
        }

        if (pendingTriggers.Count > 0)
        {
            ServerPushState(GameState.TRIGGERING_EFFECTS);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isServer && stateStack.Count == 0 && currState == GameState.NONE)
        {
            if (FindObjectsOfType<PlayerController>().Length == GetMaxPlayers())
            {
                ServerPushState(GameState.INITIALIZE);
            }
        }

        bool stateChanged = false;

        switch (stateStatus)
        {
            case StateStatus.POP_STATE:
                {
                    IGameState currGameState = stateStack.Pop();
                    currGameState.OnExit();

                    if (isServer)
                    {
                        prevState = currGameState.GetStateType();
                    }

                    if (stateStack.Count > 0)
                    {
                        currGameState = stateStack.Peek();
                        currGameState.OnResume();

                        if (isServer)
                        {
                            currState = currGameState.GetStateType();
                        }
                    }
                    else
                    {
                        if (isServer)
                        {
                            currState = GameState.NONE;
                        }
                    }

                    stateChanged = true;
                    break;
                }
            case StateStatus.PUSH_STATE:
                {
                    if (stateStack.Count > 0)
                    {
                        IGameState currGameState = stateStack.Peek();
                        currGameState.OnSuspend();

                        if (isServer)
                        {
                            prevState = currGameState.GetStateType();
                        }
                    }
                    IGameState nextGameState = gameStates[nextState];
                    nextGameState.OnEnter();
                    stateStack.Push(nextGameState);

                    if (isServer)
                    {
                        currState = nextGameState.GetStateType();
                    }

                    stateChanged = true;
                    break;
                }
            case StateStatus.CHANGE_STATE:
                {
                    IGameState currGameState = stateStack.Pop();
                    currGameState.OnExit();

                    IGameState nextGameState = gameStates[nextState];
                    nextGameState.OnEnter();
                    stateStack.Push(nextGameState);

                    if (isServer)
                    {
                        prevState = currGameState.GetStateType();
                        currState = nextGameState.GetStateType();
                    }

                    stateChanged = true;
                    break;
                }
            case StateStatus.CHANGE_SUBSTATE:
                {
                    IGameState currState = stateStack.Peek();
                    if (currState.GetSubstateIndex() != nextSubstateIndex)
                    {
                        currState.SetSubstate(nextSubstateIndex);
                    }
                    stateChanged = true;
                    break;
                }
        }

        if (stateChanged && isServer)
        {
            RpcStateChange(stateStatus, nextState, nextSubstateIndex);
            ServerSyncTimer();
        }

        if (stateStack.Count > 0)
        {
            stateStatus = StateStatus.NONE;
            nextState = GameState.NONE;
            nextSubstateIndex = 0;

            IGameState currState = stateStack.Peek();
            currState.Update(Time.deltaTime);

            // Copy over the list of events so handling events doesn't add events to get processed on the same frame
            // This also allows us to forward events to the next frame which might be a state change
            Queue<IEvent> events = new Queue<IEvent>(receivedEvents);
            receivedEvents.Clear();

            while (events.Count > 0)
            {
                currState.HandleEvent(events.Dequeue());
            }
        }
    }

    [Server]
    public void ServerSyncTimer()
    {
        if (IsGameReady())
        {
            PlayerController p = playerList[activeIndex];
            TurnTimer turnTimer = GameUtils.GetTurnTimer();
            p.RpcSyncTimer(turnTimer.turnTime, turnTimer.currTurnTime, turnTimer.timerText.text);
        }
    }


    [Server]
    public void ServerCreatureDoDamage(Creature source, int amount, Targettable target)
    {
        if (amount > 0)
        {
            ServerApplyDamage(target, amount);

            Creature targetCreature = target as Creature;
            if (targetCreature && source.HasKeyword(KeywordAttribute.PIERCING))
            {
                int excess = -targetCreature.creatureState.GetHealth();
                if (excess > 0)
                {
                    ServerCreatureDoDamage(source, excess, targetCreature.controller);
                }
            }

            if (target is PlayerController)
            {
                ServerTriggerEffects(source, TriggerCondition.ON_SELF_DAMAGE_DEALT_TO_PLAYER);
            }
        }
    }


    [Server]
    public void ServerPlayerDrawCard(PlayerController player, PlayerController srcPlayer, CardGenerationFlags flags = CardGenerationFlags.NONE)
    {
        Card card = ServerCreateCard(player);
        player.ServerAddCardToHand(srcPlayer, card, (int)(UnityEngine.Random.value * Int32.MaxValue), flags);

        CardDrawnEvent cardDrawEvent = new CardDrawnEvent(player, card);
        HandleEvent(cardDrawEvent);
    }

    [Server]
    public void ServerRemoveEffect(Effect effect)
    {
        effectStack.RemoveEffect(effect);
        RpcRemoveEffect(effect.netIdentity);
    }

    [ClientRpc]
    private void RpcRemoveEffect(NetworkIdentity effectId)
    {
        if (!isServer)
        {
            effectStack.RemoveEffect(effectId.GetComponent<Effect>());
        }
    }

    [Server]
    public Card ServerCreateCard(PlayerController p)
    {
        if (IsGameReady())
        {
            Card card = Instantiate(p.cardPrefab);
            NetworkServer.Spawn(card.gameObject);
            return card;
        }
        return null;
    }

    [Server]
    public Creature ServerCreateCreature(PlayerController p, Card card)
    {
        if (IsGameReady())
        {
            Creature creature = Instantiate(p.creaturePrefab);
            NetworkServer.Spawn(creature.gameObject);
            creature.SetCard(card);
            creature.GetComponent<CreatureState>().ServerInitialize();
            return creature;
        }
        return null;
    }

    [Server]
    public void ServerCreateToken(PlayerController player, CreatureType creatureType)
    {
        if (IsGameReady())
        {
            Card card = ServerCreateCard(player);
            card.cardData = new CardInstance(GameUtils.GetCreatureModelIndex().GetToken(creatureType));
            Creature creature = ServerCreateCreature(player, card);
            player.ServerPlayToken(card, creature, creatureType);
        }
    }

    [Server]
    private NetworkIdentity ServerCreateEffect()
    {
        if (IsGameReady())
        {
            Effect e = Instantiate(effectPrefab);
            NetworkServer.Spawn(e.gameObject);
            return e.netIdentity;
        }
        return null;
    }


    [Server]
    public void ServerApplyDamage(Targettable target, int amount)
    {
        if (amount > 0)
        {
            PlayerController player = target as PlayerController;
            if (player)
            {
                player.ServerDoDamage(amount);
            }

            Creature creature = target as Creature;
            if (creature)
            {
                creature.creatureState.ServerDoDamage(amount);
                ServerTriggerEffects(creature, TriggerCondition.ON_SELF_DAMAGE_TAKEN);
            }
        }
    }

    [Server]
    public void ServerHealDamage(Targettable target, int amount)
    {
        PlayerController player = target as PlayerController;
        if (player)
        {
            player.ServerHealDamage(amount);
        }

        Creature creature = target as Creature;
        if (creature)
        {
            creature.creatureState.ServerHealDamage(amount);
        }
    }

    [Server]
    public Effect ServerPopStack()
    {
        Effect effect = effectStack.PopEffect();
        effect.gameObject.SetActive(false);

        RpcPopStack();
        return effect;
    }

    [ClientRpc]
    public void RpcPopStack()
    {
        if (!isServer)
        {
            Effect effect = effectStack.PopEffect();
            effect.gameObject.SetActive(false);
        }
    }

    [Server]
    public void ServerUpdateGameState()
    {
        List<Creature> creaturesToUpdate = new List<Creature>();
        List<Creature> creaturesToTrigger = new List<Creature>();

        // Process active players creatures first
        for (int i = 0; i < playerList.Length; i++)
        {
            PlayerController player = playerList[(activeIndex + i) % playerList.Length];
            creaturesToUpdate.AddRange(player.arena.GetAllCreatures());
        }

        foreach (Creature creature in creaturesToUpdate)
        {
            if (creature.creatureState.IsDead())
            {
                // Remove the creature
                creature.controller.ServerDestroyCreature(creature.netIdentity);
                creaturesToTrigger.Add(creature);
            }
        }

        foreach (Creature creature in creaturesToTrigger)
        {
            ServerTriggerEffects(creature, TriggerCondition.ON_SELF_DIES);
        }

        ServerCheckGameState();

        if (currState != GameState.GAME_OVER)
        {
            while (delayedTriggers.Count > 0)
            {
                (Creature source, TriggerCondition trigger) = delayedTriggers.Dequeue();
                ServerProcessTriggerEffects(source, trigger);
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
        CheckGameOver(loser);
    }

    [Server]
    public void ServerSurrender(NetworkIdentity id)
    {
        PlayerController loser = null;
        foreach (PlayerController p in playerList)
        {
            if (p.netIdentity == id)
            {
                loser = p;
            }
        }
        CheckGameOver(loser);
    }

    private void CheckGameOver(PlayerController loser) {
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
            ServerChangeState(GameState.GAME_OVER);
        }
    }

    [Server]
    public void ServerAddEffectToStack(Card source, TriggerCondition trigger, NetworkIdentity[] flattenedTargets, int[] indexes)
    {
        NetworkIdentity effectId = ServerCreateEffect();
        Effect effect = effectId.GetComponent<Effect>();

        // Reconstruct jagged arrays
        Targettable[][] targets = new Targettable[indexes.Length][];
        int ind = 0;
        for (int i = 0; i < indexes.Length; i++)
        {
            targets[i] = new Targettable[indexes[i]];
            for (int j = 0; j < indexes[i]; j++, ind++)
            {
                targets[i][j] = flattenedTargets[ind].GetComponent<Targettable>();
            }
        }

        effect.SetData(source, trigger, targets);
        effectStack.PushEffect(effect);

        RpcAddEffectToStackWithTargets(effectId, source.netIdentity, trigger, flattenedTargets, indexes);
    }

    [Server]
    public void ServerAddEffectToStack(Card source, TriggerCondition trigger)
    {
        NetworkIdentity effectId = ServerCreateEffect();
        Effect effect = effectId.GetComponent<Effect>();

        effect.SetData(source.GetComponent<Card>(), trigger);
        effectStack.PushEffect(effect);

        RpcAddEffectToStack(effectId, source.netIdentity, trigger);
    }

    [ClientRpc]
    public void RpcAddEffectToStackWithTargets(NetworkIdentity effectId, NetworkIdentity source, TriggerCondition trigger, NetworkIdentity[] flattenedTargets, int[] indexes)
    {
        if (!isServer)
        {
            Effect effect = effectId.GetComponent<Effect>();

            // Reconstruct jagged arrays
            Targettable[][] targets = new Targettable[indexes.Length][];
            int ind = 0;
            for (int i = 0; i < indexes.Length; i++)
            {
                targets[i] = new Targettable[indexes[i]];
                for (int j = 0; j < indexes[i]; j++, ind++)
                {
                    targets[i][j] = flattenedTargets[ind].GetComponent<Targettable>();
                }
            }

            effect.SetData(source.GetComponent<Card>(), trigger, targets);
            effectStack.PushEffect(effect);
        }
    }

    [ClientRpc]
    public void RpcAddEffectToStack(NetworkIdentity effectId, NetworkIdentity source, TriggerCondition trigger)
    {
        if (!isServer)
        {
            Effect effect = effectId.GetComponent<Effect>();
            effect.SetData(source.GetComponent<Card>(), trigger);
            effectStack.PushEffect(effect);
        }
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

    public bool IsStackEmpty()
    {
        if (effectStack)
        {
            return effectStack.IsEmpty();
        }
        return false;
    }

    public bool IsStackFull()
    {
        if (effectStack)
        {
            return effectStack.IsFull();
        }
        return true;
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

    [Server]
    public void StartSelectingTargets(Card card, PlayerController controller, TriggerCondition trigger)
    {
        if (card != null)
        {
            pendingCard = card.netIdentity;
            pendingTrapIndex = controller.arena.GetTrapIndex(card);
            pendingTriggerCondition = trigger;

            if (pendingTrapIndex >= 0)
            {
                pendingAction = PendingType.USE_TRAP;
            }
            else if (trigger == TriggerCondition.ON_SELF_ENTER || trigger == TriggerCondition.NONE)
            {
                pendingAction = PendingType.PLAY_CARD;
            }
            else
            {
                pendingAction = PendingType.TRIGGER_EFFECT;
            }

            for (int i = 0; i < playerList.Length; i++)
            {
                if (playerList[i] == controller)
                {
                    waitingIndex = i;
                }
            }

            controller.TargetNotifySelectTargets(controller.connectionToClient);
            ServerPushState(GameState.SELECTING_TARGETS);
        }
        else
        {
            pendingCard = null;
            pendingTrapIndex = -1;
            pendingTriggerCondition = TriggerCondition.NONE;
        }
    }

    public bool IsGameOverState()
    {
        return currState == GameState.GAME_OVER;
    }

    public int GetMaxPlayers()
    {
        return playerAreas.Length;
    }
}

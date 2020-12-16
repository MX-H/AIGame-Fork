using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Card : Targettable
{
    public CardInstance cardData;

    public bool selected = false;
    public bool dragging = false;
    public bool hovering = false;
    public bool isRevealed = false;

    public bool isDraggable = true;

    public bool selectorOption = false;

    public Vector3 currMousePos;

    private Vector3 savedPosition;
    private Quaternion savedRotation;
    private Vector3 savedScale;

    public PlayerController owner;
    public PlayerController controller;
    public Hand context;


    protected override void Start()
    {
        base.Start();
        SaveTransform();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (dragging)
        {
            transform.position = currMousePos;
            Debug.DrawRay(currMousePos, Camera.main.transform.forward * 100, Color.red);

        }
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();

        if (selectorOption)
        {
            if (isRevealed && owner.IsWaitingOnPlayer() && owner.isLocalPlayer)
            {
                owner.ClientRequestCardSelection(cardData.srcPlayer, cardData.cardSeed, cardData.cardFlags);
            }
        }
        else if (isDraggable && isRevealed && context != null && !controller.IsSelectingTargets())
        {
            dragging = true;
            currMousePos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(0, 0, transform.position.z - Camera.main.transform.position.z));

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            hovering = false;
        }
    }

    void OnMouseDrag()
    {
        if (dragging)
        {
            currMousePos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(0, 0, GameConstants.Z_LAYERS.HOVER_LAYER - Camera.main.transform.position.z));
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Camera.main.transform.up);
        }
    }

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        if (!IsInteracting() && isRevealed)
        {
            hovering = true;
            SaveTransform();

            HoverZoom();
        }
    }

    void OnMouseUp()
    {
        if (dragging)
        {
            if (transform.position.y > GameConstants.Y_LAYERS.PLAY_LEVEL && IsTargettable() && controller.CanPlayCards())
            {
                controller.ClientRequestPlayCard(this);
            }
            dragging = false;
        }
    }

    void OnMouseOver()
    {

    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        if (hovering)
        {
            hovering = false;
            RestoreTransform();
        }
    }

    public bool IsInteracting()
    {
        return selected || dragging || hovering || selectorOption;
    }

    public void HoverZoom()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Camera.main.transform.up);
        transform.position += transform.forward * ((GameConstants.Z_LAYERS.HOVER_LAYER - transform.position.z) / transform.forward.z);
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);

        Renderer rend = GetComponent<Renderer>();
        Vector3 extents = rend.bounds.extents;
        Vector3 bottomBound = rend.bounds.center - new Vector3(0, extents.y, extents.z);
        Vector3 topBound = rend.bounds.center - new Vector3(0, -extents.y, extents.z);

        float distance = Vector3.Dot((bottomBound - Camera.main.transform.position), Camera.main.transform.forward);
        float frustumHalfHeight = distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Vector3 camCenter = distance * Camera.main.transform.forward + Camera.main.transform.position;

        // Keep hover zoom-in within camera bounds
        if (Mathf.Abs(Vector3.Dot(bottomBound, Camera.main.transform.up)) > frustumHalfHeight)
        {
            Vector3 camBottom = camCenter - Camera.main.transform.up * frustumHalfHeight;
            transform.position += Vector3.Dot((camBottom - bottomBound), Camera.main.transform.up) * Camera.main.transform.up;
        }
        else if (Mathf.Abs(Vector3.Dot(topBound, Camera.main.transform.up)) > frustumHalfHeight)
        {
            Vector3 camTop = camCenter + Camera.main.transform.up * frustumHalfHeight;
            transform.position += Vector3.Dot((camTop - topBound), Camera.main.transform.up) * Camera.main.transform.up;
        }
    }

    public void HoverZoomToCam()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        screenPos.z = 0.0f;
        Vector3 pos = Camera.main.ScreenToWorldPoint(
            screenPos + new Vector3(0, 0, GameConstants.Z_LAYERS.HOVER_LAYER - Camera.main.transform.position.z));
        transform.position = pos;
        transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward, Camera.main.transform.up);

        Renderer rend = GetComponent<Renderer>();
        Vector3 extents = rend.bounds.extents;
        Vector3 bottomBound = rend.bounds.center - new Vector3(0, extents.y, extents.z);
        Vector3 topBound = rend.bounds.center - new Vector3(0, -extents.y, extents.z);

        float distance = Vector3.Dot((bottomBound - Camera.main.transform.position), Camera.main.transform.forward);
        float frustumHalfHeight = distance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Vector3 camCenter = distance * Camera.main.transform.forward + Camera.main.transform.position;

        // Keep hover zoom-in within camera bounds
        if (Mathf.Abs(Vector3.Dot(bottomBound, Camera.main.transform.up)) > frustumHalfHeight)
        {
            Vector3 camBottom = camCenter - Camera.main.transform.up * frustumHalfHeight;
            transform.position += Vector3.Dot((camBottom - bottomBound), Camera.main.transform.up) * Camera.main.transform.up;
        }
        else if (Mathf.Abs(Vector3.Dot(topBound, Camera.main.transform.up)) > frustumHalfHeight)
        {
            Vector3 camTop = camCenter + Camera.main.transform.up * frustumHalfHeight;
            transform.position += Vector3.Dot((camTop - topBound), Camera.main.transform.up) * Camera.main.transform.up;
        }
    }

    private void SaveTransform()
    {
        savedPosition = transform.localPosition;
        savedRotation = transform.localRotation;
        savedScale = transform.localScale;
    }

    private void RestoreTransform()
    {
        transform.localPosition = savedPosition;
        transform.localRotation = savedRotation;
        transform.localScale = savedScale;
    }

    public override bool IsTargettable()
    {
        if (controller)
        {
            if (controller.CanPlayCards() && controller.isLocalPlayer && context != null)
            {
                if (cardData != null && cardData.GetManaCost() <= owner.currMana)
                {
                    switch (cardData.GetCardType())
                    {
                        case CardType.SPELL:
                            return HasValidTargets(cardData.GetSelectableTargets(TriggerCondition.NONE));
                        case CardType.CREATURE:
                        case CardType.TRAP:
                            return true;
                    }
                    return true;
                }
            }
        }
        else if (owner)
        {
            if (owner.isLocalPlayer && selectorOption)
            {
                return true;
            }
        }
        return false;
    }

    public bool HasValidTargets(List<ITargettingDescription> targets)
    {
        List<Targettable> potentialTargets = FindObjectOfType<GameSession>().GetPotentialTargets();

        foreach (ITargettingDescription desc in targets)
        {
            ITargettingDescription descToCheck = desc;

            if (desc.targettingType == TargettingType.EXCEPT)
            {
                ExceptTargetDescription exceptDesc = (ExceptTargetDescription)desc;
                descToCheck = exceptDesc.targetDescription;
            }

            int targetsNeeded = 0;

            switch (descToCheck.targettingType)
            {
                case TargettingType.TARGET:
                    {
                        TargetXDescription targetDesc = (TargetXDescription)descToCheck;
                        targetsNeeded = targetDesc.amount;
                    }
                    break;
                case TargettingType.UP_TO_TARGET:
                    // Up to is valid for 0 so we can skip uneccessary checks
                    continue;
            }

            TargettingQuery query = new TargettingQuery(desc, controller);

            foreach (Targettable t in potentialTargets)
            {
                if (t.IsTargettable(query))
                {
                    targetsNeeded -= 1;
                    if (targetsNeeded <= 0)
                    {
                        break;
                    }
                }
            }

            if (targetsNeeded > 0)
            {
                return false;
            }
        }

        return true;
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
            case TargetType.CARDS:
                valid = true;
                break;
            case TargetType.CREATURE_CARDS:
                valid = cardData.GetCardType() == CardType.CREATURE;
                break;
            case TargetType.SPELL_CARDS:
                valid = cardData.GetCardType() == CardType.SPELL;
                break;
            case TargetType.TRAP_CARDS:
                valid = cardData.GetCardType() == CardType.TRAP;
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
                        case QualifierType.CREATURE_TYPE:
                            {
                                CreatureTypeQualifierDescription creatureQualifier = (CreatureTypeQualifierDescription)qualifier;
                                valid = creatureQualifier.creatureType == cardData.GetCreatureType();
                            }
                            break;
                        case QualifierType.CARD_TYPE:
                            {
                                CardTypeQualifierDescription cardTypeQualifier = (CardTypeQualifierDescription)qualifier;
                                valid = cardTypeQualifier.cardType == cardData.GetCardType();
                            }
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
}

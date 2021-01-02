using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

[RequireComponent(typeof(CreatureState))]
public class Creature : Targettable
{
    // Start is called before the first frame update
    public Card card;
    public bool dragging = false;
    public bool hovering = false;
    public Vector3 currMousePos;
    public Image creatureImage;

    public PlayerController owner;
    public PlayerController controller;
    public Arena context;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI hpText;

    public CreatureState creatureState;

    protected override void Start()
    {
        base.Start();
        if (card)
        {
            SetCard(card);
        }
        creatureState = gameObject.GetComponent<CreatureState>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (dragging)
        {
            transform.position = currMousePos;
        }

        if (card != null && card.cardData != null)
        {
            int atk = creatureState.GetAttack();
            int baseAtk = creatureState.GetBaseAttack();

            int hp = creatureState.GetHealth();
            int maxHp = creatureState.GetMaxHealth();
            int baseHp = creatureState.GetBaseHealth();

            atkText.text = atk.ToString();
            if (atk > baseAtk)
            {
                atkText.text = "<color=green>" + atkText.text + "</color>";
            }
            else if (atk < baseAtk)
            {
                atkText.text = "<color=yellow>" + atkText.text + "</color>";
            }

            hpText.text = hp.ToString();
            if (hp < maxHp)
            {
                hpText.text = "<color=yellow>" + hpText.text + "</color>";
            }
            else if (maxHp > baseHp)
            {
                hpText.text = "<color=green>" + hpText.text + "</color>";
            }
        }
        else
        {
            atkText.text = "0";
            hpText.text = "0";
        }
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        if (IsDraggable())
        {
            dragging = true;
            currMousePos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(0, 0, transform.position.z - Camera.main.transform.position.z));

            if (card)
            {
                hovering = false;
                card.gameObject.SetActive(false);
            }
        }
    }

    void OnMouseDrag()
    {
        if (dragging && IsDraggable())
        {
            currMousePos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(0, 0, GameConstants.Z_LAYERS.CREATURE_DRAG_LAYER - Camera.main.transform.position.z));
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position, Camera.main.transform.up);

        }
    }

    void OnMouseUp()
    {
        if (dragging)
        {
            if (context)
            {
                context.DropCreature(this);
            }
            dragging = false;
        }
    }

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        if (card)
        {
            card.HoverZoomToCam();
            hovering = true;
            card.gameObject.SetActive(true);
        }
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        if (hovering)
        {
            hovering = false;
            card.transform.localPosition = new Vector3(-1.5f, 0.0f, 0.0f);
            card.gameObject.SetActive(false);
        }
    }

    public void SetCard(Card c)
    {
        card = c;
        c.transform.SetParent(transform);
        c.transform.localPosition = new Vector3(-1.5f, 0.0f, 0.0f);
        c.gameObject.SetActive(false);
        if (c.cardData.GetImage())
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(GameConstants.PATHS.CARD_IMAGES + c.cardData.GetImage().name);
            creatureImage.sprite = sprites[0];
        }
    }

    public bool HasKeyword(KeywordAttribute keyword)
    {
        if (card)
        {
            return card.cardData.HasKeywordAttribute(keyword);
        }
        return false;
    }

    public override bool IsTargettable()
    {
        return IsDraggable();
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
            case TargetType.CREATURES:
            case TargetType.PERMANENT:
            case TargetType.DAMAGEABLE:
                valid = true;
                break;
        }

        if (!targetQuery.ignoreUntouchable && HasKeyword(KeywordAttribute.UNTOUCHABLE))
        {
            valid = false;
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
                                valid = creatureQualifier.creatureType == card.cardData.GetCreatureType();
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

    public bool IsDraggable()
    {
        if (controller)
        {
            if (controller.isLocalPlayer && controller.CanMoveCreatures())
            {
                if (context.GetState() != Arena.State.BLOCKING)
                {
                    return !creatureState.IsSummoningSick();
                }
                return true;
            }
        }
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : Targettable
{
    public GameObject trapVisual;
    public Card card;
    public bool hovering = false;

    public PlayerController owner;
    public PlayerController controller;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        if (card)
        {
            SetCard(card);
        }
        else
        {
            trapVisual.SetActive(false);
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

    }

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        if (card && card.isRevealed)
        {
            card.HoverZoom();
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
            card.gameObject.SetActive(false);
        }
    }

    public void SetCard(Card c)
    {
        card = c;
        c.transform.SetParent(transform);
        c.transform.localPosition = new Vector3(-2.0f, 0.0f, 0.0f);
        c.gameObject.SetActive(false);
        trapVisual.SetActive(true);
    }

    public bool IsActive()
    {
        return card;
    }

    public override bool IsTargettable()
    {
        return false;
    }

    public override bool IsTargettable(TargettingQuery targetQuery)
    {
        bool valid = false;

        if (IsActive())
        {
            ITargettingDescription desc = targetQuery.targettingDesc;
            if (desc.targettingType == TargettingType.EXCEPT)
            {
                ExceptTargetDescription exceptDesc = (ExceptTargetDescription)desc;
                desc = exceptDesc.targetDescription;
            }

            switch (desc.targetType)
            {
                case TargetType.PERMANENT:
                case TargetType.SET_TRAPS:
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
        }
        return valid;
    }
}

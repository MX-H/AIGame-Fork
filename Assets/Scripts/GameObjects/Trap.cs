using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : Targettable
{
    public GameObject trapVisual;
    public Card card;
    public bool hovering = false;
    public bool dragging = false;

    private Vector3 currMousePos;
    private Vector3 startDragPos;

    public PlayerController owner;

    private float originalY;
    public float maxYOffset;
    public float activateYThreshold;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        SetCard(card);
        originalY = transform.position.y;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (dragging)
        {
            // We only care about translating in the y direction and we clamp the values

            float yChange = currMousePos.y - startDragPos.y;

            if (yChange < 0)
            {
                yChange = 0;
            }
            else if (yChange > maxYOffset)
            {
                yChange = maxYOffset;
            }

            transform.position = new Vector3(transform.position.x, originalY + yChange, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, originalY, transform.position.z);
        }
    }

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        if (card && card.isRevealed && !dragging)
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

    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        if (IsTargettable())
        {
            dragging = true;
            currMousePos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(0, 0, transform.position.z - Camera.main.transform.position.z));
            startDragPos = currMousePos;

            hovering = false;
            card.gameObject.SetActive(false);
        }
    }

    void OnMouseDrag()
    {
        if (dragging && IsTargettable())
        {
            currMousePos = Camera.main.ScreenToWorldPoint(
                Input.mousePosition + new Vector3(0, 0, transform.position.z - Camera.main.transform.position.z));
        }
    }

    void OnMouseUp()
    {
        if (dragging)
        {
            dragging = false;

            if (IsTargettable() && transform.position.y >= (originalY + activateYThreshold))
            {
                owner.ClientRequestActivateTrap(card);
            }
        }
    }

    public void SetCard(Card c)
    {
        if (card != null)
        {
            card.gameObject.SetActive(false);
            card.ResetTrapContext();
        }

        card = c;
        if (c != null)
        {
            c.transform.SetParent(transform);
            c.transform.localPosition = new Vector3(2.0f, 0.0f, 0.0f);
            c.gameObject.SetActive(false);
            trapVisual.SetActive(true);
            card.SetTrapContext(this);
        }
        else
        {
            trapVisual.SetActive(false);
        }
    }

    public bool IsActive()
    {
        return card != null;
    }

    public override bool IsTargettable()
    {
        if (owner && IsActive())
        {
            if (owner.isLocalPlayer && owner.CanActivateTraps() && card.HasValidTargets(card.cardData.GetSelectableTargets(TriggerCondition.NONE)))
            {
                return true;
            }
        }
        return false;
    }

    public override bool IsTargettable(TargettingQuery targetQuery)
    {
        bool valid = false;

        if (IsActive())
        {
            valid = GetTargettableEntity().IsTargettable(targetQuery);
        }
        return valid;
    }

    public override Targettable GetTargettableEntity()
    {
        return card;
    }

    public override Alignment GetAlignmentToPlayer(PlayerController player)
    {
        return GetTargettableEntity().GetAlignmentToPlayer(player);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
public abstract class Targettable : NetworkBehaviour
{
    public bool isTargettable;
    public bool isSelected;
    public TextMeshProUGUI text;

    private OutlineController outline;
    private TargettingQuery targettingQuery;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        outline = gameObject.GetComponent<OutlineController>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        TargettingQuery query = GetTargettingQuery();
        bool targettable = IsTargettable() || (query != null && IsTargettable(query));
        SetTargettable(targettable);

        if (outline)
        {
            outline.SetOutline1(isTargettable || isSelected);
            if (isSelected)
            {
                outline.UseSelectColor();
            }
            else if (isTargettable)
            {
                outline.UseTargetColor();
            }
        }
    }

    protected virtual void OnMouseEnter()
    {
        if (isTargettable && outline)
        {
            outline.SetOutline2(true);
        }
    }

    protected virtual void OnMouseExit()
    {
        if (outline && !isSelected)
        {
            outline.SetOutline2(false);
        }
    }

    protected virtual void OnMouseDown()
    {
        if (isSelected)
        {
            PlayerController localPlayer = FindObjectOfType<GameSession>().GetLocalPlayer();

            if (localPlayer != null)
            {
                localPlayer.RemoveTarget(GetTargettableEntity());
            }
            isSelected = false;
        }
        else if (targettingQuery != null && IsTargettable(targettingQuery))
        {
            PlayerController localPlayer = FindObjectOfType<GameSession>().GetLocalPlayer();
            if (localPlayer != null && localPlayer.CanSelectMoreTargets())
            {
                isSelected = true;
                localPlayer.AddTarget(GetTargettableEntity());
            }
        }
    }

    public void Select()
    {
        isSelected = true;
    }

    public void Deselect()
    {
        isSelected = false;
    }

    public abstract bool IsTargettable();
    public abstract bool IsTargettable(TargettingQuery targetQuery);

    public abstract Alignment GetAlignmentToPlayer(PlayerController player);
    public void SetTargettingQuery(TargettingQuery targettingDesc)
    {
        targettingQuery = targettingDesc;
    }

    public void ResetTargettingQuery()
    {
        targettingQuery = null;
    }

    public void SetText(string msg)
    {
        if (text)
        {
            text.text = msg;
        }
    }

    public void AddText(string msg)
    {
        if (text)
        {
            text.text += msg;
        }
    }

    public void ClearText()
    {
        SetText("");
    }

    public TargettingQuery GetTargettingQuery()
    {
        return targettingQuery;
    }

    public virtual Targettable GetTargettableEntity()
    {
        return this;
    }

    public virtual Targettable GetTargettableUI()
    {
        return this;
    }

    private void SetTargettable(bool targettable)
    {
        isTargettable = targettable;
    }
}

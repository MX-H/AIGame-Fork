using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Targettable : MonoBehaviour
{
    public OutlineController outline;
    public bool isTargettable;
    // Start is called before the first frame update
    protected virtual void Start()
    {
        outline = gameObject.GetComponent<OutlineController>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        SetTargettable(IsTargettable());
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
        if (outline)
        {
            outline.SetOutline2(false);
        }
    }

    public abstract bool IsTargettable();

    private void SetTargettable(bool targettable)
    {
        isTargettable = targettable;
        if (outline)
        {
            outline.SetOutline1(targettable);
        }
    }
}

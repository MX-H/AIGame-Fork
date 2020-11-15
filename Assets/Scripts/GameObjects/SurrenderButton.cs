using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SurrenderButton : Targettable
{
    public PlayerController localPlayer;

    protected override void Start()
    {
        base.Start();
    }

    public override bool IsTargettable()
    {
        return true;
    }

    public override bool IsTargettable(TargettingQuery targetQuery)
    {
        return true;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void OnMouseDown()
    {
        if (localPlayer)
        {
            localPlayer.ClientRequestSurrender();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerUI : Targettable
{
    public PlayerController player;
    public TextMeshProUGUI healthDisplay;
    public TextMeshProUGUI manaCount;

    public ManaUI[] manaDisplay;

    public override bool IsTargettable()
    {
        if (player)
        {
            return player.IsTargettable();
        }
        return false;
    }

    public override bool IsTargettable(TargettingQuery targetQuery)
    {
        if (player)
        {
            return player.IsTargettable(targetQuery);
        }
        return false;
    }

    public override Targettable GetTargettableEntity()
    {
        return player;
    }

    public void AssignPlayer(PlayerController p)
    {
        player = p;
        p.playerUI = this;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (player != null)
        {
            if (player.health < GameConstants.MAX_PLAYER_HEALTH)
            {
                healthDisplay.text = "<color=yellow>" + player.health.ToString() + "</color>";

            }
            else
            {
                healthDisplay.text = player.health.ToString();

            }

            manaCount.text = player.currMana.ToString();

            for (int i = 0; i < GameConstants.MAX_MANA; i++)
            {
                if (i < player.currMana)
                {
                    manaDisplay[i].SetState(ManaUI.State.AVAILABLE);
                }
                else if (i < player.totalMana)
                {
                    manaDisplay[i].SetState(ManaUI.State.EXPENDED);
                }
                else
                {
                    manaDisplay[i].SetState(ManaUI.State.EMPTY);

                }
            }
        }
    }

    public override Alignment GetAlignmentToPlayer(PlayerController player)
    {
        return GetTargettableEntity().GetAlignmentToPlayer(player);
    }
}

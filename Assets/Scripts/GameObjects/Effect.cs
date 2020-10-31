using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Effect : Targettable
{
    public Card source;
    public SpriteRenderer icon;

    private bool hovering;

    private Targettable[][] targetList;
    private TriggerCondition triggerCondition;

    public override bool IsTargettable()
    {
        return false;
    }

    public override bool IsTargettable(TargettingQuery targetQuery)
    {
        return false;
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        source.transform.SetParent(transform);
        source.transform.localPosition = new Vector3(-2.5f, 0.0f, 0.0f);
        source.gameObject.SetActive(false);
        source.enabled = false;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        if (source)
        {
            source.HoverZoomToCam();
            hovering = true;
            source.gameObject.SetActive(true);
        }
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        if (hovering)
        {
            hovering = false;
            source.transform.localPosition = new Vector3(-2.5f, 0.0f, 0.0f);
            source.gameObject.SetActive(false);
        }
    }
    public void SetData(Card c, TriggerCondition trigger, Targettable[][] targets)
    {
        source.cardData = c.cardData.Clone();

        if (c.cardData.GetImage() && icon)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(GameConstants.PATHS.CARD_IMAGES + c.cardData.GetImage().name);
            icon.sprite = sprites[1];
        }

        triggerCondition = trigger;
        targetList = targets;
    }

    public void SetData(Card c, TriggerCondition trigger)
    {
        source.cardData = c.cardData.Clone();
        source.controller = c.controller;

        if (c.cardData.GetImage() && icon)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(GameConstants.PATHS.CARD_IMAGES + c.cardData.GetImage().name);
            icon.sprite = sprites[1];
        }

        triggerCondition = trigger;
    }

    [Server]
    public void ServerResolve()
    {
        List<CardEffectDescription> effectList = source.cardData.GetEffectsOnTrigger(triggerCondition);

        int targetIndex = 0;
        foreach (CardEffectDescription effect in effectList)
        {
            if (effect.targettingType != null && effect.targettingType.RequiresSelection())
            {
                effect.ResolveEffect(targetList[targetIndex], source.controller);
                targetIndex++;
            }
            else
            {
                effect.ResolveEffect(null, source.controller);
            }
        }
    }
}

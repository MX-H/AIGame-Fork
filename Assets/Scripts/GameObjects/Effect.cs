using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
public class Effect : Targettable
{
    public Card source;
    public SpriteRenderer icon;
    public TextPrompt textPrompt;

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
        textPrompt.gameObject.SetActive(false);
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
            textPrompt.gameObject.SetActive(true);

            if (targetList != null)
            {
                for (int i = 0; i < targetList.Length; i++)
                {
                    foreach(Targettable targetable in targetList[i])
                    {
                        targetable.GetTargettableUI().AddText("<sprite=" + i + ">");
                    }
                }
            }
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
            textPrompt.gameObject.SetActive(false);
            if (targetList != null)
            {
                foreach (Targettable[] targettables in targetList)
                {
                    foreach (Targettable targetable in targettables)
                    {
                        targetable.GetTargettableUI().ClearText();
                    }
                }
            }
        }
    }
    public void SetData(Card c, TriggerCondition trigger, Targettable[][] targets = null)
    {
        source.cardData = c.cardData.Clone();
        source.controller = c.controller;

        if (c.cardData.GetImage() && icon)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(GameConstants.PATHS.CARD_IMAGES + c.cardData.GetImage().name);
            icon.sprite = sprites[1];
        }

        if (textPrompt != null)
        {
            string effectMessage = "";
            int targetsIndex = 0;
            foreach (CardEffectDescription desc in source.cardData.GetEffectsOnTrigger(trigger))
            {
                if (desc.targettingType.RequiresSelection())
                {
                    effectMessage += "<sprite=" + targetsIndex + "> ";
                    targetsIndex++;
                }
                effectMessage += desc.CardText() + '\n';
            }
            textPrompt.SetText(effectMessage);
        }

        triggerCondition = trigger;
        targetList = targets;
    }

    public Queue<EffectResolutionTask> GetEffectTasks()
    {
        Queue<EffectResolutionTask> tasks = new Queue<EffectResolutionTask>();
        List<CardEffectDescription> effectList = source.cardData.GetEffectsOnTrigger(triggerCondition);

        int targetIndex = 0;
        foreach (CardEffectDescription effect in effectList)
        {
            if (effect.targettingType != null && effect.targettingType.RequiresSelection())
            {
                Queue<EffectResolutionTask> effectTasks = effect.GetEffectTasks(targetList[targetIndex], source.controller);
                while (effectTasks.Count > 0)
                {
                    tasks.Enqueue(effectTasks.Dequeue());
                }

                targetIndex++;
            }
            else
            {
                Queue<EffectResolutionTask> effectTasks = effect.GetEffectTasks(null, source.controller);
                while (effectTasks.Count > 0)
                {
                    tasks.Enqueue(effectTasks.Dequeue());
                }
            }
        }
        return tasks;
    }
}

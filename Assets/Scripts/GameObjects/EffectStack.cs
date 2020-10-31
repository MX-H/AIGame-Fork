using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EffectStack : MonoBehaviour
{
    public List<Effect> effectStack;

    // Start is called before the first frame update
    void Start()
    {
        effectStack = new List<Effect>();

        foreach (Effect effect in GetComponentsInChildren<Effect>())
        {
            PushEffect(effect);
        }
    }

    // Update is called once per frame
    void Update()
    {
        float center = (float)(effectStack.Count - 1) / 2.0f;

        for (int i = 0; i < effectStack.Count; i++)
        {
            effectStack[i].transform.localPosition = new Vector3(0, (center - i) * -3.5f, 0);
        }
    }

    public void PushEffect(Effect effect)
    {
        effectStack.Add(effect);
        effect.transform.SetParent(transform);
    }
    public Effect PopEffect()
    {
        Effect effect = effectStack[effectStack.Count - 1];
        effectStack.RemoveAt(effectStack.Count - 1);
        return effect;
    }

    public void RemoveEffect(Effect effect)
    {
        effectStack.Remove(effect);
    }

    public bool IsFull()
    {
        return effectStack.Count >= GameConstants.MAX_STACK_SIZE;
    }

    public bool IsEmpty()
    {
        return effectStack.Count == 0;
    }
}

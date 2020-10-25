using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    public MeshRenderer[] renderers;
    float[] outlineWidth1; // For targettable/selected
    float[] outlineWidth2; // For hovering

    public Color targetColor;
    public Color selectColor;

    private bool initialized = false;

    // Start is called before the first frame update
    void Start()
    {
        outlineWidth1 = new float[renderers.Length];
        outlineWidth2 = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            outlineWidth1[i] = renderers[i].material.GetFloat("_FirstOutlineWidth");
            outlineWidth2[i] = renderers[i].material.GetFloat("_SecondOutlineWidth");
        }

        initialized = true;

        SetOutline1(false);
        SetOutline2(false);
    }

    public void SetOutline1(bool on)
    {
        if (initialized)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_FirstOutlineWidth", on ? outlineWidth1[i] : 0f);
            }
        }
    }

    public void SetOutline2(bool on)
    {
        if (initialized)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_SecondOutlineWidth", on ? outlineWidth2[i] : 0f);
            }
        }
    }

    public void UseSelectColor()
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.material.SetColor("_FirstOutlineColor", selectColor);
        }
    }
    public void UseTargetColor()
    {
        foreach (Renderer renderer in renderers)
        {
            renderer.material.SetColor("_FirstOutlineColor", targetColor);
        }
    }
}

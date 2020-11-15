using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextPrompt : MonoBehaviour
{
    public TextMeshProUGUI textOutput;
    public void SetText(string text)
    {
        if (textOutput)
        {
            textOutput.text = text;
        }
    }
}

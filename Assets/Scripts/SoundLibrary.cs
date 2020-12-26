using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class used to play sound effects for various actions or states
*/

public class SoundLibrary : MonoBehaviour
{
    public static AudioClip victorySound, defeatSound, combatSound, drawSound, addEffectSound, playCardSound, activateTrapSound;
    static AudioSource audioSource;
    
    void Start()
    {
        victorySound = Resources.Load<AudioClip>("Music/SFX/Victory");
        defeatSound = Resources.Load<AudioClip>("Music/SFX/Defeat");
        combatSound = Resources.Load<AudioClip>("Music/SFX/Combat");
        drawSound = Resources.Load<AudioClip>("Music/SFX/DrawCard");
        addEffectSound = Resources.Load<AudioClip>("Music/SFX/AddEffect");
        playCardSound = Resources.Load<AudioClip>("Music/SFX/PlayCard");
        activateTrapSound = Resources.Load<AudioClip>("Music/SFX/ActivateTrap");
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
    
    }

    public static void PlaySound(string action)
    {
        switch (action)
        {
            case "draw":
                audioSource.PlayOneShot(drawSound);
                break;
            case "victory":
                audioSource.PlayOneShot(victorySound);
                break;
            case "defeat":
                audioSource.PlayOneShot(defeatSound);
                break;
            case "combat":
                audioSource.PlayOneShot(combatSound);
                break;
            case "card":
                audioSource.PlayOneShot(playCardSound);
                break;
            case "trap":
                audioSource.PlayOneShot(activateTrapSound);
                break;
            case "effect":
                audioSource.PlayOneShot(addEffectSound);
                break;
        }
    }
}
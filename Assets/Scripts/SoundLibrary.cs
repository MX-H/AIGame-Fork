using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Class used to play sound effects for various actions or states
*/

public class SoundLibrary : MonoBehaviour
{
    public AudioClip victorySound, defeatSound, combatSound, turnStartSound, drawSound, gameStartSound;
    AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
    
    }

    public void PlaySound(string action)
    {
        switch (action)
        {
            case "gameStart":
                audioSource.PlayOneShot(gameStartSound);
                break;
            case "turnStart":
                audioSource.PlayOneShot(turnStartSound);
                break;
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
        }
    }
}
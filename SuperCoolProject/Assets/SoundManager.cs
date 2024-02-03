using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip UIHover;
    public AudioClip UISelect;
    public AudioClip UIPress;

    private AudioSource audioSource;


    public static SoundManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        audioSource = GetComponent<AudioSource>();
    }


    #region UI
    public void PlaySoundClick()
    {
        audioSource.PlayOneShot(UIPress, 1f);
    }

    public void PlaySoundOnSelect()
    {
        audioSource.PlayOneShot(UISelect, 1f);
    }
    public void PlaySoundOnHover()
    {
        audioSource.PlayOneShot(UIHover, 1f);
    }
    #endregion
}

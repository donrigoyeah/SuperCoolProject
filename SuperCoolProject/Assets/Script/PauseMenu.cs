using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu SharedInstance;

    [Header("Volume Settings")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioMixer myMixer;

    [Header("Kill Counter")]
    [SerializeField] private TextMeshProUGUI sphereKillCounter;
    [SerializeField] private TextMeshProUGUI squareKillCounter;
    [SerializeField] private TextMeshProUGUI triangleKillCounter;

    [SerializeField] private TextMeshProUGUI playtimeText;
    private float startTime;
    public bool isPaused;

    [Header("Camera Shake")]
    [SerializeField] private CameraShake cameraShake;
    //[SerializeField] private Slider cameraShakeSlider;

    //TODO: mouse sensivity

    private void Awake()
    {
        SharedInstance = this;
        Debug.Log("Maybe UI update here?");
        Debug.Log("Redid volume and audio mixer is being used to handle volume slider for music and sfx");
        // Handle entire UI from this script? Add values of resourceUI and other displays here"); 
    }

    void Start()
    {
        // Check if there is old volume settings or not
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            Load();
        }
        else
        {
            SetMusicVol();
            SetSFXVol();
        }
        startTime = Time.time;

    }

    void Update()
    {
        float currentTime = Time.time - startTime;
        string formattedTime = FormatTime(currentTime);
        playtimeText.text = "Playtime: " + formattedTime;
    }

    private string FormatTime(float timeInSeconds)
    {
        int hours = (int)(timeInSeconds / 3600);
        int minutes = (int)((timeInSeconds % 3600) / 60);
        int seconds = (int)(timeInSeconds % 60);

        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    // Gets called on playerLocomotion
    public void Pause()
    {
        Time.timeScale = 0;
        Debug.Log("Moved to own function");
        UpdatePauseUI();

        pauseMenu.SetActive(true);
        isPaused = true;
    }

    private void UpdatePauseUI()
    {
        sphereKillCounter.text = GameManager.SharedInstance.sphereKilled.ToString();
        squareKillCounter.text = GameManager.SharedInstance.squareKilled.ToString();
        triangleKillCounter.text = GameManager.SharedInstance.triangleKilled.ToString();
    }
    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        isPaused = false;
    }

    public void Restart()
    {
        pauseMenu.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Debug.Log("Wish quitting would be this easy");
        Application.Quit();
    }

    public void SetMusicVol()
    {
        float volume = musicSlider.value;
        myMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSFXVol()
    {
        float volume = sfxSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    private void Load()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        // TODO: Add this later when slider is found
        //cameraShakeSlider.value = PlayerPrefs.GetFloat("CameraShakeIntensity");

        //CameraShakeController();
        SetMusicVol();
        SetSFXVol();
    }

    //private void CameraShakeController()
    //{
    //    float cameraShakeModifier = cameraShakeSlider.value * 2.5f;
    //    cameraShake.shakeIntensity = cameraShakeModifier;
    //    PlayerPrefs.SetFloat("CameraShakeIntensity", cameraShakeModifier);
    //}
}


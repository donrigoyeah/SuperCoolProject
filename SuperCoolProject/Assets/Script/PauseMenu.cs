using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu SharedInstance;

    [SerializeField] private GameObject pauseMenu;   // using gameobject for canvas because for some reason canvas.enable = true was not working
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI playtimeText;
    [SerializeField] private TextMeshProUGUI sphereKillCounter;
    [SerializeField] private TextMeshProUGUI squareKillCounter;
    [SerializeField] private TextMeshProUGUI triangleKillCounter;

    private float startTime;
    public bool isPaused;

    //TODO: SFX volume and mouse sensivity

    private void Awake()
    {
        SharedInstance = this;
    }

    void Start()
    {
        // Check if there is old volume settings or not
        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            PlayerPrefs.SetFloat("musicVolume", 1);
            Load();
        }
        else
        {
            Load();
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
        sphereKillCounter.text = GameManager.SharedInstance.sphereKilled.ToString();
        squareKillCounter.text = GameManager.SharedInstance.squareKilled.ToString();
        triangleKillCounter.text = GameManager.SharedInstance.triangleKilled.ToString();
        pauseMenu.SetActive(true);
        isPaused = true;
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

    //Changes volumes this function is directly used on inspector
    public void ChangeVolume()
    {
        AudioListener.volume = volumeSlider.value;
        Save();
    }

    private void Load()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("musicVolume");
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", volumeSlider.value);
    }

}


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
    public GameObject pauseMenu;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioMixer myMixer;

    [Header("Resources Counter")]
    [SerializeField] private Image ResourceSphere;
    [SerializeField] private Image ResourceSquare;
    [SerializeField] private Image ResourceTriangle;
    [SerializeField] private Image CloneJuice;
    [SerializeField] private TextMeshProUGUI spaceShipParts;

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
    // This needs more refinement for waitforseconds wait time but we can take care of it eventually 
    public IEnumerator Pause()
    {
        UpdatePauseUI();
        PauseMenu.SharedInstance.pauseMenu.SetActive(true);
        Time.timeScale = 0.01f;
        yield return new WaitForSeconds(0.02f);
        isPaused = true;
    }

    private void UpdatePauseUI()
    {
        sphereKillCounter.text = GameManager.SharedInstance.sphereKilled.ToString();
        squareKillCounter.text = GameManager.SharedInstance.squareKilled.ToString();
        triangleKillCounter.text = GameManager.SharedInstance.triangleKilled.ToString();

        // TODO: This only shows the stats of the first player / Maybe check which player opend the pause menu?!
        ResourceSphere.fillAmount = GameManager.SharedInstance.players[0].currentSphereResource / GameManager.SharedInstance.players[0].maxSphereResource;
        ResourceSquare.fillAmount = GameManager.SharedInstance.players[0].currentSquareResource / GameManager.SharedInstance.players[0].maxSquareResource;
        ResourceTriangle.fillAmount = GameManager.SharedInstance.players[0].currentTriangleResource / GameManager.SharedInstance.players[0].maxTriangleResource;

        spaceShipParts.text = GameManager.SharedInstance.currentSpaceShipParts.ToString() + "/" + GameManager.SharedInstance.totalSpaceShipParts.ToString();
        CloneJuice.fillAmount = GameManager.SharedInstance.currentCloneJuice / GameManager.SharedInstance.maxCloneJuice;
    }
    public IEnumerator Resume()
    {
        Time.timeScale = 1;
        PauseMenu.SharedInstance.pauseMenu.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        isPaused = false;
    }

    public void ResumeForButton() // UI buttons only works with void functions
    {
        isPaused = false;
        Time.timeScale = 1;
        PauseMenu.SharedInstance.pauseMenu.SetActive(false);
    }

    public void Restart()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
        SceneManager.LoadScene("MenuScene");
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


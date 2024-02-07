using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    [Header("Volume Settings")]
    public GameObject pauseMenu;
    public GameObject stats;
    public GameObject options;
    public Button resumeButton;
    public Slider musicSlider;
    public Slider sfxSlider;
    public AudioMixer myMixer;

    [Header("Resources Counter")]
    public Image ResourceSphere;
    public Image ResourceSquare;
    public Image ResourceTriangle;
    public Image CloneJuice;
    public TextMeshProUGUI spaceShipParts;

    [Header("Kill Counter")]
    public TextMeshProUGUI sphereKillCounter;
    public TextMeshProUGUI squareKillCounter;
    public TextMeshProUGUI triangleKillCounter;

    public TextMeshProUGUI playtimeText;
    private float startTime;
    public bool isPaused;
    private float currentTime;
    private string formattedTime;

    private int hours;
    private int minutes;
    private int seconds;


    [Header("Camera Shake")]
    public CameraShake cameraShake;
    [SerializeField] private Slider cameraShakeSlider;

    //TODO: mouse sensivity

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
        currentTime = Time.time - startTime;
        formattedTime = FormatTime(currentTime);
        playtimeText.text = "Playtime: " + formattedTime;

        //if (Input.GetMouseButtonDown(0)) // Debugging to check if mouse is detecting button UI or not
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;

        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        Debug.Log("Mouse hit: " + hit.collider.gameObject.name);
        //    }
        //}
    }

    private string FormatTime(float timeInSeconds)
    {
        hours = (int)(timeInSeconds / 3600);
        minutes = (int)((timeInSeconds % 3600) / 60);
        seconds = (int)(timeInSeconds % 60);

        return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
    }

    // Gets called on playerLocomotion
    // This needs more refinement for waitforseconds wait time but we can take care of it eventually 
    public IEnumerator Pause()
    {
        UpdatePauseUI();
        PauseMenu.Instance.pauseMenu.SetActive(true);
        PauseMenu.Instance.stats.SetActive(true);
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        Time.timeScale = 0.01f;
        yield return new WaitForSeconds(0.02f);
        isPaused = true;
    }

    private void UpdatePauseUI()
    {
        sphereKillCounter.text = AlienManager.Instance.sphereKilled.ToString();
        squareKillCounter.text = AlienManager.Instance.squareKilled.ToString();
        triangleKillCounter.text = AlienManager.Instance.triangleKilled.ToString();

        // TODO: This only shows the stats of the first player / Maybe check which player opend the pause menu?!
        ResourceSphere.fillAmount = GameManager.Instance.players[0].currentSphereResource / GameManager.Instance.players[0].maxSphereResource;
        ResourceSquare.fillAmount = GameManager.Instance.players[0].currentSquareResource / GameManager.Instance.players[0].maxSquareResource;
        ResourceTriangle.fillAmount = GameManager.Instance.players[0].currentTriangleResource / GameManager.Instance.players[0].maxTriangleResource;

        spaceShipParts.text = GameManager.Instance.currentSpaceShipParts.ToString() + "/" + GameManager.Instance.totalSpaceShipParts.ToString();
        CloneJuice.fillAmount = GameManager.Instance.currentCloneJuice / GameManager.Instance.maxCloneJuice;
    }
    public IEnumerator Resume()
    {
        Time.timeScale = 1;
        yield return new WaitForSeconds(0.2f);
        isPaused = false;
        options.SetActive(false);
        pauseMenu.SetActive(false);

    }

    public void PauseForButton()
    {
        PauseMenu.Instance.pauseMenu.SetActive(true);
        PauseMenu.Instance.stats.SetActive(true);
        resumeButton.Select();
        Time.timeScale = 0.01f;
        isPaused = true;

    }

    public void ResumeForButton() // UI buttons only works with void functions
    {
        isPaused = false;
        Time.timeScale = 1;
        EventSystem.current.SetSelectedGameObject(null);
        options.SetActive(false);
        pauseMenu.SetActive(false);
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
        cameraShakeSlider.value = PlayerPrefs.GetFloat("CameraShakeIntensity");

        CameraShakeController();
        SetMusicVol();
        SetSFXVol();
    }

    public void ResumeTIme()
    {
        Time.timeScale = 1;
    }

    public void ChangeSelectedButton()
    {
        musicSlider.Select();
    }

    public void CameraShakeController()
    {
        float cameraShakeModifier = cameraShakeSlider.value * 5.5f;
        cameraShake.shakeIntensity = cameraShakeModifier;
        PlayerPrefs.SetFloat("CameraShakeIntensity", cameraShakeModifier);
    }
}


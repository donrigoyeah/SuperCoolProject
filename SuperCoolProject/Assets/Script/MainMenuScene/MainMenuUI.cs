using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    private bool loadScene = false;

    [SerializeField]
    private int scene;
    [SerializeField]
    public TextMeshProUGUI loadingText;

    public Button playButton;
    public GameObject MainMenuLoadingScreen;


    private void Awake()
    {
        MainMenuLoadingScreen.SetActive(false);
        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }

    // Updates once per frame
    void Update()
    {
        // If the new scene has started loading...
        if (loadScene == true)
        {
            // ...then pulse the transparency of the loading text to let the player know that the computer is still working.
            loadingText.color = new Color(loadingText.color.r, loadingText.color.g, loadingText.color.b, Mathf.PingPong(Time.time, 1));

        }
    }


    // The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
    IEnumerator LoadNewScene()
    {
        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
        AsyncOperation async = SceneManager.LoadSceneAsync(scene);

        // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
        while (!async.isDone)
        {
            yield return null;
        }

    }

    public void StartGame()
    {
        // If the player has pressed the button and a new scene is not loading yet...
        if (loadScene == false)
        {
            // enable the loading Screen gameobject
            MainMenuLoadingScreen.SetActive(true);

            // ...set the loadScene boolean to true to prevent loading a new scene more than once...
            loadScene = true;

            // ...change the instruction text to read "Loading..."
            loadingText.text = "Loading...";

            // ...and start a coroutine that will load the desired scene.
            StartCoroutine(LoadNewScene());

        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

}

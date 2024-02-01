using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class FPS : MonoBehaviour
{
    private TMP_Text _fpsText;

    private void Start()
    {
        _fpsText = GetComponent<TMP_Text>();
        StartCoroutine(FramesPerSecond());
    }

    private IEnumerator FramesPerSecond()
    {
        WaitForEndOfFrame frame = new WaitForEndOfFrame();

        while (true)
        {
            int fps = (int)(1f / Time.fixedDeltaTime);
            DisplayFPS(fps);

            yield return frame;
        }
    }

    private void DisplayFPS(float fps)
    {
        _fpsText.text = $"{fps} FPS";
    }
}

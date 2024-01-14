using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DamageUIHandler : MonoBehaviour
{
    public TextMeshProUGUI damageText;
    public float damageValue;
    int steps = 30;
    float animationDuration = .2f;

    private void OnEnable()
    {
        StartCoroutine(DisplayMotion());
    }

    IEnumerator DisplayMotion()
    {
        damageText.text = damageValue.ToString();

        RectTransform RT = this.gameObject.GetComponent<RectTransform>();
        RT.localScale = Vector3.one;
        RT.localPosition = new Vector3(0, 5, 0);

        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(animationDuration / steps);
            RT.localPosition = new Vector3(0, 5 + (2 * i / steps), 0);
            RT.localScale = Vector3.one + ((Vector3.one * i) / (2 * steps));
        }

        this.gameObject.SetActive(false);
    }




}

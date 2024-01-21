using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DamageUIHandler : MonoBehaviour
{
    public GameObject parentGO;
    public TextMeshProUGUI damageText;
    public float damageValue;
    int steps = 30;
    float animationDuration = .15f;

    RectTransform RT;

    private void OnEnable()
    {
        if (RT == null)
        {
            RT = this.gameObject.GetComponent<RectTransform>();
        }
        StartCoroutine(DisplayMotion());
    }

    IEnumerator DisplayMotion()
    {
        damageText.text = damageValue.ToString();
        RT.localScale = Vector3.one;
        RT.localPosition = new Vector3(0, 5, 0);

        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(animationDuration / steps);
            RT.localPosition = new Vector3(0, 5 + (2 * i / steps), 0);
            RT.localScale = Vector3.one + ((Vector3.one * i) / (2 * steps));
        }

        parentGO.SetActive(false);
    }




}

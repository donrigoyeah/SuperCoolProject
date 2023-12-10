using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHandler : MonoBehaviour
{
    private void OnEnable()
    {
        // Gets rendered everytime object gets setActive
        StartCoroutine(DisableAfterSeconds());
    }

    IEnumerator DisableAfterSeconds()
    {
        yield return new WaitForSeconds(2);
        this.gameObject.SetActive(false);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeadAlienHandler : MonoBehaviour
{
    public Rigidbody[] Rigidbodies;
    public int currentAlienSpecies;
    public Vector3 bulletForce;

    [Header("Dissolve")]
    public Material dissolve;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;
    private float steps;
    private float durtaion;
    private float dissolveAMount;
    
    /*[Header("Triangle Bone Position")]
    public Transform TBone1;
    public Transform TBone2;
    public Transform TBone3;

    private Vector3 TBone1Pos;
    private Quaternion TBone1Rot;
    private Vector3 TBone2Pos;
    private Quaternion TBone2Rot;
    private Vector3 TBone3Pos;
    private Quaternion TBone3Rot;

    [Header("Sphere Bone Position")]
    public Transform SpBone1;
    public Transform SpBone2;
    public Transform SpBone3;

    private Vector3 SpBone1Pos;
    private Quaternion SpBone1Rot;
    private Vector3 SpBone2Pos;
    private Quaternion SpBone2Rot;
    private Vector3 SpBone3Pos;
    private Quaternion SpBone3Rot;

    [Header("Square Bone Position")]
    public Transform SqBone1;
    public Transform SqBone2;
    public Transform SqBone3;
    public Transform SqBone4;
    public Transform SqBone5;
    public Transform SqBone6;

    private Vector3 SqBone1Pos;
    private Quaternion SqBone1Rot;
    private Vector3 SqBone2Pos;
    private Quaternion SqBone2Rot;
    private Vector3 SqBone3Pos;
    private Quaternion SqBone3Rot;
    private Vector3 SqBone4Pos;
    private Quaternion SqBone4Rot;
    private Vector3 SqBone5Pos;
    private Quaternion SqBone5Rot;
    private Vector3 SqBone6Pos;
    private Quaternion SqBone6Rot;*/

    private void Awake()
    {
        /*for (int i = 0; i < Rigidbodies.Length; i++)
        {
            Rigidbodies[i].velocity = Vector3.zero;
            Rigidbodies[i].transform.position = Vector3.zero;
            Rigidbodies[i].transform.rotation = Quaternion.identity;
            deadAlienSpecies[i].SetActive(false);
        }

        TBone1Pos = TBone1.position;
        TBone1Rot = TBone1.rotation;
        TBone2Pos = TBone2.position;
        TBone2Rot = TBone2.rotation;
        TBone3Pos = TBone3.position;
        TBone3Rot = TBone3.rotation;

        SpBone1Pos = SpBone1.position;
        SpBone1Rot = SpBone1.rotation;
        SpBone2Pos = SpBone2.position;
        SpBone2Rot = SpBone2.rotation;
        SpBone3Pos = SpBone3.position;
        SpBone3Rot = SpBone3.rotation;

        SqBone1Pos = SqBone1.position;
        SqBone1Rot = SqBone1.rotation;
        SqBone2Pos = SqBone2.position;
        SqBone2Rot = SqBone2.rotation;
        SqBone3Pos = SqBone3.position;
        SqBone3Rot = SqBone3.rotation;
        SqBone4Pos = SqBone4.position;
        SqBone4Rot = SqBone4.rotation;
        SqBone5Pos = SqBone5.position;
        SqBone5Rot = SqBone5.rotation;
        SqBone6Pos = SqBone6.position;
        SqBone6Rot = SqBone6.rotation;*/
        
        
    }

    private void OnEnable()
    {
        // GetComponent<Rigidbody>().AddForce((bulletForce * 4) + Vector3.up * 4);
        StartCoroutine(Dissolve());
    }

    private void OnDisable()
    {
        // for (int i = 0; i < Rigidbodies.Length; i++)
        // {
        //     Rigidbodies[i].velocity = Vector3.zero;
        //     Rigidbodies[i].transform.localPosition = Vector3.zero;
        //     Rigidbodies[i].transform.rotation = Quaternion.identity;
        //     deadAlienSpecies[i].SetActive(false);
        // }
        // bulletForce = Vector3.zero;
    }

    /*
    private void ResetAllBones()
    {
        if (currentAlienSpecies == 0)
        {
            SpBone1.position = Vector3.zero;
            SpBone2.position = Vector3.zero;
            SpBone3.position = Vector3.zero;
            //SpBone1.position = SpBone1Pos;
            //SpBone2.position = SpBone2Pos;
            //SpBone3.position = SpBone3Pos;
            SpBone1.rotation = SpBone1Rot;
            SpBone2.rotation = SpBone2Rot;
            SpBone3.rotation = SpBone3Rot;
        }
        else if (currentAlienSpecies == 1)
        {
            SqBone1.position = Vector3.zero;
            SqBone2.position = Vector3.zero;
            SqBone3.position = Vector3.zero;
            SqBone4.position = Vector3.zero;
            SqBone5.position = Vector3.zero;
            SqBone6.position = Vector3.zero;
            //SqBone1.position = SqBone1Pos;
            //SqBone2.position = SqBone2Pos;
            //SqBone3.position = SqBone3Pos;
            //SqBone4.position = SqBone4Pos;
            //SqBone5.position = SqBone5Pos;
            //SqBone6.position = SqBone6Pos;
            SqBone1.rotation = SqBone1Rot;
            SqBone2.rotation = SqBone2Rot;
            SqBone3.rotation = SqBone3Rot;
            SqBone4.rotation = SqBone4Rot;
            SqBone5.rotation = SqBone5Rot;
            SqBone6.rotation = SqBone6Rot;
        }
        else if (currentAlienSpecies == 2)
        {
            TBone1.position = Vector3.zero;
            TBone2.position = Vector3.zero;
            TBone3.position = Vector3.zero;
            //TBone1.position = TBone1Pos;
            //TBone2.position = TBone2Pos;
            //TBone3.position = TBone3Pos;
            TBone1.rotation = TBone1Rot;
            TBone2.rotation = TBone2Rot;
            TBone3.rotation = TBone3Rot;
        }
    }
    */


    IEnumerator Dissolve()
    {
        dissolve.SetFloat("_DissolveAmount", 0);
        durtaion = 1f;
        steps = 30;

        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForSeconds(durtaion / steps);
            dissolveAMount = (i / steps);
            dissolve.SetFloat("_DissolveAmount", (dissolveAMount));
        }
        // Rigidbodies[currentAlienSpecies].velocity = Vector3.zero;
        this.gameObject.SetActive(false);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpaceShipPart")]
public class SpaceShipScriptable : ScriptableObject
{
    public string partName;
    public float mass;
    public string abilityUnlockText;
    public GameObject model;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager SharedInstance;

    public int worldBoundaryX;
    public int worldBoundaryMinusX;
    public int worldBoundaryZ;
    public int worldBoundaryMinusZ;


    private void Awake()
    {
        SharedInstance = this;
    }
}

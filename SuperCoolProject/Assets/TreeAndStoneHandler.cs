using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAndStoneHandler : MonoBehaviour
{
    [Header("General")]
    public int treeCount = 0;
    public int stoneCount = 0;


    [Header("References")]
    public GameObject Tree;
    public GameObject Stone;
    private GameObject tmpTree;
    private GameObject tmpStone;
    public List<Light> allTreeLights;
    private Vector3 potentialPosition;
    Transform tmpTreeTransform;
    Transform tmpStoneTransform;

    private float r;
    private float angle;
    private float randomRotation;
    private float randPosX;
    private float randPosZ;

    public static TreeAndStoneHandler Instance;

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
    }

    private void Start()
    {
        PlanetTreesAndStones();
    }

    public void PlanetTreesAndStones()
    {

        for (int i = 0; i < treeCount; i++)
        {
            r = Random.Range(30, GameManager.Instance.worldRadius);
            angle = Random.Range(0, 360);
            randomRotation = Random.Range(0, 360);

            randPosX = r * Mathf.Cos(Mathf.Deg2Rad * angle);
            randPosZ = r * Mathf.Sin(Mathf.Deg2Rad * angle);

            potentialPosition = new Vector3(randPosX, 0.1f, randPosZ);


            // TODO: Check wheater spot is free or not
            //while (Physics.OverlapSphere(potentialPosition, .1f, 9) != null)

            tmpTree = Instantiate(Tree);
            tmpTreeTransform = tmpTree.transform;
            tmpTreeTransform.position = potentialPosition;
            tmpTreeTransform.rotation = Quaternion.Euler(0, randomRotation, 0);
            tmpTreeTransform.SetParent(this.transform);

            Light[] allTmpLights = tmpTree.GetComponentsInChildren<Light>();
            foreach (var item in allTmpLights)
            {
                allTreeLights.Add(item);
            }
        }


        for (int i = 0; i < stoneCount; i++)
        {
            r = Random.Range(30, GameManager.Instance.worldRadius);
            angle = Random.Range(0, 360);
            randomRotation = Random.Range(0, 360);

            randPosX = r * Mathf.Cos(Mathf.Deg2Rad * angle);
            randPosZ = r * Mathf.Sin(Mathf.Deg2Rad * angle);

            potentialPosition = new Vector3(randPosX, 0.1f, randPosZ);
            // TODO: Check wheater spot is free or not
            //while (Physics.OverlapSphere(potentialPosition, .1f, 9) != null)


            tmpStone = Instantiate(Stone);
            tmpStoneTransform = tmpStone.transform;
            tmpStoneTransform.position = potentialPosition;
            tmpStoneTransform.rotation = Quaternion.Euler(0, randomRotation, 0);
            tmpStoneTransform.SetParent(this.transform);

        }
    }

    public IEnumerator TurnOffAllTreeLights()
    {
        for (int i = 0; i < allTreeLights.Count; i++)
        {
            float tmpWait = Random.Range(0, 10);
            yield return new WaitForSeconds(tmpWait / 10);
            allTreeLights[i].enabled = false;
        }
    }

    public IEnumerator TurnOnAllTreeLights()
    {
        for (int i = 0; i < allTreeLights.Count; i++)
        {
            float tmpWait = Random.Range(0, 10);
            yield return new WaitForSeconds(tmpWait / 10);
            allTreeLights[i].enabled = true;
        }
    }
}

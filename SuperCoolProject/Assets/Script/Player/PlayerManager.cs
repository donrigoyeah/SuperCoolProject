using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Variables")]
    public bool isAlive;
    public bool canAim;
    public bool isInteracting;
    public bool isCarryingPart;
    public float playerResourceScanRadius = 100;
    //public List<Collider> resourceInRange;
    public float playerDetectionRadius = 10;
    public int aliensInRangePlayerCount;
    //public Collider[] aliensInRangePlayer = new Collider[10];
    public List<Collider> aliensInRangePlayer;
    public GameObject currentPart;
    public GameObject LightBeam;
    public GameObject deadPlayer;
    Transform MyTransform;

    [Header("Shield")]
    public bool hasShield;
    public GameObject playerShieldGO;
    public Material dissolve;
    public float timeSinceLastHit;
    public float invincibleFrames = .5f;
    public float shieldRechargeTime = 2;
    public float shieldRechargeTimeWithUpgrade = 1;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;
    public float counter;

    [Header("Resource Variables")]
    public float lightBulbMultiplicator = 1;
    public AlienHandler[] closestResource = new AlienHandler[] { null, null, null };  // 0:Sphere, 1:Square, 2:Triangle
    public float maxSphereResource = 100;
    public float maxSquareResource = 100;
    public float maxTriangleResource = 100;
    public float currentSphereResource;
    public float currentSquareResource;
    public float currentTriangleResource;
    public bool sphereUnfolded = false;
    public bool squareUnfolded = false;
    public bool triangleUnfolded = false;

    public float resourceDrain = .3f;
    public float resourceGain = 5;

    // 0:Sphere, 1:Square, 2:Triangle
    [Header("UI Elements")]
    public Image[] resourcePieCharts;
    public GameObject ResourceUISphere;
    public GameObject ResourceUISquare;
    public GameObject ResourceUITriangle;
    public GameObject[] closestResourceIndicator;  // 0:Sphere, 1:Square, 2:Triangle
    public Material[] resourceMaterial; // 0:Sphere, 1:Square, 2:Triangle
    public float currentSphereResourceInverse;
    public float currentSquareResourceInverse;
    public float currentTriangleResourceInverse;
    public float sphereEmissionTime;
    public float squareEmissionTime;
    public float triangleEmissionTime;
    public float EmissionSpeedDivider = 25f; //Increase it to slower the speed


    [Header("Audio")]
    public AudioClip shieldRechargeAudio;
    public AudioClip shieldBreakAudio;
    public AudioClip deathAudio;
    private AudioSource audioSource;

    private InputHandler inputHandler;
    private Animator playerAnim;
    private AlienHandler CurrentSurroundingAH;
    public GameObject UpgradeParticles;
    public ParticleSystem.MainModule ParticleSystem1Main;
    public ParticleSystem.MainModule ParticleSystem2Main;
    public ParticleSystem.MainModule ParticleSystem3Main;

    private float dist;
    private float currentDist;
    private int loopAmount;
    private GameObject ResourceAlienPoolGo;
    private AlienHandler ResourceAlienPoolGoHandler;
    private Vector3 targetRotation;
    private Quaternion newRotation;
    public int stepsUnfold;
    private float animationDurationUnfold;
    private int stepsFold;
    private float animationDurationFold;
    private int stepsShield;
    private float animationDurationShield;


    private int layerMaskAlien = 1 << 9; // Lyer 9 is Alien

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        inputHandler = GetComponent<InputHandler>();
        MyTransform = GetComponent<Transform>();
        playerAnim = GetComponentInChildren<Animator>();

        dissolve = playerShieldGO.gameObject.GetComponent<Renderer>().material;
        dissolve.SetFloat("_DissolveAmount", 0.016f);
        StartCoroutine(RespawnShield(2));

        ParticleSystem[] particleSystems = UpgradeParticles.GetComponentsInChildren<ParticleSystem>();

        ParticleSystem1Main = particleSystems[0].main;
        ParticleSystem2Main = particleSystems[1].main;
        ParticleSystem3Main = particleSystems[1].main;
        UpgradeParticles.SetActive(false);
    }

    private void FixedUpdate()
    {
        timeSinceLastHit += Time.deltaTime;
        HandleSurroundingAliens();
        HandleResource();
    }

    public void HandleHit()
    {
        if (!isAlive) { return; }
        if (timeSinceLastHit < invincibleFrames)
        {
            return;
        }
        else
        {
            if (hasShield == false)
            {
                HandleDeath(true);
            }
            else
            {
                StartCoroutine(ShieldBreak(shieldRechargeTime));
            }
            timeSinceLastHit = 0;
            return;
        }
    }

    private void HandleDeath(bool byAlien)
    {
        // Set Variable to disable movement/input
        isAlive = false;
        StopAllCoroutines();
        audioSource.PlayOneShot(deathAudio, 1f);
        GameObject deadPlayerInstance = Instantiate(deadPlayer, MyTransform.position, Quaternion.identity);

        Rigidbody deadPlayerRigidbody = deadPlayerInstance.GetComponent<Rigidbody>();
        if (deadPlayerRigidbody != null)
        {
            Vector3 forceDirection = Vector3.up;
            float forceMagnitude = 1f;
            deadPlayerRigidbody.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
        }

        // Enable UI Element
        // TODO: Check if all players are dead. otherwise maybe make deathscreen on playerHUD as well

        if (GameManager.Instance.currentCloneJuice < 0)
        {
            GameManager.Instance.hasLost = true;
            return;
        }

        if (GameManager.Instance.players.Count == 1)
        {
            GameManager.Instance.DeathScreen.SetActive(true);
            if (byAlien == true)
            {
                GameManager.Instance.deathScreenReason.text = "Your clone got killed by an Alien!";
            }
            else
            {
                GameManager.Instance.deathScreenReason.text = "Your clone ran out of resources!";
            }

            EventSystem.current.SetSelectedGameObject(GameManager.Instance.respawnButton.gameObject);
            GameManager.Instance.DeathScreenCloneJuiceUI.fillAmount = GameManager.Instance.currentCloneJuice / GameManager.Instance.maxCloneJuice;
        }

        dissolve.SetFloat("_DissolveAmount", 0.016f);
        hasShield = true;
        counter = 0;
        // Reset all resource variables back to max on new clone
        currentSphereResource = maxSphereResource;
        currentSquareResource = maxSquareResource;
        currentTriangleResource = maxTriangleResource;
    }

    private void HandleSurroundingAliens()
    {
        aliensInRangePlayer.Clear();
        foreach (var item in Physics.OverlapSphere(MyTransform.position, playerDetectionRadius, layerMaskAlien, QueryTriggerInteraction.Ignore))
        {
            aliensInRangePlayer.Add(item);
        }
        aliensInRangePlayerCount = aliensInRangePlayer.Count;

        //aliensInRangePlayerCount = Physics.OverlapSphereNonAlloc(MyTransform.position, playerDetectionRadius, aliensInRangePlayer, layerMaskAlien, QueryTriggerInteraction.Ignore);
        if (aliensInRangePlayerCount == 0) { return; }

        for (int i = 0; i < aliensInRangePlayerCount; i++)
        {
            CurrentSurroundingAH = aliensInRangePlayer[i].gameObject.GetComponentInParent<AlienHandler>();
            if (CurrentSurroundingAH.brainWashed == true) { continue; } // Interaction with player in TutorialScene, prevents HandleUpdateTarget error

            if (CurrentSurroundingAH.currentAge == AlienHandler.AlienAge.resource) // If sorrounding Alien is resource, put into resource Array
            {
                closestResource[CurrentSurroundingAH.currentSpecies] = CurrentSurroundingAH;
                continue;
            }

            CurrentSurroundingAH.targetAlien = this.gameObject;

            if (CurrentSurroundingAH.currentAge == AlienHandler.AlienAge.fullyGrown)
            {
                CurrentSurroundingAH.SetTarget(this.gameObject);
                CurrentSurroundingAH.lastAlienState = CurrentSurroundingAH.currentState;
                CurrentSurroundingAH.currentState = AlienHandler.AlienState.hunting;
                //CurrentSurroundingAH.TargetAlienTransform = MyTransform;
                continue;
            }
            else
            {
                CurrentSurroundingAH.SetTarget(this.gameObject);
                CurrentSurroundingAH.lastAlienState = CurrentSurroundingAH.currentState;
                CurrentSurroundingAH.currentState = AlienHandler.AlienState.evading;
                //CurrentSurroundingAH.TargetAlienTransform = MyTransform;
                continue;
            }

        }
    }

    private void HandleResourceDetection(int neededResource)
    {
        // TODO: This is possible quite cost intense!!!
        // TODO: Make an Array of resources, add aliens to it after spawning, remove when eaten or evolved

        #region Find via OverlappingSphere

        //int layerMask = 1 << 9; // Lyer 9 is Alien
        //float distanceToResource = playerResourceScanRadius;

        //if (closestResource[neededResource] != null)
        //{
        //    closestResourceIndicator[neededResource].SetActive(true);
        //    HandleResourceDetectionIndicator(closestResource[neededResource].transform.position, neededResource);
        //    if (closestResource[neededResource].currentAge != AlienHandler.AlienAge.resource)
        //    {
        //        closestResource[neededResource] = null;
        //        //Debug.Log("Resource became unavailable");
        //        return;
        //    }

        //    if (closestResource[neededResource].gameObject.activeInHierarchy == false)
        //    {
        //        closestResource[neededResource] = null;
        //    }
        //    return;
        //}
        //closestResourceIndicator[neededResource].SetActive(false);

        ////Debug.Log("Search for Closest Resource");
        //resourceInRange = Physics.OverlapSphere(this.transform.position, playerResourceScanRadius, layerMask);
        //foreach (var item in aliensInRange)
        //{
        //    AlienHandler AH = item.gameObject.GetComponent<AlienHandler>();
        //    if (AH.currentAge != AlienHandler.AlienAge.resource) { continue; }
        //    if (AH.currentSpecies != neededResource) { continue; }

        //    float tmpDistance = Vector3.Distance(AH.transform.position, this.transform.position);
        //    //Debug.Log("Distance to Resource: " + tmpDistance);
        //    if (tmpDistance > distanceToResource) { continue; }

        //    distanceToResource = tmpDistance;
        //    closestResource[neededResource] = AH;
        //    //Debug.Log("Found Closest Resource");
        //}

        #endregion

        #region Find via ListScan

        if (closestResource[neededResource] != null)
        {
            if (closestResource[neededResource].currentAge != AlienHandler.AlienAge.resource ||
                closestResource[neededResource].gameObject.activeInHierarchy == false)
            {
                closestResource[neededResource] = null;
                return;
            }

            closestResourceIndicator[neededResource].SetActive(true);
            HandleResourceDetectionIndicator(closestResource[neededResource].transform.position, neededResource);
            return;
        }

        if (closestResource[neededResource] == null)
        {
            closestResourceIndicator[neededResource].SetActive(false);

            dist = 1000;

            loopAmount =
                neededResource == 0 ? AlienManager.Instance.resourceSphere.Count :
                neededResource == 1 ? AlienManager.Instance.resourceSquare.Count :
                neededResource == 2 ? AlienManager.Instance.resourceTriangle.Count : 0;

            List<AlienHandler> ResourceList =
               neededResource == 0 ? AlienManager.Instance.resourceSphere :
               neededResource == 1 ? AlienManager.Instance.resourceSquare :
               neededResource == 2 ? AlienManager.Instance.resourceTriangle : null;

            for (int i = 0; i < loopAmount; i++)
            {
                currentDist = Vector3.Distance(ResourceList[i].transform.position, MyTransform.position);
                if (currentDist < dist)
                {
                    dist = currentDist;
                    closestResource[neededResource] = ResourceList[i];

                    if (dist < 10)
                    {
                        break;
                    }
                }
            }

            // BackUp spawning new resource in case of none available
            while (closestResource[neededResource] == null)
            {
                for (int i = 0; i < PoolManager.Instance.AlienPool.Count; i++)
                {
                    Vector3 locationForResource = PoolManager.Instance.AlienPool[i].transform.position;
                    float distToAlien = Vector3.Distance(locationForResource, MyTransform.position);
                    if (distToAlien > 50 && distToAlien < 70)
                    {
                        PoolManager.Instance.AlienPool[i].GetComponent<AlienHandler>().HandleDeath();

                        ResourceAlienPoolGo = PoolManager.Instance.GetPooledAliens(false);
                        if (ResourceAlienPoolGo != null)
                        {
                            ResourceAlienPoolGoHandler = ResourceAlienPoolGo.GetComponent<AlienHandler>();
                            ResourceAlienPoolGoHandler.currentSpecies = neededResource;
                            ResourceAlienPoolGoHandler.lifeTime = -10;
                            ResourceAlienPoolGo.transform.position = locationForResource;
                            ResourceAlienPoolGo.SetActive(true);

                            closestResource[neededResource] = ResourceAlienPoolGoHandler;
                        }
                    }
                }
            }
        }

        #endregion
    }

    private void HandleResourceDetectionIndicator(Vector3 targetResource, int neededResource)
    {
        if (targetResource == null)
        {
            closestResourceIndicator[neededResource].SetActive(false);
            return;
        }

        // 0:Sphere, 1:Square, 2:Triangle
        closestResourceIndicator[neededResource].SetActive(true);
        targetRotation = targetResource - MyTransform.position;
        newRotation = Quaternion.LookRotation(targetRotation, Vector3.up);
        closestResourceIndicator[neededResource].transform.rotation = newRotation;
    }

    private void DeactivateResourceDetectionIndicator(int neededResource)
    {
        // 0:Sphere, 1:Square, 2:Triangle
        closestResourceIndicator[neededResource].SetActive(false);
    }

    private void HandleResource()
    {
        if (isAlive == false) { return; }
        // 0:Sphere, 1:Square, 2:Triangle
        if (currentSphereResource > 0) { currentSphereResource -= resourceDrain; }
        if (currentSquareResource > 0) { currentSquareResource -= resourceDrain; }
        if (currentTriangleResource > 0) { currentTriangleResource -= resourceDrain; }


        // Will go from 0 to max
        currentSphereResourceInverse = maxSphereResource - currentSphereResource;
        currentSquareResourceInverse = maxSquareResource - currentSquareResource;
        currentTriangleResourceInverse = maxTriangleResource - currentTriangleResource;

        MaterialEmmissionControler();


        // Only show resource UI if below 75%
        if (currentSphereResource < 3 * maxSphereResource / 4)
        {
            HandleResourceDetection(0);
            // MaterialEmmissionControler(0);
        }
        else
        {
            DeactivateResourceDetectionIndicator(0);
            // MaterialEmmissionControler(0);
        }

        // Only show resource UI if below 75%
        if (currentSquareResource < 3 * maxSquareResource / 4)
        {
            HandleResourceDetection(1);
            // MaterialEmmissionControler(1);
        }
        else
        {
            DeactivateResourceDetectionIndicator(1);
            // MaterialEmmissionControler(1);
        }

        // Only show resource UI if below 75%
        if (currentTriangleResource < 3 * maxTriangleResource / 4)
        {
            HandleResourceDetection(2);
            // MaterialEmmissionControler(2);
        }
        else
        {
            DeactivateResourceDetectionIndicator(2);
            // MaterialEmmissionControler(2);
        }

        // Update UI
        resourcePieCharts[0].fillAmount = currentSphereResource / maxSphereResource;
        resourcePieCharts[1].fillAmount = currentSquareResource / maxSquareResource;
        resourcePieCharts[2].fillAmount = currentTriangleResource / maxTriangleResource;

        // Check if enough resources
        if (currentSphereResource <= 0 ||
            currentSquareResource <= 0 ||
            currentTriangleResource <= 0)
        {
            HandleDeath(false);
        }
    }

    private void MaterialEmmissionControler()
    {
        // TODO: Need to use own c,olors here, new Color(0.4f, 0.9f 0.7f, 1.0f); so this would not work

        sphereEmissionTime = (Time.time * currentSphereResourceInverse) / EmissionSpeedDivider;
        resourceMaterial[0].SetColor("_EmissionColor", Color.blue * (Mathf.PingPong(sphereEmissionTime, 5f)));


        squareEmissionTime = (Time.time * currentSquareResourceInverse) / EmissionSpeedDivider;
        resourceMaterial[1].SetColor("_EmissionColor", Color.yellow * (Mathf.PingPong(squareEmissionTime, 3f)));

        triangleEmissionTime = (Time.time * currentTriangleResourceInverse) / EmissionSpeedDivider;
        resourceMaterial[2].SetColor("_EmissionColor", Color.red * (Mathf.PingPong(triangleEmissionTime, 3f)));

    }

    public void HandleGainResource(int rescourseIndex)
    {
        if (rescourseIndex == 0)
        {
            currentSphereResource += resourceGain;
            if (currentSphereResource > maxSphereResource)
            {
                currentSphereResource = maxSphereResource;
            }
        }
        if (rescourseIndex == 1)
        {
            currentSquareResource += resourceGain;
            if (currentSquareResource > maxSquareResource)
            {
                currentSquareResource = maxSquareResource;
            }
        }
        if (rescourseIndex == 2)
        {
            currentTriangleResource += resourceGain;
            if (currentTriangleResource > maxTriangleResource)
            {
                currentTriangleResource = maxTriangleResource;
            }
        }
    }

    public void HandleRespawn()
    {
        if (!isAlive)
        {
            if (GameManager.Instance.DeathScreen.activeInHierarchy)
            {
                GameManager.Instance.DeathScreen.SetActive(false);
            }

            GameManager.Instance.HandleDrainCloneJuice();
            // TODO: Add Transition/ Fade to black/ camera shutter effect?!

            currentSphereResource = maxSphereResource;
            currentSquareResource = maxSquareResource;
            currentTriangleResource = maxTriangleResource;
            MyTransform.position = Vector3.zero;
            isCarryingPart = false;
            isInteracting = false;


            isAlive = true;
        }
    }

    private void HandleGameOver()
    {
        if (GameManager.Instance.hasLost)
        {
            SceneManager.LoadScene("MenuScene");
        }
    }

    public IEnumerator UnfoldResource(GameObject Resource, float degree)
    {
        RectTransform GORT;

        stepsUnfold = 30;
        animationDurationUnfold = .3f;
        Resource.gameObject.SetActive(true);
        GORT = Resource.GetComponent<RectTransform>();
        GORT.localScale = Vector3.zero;
        for (int i = 0; i < stepsUnfold; i++)
        {
            yield return new WaitForSeconds(animationDurationUnfold / stepsUnfold);
            GORT.localScale = Vector3.one * 3 * i / stepsUnfold;
            GORT.localEulerAngles = new Vector3(0, 0, degree * i / stepsUnfold);
        }

    }

    public IEnumerator FoldResource(GameObject Resource)
    {
        stepsFold = 30;
        animationDurationFold = .2f;
        RectTransform GORT;

        GORT = Resource.GetComponent<RectTransform>();
        GORT.localScale = Vector3.one * 2;
        for (int i = 0; i < stepsFold; i++)
        {
            yield return new WaitForSeconds(animationDurationFold / stepsFold);
            GORT.localScale = Vector3.one * 2 - Vector3.one * 2 * i / stepsFold;
        }
        GORT.localEulerAngles = Vector3.zero;
        Resource.gameObject.SetActive(false);
    }

    IEnumerator ShieldBreak(float timeToRecharge)
    {
        hasShield = false;
        stepsShield = 30;
        animationDurationShield = .5f;
        dissolve.SetFloat("_DissolveAmount", 0.016f); // Bug at 0 seems very opaque
        audioSource.PlayOneShot(shieldBreakAudio, 1f);


        for (int i = 0; i < stepsShield; i++)
        {
            counter += dissolveRate;
            yield return new WaitForSeconds(refreshRate);
            dissolve.SetFloat("_DissolveAmount", counter);
        }

        counter = 0;
        StartCoroutine(RespawnShield(timeToRecharge));
    }

    IEnumerator RespawnShield(float timeToRecharge)
    {
        playerShieldGO.SetActive(false);
        yield return new WaitForSeconds(timeToRecharge);
        dissolve.SetFloat("_DissolveAmount", 0.016f);
        hasShield = true;
        audioSource.PlayOneShot(shieldRechargeAudio, 1f);
        playerShieldGO.SetActive(true);

    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
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
    //public Collider[] aliensInRangePlayer = new Collider[10];
    public List<Collider> aliensInRangePlayer;
    private int aliensInRangePlayerCount;
    public GameObject currentPart;
    public GameObject LightBeam;
    public GameObject deadPlayer;
    Transform MyTransform;
    Vector3 locationForResource;
    float distToAlien;

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
    public List<AlienHandler> closestResource = new List<AlienHandler>(3);  // 0:Sphere, 1:Square, 2:Triangle
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
    public int stepsUnfold;
    private float animationDurationUnfold;
    private int stepsFold;
    private float animationDurationFold;
    private int stepsShield;

    public GameObject playerShield;
    public GameObject playerAntenna;

    [Header("Tick stats")]
    public float tickTimer;
    public float tickTimerMax = .5f;
    private float delta;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        MyTransform = GetComponent<Transform>();
        playerAnim = GetComponentInChildren<Animator>();


        // dissolve.SetFloat("_DissolveAmount", 0.016f);
        StartCoroutine(RespawnShield(2));

        ParticleSystem[] particleSystems = UpgradeParticles.GetComponentsInChildren<ParticleSystem>();

        ParticleSystem1Main = particleSystems[0].main;
        ParticleSystem2Main = particleSystems[1].main;
        ParticleSystem3Main = particleSystems[1].main;
        UpgradeParticles.SetActive(false);
    }

    private void FixedUpdate()
    {
        delta = Time.deltaTime;
        HandleResouceDrain(delta);
        timeSinceLastHit += delta;
        tickTimer += delta;

        if (tickTimer >= tickTimerMax)
        {
            tickTimer -= tickTimerMax;
            HandleSurroundingAliens();
            HandleResource();
        }
    }


    public void HandleHit()
    {
        if (!isAlive) { return; }
        if (timeSinceLastHit < invincibleFrames) { return; }

        if (hasShield == false)
        {
            HandleDeath("alien");
        }
        else
        {
            // StartCoroutine(ShieldBreak(shieldRechargeTime));
            StartCoroutine(RespawnShield(2f));
        }
        timeSinceLastHit = 0;
        return;

    }

    public void HandleDeath(string causeOfDeath)
    {
        isAlive = false;
        GameManager.Instance.isAliveBool.Add(isAlive);

        StopAllCoroutines();
        audioSource.PlayOneShot(deathAudio, 1f);
        GameObject deadPlayerInstance = Instantiate(deadPlayer, MyTransform.position, Quaternion.identity, PoolManager.Instance.DeadPlayerContainer);

        Rigidbody deadPlayerRigidbody = deadPlayerInstance.GetComponent<Rigidbody>();
        if (deadPlayerRigidbody != null)
        {
            Vector3 forceDirection = Vector3.up;
            float forceMagnitude = 1f;
            deadPlayerRigidbody.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);
        }

        if (GameManager.Instance.currentCloneJuice <= 0)
        {
            GameManager.Instance.HandleLoss();
            return;
        }

        if (GameManager.Instance.players.Count == 1)
        {
            GameManager.Instance.DeathScreen.SetActive(true);
            if (causeOfDeath == "alien")
            {
                GameManager.Instance.deathScreenReason.text = "Your clone got killed by an Alien!";
            }
            else if (causeOfDeath == "resources")
            {
                GameManager.Instance.deathScreenReason.text = "Your clone ran out of resources!";
            }
            else if (causeOfDeath == "enviroment")
            {
                GameManager.Instance.deathScreenReason.text = "You fell to your death";
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
        int layerMaskAlien = 1 << 9; // Lyer 9 is Alien
        //aliensInRangePlayerCount = Physics.OverlapSphereNonAlloc(MyTransform.position, playerDetectionRadius, aliensInRangePlayer, layerMaskAlien);

        aliensInRangePlayer.Clear();
        foreach (var item in Physics.OverlapSphere(MyTransform.position, playerDetectionRadius, layerMaskAlien))
        {
            aliensInRangePlayer.Add(item);
        }

        aliensInRangePlayerCount = aliensInRangePlayer.Count;

        if (aliensInRangePlayerCount == 0) { return; }

        for (int i = 0; i < aliensInRangePlayerCount; i++)
        {
            if (aliensInRangePlayer[i] == null || aliensInRangePlayer[i].gameObject.activeInHierarchy == false) { continue; }

            CurrentSurroundingAH = aliensInRangePlayer[i].gameObject.GetComponentInParent<AlienHandler>();

            if (CurrentSurroundingAH.targetAlien = this.gameObject) { continue; } // Check if already 
            if (CurrentSurroundingAH.brainWashed == true) { continue; } // Interaction with player in TutorialScene, prevents HandleUpdateTarget error
            if (CurrentSurroundingAH.currentAge == AlienHandler.AlienAge.resource) // If sorrounding Alien is resource, put into resource Array
            {
                closestResource[CurrentSurroundingAH.currentSpecies] = CurrentSurroundingAH;
                continue;
            }

            CurrentSurroundingAH.SetTarget(this.gameObject);

            // TODO: need a way to do StopCoroutine(CurrentSurroundingAH.IdleSecsUntilNewState(what params here?))
            if (CurrentSurroundingAH.currentAge == AlienHandler.AlienAge.fullyGrown)
            {
                if (CurrentSurroundingAH.currentSpecies == 0 && AlienManager.Instance.sphereKilled > 20)
                {
                    CurrentSurroundingAH.isAttackingPlayer = true;
                    CurrentSurroundingAH.IdleSecsUntilNewState(AlienHandler.AlienState.hunting);
                }
                else if (CurrentSurroundingAH.currentSpecies == 1 && AlienManager.Instance.squareKilled > 20)
                {
                    CurrentSurroundingAH.isAttackingPlayer = true;
                    CurrentSurroundingAH.IdleSecsUntilNewState(AlienHandler.AlienState.hunting);
                }
                else if (CurrentSurroundingAH.currentSpecies == 2 && AlienManager.Instance.triangleKilled > 20)
                {
                    CurrentSurroundingAH.isAttackingPlayer = true;
                    CurrentSurroundingAH.IdleSecsUntilNewState(AlienHandler.AlienState.hunting);
                }
                else
                {
                    CurrentSurroundingAH.isEvadingPlayer = true;
                    CurrentSurroundingAH.IdleSecsUntilNewState(AlienHandler.AlienState.evading);
                }
            }
            else
            {
                CurrentSurroundingAH.isEvadingPlayer = true;
                CurrentSurroundingAH.IdleSecsUntilNewState(AlienHandler.AlienState.evading);
            }
        }
    }

    private void HandleResourceDetection(int neededResource)
    {
        // Check the initla List if has resource already in mind
        if (closestResource[neededResource] != null)
        {
            if (closestResource[neededResource].currentAge != AlienHandler.AlienAge.resource ||
                closestResource[neededResource].gameObject.activeInHierarchy == false)
            {
                closestResource[neededResource] = null;
                return;
            }

            closestResourceIndicator[neededResource].SetActive(true);
            HandleResourceDetectionIndicator(closestResource[neededResource], neededResource);
            return;
        }

        // Check the resourceList of AlienManager for available resources (Might obsolute since no simulation)
        if (closestResource[neededResource] == null)
        {
            closestResourceIndicator[neededResource].SetActive(false);
        }

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
                    return;
                }
            }
        }

        // BackUp spawning new resource in case of none available
        Debug.Log("BackUp Resourc spawn");
        if (closestResource[neededResource] == null)
        {
            for (int i = 0; i < AlienManager.Instance.allAlienHandlers.Count; i++)
            {
                if (AlienManager.Instance.allAlienHandlers[i].currentSpecies != neededResource) { continue; }

                locationForResource = AlienManager.Instance.allAlienHandlers[i].transform.position;
                distToAlien = Vector3.Distance(locationForResource, MyTransform.position);

                if (distToAlien < 50) { continue; }

                AlienManager.Instance.allAlienHandlers[i].gameObject.SetActive(false);

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
                return;
            }
        }
    }

    private void HandleResourceDetectionIndicator(AlienHandler targetAlienResource, int neededResource)
    {
        if (targetAlienResource == null)
        {
            closestResourceIndicator[neededResource].SetActive(false);
            return;
        }

        closestResourceIndicator[neededResource].SetActive(true);
        closestResourceIndicator[neededResource].transform.LookAt(targetAlienResource.transform.position + Vector3.up);
    }

    private void DeactivateResourceDetectionIndicator(int neededResource)
    {
        closestResourceIndicator[neededResource].SetActive(false);
    }

    private void HandleResouceDrain(float delta)
    {
        if (isAlive == false) { return; }
        if (currentSphereResource > 0) { currentSphereResource -= resourceDrain * delta; }
        if (currentSquareResource > 0) { currentSquareResource -= resourceDrain * delta; }
        if (currentTriangleResource > 0) { currentTriangleResource -= resourceDrain * delta; }


        // Will go from 0 to max
        currentSphereResourceInverse = maxSphereResource - currentSphereResource;
        currentSquareResourceInverse = maxSquareResource - currentSquareResource;
        currentTriangleResourceInverse = maxTriangleResource - currentTriangleResource;

        MaterialEmmissionControler();
    }
    private void HandleResource()
    {
        if (isAlive == false) { return; }
        // 0:Sphere, 1:Square, 2:Triangle



        // Only show resource UI if below 50%
        if (currentSphereResource < 2 * maxSphereResource / 4) { HandleResourceDetection(0); }
        else { DeactivateResourceDetectionIndicator(0); }

        // Only show resource UI if below 50%
        if (currentSquareResource < 2 * maxSquareResource / 4) { HandleResourceDetection(1); }
        else { DeactivateResourceDetectionIndicator(1); }

        // Only show resource UI if below 50%
        if (currentTriangleResource < 2 * maxTriangleResource / 4) { HandleResourceDetection(2); }
        else { DeactivateResourceDetectionIndicator(2); }

        // Update UI
        resourcePieCharts[0].fillAmount = currentSphereResource / maxSphereResource;
        resourcePieCharts[1].fillAmount = currentSquareResource / maxSquareResource;
        resourcePieCharts[2].fillAmount = currentTriangleResource / maxTriangleResource;

        // Check if enough resources
        if (
            currentSphereResource <= 0 ||
            currentSquareResource <= 0 ||
            currentTriangleResource <= 0
            )
        {
            HandleDeath("resources");
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
                Debug.Log("DeathScreen#");
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

    public IEnumerator UnfoldResource(GameObject Resource, float degree)
    {
        RectTransform GORT;
        GORT = Resource.GetComponent<RectTransform>();
        float delta = 0;

        WaitForEndOfFrame frame = new WaitForEndOfFrame();
        Resource.gameObject.SetActive(true);

        while (delta < HUDHandler.Instance.scalingTransitionDuration)
        {
            GORT.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 3, delta / HUDHandler.Instance.scalingTransitionDuration);
            GORT.localEulerAngles = new Vector3(0, 0, degree * delta / HUDHandler.Instance.scalingTransitionDuration);
            delta += Time.deltaTime;

            yield return frame;
        }
    }


    public IEnumerator FoldResource(GameObject Resource, float degree)
    {
        RectTransform GORT;
        GORT = Resource.GetComponent<RectTransform>();
        float delta = 0;

        WaitForEndOfFrame frame = new WaitForEndOfFrame();
        Resource.gameObject.SetActive(true);

        while (delta < HUDHandler.Instance.scalingTransitionDuration)
        {
            GORT.localScale = Vector3.Lerp(Vector3.one * 3, Vector3.zero, delta / HUDHandler.Instance.scalingTransitionDuration);
            GORT.localEulerAngles = new Vector3(0, 0, degree - (degree * delta / HUDHandler.Instance.scalingTransitionDuration));
            delta += Time.deltaTime;

            yield return frame;
        }

        Resource.gameObject.SetActive(false);
    }

    IEnumerator ShieldBreak(float timeToRecharge)
    {
        hasShield = false;
        stepsShield = 30;
        //animationDurationShield = .5f;
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
        // dissolve.SetFloat("_DissolveAmount", 0.016f);
        hasShield = true;
        audioSource.PlayOneShot(shieldRechargeAudio, 1f);
        playerShieldGO.SetActive(true);

    }
}

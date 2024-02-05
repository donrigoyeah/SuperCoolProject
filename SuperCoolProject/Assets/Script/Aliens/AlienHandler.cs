using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AlienHandler : MonoBehaviour
{
    public enum AlienState
    {
        looking,
        hunting,
        evading,
        loving,
        roaming,
        resource,
        idle
    }

    public AlienState currentStateValue; //this holds the actual value, should be private
    public AlienState currentState
    {
        get
        {
            return currentStateValue;
        }
        set
        {
            currentStateValue = value;
            HandleStateIcon(currentStateValue);
            // Handle Behaviour
            //switch (value)
            //{
            //    case AlienState.looking:
            //        HandleLooking();
            //        break;
            //    case AlienState.hunting:
            //        HandleAttacking(targetAlien);
            //        break;
            //    case AlienState.evading:
            //        HandleFleeing(targetAlien);
            //        break;
            //    case AlienState.loving:
            //        HandleLoveApproach(targetAlien);
            //        break;
            //    case AlienState.roaming:
            //        HandleRoaming();
            //        break;
            //    case AlienState.resource:
            //        break;
            //    case AlienState.idle: // Do nothing
            //        HandleIdleAnimation();
            //        break;
            //}
        }
    } //this is public and accessible, and should be used to change "State"

    public enum AlienAge
    {
        resource,
        child,
        sexualActive,
        fullyGrown
    }

    #region Variables
    public AlienState lastAlienState;
    private int layerMaskAlien = 1 << 9; // Lyer 9 is Alien
    public List<Collider> aliensInRange = new List<Collider>(10);
    private Collider[] aliensInRangeCollider;
    private Collider[] aliensInRangeColliderOrdered;
    public int aliensInRangeCount;
    private float worldRadiusSquared;
    private int amountOfBabies;
    private BulletHandler CurrentBH;
    private float currentBulletDamage;

    public float resourceScale = 0.7f;
    public float childScale = 0.6f;
    public float sexualActiveScale = 0.8f;
    public float fullGrownScale = 1f;

    public bool brainWashed = false;

    [Header("This Alien")]
    private Rigidbody MyRigidbody;
    public Transform MyTransform;
    private Vector2 MyTransform2D;
    public AlienAge currentAge;
    public Collider MyCollisionCollider;

    public bool spawnAsAdults = false;
    public bool gotAttackedByPlayer = false;
    public bool isRendered = true;
    public bool canAct = true;
    public int currentSpecies;
    public bool hasUterus;
    public float alienHealth;
    public bool isDead = true;
    public float lifeTime;
    public float lustTimer = 0;
    public float hungerTimer = 0;
    public float lustTimerThreshold = 5;
    public int maxAmountOfBabies = 10;
    public float hungerTimerThreshold = 5;
    public RawImage currentStateIcon;
    public Texture[] allStateIcons; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
    public Vector3 targetPosition3D;
    private Vector2 targetPosition2D;
    public float distanceToCurrentTarget;
    private float currentShortestDistanceLooking;
    private float currentDistanceLooking;
    private float randDirXRoaming;
    private float randDirZRoaming;
    private GameObject newBornAlienPoolGo;
    private AlienHandler newBornAlien;
    private float randomOffSetBabySpawn;

    [Header("Target Alien")]
    public GameObject targetAlien;
    public GameObject lastTargetAlien;
    public AlienHandler targetAlienHandler;
    public Transform TargetAlienTransform;
    public AlienHandler otherAlienHandler;
    private Vector2 TargetAlienTransform2D;

    [Header("General AlienStuff")]
    public GameObject[] alienSpecies; // 0:Sphere > 1:Square > 2:Triangle  
    public GameObject[] alienSpeciesChild; // 0:Sphere > 1:Square > 2:Triangle  
    public GameObject[] alienSpeciesAdult; // 0:Sphere > 1:Square > 2:Triangle  
    public Animation[] anim;
    public Renderer alienMiniMapMarker;
    public GameObject resourceSteamGO;
    public GameObject alienActionParticlesGO;
    public ParticleSystem resourceSteam;
    public ParticleSystem alienActionFog;
    ParticleSystem.MainModule alienActionFogMain;
    ParticleSystem.MainModule resourceSteamMain;
    public float alienSpeed;
    public float lookRadius = 10;
    private float delta;
    private float speed;
    private int alienLifeResource = 1;
    private int alienLifeChild = 30;
    private int alienLifeSexual = 40;
    private int alienLifeFullGrown = 50;
    public int minTimeToChild = 5;
    public int timeToSexual = 15;
    public int timeToFullGrown = 25;
    private GameObject deadAlienGO;
    private DeadAlienHandler deadAlien;
    private Vector2 CameraFollowSpot2D;
    private float randomNumber;
    public float distanceToCameraSpot;
    private float lookTimeIdle;
    private bool isPlayerBullet;
    private GameObject damageUIGo;
    private DamageUIHandler DUIH;

    [Header("Dissolve")]
    public Material dissolve;
    public SkinnedMeshRenderer skinRenderer1;
    public SkinnedMeshRenderer skinRenderer2;
    public SkinnedMeshRenderer skinRenderer3;
    public Material[] orignalMaterial;
    public float dissolveRate = 0.0125f;
    public float refreshRate = 0.025f;

    [Header("Alien Audio")]
    private bool playClipSpawned = false;
    public List<AudioClip> aliensEating;
    public List<AudioClip> aliensLoving;
    public AudioSource audioSource;

    [Header("Water / Sphere Alien Audio")]
    public AudioClip[] sphereAttackAudio;
    public AudioClip[] sphereDyingAudio;
    public AudioClip[] sphereBeingAttackedAudio;
    public AudioClip[] sphereLovemakingAudio;
    public AudioClip[] sphereEvadingAudio;

    [Header("Oxygen / Square Alien Audio")]
    public AudioClip[] squareAttackAudio;
    public AudioClip[] squareDyingAudio;
    public AudioClip[] squareBeingAttackedAudio;
    public AudioClip[] squareLovemakingAudio;
    public AudioClip[] squareEvadingAudio;

    [Header("Meat / Triangle Alien Audio")]
    public AudioClip[] triangleAttackAudio;
    public AudioClip[] triangleDyingAudio;
    public AudioClip[] triangleBeingAttackedAudio;
    public AudioClip[] triangleLovemakingAudio;
    public AudioClip[] triangleEvadingAudio;

    [Header("Array of all alien state")]
    public List<AudioClip[]> attackAudioList = new List<AudioClip[]>();
    private List<AudioClip[]> dyingAudioList = new List<AudioClip[]>();
    private List<AudioClip[]> beingAttackedAudioList = new List<AudioClip[]>();
    private List<AudioClip[]> lovemakingAudioList = new List<AudioClip[]>();
    private List<AudioClip[]> evadingAudioList = new List<AudioClip[]>();

    [Header("Tick stats")]
    public float tickTimer;
    public float tickTimerMax = .5f;
    public bool hasNewTarget = false;


    [Header("DeadAliens")] // 0:Sphere > 1:Square > 2:Triangle
    public GameObject[] deadAliensPrerfabs;

    #endregion

    private void Awake()
    {
        MyRigidbody = this.gameObject.GetComponent<Rigidbody>();
        resourceSteamMain = resourceSteamGO.GetComponent<ParticleSystem>().main;
    }

    private void Start()
    {
        attackAudioList.Add(sphereAttackAudio);
        attackAudioList.Add(squareAttackAudio);
        attackAudioList.Add(triangleAttackAudio);

        dyingAudioList.Add(sphereDyingAudio);
        dyingAudioList.Add(squareDyingAudio);
        dyingAudioList.Add(triangleDyingAudio);

        beingAttackedAudioList.Add(sphereBeingAttackedAudio);
        beingAttackedAudioList.Add(squareBeingAttackedAudio);
        beingAttackedAudioList.Add(triangleBeingAttackedAudio);

        lovemakingAudioList.Add(sphereLovemakingAudio);
        lovemakingAudioList.Add(squareLovemakingAudio);
        lovemakingAudioList.Add(triangleLovemakingAudio);

        evadingAudioList.Add(sphereEvadingAudio);
        evadingAudioList.Add(squareEvadingAudio);
        evadingAudioList.Add(triangleEvadingAudio);

        worldRadiusSquared = GameManager.Instance.worldRadius * GameManager.Instance.worldRadius;
        alienActionFogMain = alienActionFog.gameObject.GetComponent<ParticleSystem>().main;
        if (MyTransform == null) { MyTransform = this.gameObject.GetComponent<Transform>(); }
    }

    private void OnEnable()
    {
        ActivateCurrentModels(currentSpecies);

        if (brainWashed == true)
        {
            StartCoroutine(UndoBrainWash(10));
        }
        else
        {
            StartCoroutine(HandleAge(spawnAsAdults));
        }
    }

    private void OnDisable()
    {
        ResetVariable();
        DiscardCurrentAction();
        StopAllCoroutines();
        brainWashed = false;
    }

    private void FixedUpdate()
    {
        HandleDistanceCalculation();
        HandleRendering();

        if (isRendered == false) { return; }
        if (canAct == false) { return; }

        delta = Time.deltaTime;
        HandleUpdateVariables();
        HandleUpdateTarget();

        if (currentAge == AlienAge.resource || isDead == true) { return; }
        HandleAnimation();

        // Finaly move the alien if it can
        HandleMovement();

        //if (tickTimer >= tickTimerMax + randomNumber)
        //{
        //    // Reset Tick timer
        //    tickTimer -= tickTimerMax;

        if (currentState == AlienState.hunting)
        {
            HandleAttacking(targetAlien);
            return;
        }
        if (currentState == AlienState.evading)
        {
            HandleFleeing(targetAlien);
            return;
        }
        if (currentState == AlienState.loving)
        {
            HandleLoveApproach(targetAlien);
            return;
        }
        if (currentState == AlienState.roaming)
        {
            HandleRoaming();
            return;
        }
        if (currentState == AlienState.looking)
        {
            HandleLooking();
            return;
        }
        //}
    }

    private void HandleDistanceCalculation()
    {
        MyTransform2D = new Vector2(MyTransform.position.x, MyTransform.position.z);
        CameraFollowSpot2D = new Vector2(GameManager.Instance.CameraFollowSpot.position.x, GameManager.Instance.CameraFollowSpot.position.z);
        distanceToCameraSpot = Vector2.Distance(MyTransform2D, CameraFollowSpot2D);
    }

    private void HandleRendering()
    {
        if (distanceToCameraSpot > renderDistance)
        {
            if (isRendered == true)
            {
                DeactivateAllModels();
                isRendered = false;
            }
        }
        else
        {
            if (isRendered == false)
            {
                ActivateCurrentModels(currentSpecies);
                isRendered = true;
            }
        }
    } // Commented out for now

    public void SetTarget(GameObject currentTargetGO)
    {
        if (targetAlien != null)
        {
            lastTargetAlien = targetAlien;
        }

        targetAlien = currentTargetGO;
    }

    public void HandleUpdateTarget()
    {
        if (brainWashed == true) { return; }

        if (currentState != AlienState.roaming)
        {
            if (TargetAlienTransform == null) { return; }

            if (currentState == AlienState.evading) // Away from target
            {
                targetPosition3D = MyTransform.position + (MyTransform.position - TargetAlienTransform.position);
            }
            else // towards target
            {
                targetPosition3D = TargetAlienTransform.position;
            }
        }

        if (targetPosition3D == Vector3.zero) { return; }

        targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
        distanceToCurrentTarget = Vector2.Distance(MyTransform2D, targetPosition2D);
    }

    private void DiscardCurrentAction()
    {
        if (targetAlien == null) { return; }

        lastTargetAlien = targetAlien;
        targetAlien = null;
        TargetAlienTransform = null;
        targetPosition3D = Vector3.zero;
    }

    private void HandleUpdateVariables()
    {
        lifeTime += delta;
        lustTimer += delta;
        hungerTimer += delta;
        tickTimer += delta;
        lifeTime += delta;
        speed = (alienSpeed + ((lustTimer + hungerTimer) / 100)) * delta; // + ((2 * (lustTimer + hungerTimer)) / (lustTimer + hungerTimer)); TODO: make better?! Way too fast

        randomNumber = Random.Range(1, 11) / 10;
    }

    private void ResetVariable()
    {
        lustTimer = 0;
        hungerTimer = 0;
        lifeTime = 0;
        MyRigidbody.velocity = Vector3.zero;
        currentAge = AlienAge.resource;
        minTimeToChild += UnityEngine.Random.Range(0, 10); // This just get added on top of minTimeToChild 
        hasUterus = UnityEngine.Random.Range(0, 2) == 1;
        alienHealth = alienLifeResource;
        brainWashed = false; // AKA tutuorial scene
        canAct = true;
        isDead = false;
        spawnAsAdults = false;
        gotAttackedByPlayer = false;
    }

    public void DeactivateAllModels()
    {
        for (int i = 0; i < alienSpecies.Length; i++)
        {
            alienSpecies[i].SetActive(false);
            alienSpeciesChild[i].SetActive(false);
            alienSpeciesAdult[i].SetActive(false);
        }
    }

    public void ActivateCurrentModels(int currentSpeziesIndex)
    {
        DeactivateAllModels();
        if (currentAge == AlienAge.resource)
        {
            alienSpeciesChild[currentSpeziesIndex].SetActive(true);
        }
        else
        {
            alienSpeciesAdult[currentSpeziesIndex].SetActive(true);
        }
        alienSpecies[currentSpeziesIndex].SetActive(true);

        MyCollisionCollider = alienSpecies[currentSpeziesIndex].GetComponentInChildren<Collider>();
    }

    private IEnumerator HandleAge(bool isSpawningAsAdult)
    {
        if (isSpawningAsAdult == false)
        {
            // Resource Life
            UpdateResourceSteam(currentSpecies);
            resourceSteamGO.SetActive(true);
            MyRigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            currentAge = AlienAge.resource;
            alienHealth = alienLifeResource;
            MyTransform.localScale = Vector3.one * resourceScale;
            yield return new WaitForSeconds(minTimeToChild);
        }

        // Child Life
        resourceSteamGO.SetActive(false);
        MyRigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        alienHealth = alienLifeChild;
        currentAge = AlienAge.child;
        currentState = AlienState.roaming;
        MyTransform.localScale = Vector3.one * childScale;
        alienSpeciesChild[currentSpecies].SetActive(false);
        alienSpeciesAdult[currentSpecies].SetActive(true);
        if (AlienManager.Instance.resourceSphere.Count + AlienManager.Instance.resourceSquare.Count + AlienManager.Instance.resourceTriangle.Count > 0)
        {
            AlienManager.Instance.RemoveFromResourceList(this); // TODO: Check if available in List?!
        }
        yield return new WaitForSeconds(timeToSexual);

        // Sexual active Life
        alienHealth = alienLifeSexual;
        currentAge = AlienAge.sexualActive;
        StartCoroutine(HandleGrowing(childScale, sexualActiveScale));
        yield return new WaitForSeconds(timeToFullGrown);

        // Full Grown Life
        alienHealth = alienLifeFullGrown;
        currentAge = AlienAge.fullyGrown;
        StartCoroutine(HandleGrowing(sexualActiveScale, fullGrownScale));
    }

    private IEnumerator HandleGrowing(float oldFactor, float newFactor)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.5f / 10); // Total duration of transform 0.5f seconds
            MyTransform.localScale = Vector3.one * ((oldFactor + newFactor * i / 10) - (oldFactor * i / 10));
        }
    }



    public void HandleDeath()
    {
        isDead = true;
        brainWashed = false;
        anim[currentSpecies].Stop();
        StopAllCoroutines();
        this.gameObject.SetActive(false);
        return;
    }

    public void HandleDeathByBullet(bool isPlayerBullet, Vector3 bulletForce)
    {
        if (isPlayerBullet)
        {
            AlienManager.Instance.KillAlien(currentSpecies);
        }
        // deadAlienGO = PoolManager.Instance.GetPooledDeadAlien();

        // Instantiate(deadAliensPrerfabs[currentSpecies], this.transform.position, this.transform.rotation);

        // 0:Sphere > 1:Square > 2:Triangle

        if (currentSpecies == 0)
        {
            deadAlienGO = PoolManager.Instance.GetPooledDeadSphereAlien();
            deadAlienGO.transform.localPosition = MyTransform.position;
            deadAlienGO.transform.localRotation = MyTransform.rotation;
            deadAlienGO.gameObject.SetActive(true);
        }

        if (currentSpecies == 1)
        {
            deadAlienGO = PoolManager.Instance.GetPooledDeadSquareAlien();
            deadAlienGO.transform.localPosition = MyTransform.position;
            deadAlienGO.transform.localRotation = MyTransform.rotation;
            deadAlienGO.gameObject.SetActive(true);
        }

        if (currentSpecies == 2)
        {
            deadAlienGO = PoolManager.Instance.GetPooledDeadTriangleAlien();
            deadAlienGO.transform.localPosition = MyTransform.position;
            deadAlienGO.transform.localRotation = MyTransform.rotation;
            deadAlienGO.gameObject.SetActive(true);
        }

        Debug.Break();
        /*if (deadAlienGO != null)
        {
            deadAlienGO.transform.position = MyTransform.position;
            deadAlienGO.transform.rotation = MyTransform.rotation;

            deadAlien = deadAlienGO.GetComponent<DeadAlienHandler>();
            deadAlien.Rigidbodies[currentSpecies].position = this.transform.position;
            deadAlien.transform.rotation = MyTransform.rotation;
            deadAlien.bulletForce = bulletForce;
            deadAlien.currentAlienSpecies = currentSpecies;

        }*/
        Debug.Log("TODO: Dead Alien Ragdoll here");
        StartCoroutine(WaitForDeath(.2f));

    }

    public void HandleDeathByCombat()
    {
        isDead = true;
        StartCoroutine(PlayActionParticle(AlienState.hunting));
        StartCoroutine(WaitForDeath(1));
    }

    private IEnumerator WaitForDeath(float time)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(dyingAudioList, currentSpecies), 1f);
        }
        yield return new WaitForSeconds(time);
        HandleDeath();
    }



    public void BrainwashAlien()
    {
        brainWashed = true;
        resourceSteamGO.SetActive(false);
        StopAllCoroutines();
    }

    private IEnumerator UndoBrainWash(float time)
    {
        yield return new WaitForSeconds(time);
        brainWashed = false; // AKA tutuorial scene
        StartCoroutine(HandleAge(true));
    }


    #region Alien Actions
    private void HandleMovement()
    {
        // Outside bounds
        if ((targetPosition3D.x * targetPosition3D.x + targetPosition3D.z * targetPosition3D.z) > worldRadiusSquared) { HandleDeath(); }

        // Place on Floor
        if (MyTransform.position.y != -1) { MyTransform.position = new Vector3(MyTransform.position.x, -1, MyTransform.position.z); }

        // If not yet set to targetPosition
        if (targetPosition3D == Vector3.zero) { return; }

        // Prevent spinning or tilting
        if (distanceToCurrentTarget > 1) { MyTransform.LookAt(targetPosition3D); }

        // If brainwashed or any state but idle, alien can move
        if (brainWashed == true || currentState != AlienState.idle)
        {
            MyTransform.position = Vector3.MoveTowards(MyTransform.position, targetPosition3D, speed);
        }

        // Act upon state and distance to current target
        if (currentState == AlienState.evading || currentState == AlienState.hunting)
        {
            if (distanceToCurrentTarget > lookRadius + 1)  // Add +1 so i is out of the lookradius
            {
                StartCoroutine(IdleSecsUntilNewState(AlienState.looking));
            }
        }
        else // AlienStates: .resource .loving .looking .roaming
        {
            if (distanceToCurrentTarget < 1f)
            {
                // Prevent the tutorial brainwashed aliens to walk away
                if (brainWashed == true) { return; }
                StartCoroutine(IdleSecsUntilNewState(AlienState.looking));
            }
        }
    }

    public void HandleLooking()
    {
        currentShortestDistanceLooking = lookRadius;
        currentDistanceLooking = lookRadius;

        aliensInRangeCount = aliensInRange.Count;

        // If does not have an array of nearby aliens, create one
        if (aliensInRangeCount == 0)
        {
            aliensInRangeCollider = Physics.OverlapSphere(MyTransform.position, lookRadius, layerMaskAlien, QueryTriggerInteraction.Ignore);
            aliensInRangeColliderOrdered = aliensInRangeCollider.OrderBy(c => (MyTransform.position - c.transform.position).sqrMagnitude).ToArray();

            foreach (var item in aliensInRangeColliderOrdered)
            {
                aliensInRange.Add(item);
            }
        }

        // If did not find any, just go back to roaming
        if (aliensInRange.Count == 0)
        {
            StartCoroutine(IdleSecsUntilNewState(AlienState.roaming));
            return;
        }

        // Big Loop over all aliens in range until match is found
        for (int i = 0; i < aliensInRangeCount; i++)
        {
            if (aliensInRange[i] == null) { continue; }

            // Prevent checking on self and last alien
            if (aliensInRange[i] == MyCollisionCollider) { continue; }

            //if (aliensInRange[i].gameObject == lastTargetAlien) { continue; } // Can go behind last target if other interference happend in between
            if (aliensInRange[i].gameObject.activeInHierarchy == false) { continue; }

            targetAlienHandler = aliensInRange[i].gameObject.GetComponentInParent<AlienHandler>();

            if (targetAlienHandler.currentAge == AlienAge.resource) { continue; }

            // Find potential Alien to trigger certain State
            if (currentSpecies == targetAlienHandler.currentSpecies)
            {
                if (
                    hasUterus != targetAlienHandler.hasUterus && // opposite Sex
                    currentAge == AlienAge.sexualActive && // Sexual active
                    targetAlienHandler.currentAge == AlienAge.sexualActive && // potential partner also sexual active
                    lustTimer > lustTimerThreshold && // can mate
                    targetAlienHandler.lustTimer > lustTimerThreshold // partner can mate
                    )
                {
                    currentState = AlienState.loving;
                    SetTarget(aliensInRange[i].gameObject);

                    targetAlienHandler.currentState = AlienState.loving;
                    targetAlienHandler.SetTarget(this.gameObject);
                    break;
                }
            }
            else
            {
                #region Who Eats Who
                // This aliens eats the other
                // 0:Sphere > 1:Square > 2:Triangle 
                // Triangle eats Square / 2 eats 1
                // Square eats Sphere / 1 eats 0
                // Sphere eats Triangle / 0 eats 2
                #endregion

                if (hungerTimer > hungerTimerThreshold &&
                    (currentSpecies == targetAlienHandler.currentSpecies + 1 ||
                    (currentSpecies == 0 && targetAlienHandler.currentSpecies == 2))) // potential food || if closestAlienHandler is smaller || hunting state
                {
                    currentState = AlienState.hunting;
                    SetTarget(aliensInRange[i].gameObject);

                    targetAlienHandler.currentState = AlienState.evading;
                    targetAlienHandler.SetTarget(this.gameObject);
                }
                else if ((currentSpecies == targetAlienHandler.currentSpecies - 1 ||
                    (currentSpecies == 2 && targetAlienHandler.currentSpecies == 0))) // 0:Sphere > 1:Square > 2:Triangle || if closestAlienHandler is bigger
                {
                    currentState = AlienState.evading;
                    SetTarget(aliensInRange[i].gameObject);
                }
            }

            #region pick better target

            //if (TargetAlienTransform == null) { continue; } // Continue the for loop to find another target

            //TargetAlienTransform2D = new Vector2(TargetAlienTransform.position.x, TargetAlienTransform.position.z);
            //currentDistanceLooking = Vector2.Distance(MyTransform2D, TargetAlienTransform2D);

            //if (currentDistanceLooking < currentShortestDistanceLooking)
            //{
            //    currentShortestDistanceLooking = currentDistanceLooking;
            //}

            //if (currentShortestDistanceLooking <= 2)
            //{
            //    break;
            //}
            #endregion
        }

        // Set state on closest target
        if (targetAlien == null)
        {
            Debug.Log("found nothing");
            StartCoroutine(IdleSecsUntilNewState(AlienState.roaming));
            return;
        }

        #region handle state change if better target code is used
        //if (currentSpecies == targetAlienHandler.currentSpecies)
        //{
        //    StartCoroutine(IdleSecsUntilNewState(1f, AlienState.loving));
        //    targetAlienHandler.currentState = AlienState.loving;
        //    targetAlienHandler.targetAlienHandler = this;
        //    return;
        //}

        //if (currentSpecies == targetAlienHandler.currentSpecies - 1 ||
        //   (currentSpecies == 2 && targetAlienHandler.currentSpecies == 0)) // If target is bigger
        //{

        //    StartCoroutine(IdleSecsUntilNewState(1f, AlienState.evading));
        //    return;
        //}

        //if (currentSpecies == targetAlienHandler.currentSpecies + 1 ||
        //   (currentSpecies == 0 && targetAlienHandler.currentSpecies == 2)) // If target is smaller
        //{
        //    StartCoroutine(IdleSecsUntilNewState(1f, AlienState.hunting));
        //    targetAlienHandler.currentState = AlienState.evading;
        //    return;
        //}

        #endregion

        #region Loop over List approach
        //for (int i = 0; i < PoolManager.Instance.AlienPool.Count; i++)  //list of gameObjects to search through
        //{
        //    if (PoolManager.Instance.AlienPool[i] == this.gameObject || PoolManager.Instance.AlienPool[i] == lastClosestAlien) continue;

        //    float dist = Vector3.Distance(PoolManager.Instance.AlienPool[i].transform.position, transform.position);
        //    if (dist < lookRadius)
        //    {
        //        closestAlien = PoolManager.Instance.AlienPool[i];
        //        closestAlienIndex = closestAlien.GetComponent<AlienHandler>().currentSpecies;
        //        break;
        //    }
        //}
        //return closestAlien;
        #endregion
    }

    private void HandleRoaming()
    {
        Debug.Log("do roaming");

        if (hasNewTarget == true) { return; }
        if (brainWashed == true) { return; }

        if (targetPosition3D == Vector3.zero)
        {
            Debug.Log("TargetPosition3D is ZEROOOOOOOOO");
            randDirXRoaming = Random.Range(0, 2) - .5f;
            randDirZRoaming = Random.Range(0, 2) - .5f;
            targetPosition3D = MyTransform.position + new Vector3(randDirXRoaming, 0, randDirZRoaming) * 20;
            targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
            distanceToCurrentTarget = Vector2.Distance(MyTransform2D, targetPosition2D);
        }
        // Find new target that is not too close, inside the ring and has no other object there yet
        while (
            (targetPosition3D.x * targetPosition3D.x + targetPosition3D.z * targetPosition3D.z) > worldRadiusSquared ||
            distanceToCurrentTarget < 1 ||
            Physics.OverlapSphere(new Vector3(targetPosition3D.x, 1, targetPosition3D.z), .1f).Length > 0
            )
        {
            Debug.Log("TargetPosition3D is in loop");
            randDirXRoaming = Random.Range(0, 2) - .5f;
            randDirZRoaming = Random.Range(0, 2) - .5f;
            targetPosition3D = MyTransform.position + new Vector3(randDirXRoaming, 0, randDirZRoaming) * 20;
            targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
            distanceToCurrentTarget = Vector2.Distance(MyTransform2D, targetPosition2D);
            hasNewTarget = true;
        }
    }

    public void HandleFleeing(GameObject currentTargetGO)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(evadingAudioList, currentSpecies), 1f);
        }

        if (brainWashed == true) { return; }

        if (currentTargetGO == null) { return; }
        SetTarget(currentTargetGO);
    } // Use this here on the player as well to scare the aliens away

    public void HandleAttacking(GameObject currentTargetGO) // Player makes them flee as well and by acting als targetAlien in PlayerManager
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(attackAudioList, currentSpecies), 1f);
        }

        if (brainWashed == true) { return; }

        if (currentTargetGO == null) { return; }
        SetTarget(currentTargetGO);
    }

    private void HandleLoveApproach(GameObject currentTargetGO)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(attackAudioList, currentSpecies), 1f);
        }

        if (brainWashed == true) { return; }

        if (currentTargetGO == null) { return; }
        SetTarget(currentTargetGO);
    }

    private void HandleMating()
    {
        // Check if possible to spawn more aliens
        if (brainWashed == false && PoolManager.Instance.currentAlienAmount >= PoolManager.Instance.alienAmount + PoolManager.Instance.alienAmountExtra)
        {
            StartCoroutine(IdleSecsUntilNewState(AlienState.looking));
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(lovemakingAudioList, currentSpecies), 1f);
        }

        if (hasUterus == true)
        {
            amountOfBabies = UnityEngine.Random.Range(1, maxAmountOfBabies);
            if (brainWashed == true) { amountOfBabies = 1; }
            for (var i = 0; i < amountOfBabies; i++)
            {
                newBornAlienPoolGo = PoolManager.Instance.GetPooledAliens(brainWashed);
                if (newBornAlienPoolGo != null)
                {
                    randomOffSetBabySpawn = (UnityEngine.Random.Range(0, 5) - 2) / 2;

                    newBornAlien = newBornAlienPoolGo.GetComponent<AlienHandler>();
                    newBornAlien.currentSpecies = currentSpecies;
                    newBornAlien.ActivateCurrentModels(currentSpecies);
                    newBornAlien.transform.position = new Vector3(MyTransform.position.x + randomOffSetBabySpawn, 0.5f, MyTransform.position.z + randomOffSetBabySpawn);
                    newBornAlien.gameObject.SetActive(true);
                }
            }
        }
        if (brainWashed)
        {
            return;
        }

        StartCoroutine(IdleSecsUntilNewState(AlienState.looking));
    }

    #endregion


    public IEnumerator IdleSecsUntilNewState(AlienState nextState)
    {
        canAct = false;
        hasNewTarget = false;
        if (brainWashed == false)
        {
            DiscardCurrentAction(); // To prevent the lost of target Alien
        }
        currentState = nextState;
        lookTimeIdle = UnityEngine.Random.Range(1, (randomNumber + 1) * 10) / 10;
        yield return new WaitForSeconds(lookTimeIdle);
        canAct = true;
    }
    private IEnumerator PlayActionParticle(AlienState currentState)
    {
        if (isRendered == false)
        {
            yield return null;
        }
        else
        {
            if (currentState == AlienState.loving)
            {
                alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(Color.red, Color.magenta);
            }
            else if (currentState == AlienState.hunting)
            {
                alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(Color.gray, Color.black);
            }

            alienActionParticlesGO.SetActive(true);
            yield return new WaitForSeconds(1f);
            alienActionParticlesGO.SetActive(false);
        }
    }

    public void HandleStateIcon(AlienState currentState)
    {
        switch (currentState)
        {
            case AlienState.roaming:
                currentStateIcon.texture = allStateIcons[5]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
                break;
            case AlienState.evading:
                currentStateIcon.texture = allStateIcons[2]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
                break;
            case AlienState.loving:
                currentStateIcon.texture = allStateIcons[3]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
                break;
            case AlienState.hunting:
                currentStateIcon.texture = allStateIcons[1]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
                break;
            case AlienState.looking:
                currentStateIcon.texture = allStateIcons[0]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
                break;
            default:
                currentStateIcon.texture = allStateIcons[0]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
                break;
        }
    }

    private void UpdateResourceSteam(int currentIndex)
    {
        if (AlienManager.Instance == null) { return; }

        if (currentIndex == 0)
        {
            resourceSteamMain.startColor = AlienManager.Instance.alienColors[currentIndex].color;
            alienMiniMapMarker.material = AlienManager.Instance.alienColors[currentIndex];
        }

        if (currentIndex == 1)
        {
            resourceSteamMain.startColor = AlienManager.Instance.alienColors[currentIndex].color;
            alienMiniMapMarker.material = AlienManager.Instance.alienColors[currentIndex];
        }

        if (currentIndex == 2)
        {
            resourceSteamMain.startColor = AlienManager.Instance.alienColors[currentIndex].color;
            alienMiniMapMarker.material = AlienManager.Instance.alienColors[currentIndex];
        }
    }

    private void HandleAnimation()
    {
        if (anim[currentSpecies] != null) { return; }

        if (currentState == AlienState.hunting || currentState == AlienState.loving || currentState == AlienState.roaming)
        {
            anim[currentSpecies]["Armature|WALK"].speed = 1;
            anim[currentSpecies].Play("Armature|WALK");
        }
        if (currentState == AlienState.evading)
        {
            anim[currentSpecies]["Armature|WALK"].speed = 2;
            anim[currentSpecies].Play("Armature|WALK");
        }
        if (currentState == AlienState.idle)
        {
            if (currentSpecies != 0)
            {
                anim[currentSpecies].Play("Armature|IDLE");
            }
        }
    }

    AudioClip RandomAudioSelectorAliens(List<AudioClip[]> audioList, int species) // incase we plan to add more audio for each state
    {
        // TODO: think of something to have ot play an audio only 50% of the time?
        AudioClip[] selectedAudioArray = audioList[species];

        int randomIndex = Random.Range(0, selectedAudioArray.Length);
        AudioClip selectedAudio = selectedAudioArray[randomIndex];

        return selectedAudio;
    }

    AudioClip RandomAudioSelectorFoley(List<AudioClip> audioList) // incase we plan to add more audio for each state
    {
        int randomIndex = Random.Range(0, audioList.Count);
        AudioClip selectedAudio = audioList[randomIndex];

        return selectedAudio;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (currentAge == AlienAge.resource) { return; }

        // Handle Alien interaction
        if (other.gameObject.transform.parent && other.gameObject.transform.parent.CompareTag("Alien"))
        {
            otherAlienHandler = other.gameObject.GetComponentInParent<AlienHandler>();
            if (otherAlienHandler == null) { return; }

            // Check that no non-intensional interactions are triggered
            if (targetAlienHandler != otherAlienHandler) { return; }

            if (currentSpecies == otherAlienHandler.currentSpecies)
            {
                if (currentState == AlienState.loving && otherAlienHandler.currentState == AlienState.loving)
                {
                    lustTimer = 0;
                    otherAlienHandler.lustTimer = 0;
                    StartCoroutine(PlayActionParticle(AlienState.loving)); // Loving Partilce
                    audioSource.PlayOneShot(RandomAudioSelectorFoley(aliensLoving));
                    HandleMating();
                }
            }
            else
            {
                if (otherAlienHandler.currentAge == AlienAge.resource) // You, the resource, gets trampled
                {
                    AlienManager.Instance.RemoveFromResourceList(otherAlienHandler);
                    otherAlienHandler.HandleDeath();
                    return;
                }
                else
                {
                    // Handles eat other alien
                    hungerTimer = 0;
                    if (other.gameObject.activeInHierarchy)
                    {
                        otherAlienHandler.HandleDeathByCombat();
                        audioSource.PlayOneShot(RandomAudioSelectorFoley(aliensEating));
                    }

                    if (brainWashed == true) { return; }// For tutorial

                    StartCoroutine(IdleSecsUntilNewState(AlienState.looking));
                }
            }
            return;
        }

        // Handle Bullet interaction
        if (other.gameObject.CompareTag("Bullet"))
        {
            if (currentAge == AlienAge.resource) { return; } // Cannot shoot resource

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(RandomAudioSelectorAliens(beingAttackedAudioList, currentSpecies), 1f);
            }

            CurrentBH = other.gameObject.GetComponent<BulletHandler>();
            currentBulletDamage = CurrentBH.bulletDamage;
            alienHealth -= currentBulletDamage;
            isPlayerBullet = CurrentBH.isPlayerBullet;

            // SET Alien state to hunting or evading and setting player as target
            if (gotAttackedByPlayer == false)
            {
                SetTarget(CurrentBH.shootinPlayerAttacker.gameObject);
                lastAlienState = currentState;

                if (Random.Range(0, 2) == 1)
                {
                    currentState = AlienState.hunting;
                    gotAttackedByPlayer = true;
                }
                else
                {
                    currentState = AlienState.evading;
                    gotAttackedByPlayer = true;
                }
            }

            damageUIGo = PoolManager.Instance.GetPooledDamageUI();
            if (damageUIGo != null)
            {
                damageUIGo.transform.position = MyTransform.position;

                DUIH = damageUIGo.GetComponentInChildren<DamageUIHandler>();
                DUIH.damageValue = currentBulletDamage;

                damageUIGo.SetActive(true);
            }

            CurrentBH = null;
            other.gameObject.SetActive(false);

            // Handle Alien Death
            if (alienHealth <= 0 && isDead == false)
            {
                HandleDeathByBullet(isPlayerBullet, other.GetComponent<Rigidbody>().velocity);
            };

            return;
        }
    }

}

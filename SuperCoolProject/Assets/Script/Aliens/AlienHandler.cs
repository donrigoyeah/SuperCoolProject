using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            switch (value)
            {
                case AlienState.looking:
                    HandleLooking();
                    break;
                case AlienState.hunting:
                    HandleAttacking(targetAlien);
                    break;
                case AlienState.evading:
                    HandleFleeing(targetAlien);
                    break;
                case AlienState.loving:
                    HandleLoveApproach(targetAlien);
                    break;
                case AlienState.roaming:
                    HandleRoaming();
                    break;
                case AlienState.resource:
                    break;
                case AlienState.idle: // Do nothing
                    HandleIdleAnimation();
                    break;
            }
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
    private int layerMaskAlien = 1 << 9; // Lyer 9 is Alien
    //public Collider[] aliensInRange = new Collider[10];
    public List<Collider> aliensInRange = new List<Collider>(10);
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
    //public AlienState currentState;
    public Collider MyCollisionCollider;

    public bool spawnAsAdults = false;
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
    public bool hasNewTarget = false;
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
    private List<AudioClip[]> attackAudioList = new List<AudioClip[]>();
    private List<AudioClip[]> dyingAudioList = new List<AudioClip[]>();
    private List<AudioClip[]> beingAttackedAudioList = new List<AudioClip[]>();
    private List<AudioClip[]> lovemakingAudioList = new List<AudioClip[]>();
    private List<AudioClip[]> evadingAudioList = new List<AudioClip[]>();

    [Header("Tick stats")]
    public float tickTimer;
    public float tickTimerMax = .5f;

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
        HandleTickUpdateVariables();
        HandleRendering();

        if (canAct == false) { return; }

        HandleUpdateTarget();
        delta = Time.deltaTime;
        HandleUpdateVariables(delta);

        // For Aliens within 50 units of cameraSpot
        //if (isRendered)
        //{
        //if (tickTimer >= tickTimerMax + randomNumber)
        //{
        //    // Reset Tick timer
        //    tickTimer -= tickTimerMax;
        //    HandleTickUpdateVariables();

        //    // Dont update the target if brainwashed
        //    //if (brainWashed == true || currentState == AlienState.roaming || targetAlien == null) { return; }
        //    HandleUpdateTarget();
        //}
        //}
        //else
        //{
        //    // For Aliens outside 50 units of cameraSpot
        //    if (tickTimer >= tickTimerMax + randomNumber * 3)
        //    {
        //        // Reset Tick timer
        //        tickTimer -= tickTimerMax;
        //        HandleTickUpdateVariables();

        //        // Dont update the target if brainwashed
        //        //if (brainWashed == true || currentState == AlienState.roaming || targetAlien == null) { return; }
        //        HandleUpdateTarget();
        //    }
        //}

        // Finaly move the alien
        HandleMovement();
    }

    #region Functions for Alien Handler
    private void HandleMovement()
    {
        // If is doing action, is resource or dead -> return
        if (canAct == false || currentAge == AlienAge.resource || isDead == true) { return; }

        if ((targetPosition3D.x * targetPosition3D.x + targetPosition3D.z * targetPosition3D.z) > worldRadiusSquared) { HandleDeath(); }

        if (MyTransform.position.y != 0.1f) { MyTransform.position = new Vector3(MyTransform.position.x, 0.1f, MyTransform.position.z); }

        if (anim[currentSpecies] != null) { anim[currentSpecies].Play("Armature|WALK"); }

        // Prevent spinning or tilting
        if (distanceToCurrentTarget > 2)
        {
            MyTransform.LookAt(targetPosition3D);
        }

        // If brainwashed can move anyway
        if (brainWashed == true || currentState != AlienState.idle)
        {
            MyTransform.position = Vector3.MoveTowards(MyTransform.position, targetPosition3D, speed);
        }

        if (currentState == AlienState.evading || currentState == AlienState.hunting)
        {
            if (distanceToCurrentTarget > lookRadius + 1)  // Add +1 so i is out of the lookradius
            {
                if (currentState == AlienState.evading)
                {
                    if (lastTargetAlien != null)
                    {
                        targetAlien = lastTargetAlien;
                    }
                }
                canAct = false;
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                return;
            }
        }
        else // AlienStates: .resource .loving .looking .roaming
        {
            if (distanceToCurrentTarget < 1f)
            {
                canAct = false;
                if (brainWashed == true) { return; }
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                hasNewTarget = false;
            }
        }

    }

    public void HandleLooking()
    {
        if (isRendered == false) { return; }

        if (anim[currentSpecies] != null)
        {
            if (currentSpecies != 0)
            {
                anim[currentSpecies].Play("Armature|IDLE");
            }
        }

        currentShortestDistanceLooking = lookRadius;
        currentDistanceLooking = lookRadius;

        // TODO: Try out different things
        //aliensInRangeCount = Physics.OverlapSphereNonAlloc(MyTransform.position, lookRadius, aliensInRange, layerMaskAlien, QueryTriggerInteraction.Ignore);

        aliensInRangeCount = aliensInRange.Count;

        if (aliensInRangeCount == 0)
        {
            foreach (var item in Physics.OverlapSphere(MyTransform.position, lookRadius, layerMaskAlien, QueryTriggerInteraction.Ignore))
            {
                aliensInRange.Add(item);
            }
            if (aliensInRange.Count == 0)
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.roaming));
                return;
            }
        }

        for (int i = 0; i < aliensInRangeCount; i++)
        {
            if (aliensInRange[i] == null) { return; }

            // Prevent checking on self and last alien
            if (aliensInRange[i] == MyCollisionCollider) { continue; }

            //if (aliensInRange[i].gameObject == lastTargetAlien) { continue; } // Can go behind last target if other interference happend in between
            if (aliensInRange[i].gameObject.activeInHierarchy == false) { continue; }

            targetAlienHandler = aliensInRange[i].gameObject.GetComponentInParent<AlienHandler>();
            if (targetAlienHandler.currentAge == AlienAge.resource)
            {
                continue;
            }

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

                #region Who Eats Who
                // This aliens eats the other
                // 0:Sphere > 1:Square > 2:Triangle 
                // Triangle eats Square / 2 eats 1
                // Square eats Sphere / 1 eats 0
                // Sphere eats Triangle / 0 eats 2
                #endregion

            }
            else
            {

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
        }



        // Set state on closest target
        if (targetAlien == null)
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.roaming));
            return;
        }

        #region pick better target
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
        if (brainWashed == true) { return; }
        if (hasNewTarget == true) { return; }
        // Find new target
        for (int i = 0; i < 3; i++)
        {
            randDirXRoaming = UnityEngine.Random.Range(0, 2) - .5f;
            randDirZRoaming = UnityEngine.Random.Range(0, 2) - .5f;
            targetPosition3D = MyTransform.position + new Vector3(randDirXRoaming, 0, randDirZRoaming) * 20;
            targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
            distanceToCurrentTarget = Vector2.Distance(MyTransform2D, targetPosition2D);

            if ((targetPosition3D.x * targetPosition3D.x + targetPosition3D.z * targetPosition3D.z) < worldRadiusSquared || distanceToCurrentTarget > 1)
            {
                break;
            }
        }

        hasNewTarget = true;
        return;
    }

    public void HandleUpdateTarget()
    {
        if (currentState != AlienState.roaming)
        {
            if (TargetAlienTransform == null) { return; }
            if (TargetAlienTransform.position == Vector3.zero) { return; }
            if (TargetAlienTransform.gameObject.activeInHierarchy == false)
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
            }

            if (currentState == AlienState.evading) // Away from target
            {
                targetPosition3D = MyTransform.position + (MyTransform.position - TargetAlienTransform.position);
            }
            else // towards target
            {
                targetPosition3D = TargetAlienTransform.position;
            }
        }
        if (targetPosition3D == null) { return; }
        targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
        distanceToCurrentTarget = Vector2.Distance(MyTransform2D, targetPosition2D);
    }

    private void HandleMating()
    {
        // Check if possible to spawn more aliens
        if (brainWashed == false && PoolManager.Instance.currentAlienAmount >= PoolManager.Instance.alienAmount + PoolManager.Instance.alienAmountExtra)
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
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

        StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
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
        deadAlienGO = PoolManager.Instance.GetPooledDeadAlien();
        if (deadAlienGO != null)
        {
            deadAlienGO.transform.position = MyTransform.position;
            deadAlienGO.transform.rotation = MyTransform.rotation;

            deadAlien = deadAlienGO.GetComponent<DeadAlienHandler>();
            deadAlien.Rigidbodies[currentSpecies].position = this.transform.position;
            deadAlien.transform.rotation = MyTransform.rotation;
            deadAlien.bulletForce = bulletForce;
            deadAlien.currentAlienSpecies = currentSpecies;

            deadAlienGO.gameObject.SetActive(true);
        }
        Debug.Log("TODO: Dead Alien Ragdoll here");
        HandleDeath();
    }

    public void HandleDeathByCombat()
    {
        StartCoroutine(WaitForDeath(.2f));
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

    private void DiscardCurrentAction()
    {
        if (targetAlien != null)
        {
            lastTargetAlien = targetAlien;
        }
        Debug.Log("Stack overflow error is coming from next two lines. Comment them out and error stops popping up");
        //targetAlien = null;
        //TargetAlienTransform = null;
        targetPosition3D = Vector3.zero;
        targetPosition2D = Vector2.zero;
    }

    private void HandleUpdateVariables(float delta)
    {
        lifeTime += delta;
        lustTimer += delta;
        hungerTimer += delta;
        tickTimer += delta;
        lifeTime += delta;
        speed = (alienSpeed + ((lustTimer + hungerTimer) / 100)) * delta; // + ((2 * (lustTimer + hungerTimer)) / (lustTimer + hungerTimer)); TODO: make better?! Way too fast
    }

    private void HandleTickUpdateVariables()
    {
        MyTransform2D = new Vector2(MyTransform.position.x, MyTransform.position.z);
        CameraFollowSpot2D = new Vector2(GameManager.Instance.CameraFollowSpot.position.x, GameManager.Instance.CameraFollowSpot.position.z);
        distanceToCameraSpot = Vector2.Distance(MyTransform2D, CameraFollowSpot2D);
        randomNumber = Random.Range(1, 11) / 10;
    }

    private void HandleRendering()
    {
        if (distanceToCameraSpot > 50)
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
    }

    public void BrainwashAlien()
    {
        brainWashed = true;
        resourceSteamGO.SetActive(false);
        StopAllCoroutines();
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

    private void OnTriggerEnter(Collider other)
    {
        if (currentAge == AlienAge.resource) { return; }

        // Handle Alien interaction
        if (other.gameObject.transform.parent && other.gameObject.transform.parent.CompareTag("Alien"))
        {
            otherAlienHandler = other.gameObject.GetComponentInParent<AlienHandler>();
            if (otherAlienHandler == null) { return; }
            if (targetAlienHandler != otherAlienHandler) { return; }

            if (currentSpecies == otherAlienHandler.currentSpecies)
            {
                // TODO: not a better way with just targetAlien?
                //hasUterus == true && // opposite Sex
                //otherAlienHandler.hasUterus == false &&
                //currentAge == AlienAge.sexualActive && // Sexual active
                //otherAlienHandler.currentAge == AlienAge.sexualActive && // potential partner also sexual active
                //lustTimer > lustTimerThreshold && // can mate
                //otherAlienHandler.lustTimer > lustTimerThreshold // partner can mate)
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
                        StartCoroutine(PlayActionParticle(AlienState.hunting));
                        otherAlienHandler.HandleDeathByCombat();
                        audioSource.PlayOneShot(RandomAudioSelectorFoley(aliensEating));
                    }

                    if (brainWashed == true) { return; }// For tutorial

                    StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
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

            // TODO: Add Player Interaction color
            //else if (currentState == AlienState.hunting)
            //{

            //}

            alienActionParticlesGO.SetActive(true);
            yield return new WaitForSeconds(1f);
            alienActionParticlesGO.SetActive(false);
        }
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
        //transform.localScale = Vector3.one * 1.2f;
    }

    private IEnumerator HandleGrowing(float oldFactor, float newFactor)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.5f / 10); // Total duration of transform 0.5f seconds
            MyTransform.localScale = Vector3.one * ((oldFactor + newFactor * i / 10) - (oldFactor * i / 10));
        }
    }

    private IEnumerator UndoBrainWash(float time)
    {
        yield return new WaitForSeconds(time);
        brainWashed = false; // AKA tutuorial scene
        StartCoroutine(HandleAge(true));
    }

    private IEnumerator WaitForDeath(float time)
    {
        yield return new WaitForSeconds(time);
        HandleDeath();
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

    #endregion

    private void HandleIdleAnimation()
    {
        if (anim[currentSpecies] != null) { anim[currentSpecies]["Armature|WALK"].speed = 1; }

        if (anim[currentSpecies] != null)
        {
            if (currentSpecies != 0)
            {
                anim[currentSpecies].Play("Armature|IDLE");
            }
        }
    }

    public void HandleFleeing(GameObject currentTargetGO)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(evadingAudioList, currentSpecies), 1f);
        }
        if (anim[currentSpecies] != null) { anim[currentSpecies]["Armature|WALK"].speed = 2; }

        if (brainWashed == true)
        {
            return;
        }

        SetTarget(currentTargetGO);
    } // Use this here on the player as well to scare the aliens away

    public void HandleAttacking(GameObject currentTargetGO) // Player makes them flee as well and by acting als targetAlien in PlayerManager
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(attackAudioList, currentSpecies), 1f);
        }

        if (anim[currentSpecies] != null) { anim[currentSpecies]["Armature|WALK"].speed = 2; }


        if (brainWashed == true) { return; }

        SetTarget(currentTargetGO);
    }

    private void HandleLoveApproach(GameObject targetAlien)
    {
        if (brainWashed == true) { return; }

        if (targetAlien == null)
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
        }
    }

    public void SetTarget(GameObject TargetGO)
    {
        if (targetAlien != null)
        {
            lastTargetAlien = targetAlien;
        }

        targetAlien = TargetGO;
        if (targetAlien == null) { return; }

        if (brainWashed == false)
        {
            TargetAlienTransform = targetAlien.transform;
            targetPosition3D = TargetAlienTransform.position;
            targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
            HandleUpdateTarget();
        }
    }

    public IEnumerator IdleSecsUntilNewState(float seconds, AlienState nextState)
    {
        canAct = false;
        HandleIdleAnimation();
        //if (brainWashed == false)
        //{
        //    DiscardCurrentAction(); // To prevent the lost of target Alien
        //}
        currentState = nextState;
        lookTimeIdle = UnityEngine.Random.Range(0, (seconds + 1) * 10) / 10;
        yield return new WaitForSeconds(lookTimeIdle);
        canAct = true;
    }

}

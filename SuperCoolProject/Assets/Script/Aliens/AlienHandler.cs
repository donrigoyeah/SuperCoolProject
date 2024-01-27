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
                    HandleAttacking(targetAlien, false); // false  for isAttackingPlayer
                    break;
                case AlienState.evading:
                    HandleFleeing(targetAlien, false); // false  for isEvadingPlayer
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
                    HandleIdle();
                    break;
                default:
                    HandleLooking();
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
    private Collider[] aliensInRange = new Collider[10];
    private int aliensInRangeCount;
    private float worldRadiusSquared;
    private int amountOfBabies;
    private Transform TargetAlienTransformFound;
    private AlienHandler otherAlien;
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

    public bool isRendered = true;
    public bool canAct = true;
    public int currentSpecies;
    public bool hasUterus;
    public float alienHealth;
    public bool isDead = false;
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
    private AlienHandler targetAlienHandler;
    private Vector2 TargetAlienTransformFound2D;

    [Header("General AlienStuff")]
    public GameObject[] alienSpecies; // 0:Sphere > 1:Square > 2:Triangle  
    public GameObject[] alienSpeciesChild; // 0:Sphere > 1:Square > 2:Triangle  
    public GameObject[] alienSpeciesAdult; // 0:Sphere > 1:Square > 2:Triangle  
    public Material[] alienColors; // 0:Blue > 1:Green > 2:Red  
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

    public AlienManager alienManager;

    #endregion

    private void Awake()
    {
        alienManager = AlienManager.Instance; // TODO: Why this again here? I think to have the reference already and not all calls like HandleDeathByBullet();
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
            StartCoroutine(HandleAge());
        }
    }

    private void OnDisable()
    {
        // TODO: maybe clear variables here
        ResetVariable();
        DiscardCurrentAction();
        StopAllCoroutines();
        brainWashed = false;
    }

    private void FixedUpdate()
    {
        delta = Time.deltaTime;

        HandleUpdateTimers(delta);

        // If is dead, skip everytthing
        if (isDead == true) { return; }

        // Within X units from player
        HandleRendering(); // Necessaray?!

        // If is doing action
        if (canAct == false) { return; }

        // Is still resource
        if (currentAge == AlienAge.resource) { return; }

        // No need for movement on resource
        HandleMovement();

        // Only Render on Tick condition
        while (tickTimer >= tickTimerMax)
        {
            // Reset Tick timer
            tickTimer -= tickTimerMax;
            MyTransform2D = new Vector2(MyTransform.position.x, MyTransform.position.z);

            // Dont update the target if brainwashed
            if (brainWashed == true) { return; }

            // Dont update target if roaming (cause has no targetAlien)
            if (currentState == AlienState.roaming) { return; }

            // If targetAlien
            if (targetAlien == null || targetAlien.activeInHierarchy == false)
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                return;
            }

            HandleUpdateTarget(targetAlien.transform.position);
        }
    }

    private void HandleIdle()
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

    public void HandleLooking()
    {
        if (anim[currentSpecies] != null)
        {
            if (currentSpecies != 0)
            {
                anim[currentSpecies].Play("Armature|IDLE");
            }
        }

        currentShortestDistanceLooking = lookRadius;
        currentDistanceLooking = lookRadius;

        aliensInRangeCount = Physics.OverlapSphereNonAlloc(MyTransform.position, lookRadius, aliensInRange, layerMaskAlien);

        // TODO: a while loop here? while targetAlien == null || distance > somethreshold
        for (int i = 0; i < aliensInRangeCount; i++)
        {
            // Prevent checking on self and last alien
            if (aliensInRange[i].gameObject == this.gameObject) { continue; }
            if (aliensInRange[i].gameObject.activeInHierarchy == false) { continue; }

            // TODO: This a good spot for this?!
            targetAlienHandler = aliensInRange[i].gameObject.GetComponent<AlienHandler>();
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
                    SetTargetAlien(aliensInRange[i].gameObject);
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
                    (currentSpecies == 0 && targetAlienHandler.currentSpecies == 2))) // potential food || if closestAlienHandler is smaller
                {
                    SetTargetAlien(aliensInRange[i].gameObject);
                }
                else if ((currentSpecies == targetAlienHandler.currentSpecies - 1 ||
                    (currentSpecies == 2 && targetAlienHandler.currentSpecies == 0))) // 0:Sphere > 1:Square > 2:Triangle || if closestAlienHandler is bigger
                {
                    SetTargetAlien(aliensInRange[i].gameObject);
                }
            }

            if (TargetAlienTransformFound == null) { continue; } // Continue the for loop to find another target

            TargetAlienTransformFound2D = new Vector2(TargetAlienTransformFound.position.x, TargetAlienTransformFound.position.z);
            currentDistanceLooking = Vector2.Distance(MyTransform2D, TargetAlienTransformFound2D);

            if (currentDistanceLooking < currentShortestDistanceLooking)
            {
                currentShortestDistanceLooking = currentDistanceLooking;
            }

            if (currentShortestDistanceLooking <= 2)
            {
                break;
            }
        }


        // Set state on closest target
        if (TargetAlienTransformFound == null)
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.roaming));
            return;
        }
        else
        {

            if (currentSpecies == targetAlienHandler.currentSpecies)
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.loving));
                return;
            }

            if (currentSpecies == targetAlienHandler.currentSpecies - 1 ||
                    (currentSpecies == 2 && targetAlienHandler.currentSpecies == 0)) // If target is bigger
            {

                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.evading));
                return;
            }

            if (currentSpecies == targetAlienHandler.currentSpecies + 1 ||
                    (currentSpecies == 0 && targetAlienHandler.currentSpecies == 2)) // If target is smaller
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.hunting));
                return;
            }
        }

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

        // Find new target
        while (targetPosition3D == Vector3.zero || (targetPosition3D.x * targetPosition3D.x + targetPosition3D.z * targetPosition3D.z) > worldRadiusSquared)
        {
            randDirXRoaming = UnityEngine.Random.Range(0, 2) - .5f;
            randDirZRoaming = UnityEngine.Random.Range(0, 2) - .5f;
            targetPosition3D = MyTransform.position + new Vector3(randDirXRoaming, 0, randDirZRoaming) * 20;
        }
        targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
    }

    public void HandleFleeing(GameObject currentTargetAlien, bool isEvadingPlayer)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelector(evadingAudioList, currentSpecies), 1f);
        }
        if (anim[currentSpecies] != null) { anim[currentSpecies]["Armature|WALK"].speed = 2; }

        if (isEvadingPlayer == true || brainWashed == true)
        {
            return;
        }

        targetAlien = currentTargetAlien;
    } // Use this here on the player as well to scare the aliens away

    public void HandleAttacking(GameObject currentTargetAlien, bool isAttackingPlayer) // Player makes them flee as well and by acting als targetAlien in PlayerManager
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelector(attackAudioList, currentSpecies), 1f);
        }

        if (anim[currentSpecies] != null) { anim[currentSpecies]["Armature|WALK"].speed = 2; }


        if (isAttackingPlayer == true || brainWashed == true)
        {
            return;
        }

        targetAlien = currentTargetAlien;
    }

    private void HandleLoveApproach(GameObject targetAlien)
    {
        if (brainWashed == false)
        {
            if (targetAlien == null)
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                return;
            }
        }
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
            audioSource.PlayOneShot(RandomAudioSelector(lovemakingAudioList, currentSpecies), 1f);
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
        return;
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
        Debug.Log("Work wrok wrokokr");
        HandleDeath();
    }

    public void HandleDeathByCombat()
    {
        StartCoroutine(WaitForDeath(.2f));
    }

    private void HandleMovement()
    {
        //if (targetPosition == Vector3.zero) { return; }
        if (MyTransform.position.y != 0.1f) { MyTransform.position = new Vector3(MyTransform.position.x, 0.1f, MyTransform.position.z); }
        if (anim[currentSpecies] != null) { anim[currentSpecies].Play("Armature|WALK"); }

        if (currentState != AlienState.idle || brainWashed == true) // If brainwashed can move anyway
        {
            MyTransform.position = Vector3.MoveTowards(MyTransform.position, targetPosition3D, speed);
        }

        if (Vector2.Distance(MyTransform2D, targetPosition2D) > 1)
        {
            MyTransform.LookAt(targetPosition3D);
        }


        if (currentState == AlienState.evading || currentState == AlienState.hunting)
        {
            if (Vector2.Distance(MyTransform2D, targetPosition2D) > lookRadius + 1)  // Add +1 so i is out of the lookradius
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                return;
            }
        }
        else // AlienStates: .resource .loving .looking
        {
            if (Vector2.Distance(MyTransform2D, targetPosition2D) < .1f)
            {
                if (currentState == AlienState.roaming) // We need this check so if state is hunting or love making, we dont overwrite state of onTriggerEnter
                {
                    if (brainWashed == false)
                    {
                        StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                        return;
                    }
                }
            }
        }

    }

    public void HandleUpdateTarget(Vector3 targetToUpdate)
    {
        if (currentState == AlienState.evading) // Away from target
        {
            targetPosition3D = MyTransform.position + (MyTransform.position - targetToUpdate);
        }
        else // towards target
        {
            targetPosition3D = targetToUpdate;
        }

        targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
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

    private void SetTargetAlien(GameObject TargetAlienGO)
    {
        targetAlien = TargetAlienGO;

        if (brainWashed == false)
        {
            TargetAlienTransformFound = targetAlien.GetComponent<Transform>();
            targetPosition3D = TargetAlienTransformFound.position;
            targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
        }
    }

    private void DiscardCurrentAction()
    {
        if (targetAlien != null)
        {
            lastTargetAlien = targetAlien;
        }
        targetAlien = null;
        TargetAlienTransformFound = null;
        targetPosition3D = Vector3.zero;
        targetPosition2D = Vector2.zero;
    }

    private void HandleUpdateTimers(float delta)
    {
        lifeTime += delta;
        lustTimer += delta;
        hungerTimer += delta;
        tickTimer += delta;
        lifeTime += delta;
        speed = (alienSpeed + ((lustTimer + hungerTimer) / 100)) * delta; // + ((2 * (lustTimer + hungerTimer)) / (lustTimer + hungerTimer)); TODO: make better?! Way too fast
    }

    private void HandleRendering()
    {
        MyTransform2D = new Vector2(MyTransform.position.x, MyTransform.position.z);
        CameraFollowSpot2D = new Vector2(GameManager.Instance.CameraFollowSpot.position.x, GameManager.Instance.CameraFollowSpot.position.z);
        if (Vector2.Distance(MyTransform2D, CameraFollowSpot2D) > 50)
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
    }

    public void BrainwashAlien()
    {
        brainWashed = true;
        resourceSteamGO.SetActive(false);
        StopAllCoroutines();
    }

    private void UpdateResourceSteam(int currentIndex)
    {
        if (currentIndex == 0)
        {
            resourceSteamMain.startColor = Color.blue;
            alienMiniMapMarker.material = alienColors[0];
        }
        else if (currentIndex == 1)
        {
            resourceSteamMain.startColor = Color.green;
            alienMiniMapMarker.material = alienColors[1];
        }
        else if (currentIndex == 2)
        {
            resourceSteamMain.startColor = Color.red;
            alienMiniMapMarker.material = alienColors[2];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Handle Alien interaction
        if (other.gameObject.CompareTag("Alien"))
        {
            otherAlien = other.gameObject.GetComponent<AlienHandler>();

            if (otherAlien.currentSpecies == currentSpecies)
            {
                if (targetAlien != otherAlien) { return; }

                lustTimer = 0;
                otherAlien.lustTimer = 0;
                StartCoroutine(PlayActionParticle(AlienState.loving)); // Loving Partilce
                HandleMating();
            }
            else
            {
                if (currentAge == AlienAge.resource) // You, the resource, gets trampled
                {
                    AlienManager.Instance.RemoveFromResourceList(this);
                    HandleDeath();
                    return;
                }
                else
                {
                    if (targetAlien != otherAlien) { return; }

                    // Handles eat other alien
                    hungerTimer = 0;
                    if (other.gameObject.activeInHierarchy)
                    {
                        StartCoroutine(PlayActionParticle(AlienState.hunting));
                        otherAlien.HandleDeathByCombat();
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
                audioSource.PlayOneShot(RandomAudioSelector(beingAttackedAudioList, currentSpecies), 1f);
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

    public IEnumerator IdleSecsUntilNewState(float seconds, AlienState nextState)
    {
        HandleIdle();
        canAct = false;
        DiscardCurrentAction();
        lookTimeIdle = UnityEngine.Random.Range(0, (seconds + 1) * 10) / 10;
        yield return new WaitForSeconds(lookTimeIdle);
        canAct = true;
        currentState = nextState;
    }

    private IEnumerator PlayActionParticle(AlienState currentState)
    {
        if (Vector3.Distance(MyTransform.position, GameManager.Instance.CameraFollowSpot.position) > 50)
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

    private IEnumerator HandleAge()
    {
        // Resource Life
        UpdateResourceSteam(currentSpecies);
        resourceSteamGO.SetActive(true);
        MyRigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        currentAge = AlienAge.resource;
        alienHealth = alienLifeResource;
        MyTransform.localScale = Vector3.one * resourceScale;
        yield return new WaitForSeconds(minTimeToChild);

        // Child Life
        resourceSteamGO.SetActive(false);
        MyRigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        alienHealth = alienLifeChild;
        currentAge = AlienAge.child;
        currentState = AlienState.looking;
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
    }

    private IEnumerator WaitForDeath(float time)
    {
        yield return new WaitForSeconds(time);
        HandleDeath();
    }

    AudioClip RandomAudioSelector(List<AudioClip[]> audioList, int state) // incase we plan to add more audio for each state
    {
        // TODO: think of something to have ot play an audio only 50% of the time?
        AudioClip[] selectedAudioArray = audioList[state];

        int randomIndex = Random.Range(0, selectedAudioArray.Length);
        AudioClip selectedAudio = selectedAudioArray[randomIndex];

        return selectedAudio;
        return null;
    }
}

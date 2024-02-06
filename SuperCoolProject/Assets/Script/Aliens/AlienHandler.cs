using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private AlienState currentStateValue; //this holds the actual value, should be private
    [SerializeField] private AlienState lastAlienState;
    private int layerMaskAlien = 1 << 9; // Lyer 9 is Alien
    [SerializeField]
    private List<Collider> aliensInRange;
    private Collider[] aliensInRangeCollider;
    private Collider[] aliensInRangeColliderOrdered;
    private int aliensInRangeCount;
    private float worldRadiusSquared;
    private BulletHandler CurrentBH;
    private float currentBulletDamage;


    [Header("This Alien")]
    public bool isRendered = true;
    public bool brainWashed = false;
    public bool canAct = true;
    public int currentSpecies;
    private Rigidbody MyRigidbody;
    public Transform MyTransform;
    private Vector2 MyTransform2D;
    public AlienAge currentAge;
    public Collider MyCollisionCollider;
    public bool hasUterus;
    public float alienHealth;
    public bool isDead = true;
    public float lifeTime;
    public float lustTimer = 0;
    public float hungerTimer = 0;
    public int amountOfBabies;
    public bool gotAttackedByPlayer = false;
    public bool isAttackingPlayer = false;
    public bool isEvadingPlayer = false;

    public bool spawnAsAdults = false;
    public RawImage currentStateIcon;
    public Texture[] allStateIcons; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
    public float distanceToCurrentTarget;
    private float currentShortestDistanceLooking;
    private float currentDistanceLooking;
    private float randDirXRoaming;
    private float randDirZRoaming;
    private GameObject newBornAlienPoolGo;
    private AlienHandler newBornAlien;
    private float randomOffSetBabySpawn;
    public Vector3 targetPosition3D;
    private Vector2 targetPosition2D;

    [Header("Target Alien")]
    public GameObject targetAlien;
    public GameObject lastTargetAlien;
    public AlienHandler targetAlienHandler;
    public AlienHandler otherAlienHandler;

    [Header("General Alien References")]
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
    private float delta;
    private float speed;
    public int minTimeToChild = 5;

    [Header("Alien Audio")]
    private bool playClipSpawned = false;
    public AudioSource audioSource;

    [Header("More reference")]
    public float lookTimeIdle;
    private GameObject deadAlienGO;
    private DeadAlienHandler deadAlien;
    private Vector2 CameraFollowSpot2D;
    private float randomNumber;
    public float distanceToCameraSpot;
    private bool isPlayerBullet;
    private GameObject damageUIGo;
    private DamageUIHandler DUIH;
    Collider[] tmpColliderArray;

    [Header("Dissolve")]
    public Material dissolve;
    public SkinnedMeshRenderer skinRenderer1;
    public SkinnedMeshRenderer skinRenderer2;
    public SkinnedMeshRenderer skinRenderer3;
    public Material[] orignalMaterial;

    [Header("Tick stats")]
    public float tickTimer;
    public float tickTimerMax = .5f;
    public bool hasNewTarget = false;

    [Header("DeadAliens")] // 0:Sphere > 1:Square > 2:Triangle
    public GameObject[] deadAliensPrerfabs;
    public bool canSpawnDeadBodies;

    #endregion

    private void Awake()
    {
        MyRigidbody = this.gameObject.GetComponent<Rigidbody>();
        resourceSteamMain = resourceSteamGO.GetComponent<ParticleSystem>().main;
    }

    private void Start()
    {
        worldRadiusSquared = GameManager.Instance.worldRadius * GameManager.Instance.worldRadius;
        alienActionFogMain = alienActionFog.gameObject.GetComponent<ParticleSystem>().main;
        if (MyTransform == null) { MyTransform = this.gameObject.GetComponent<Transform>(); }
    }

    private void OnEnable()
    {
        if (AlienManager.Instance == null) { return; }

        if (brainWashed == true)
        {
            StartCoroutine(UndoBrainWash(10));
        }
        else
        {
            StartCoroutine(HandleAge(spawnAsAdults));
        }
        ActivateCurrentModels(currentSpecies);
    }

    private void OnDisable()
    {
        if (AlienManager.Instance == null) { return; }
        ResetVariable();
        StopAllCoroutines();
        brainWashed = false;
    }

    private void FixedUpdate()
    {
        HandleDistanceCalculation();
        HandleRendering();

        if (isRendered == false || canAct == false || isDead == true) { return; }

        delta = Time.deltaTime;
        HandleUpdateVariables();

        if (currentAge == AlienAge.resource) { return; }

        HandleUpdateTarget();
        HandleAnimation();

        // Finaly move the alien if it can
        HandleMovement();

        if (tickTimer >= tickTimerMax)
        {
            // Reset Tick timer
            tickTimer -= tickTimerMax;

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
            if (currentState == AlienState.hunting)
            {
                HandleAttacking();
                return;
            }
            if (currentState == AlienState.evading)
            {
                HandleFleeing();
                return;
            }
            if (currentState == AlienState.loving)
            {
                HandleLoveApproach();
                return;
            }
        }
    }

    private void HandleDistanceCalculation()
    {
        MyTransform2D = new Vector2(MyTransform.position.x, MyTransform.position.z);
        CameraFollowSpot2D = new Vector2(GameManager.Instance.CameraFollowSpot.position.x, GameManager.Instance.CameraFollowSpot.position.z);
        distanceToCameraSpot = Vector2.Distance(MyTransform2D, CameraFollowSpot2D);
    }

    private void HandleRendering()
    {
        if (distanceToCameraSpot > AlienManager.Instance.renderDistance)
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
            if (targetAlien == null) { return; }

            if (currentState == AlienState.evading) // Away from target
            {
                targetPosition3D = MyTransform.position + (MyTransform.position - targetAlien.transform.position);
            }
            else // towards target
            {
                targetPosition3D = targetAlien.transform.position;
            }
        }

        if (targetPosition3D == Vector3.zero) { return; }

        targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
        distanceToCurrentTarget = Vector2.Distance(MyTransform2D, targetPosition2D);
    }

    private void HandleUpdateVariables()
    {
        lifeTime += delta;
        lustTimer += delta;
        hungerTimer += delta;
        tickTimer += delta;
        if (currentState == AlienState.hunting)
        {

            speed = (AlienManager.Instance.alienSpeedHunting + ((lustTimer + hungerTimer) / 100)) * delta; // + ((2 * (lustTimer + hungerTimer)) / (lustTimer + hungerTimer)); TODO: make better?! Way too fast
        }
        else
        {

            speed = (AlienManager.Instance.alienSpeed + ((lustTimer + hungerTimer) / 100)) * delta; // + ((2 * (lustTimer + hungerTimer)) / (lustTimer + hungerTimer)); TODO: make better?! Way too fast
        }

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
        alienHealth = AlienManager.Instance.alienLifeResource;
        brainWashed = false; // AKA tutuorial scene
        canAct = true;
        isDead = false;
        spawnAsAdults = false;
        gotAttackedByPlayer = false;
        isAttackingPlayer = false;
        isEvadingPlayer = false;
        targetPosition3D = Vector3.zero;
        targetAlien = null;
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
        yield return new WaitForSeconds(.5f);

        if (isSpawningAsAdult == false)
        {
            // Resource Life
            UpdateResourceSteam(currentSpecies);
            resourceSteamGO.SetActive(true);
            MyRigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            currentAge = AlienAge.resource;
            alienHealth = AlienManager.Instance.alienLifeResource;
            MyTransform.localScale = Vector3.one * AlienManager.Instance.resourceScale;
            yield return new WaitForSeconds(minTimeToChild);
        }

        // Child Life
        resourceSteamGO.SetActive(false);
        MyRigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        alienHealth = AlienManager.Instance.alienLifeChild;
        currentAge = AlienAge.child;
        currentState = AlienState.roaming;
        MyTransform.localScale = Vector3.one * AlienManager.Instance.childScale;
        alienSpeciesChild[currentSpecies].SetActive(false);
        alienSpeciesAdult[currentSpecies].SetActive(true);
        if (AlienManager.Instance.resourceSphere.Count + AlienManager.Instance.resourceSquare.Count + AlienManager.Instance.resourceTriangle.Count > 0)
        {
            AlienManager.Instance.RemoveFromResourceList(this); // TODO: Check if available in List?!
        }
        yield return new WaitForSeconds(AlienManager.Instance.timeToSexual);

        // Sexual active Life
        alienHealth = AlienManager.Instance.alienLifeSexual;
        currentAge = AlienAge.sexualActive;
        StartCoroutine(HandleGrowing(AlienManager.Instance.childScale, AlienManager.Instance.sexualActiveScale));
        yield return new WaitForSeconds(AlienManager.Instance.timeToFullGrown);

        // Full Grown Life
        alienHealth = AlienManager.Instance.alienLifeFullGrown;
        currentAge = AlienAge.fullyGrown;
        StartCoroutine(HandleGrowing(AlienManager.Instance.sexualActiveScale, AlienManager.Instance.fullGrownScale));
    }

    private IEnumerator HandleGrowing(float oldFactor, float newFactor)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.5f / 10); // Total duration of transform 0.5f seconds
            MyTransform.localScale = Vector3.one * ((oldFactor + newFactor * i / 10) - (oldFactor * i / 10));
        }
    }


    public void DeadAliensRagdollSpawner()
    {
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

        if (isDead == false)
        {
            DeadAliensRagdollSpawner();
        }
        HandleDeath();
    }

    public void HandleDeathByCombat()
    {
        isDead = true;
        StartCoroutine(WaitForDeath(1));
        StartCoroutine(PlayActionParticle(AlienState.hunting));
    }

    private IEnumerator WaitForDeath(float time)
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(AlienManager.Instance.dyingAudioList, currentSpecies), 1f);
        }
        DeactivateAllModels();
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
        if (distanceToCurrentTarget > 1)
        {
            //TODO: Smoother rotation
            //Quaternion targetRotation = Quaternion.LookRotation(targetPosition3D);
            //MyTransform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, speed / Time.deltaTime);

            MyTransform.LookAt(targetPosition3D);
        }

        // If brainwashed or any state but idle, alien can move
        if (brainWashed == true || currentState != AlienState.idle)
        {
            MyTransform.position = Vector3.MoveTowards(MyTransform.position, targetPosition3D, speed);
        }

        // Act upon state and distance to current target
        if (currentState == AlienState.evading || currentState == AlienState.hunting || currentState == AlienState.looking)
        {
            if (distanceToCurrentTarget > AlienManager.Instance.lookRadius + 1)  // Add +1 so i is out of the lookradius
            {
                isAttackingPlayer = false;
                isEvadingPlayer = false;
                StartCoroutine(IdleSecsUntilNewState(lastAlienState));
            }
        }
        else // AlienStates: .resource .loving .roaming
        {
            if (distanceToCurrentTarget < .1f)
            {
                // Prevent the tutorial brainwashed aliens to walk away
                if (brainWashed == true) { return; }
                StartCoroutine(IdleSecsUntilNewState(AlienState.looking));
            }
        }
    }

    public void HandleLooking()
    {
        // Needs to do this somehow
        if (isAttackingPlayer == true) { currentState = AlienState.hunting; return; }
        if (isEvadingPlayer == true) { currentState = AlienState.evading; return; }


        currentShortestDistanceLooking = AlienManager.Instance.lookRadius;
        currentDistanceLooking = AlienManager.Instance.lookRadius;



        aliensInRangeCount = aliensInRange.Count;
        // If does not have an array of nearby aliens, create one
        if (aliensInRangeCount == 0 || aliensInRange == null)
        {
            aliensInRangeCollider = Physics.OverlapSphere(MyTransform.position, AlienManager.Instance.lookRadius, layerMaskAlien, QueryTriggerInteraction.Ignore);
            aliensInRangeColliderOrdered = aliensInRangeCollider.OrderBy(c => (MyTransform.position - c.transform.position).sqrMagnitude).ToArray();
            foreach (var item in aliensInRangeColliderOrdered)
            {
                aliensInRange.Add(item);
            }
            aliensInRangeCount = aliensInRange.Count;
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
                    lustTimer > AlienManager.Instance.lustTimerThreshold && // can mate
                    targetAlienHandler.lustTimer > AlienManager.Instance.lustTimerThreshold // partner can mate
                    )
                {
                    currentState = AlienState.loving;
                    SetTarget(targetAlienHandler.gameObject);
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

                if (hungerTimer > AlienManager.Instance.hungerTimerThreshold &&
                    (currentSpecies == targetAlienHandler.currentSpecies + 1 ||
                    (currentSpecies == 0 && targetAlienHandler.currentSpecies == 2))) // potential food || if closestAlienHandler is smaller || hunting state
                {
                    SetTarget(targetAlienHandler.gameObject);
                    currentState = AlienState.hunting;

                    targetAlienHandler.SetTarget(this.gameObject);
                    targetAlienHandler.currentState = AlienState.evading;
                    break;
                }
                else if ((currentSpecies == targetAlienHandler.currentSpecies - 1 ||
                    (currentSpecies == 2 && targetAlienHandler.currentSpecies == 0))) // 0:Sphere > 1:Square > 2:Triangle || if closestAlienHandler is bigger
                {
                    SetTarget(targetAlienHandler.gameObject);
                    currentState = AlienState.evading;
                    aliensInRange.Clear();

                    break;
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
            Debug.Log("Going bck to roaming");
            aliensInRange.Clear();
            StartCoroutine(IdleSecsUntilNewState(AlienState.roaming));
            return;
        }
    }

    private void HandleRoaming()
    {
        // Needs to do this somehow
        if (isAttackingPlayer == true) { currentState = AlienState.hunting; return; }
        if (isEvadingPlayer == true) { currentState = AlienState.evading; return; }

        if (hasNewTarget == true) { return; }
        if (brainWashed == true) { return; }

        if (targetPosition3D == Vector3.zero)
        {
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
            //Physics.OverlapSphereNonAlloc(new Vector3(targetPosition3D.x, 1, targetPosition3D.z), .1f, tmpColliderArray) > 0
            )
        {
            randDirXRoaming = Random.Range(0, 2) - .5f;
            randDirZRoaming = Random.Range(0, 2) - .5f;
            targetPosition3D = MyTransform.position + new Vector3(randDirXRoaming, 0, randDirZRoaming) * 20;
            targetPosition2D = new Vector2(targetPosition3D.x, targetPosition3D.z);
            distanceToCurrentTarget = Vector2.Distance(MyTransform2D, targetPosition2D);
            hasNewTarget = true;
        }
    }

    public void HandleFleeing()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(AlienManager.Instance.evadingAudioList, currentSpecies), 1f);
        }
    } // Use this here on the player as well to scare the aliens away

    public void HandleAttacking() // Player makes them flee as well and by acting als targetAlien in PlayerManager
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(AlienManager.Instance.attackAudioList, currentSpecies), 1f);
        }
    }

    private void HandleLoveApproach()
    {
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelectorAliens(AlienManager.Instance.lovemakingAudioList, currentSpecies), 1f);
        }
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
            audioSource.PlayOneShot(RandomAudioSelectorAliens(AlienManager.Instance.lovemakingAudioList, currentSpecies), 1f);
        }

        if (hasUterus == true)
        {
            amountOfBabies = UnityEngine.Random.Range(1, AlienManager.Instance.maxAmountOfBabies);
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
        targetPosition3D = Vector3.zero;
        distanceToCurrentTarget = 999f;
        lastAlienState = currentState;
        currentState = AlienState.idle;
        lookTimeIdle = Random.Range(1, (randomNumber + 1) * 10) / 10;
        yield return new WaitForSeconds(lookTimeIdle);
        currentState = nextState;
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
                alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(AlienManager.Instance.loveMakingColor1, AlienManager.Instance.loveMakingColor2);
            }
            else if (currentState == AlienState.hunting)
            {
                alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(AlienManager.Instance.fightingColor1, AlienManager.Instance.fightingColor2);
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
        if (anim[currentSpecies] == null) { return; }

        if (currentState == AlienState.hunting || currentState == AlienState.loving || currentState == AlienState.roaming || currentState == AlienState.looking)
        {
            if (anim[currentSpecies].IsPlaying("Armature|WALK") == false)
            {
                anim[currentSpecies].Play("Armature|WALK");
                anim[currentSpecies]["Armature|WALK"].speed = 1;
            }
        }
        if (currentState == AlienState.evading)
        {
            if (anim[currentSpecies].IsPlaying("Armature|WALK") == false)
            {
                anim[currentSpecies].Play("Armature|WALK");
                anim[currentSpecies]["Armature|WALK"].speed = 2;
            }
        }
        if (currentState == AlienState.idle)
        {
            if (anim[currentSpecies].IsPlaying("Armature|IDLE") == false)
            {
                anim[currentSpecies].Play("Armature|IDLE");
            }
        }
    }

    AudioClip RandomAudioSelectorAliens(List<AudioClip[]> audioList, int species) // incase we plan to add more audio for each state
    {
        // TODO: think of something to have ot play an audio only 50% of the time?
        if (audioList[species] == null) { return null; }
        AudioClip[] selectedAudioArray = audioList[species];

        int randomIndex = Random.Range(0, selectedAudioArray.Length);

        if (selectedAudioArray[randomIndex] == null) { return null; }
        AudioClip selectedAudio = selectedAudioArray[randomIndex];

        return selectedAudio;
    }

    AudioClip RandomAudioSelectorFoley(List<AudioClip> audioList) // incase we plan to add more audio for each state
    {
        int randomIndex = Random.Range(0, audioList.Count);

        if (audioList[randomIndex] == null) { return null; }
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
                    audioSource.PlayOneShot(RandomAudioSelectorFoley(AlienManager.Instance.aliensLoving));
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
                        audioSource.PlayOneShot(RandomAudioSelectorFoley(AlienManager.Instance.aliensEating));
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
                audioSource.PlayOneShot(RandomAudioSelectorAliens(AlienManager.Instance.beingAttackedAudioList, currentSpecies), 1f);
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
    // THIS IS NEW
}

using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.VFX;
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
        resource
    }

    private AlienState currentStateValue; //this holds the actual value 
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
    int layerMaskAlien = 1 << 9; // Lyer 9 is Alien
    private Collider[] aliensInRange;
    private float worldRadiusSquared;
    private int amountOfBabies;
    private Transform TargetAlienTransform;
    private AlienHandler otherAlien;
    private BulletHandler CurrentBH;
    private float currentBulletDamage;


    [Header("This Alien")]
    public Transform MyTransform;
    public AlienAge currentAge;

    public bool isRendered = true;
    public bool canAct = true;
    public int currentSpecies;
    public bool hasUterus;
    public float alienHealth;
    public bool isDead = false;
    public float lifeTime = 0;
    public float lustTimer = 0;
    public float hungerTimer = 0;
    public float lustTimerThreshold = 5;
    public int maxAmountOfBabies = 10;
    public float hungerTimerThreshold = 5;
    public RawImage currentStateIcon;
    public Texture[] allStateIcons; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield
    private Vector3 targetPosition = Vector3.zero;

    [Header("Target Alien")]
    public GameObject targetAlien = null;
    public GameObject lastTargetAlien = null;
    private AlienHandler closestAlienHandler = null;

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
    public float alienSpeed = 5;
    public float lookRadius = 10;
    private float delta;
    private float step;
    private int alienLifeResource = 1;
    private int alienLifeChild = 30;
    private int alienLifeSexual = 40;
    private int alienLifeFullGrown = 50;
    public int timeToChild = 5;
    public int timeToSexual = 15;
    public int timeToFullGrown = 25;

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
    [SerializeField] private AudioSource audioSource;


    [Header("Water / Sphere Alien Audio")]
    [SerializeField] private AudioClip[] sphereAttackAudio;
    [SerializeField] private AudioClip[] sphereDyingAudio;
    [SerializeField] private AudioClip[] sphereBeingAttackedAudio;
    [SerializeField] private AudioClip[] sphereLovemakingAudio;
    [SerializeField] private AudioClip[] sphereEvadingAudio;

    [Header("Oxygen / Square Alien Audio")]
    [SerializeField] private AudioClip[] squareAttackAudio;
    [SerializeField] private AudioClip[] squareDyingAudio;
    [SerializeField] private AudioClip[] squareBeingAttackedAudio;
    [SerializeField] private AudioClip[] squareLovemakingAudio;
    [SerializeField] private AudioClip[] squareEvadingAudio;

    [Header("Meat / Triangle Alien Audio")]
    [SerializeField] private AudioClip[] triangleAttackAudio;
    [SerializeField] private AudioClip[] triangleDyingAudio;
    [SerializeField] private AudioClip[] triangleBeingAttackedAudio;
    [SerializeField] private AudioClip[] triangleLovemakingAudio;
    [SerializeField] private AudioClip[] triangleEvadingAudio;

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
        alienManager = AlienManager.Instance;
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
        ResetVariable();
        DiscardCurrentAction();
    }

    private void FixedUpdate()
    {
        delta = Time.deltaTime;
        HandleUpdateTimers(delta);

        // If is dead, skip everytthing
        if (isDead == true) { return; }
        // If is doing action
        if (canAct == false) { return; }

        HandleRendering(); // Necessaray?!

        // Is still resource
        if (currentAge == AlienAge.resource) { return; }
        // Only Render on Tick condition
        while (tickTimer >= tickTimerMax)
        {
            // Reset Tick timer
            tickTimer -= tickTimerMax;
            HandleUpdateTarget(targetAlien);
        }
        HandleMovement();
    }

    public void HandleLooking()
    {
        float currentShortestDistance = lookRadius;
        float currentDistance = lookRadius;

        Vector2 MyTransform2D = new Vector2(MyTransform.position.x, MyTransform.position.z);
        Vector2 TargetAlienTransform2D;

        aliensInRange = Physics.OverlapSphere(MyTransform.position, lookRadius, layerMaskAlien);

        // TODO: a while loop here? while targetAlien == null || distance > somethreshold
        for (int i = 0; i < aliensInRange.Length; i++)
        {
            // Prevent checking on self and last alien
            if (aliensInRange[i].gameObject == lastTargetAlien ||
                aliensInRange[i].gameObject == this.gameObject)
            {
                continue;
            }

            // TODO: This a good spot for this?!
            closestAlienHandler = aliensInRange[i].gameObject.GetComponent<AlienHandler>();

            if (closestAlienHandler.currentAge == AlienAge.resource)
            {
                continue;
            }

            // Check wheater its same Species or not
            switch (currentSpecies == closestAlienHandler.currentSpecies)
            {
                case true: // Same Species
                    if (
                        hasUterus != closestAlienHandler.hasUterus && // opposite Sex
                        currentAge == AlienAge.sexualActive && // Sexual active
                        closestAlienHandler.currentAge == AlienAge.sexualActive && // potential partner also sexual active
                        lustTimer > lustTimerThreshold && // can mate
                        closestAlienHandler.lustTimer > lustTimerThreshold // partner can mate
                        )
                    {
                        SetTargetAlien(aliensInRange[i].gameObject);
                    }
                    break;

                #region Who eats who
                // Check to which state the alien switches
                // 0:Sphere > 1:Square > 2:Triangle 
                // Triangle eats Square / 2 eats 1
                // Square eats Sphere / 1 eats 0
                // Sphere eats Triangle / 0 eats 2
                #endregion

                case false: // Opposite Species
                    if (hungerTimer > hungerTimerThreshold &&
                        (currentSpecies == closestAlienHandler.currentSpecies + 1 ||
                        (currentSpecies == 0 && closestAlienHandler.currentSpecies == 2))) // potential food || if closestAlienHandler is smaller
                    {
                        SetTargetAlien(aliensInRange[i].gameObject);
                    }
                    else if ((currentSpecies == closestAlienHandler.currentSpecies - 1 ||
                        (currentSpecies == 2 && closestAlienHandler.currentSpecies == 0))) // 0:Sphere > 1:Square > 2:Triangle || if closestAlienHandler is bigger
                    {
                        SetTargetAlien(aliensInRange[i].gameObject);
                    }
                    break;

            }

            // Has found a target
            if (TargetAlienTransform == null) { continue; }

            TargetAlienTransform2D = new Vector2(TargetAlienTransform.position.x, TargetAlienTransform.position.z);
            currentDistance = Vector2.Distance(MyTransform2D, TargetAlienTransform2D);

            if (currentDistance < currentShortestDistance)
            {
                currentShortestDistance = currentDistance;
            }

            if (currentShortestDistance <= 2)
            {
                break;
            }
        }

        if (TargetAlienTransform == null)
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.roaming));
            return;
        }
        else
        {
            AlienHandler targetAlienHandler = targetAlien.GetComponent<AlienHandler>();

            if (targetAlienHandler == null)
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                return;
            }
            if (currentSpecies == targetAlienHandler.currentSpecies)
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.loving));
                return;
            }
            else if (currentSpecies == targetAlienHandler.currentSpecies - 1 ||
                    (currentSpecies == 2 && targetAlienHandler.currentSpecies == 0)) // If target is bigger
            {

                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.evading));
                return;
            }
            else if (currentSpecies == closestAlienHandler.currentSpecies + 1 ||
                    (currentSpecies == 0 && closestAlienHandler.currentSpecies == 2)) // If target is smaller
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.hunting));
                return;
            }
        }

        #region Loop over List approach
        //for (int i = 0; i < PoolManager.SharedInstance.AlienPool.Count; i++)  //list of gameObjects to search through
        //{
        //    if (PoolManager.SharedInstance.AlienPool[i] == this.gameObject || PoolManager.SharedInstance.AlienPool[i] == lastClosestAlien) continue;

        //    float dist = Vector3.Distance(PoolManager.SharedInstance.AlienPool[i].transform.position, transform.position);
        //    if (dist < lookRadius)
        //    {
        //        closestAlien = PoolManager.SharedInstance.AlienPool[i];
        //        closestAlienIndex = closestAlien.GetComponent<AlienHandler>().currentSpecies;
        //        break;
        //    }
        //}
        //return closestAlien;
        #endregion
    }

    private void HandleRoaming()
    {
        // Find new target
        if (targetPosition == Vector3.zero || (targetPosition.x * targetPosition.x + targetPosition.z * targetPosition.z) > worldRadiusSquared)
        {
            float randDirX = UnityEngine.Random.Range(0, 2) - .5f;
            float randDirZ = UnityEngine.Random.Range(0, 2) - .5f;
            targetPosition = MyTransform.position + new Vector3(randDirX, 0, randDirZ) * 10;
        }
    }

    public void HandleFleeing(GameObject targetAlien)
    {
        if (targetAlien == null)
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
            return;
        }


        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelector(evadingAudioList, currentSpecies), 1f);
        }
    } // Use this here on the player as well to scare the aliens away

    public void HandleAttacking(GameObject targetAlien) // Player makes them flee as well and by acting als targetAlien in PlayerManager
    {
        if (targetAlien == null)
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelector(attackAudioList, currentSpecies), 1f);
        }
    }

    private void HandleLoveApproach(GameObject targetAlien)
    {
        if (targetAlien == null)
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
            return;
        }
    }

    private void HandleMating()
    {
        // Check if possible to spawn more aliens
        if (PoolManager.SharedInstance.currentAlienAmount == PoolManager.SharedInstance.alienAmount + PoolManager.SharedInstance.alienAmountExtra)
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
            return;
        }

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelector(lovemakingAudioList, currentSpecies), 1f);
        }

        if (hasUterus)
        {
            amountOfBabies = UnityEngine.Random.Range(1, maxAmountOfBabies);
            for (var i = 0; i < amountOfBabies; i++)
            {
                GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
                if (alienPoolGo != null)
                {
                    float randomOffSet = (UnityEngine.Random.Range(0, 5) - 2) / 2;

                    AlienHandler newBornAlien = alienPoolGo.GetComponent<AlienHandler>();
                    newBornAlien.currentSpecies = currentSpecies;
                    newBornAlien.transform.position = new Vector3(MyTransform.position.x + randomOffSet, 0.5f, MyTransform.position.z + randomOffSet);
                    newBornAlien.gameObject.SetActive(true);
                }
            }
        }

        lustTimer = 0;
        StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
        return;
    }

    public void HandleDeath()
    {
        isDead = true;
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
        GameObject deadAlienGO = PoolManager.SharedInstance.GetPooledDeadAlien();
        if (deadAlienGO != null)
        {
            deadAlienGO.transform.position = MyTransform.position;
            deadAlienGO.transform.rotation = MyTransform.rotation;

            DeadAlienHandler deadAlien = deadAlienGO.GetComponent<DeadAlienHandler>();
            deadAlien.transform.rotation = Quaternion.identity;
            deadAlien.transform.rotation = Quaternion.Euler(0, 90, 0);
            deadAlien.bulletForce = bulletForce;
            deadAlien.currentAlienSpecies = currentSpecies;

            deadAlienGO.gameObject.SetActive(true);
        }

        HandleDeath();
    }

    private void HandleMovement()
    {
        if (targetPosition == Vector3.zero) { return; }
        if (MyTransform.position.y != 0.1f) { MyTransform.position = new Vector3(MyTransform.position.x, 0.1f, MyTransform.position.z); }


        if (anim[currentSpecies] != null) { anim[currentSpecies].Play("Armature|WALK"); }

        MyTransform.position = Vector3.MoveTowards(MyTransform.position, targetPosition, step);
        MyTransform.LookAt(targetPosition, Vector3.up);

        if (currentState == AlienState.evading || currentState == AlienState.hunting)
        {
            if (Vector3.Distance(targetPosition, MyTransform.position) > lookRadius + 1)  // Add +1 so i is out of the lookradius
            {
                StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                return;
            }
        }
        else
        {
            if (Vector3.Distance(MyTransform.position, targetPosition) < .1f)
            {
                if (currentState == AlienState.roaming) // We need this check so if state is hunting or love making, we dont overwrite state of onTriggerEnter
                {
                    StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                    return;
                }
            }
        }
    }

    private void HandleUpdateTarget(GameObject targetAlien)
    {
        if (targetAlien == null) { return; }

        if (targetAlien.gameObject.activeInHierarchy && targetAlien != null)
        {
            if (currentState == AlienState.evading) // Away from target
            {
                targetPosition = MyTransform.position + (MyTransform.position - targetAlien.transform.position);
            }
            else // towards target
            {
                targetPosition = targetAlien.transform.position;
            }
        }
        else
        {
            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
            return;
        }

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

    private void HandleStateIcon(AlienState currentState)
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

    private void SetTargetAlien(GameObject TargetAlien)
    {
        targetAlien = TargetAlien;
        TargetAlienTransform = targetAlien.GetComponent<Transform>();
        targetPosition = TargetAlienTransform.position;
    }

    private void DiscardCurrentAction()
    {
        if (targetAlien != null)
        {
            lastTargetAlien = targetAlien;
        }
        targetAlien = null;
        closestAlienHandler = null;
        TargetAlienTransform = null;
        targetPosition = Vector3.zero;
    }

    private void HandleUpdateTimers(float delta)
    {
        lifeTime += delta;
        lustTimer += delta;
        hungerTimer += delta;
        tickTimer += delta;
        step = (alienSpeed + lifeTime / 25) * delta;
    }

    private void HandleRendering()
    {
        Vector2 MyTransform2D = new Vector2(MyTransform.position.x, MyTransform.position.z);
        Vector2 CameraFollowSpot2D = new Vector2(GameManager.Instance.CameraFollowSpot.position.x, GameManager.Instance.CameraFollowSpot.position.z);
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
        isDead = false;
        canAct = true;
        timeToChild += UnityEngine.Random.Range(0, 10);
        hasUterus = UnityEngine.Random.Range(0, 2) == 1;
        alienHealth = alienLifeResource;
        currentAge = AlienAge.resource;
        lustTimer = 0;
        hungerTimer = 0;
        lifeTime = 0;

        ParticleSystem.MainModule resourceSteamMain = resourceSteamGO.GetComponent<ParticleSystem>().main;

        if (currentSpecies == 0)
        {
            resourceSteamMain.startColor = Color.blue;
            alienMiniMapMarker.material = alienColors[0];
        }
        else if (currentSpecies == 1)
        {
            resourceSteamMain.startColor = Color.green;
            alienMiniMapMarker.material = alienColors[1];
        }
        else if (currentSpecies == 2)
        {
            resourceSteamMain.startColor = Color.red;
            alienMiniMapMarker.material = alienColors[2];
        }
        if (this.gameObject.activeInHierarchy)
        {
            StartCoroutine(HandleAge());
        }
    }

    IEnumerator PlayActionParticle(bool isLoving)
    {
        if (Vector3.Distance(MyTransform.position, GameManager.Instance.CameraFollowSpot.position) > 50) { yield return null; }

        if (isLoving)
        {
            alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(Color.red, Color.magenta);
        }
        else
        {
            alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(Color.gray, Color.black);
        }
        alienActionParticlesGO.SetActive(true);
        yield return new WaitForSeconds(1f);
        alienActionParticlesGO.SetActive(false);

    }

    IEnumerator IdleSecsUntilNewState(float seconds, AlienState nextState)
    {
        if (anim[currentSpecies] != null)
        {
            if (currentSpecies != 0)
            {
                anim[currentSpecies].Play("Armature|IDLE");
            }
        }

        canAct = false;
        DiscardCurrentAction();
        float lookTime = UnityEngine.Random.Range(0, (seconds + 1) * 10) / 10;
        yield return new WaitForSeconds(lookTime);
        canAct = true;
        currentState = nextState;
    }

    IEnumerator HandleAge()
    {
        // Resource Life
        resourceSteamGO.SetActive(true);
        currentAge = AlienAge.resource;
        alienHealth = alienLifeResource;
        MyTransform.localScale = Vector3.one * 0.7f;
        alienManager.AddToResourceList(this);
        yield return new WaitForSeconds(timeToChild);

        // Child Life
        resourceSteamGO.SetActive(false);
        alienHealth = alienLifeChild;
        currentAge = AlienAge.child;
        currentState = AlienState.looking;
        MyTransform.localScale = Vector3.one * .6f;
        alienSpeciesChild[currentSpecies].SetActive(false);
        alienSpeciesAdult[currentSpecies].SetActive(true);
        alienManager.RemoveFromResourceList(this); // TODO: Check if available in List?!
        yield return new WaitForSeconds(timeToSexual);

        // Sexual active Life
        alienHealth = alienLifeSexual;
        currentAge = AlienAge.sexualActive;
        StartCoroutine(HandleGrowing(.6f, .8f));
        yield return new WaitForSeconds(timeToFullGrown);

        // Full Grown Life
        alienHealth = alienLifeFullGrown;
        currentAge = AlienAge.fullyGrown;
        StartCoroutine(HandleGrowing(.8f, 1f));
        //transform.localScale = Vector3.one * 1.2f;
    }

    IEnumerator HandleGrowing(float oldFactor, float newFactor)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.5f / 10); // Total duration of transform 0.5f seconds
            MyTransform.localScale = Vector3.one * ((oldFactor + newFactor * i / 10) - (oldFactor * i / 10));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO: Is this good?! if -> switch -> if

        // Handle Alien interaction
        if (other.gameObject.CompareTag("Alien"))
        {
            otherAlien = other.gameObject.GetComponent<AlienHandler>();

            switch (otherAlien.currentSpecies == currentSpecies)
            {
                case true: // Same Species
                    if (hasUterus != otherAlien.hasUterus && // opposite Sex
                        currentAge == AlienAge.sexualActive && // Sexual active
                        otherAlien.currentAge == AlienAge.sexualActive && // potential partner also sexual active
                        lustTimer > lustTimerThreshold && // can mate
                        otherAlien.lustTimer > lustTimerThreshold) // Babies
                    {
                        StartCoroutine(PlayActionParticle(true)); // Loving Partilce
                        HandleMating();
                    }
                    break;

                case false: // Other Species
                    if (currentAge == AlienAge.resource) // You, the resource, gets trampled
                    {
                        AlienManager.Instance.RemoveFromResourceList(this);
                        // TODO: Maybe add trampled particles?!
                        this.gameObject.SetActive(false);
                        return;
                    }
                    else
                    {
                        if (
                            hungerTimer > hungerTimerThreshold &&
                            (currentSpecies == otherAlien.currentSpecies + 1 ||
                            (currentSpecies == 0 && otherAlien.currentSpecies == 2))
                            ) // if other Alien is smaller
                        {
                            #region Who Eats Who
                            // This aliens eats the other
                            // 0:Sphere > 1:Square > 2:Triangle 
                            // Triangle eats Square / 2 eats 1
                            // Square eats Sphere / 1 eats 0
                            // Sphere eats Triangle / 0 eats 2
                            #endregion

                            // Handles eat other alien
                            hungerTimer = 0;
                            StartCoroutine(PlayActionParticle(false)); // Eating Partilce
                            otherAlien.HandleDeath();
                            StartCoroutine(IdleSecsUntilNewState(1f, AlienState.looking));
                            return;
                        }
                    }
                    break;
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
            bool isPlayerBullet = CurrentBH.isPlayerBullet;

            GameObject damageUIGo = PoolManager.SharedInstance.GetPooledDamageUI();
            if (damageUIGo != null)
            {
                damageUIGo.transform.position = MyTransform.position;

                DamageUIHandler DUIH = damageUIGo.GetComponentInChildren<DamageUIHandler>();
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



    AudioClip RandomAudioSelector(List<AudioClip[]> audioList, int state) // incase we plan to add more audio for each state
    {
        // TODO: think of something to have ot play an audio only 50% of the time?
        AudioClip[] selectedAudioArray = audioList[state];

        int randomIndex = Random.Range(0, selectedAudioArray.Length);
        AudioClip selectedAudio = selectedAudioArray[randomIndex];

        return selectedAudio;
    }
}

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
    #region Variables

    public enum AlienState
    {
        looking,
        hunting,
        evading,
        loving,
        roaming
    }

    public enum AlienAge
    {
        resource,
        child,
        sexualActive,
        fullyGrown
    }

    [Header("This Alien")]
    public Transform MyTransform;
    private Rigidbody rb;
    private Collider coll;
    public AlienAge currentAge;
    public AlienState currentState;
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
    private Vector3 targetPosition = Vector3.one * 1000;

    [Header("Target Alien")]
    public GameObject closestAlien = null;
    public GameObject lastClosestAlien = null;
    private AlienHandler closestAlienHandler = null;
    int closestAlienIndex;

    [Header("General AlienStuff")]
    public GameObject[] alienSpecies; // 0:Sphere > 1:Square > 2:Triangle  
    public GameObject[] alienSpeciesChild; // 0:Sphere > 1:Square > 2:Triangle  
    public GameObject[] alienSpeciesAdult; // 0:Sphere > 1:Square > 2:Triangle  
    public Material[] alienColors; // 0:Blue > 1:Green > 2:Red  
    public GameObject[] deadAliens;
    public Animation[] anim;
    public Renderer alienMiniMapMarker;
    public GameObject resourceSteamGO;
    public ParticleSystem resourceSteam;
    public GameObject alienActionParticles;
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

    #endregion

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
    }

    private void OnEnable()
    {
        ResetVariable();
        DisgardClosestAlien();
        ActivateCurrentModels(currentSpecies);
        if (rb == null) { rb = this.GetComponent<Rigidbody>(); }
        if (coll == null) { coll = this.GetComponent<Collider>(); }
        if (MyTransform == null) { MyTransform = this.GetComponent<Transform>(); }
        alienActionFogMain = alienActionFog.GetComponent<ParticleSystem>().main;
    }

    // Disable this script when the GameObject moves out of the camera's view
    void OnBecameInvisible()
    {
        alienSpecies[currentSpecies].SetActive(false);
        Debug.Log("Hide Mesh");
    }

    // Enable this script when the GameObject moves into the camera's view
    void OnBecameVisible()
    {
        alienSpecies[currentSpecies].SetActive(true);
    }

    private void FixedUpdate()
    {
        // If is dead, skip everytthing
        if (isDead == true) { return; }
        // If is doing action
        if (canAct == false) { return; }
        //Keep alien on the floor (y)
        if (MyTransform.position.y > .2f) { MyTransform.position = new Vector3(MyTransform.position.x, 0.1f, MyTransform.position.z); }


        if (Vector3.Distance(MyTransform.position, GameManager.SharedInstance.CameraFollowSpot.position) > 50)
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



        delta = Time.deltaTime;
        lifeTime += delta;
        lustTimer += delta;
        hungerTimer += delta;
        tickTimer += delta;
        step = (alienSpeed + lifeTime / 25) * delta;

        HandleMovement(step);

        // Only Render on Tick condition
        while (tickTimer >= tickTimerMax)
        {
            tickTimer -= tickTimerMax;

            // Handle Aging now with coroutine
            //HandleAging(lifeTime);

            if (closestAlien != null)
            {
                if (currentState == AlienState.hunting)
                {
                    HandleAttacking(closestAlien);
                    return;
                }
                else if (currentState == AlienState.evading)
                {
                    HandleFleeing(closestAlien);
                    return;
                }
                else if (currentState == AlienState.loving)
                {
                    HandleLoveApproach(closestAlien);
                    return;
                }
            }

            if (currentState == AlienState.looking)
            {
                HandleLooking();
                return;
            }
            else if (currentState == AlienState.roaming)
            {
                HandleRoaming(delta);
                return;
            }
        }
    }

    public void HandleLooking()
    {
        HandleStateIcon(0); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        StartCoroutine(DoNothingForRandomTime()); // SHort time where alien just stands and looks

        if (anim[currentSpecies] != null)
        {
            if (currentSpecies != 0)
            {
                anim[currentSpecies].Play("Armature|IDLE");
            }
        }

        #region Find closest Alien
        int layerMask = 1 << 9; // Lyer 9 is Alien
        Collider[] aliensInRange;
        aliensInRange = Physics.OverlapSphere(MyTransform.position, lookRadius, layerMask);

        float closestDistance = lookRadius; ;

        for (int i = 0; i < aliensInRange.Length; i++)
        {
            // Prevent checking on the last alien
            if (aliensInRange[i].gameObject == lastClosestAlien ||
                aliensInRange[i].gameObject == this.gameObject)
            {
                continue;
            }

            closestAlienHandler = aliensInRange[i].gameObject.GetComponent<AlienHandler>();

            if (currentSpecies == closestAlienHandler.currentSpecies) // Same species
            {
                if (closestAlienHandler.currentAge == AlienAge.resource)
                {
                    DisgardClosestAlien();
                    continue;
                }
                else if (hasUterus != closestAlienHandler.hasUterus) // Opposite Sex
                {
                    if (currentAge == AlienAge.sexualActive)
                    {
                        if (lustTimer < lustTimerThreshold)
                        {
                            DisgardClosestAlien();
                            continue;
                        }
                    }
                }
                else // Same Sex (cannot reproduce...)
                {
                    DisgardClosestAlien();
                    continue;
                }
            }
            else // other Species
            {
                closestAlienIndex = closestAlienHandler.currentSpecies;

                if (currentSpecies == closestAlienIndex + 1 || (currentSpecies == 0 && closestAlienIndex == 2)) // potential food
                {
                    if (hungerTimer < hungerTimerThreshold)
                    {
                        DisgardClosestAlien();
                        continue;
                    }
                }
            }

            // Ensures to act upon the closest alien
            float dist = Vector3.Distance(aliensInRange[i].transform.position, MyTransform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                // Set the variables
                closestAlien = aliensInRange[i].gameObject;
                targetPosition = closestAlien.transform.position;
            }

            // Stop looking if alien is really close
            //if (dist < alertDistanceThreshold) { break; }
        }

        #endregion


        #region Find fitting interaction with closest alien
        if (closestAlien == null)
        {
            currentState = AlienState.roaming;
        }
        // Have Alien look around for some time before following action
        else if (closestAlien != null || closestAlienHandler != null)
        {
            #region Who eats who
            // Check to which state the alien switches
            // 0:Sphere > 1:Square > 2:Triangle 
            // Triangle eats Square / 2 eats 1
            // Square eats Sphere / 1 eats 0
            // Sphere eats Triangle / 0 eats 2
            #endregion

            // Dublicated checks, but just in case
            if (closestAlienIndex == currentSpecies)
            {
                currentState = AlienState.loving;
            }
            else if ((currentSpecies == closestAlienIndex - 1 || (currentSpecies == 2 && closestAlienIndex == 0))) // 0:Sphere > 1:Square > 2:Triangle 
            {
                currentState = AlienState.evading;
            }
            else if ((currentSpecies == closestAlienIndex + 1 || (currentSpecies == 0 && closestAlienIndex == 2)) && hungerTimer > hungerTimerThreshold) // 0:Sphere > 1:Square > 2:Triangle 
            {
                currentState = AlienState.hunting;
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

        #endregion
    }

    private void HandleRoaming(float delta)
    {
        HandleStateIcon(5); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        HandleFindRandomSpot();
    }

    private void HandleFindRandomSpot()
    {
        if (targetPosition == Vector3.one * 1000)
        {
            float randDirX = UnityEngine.Random.Range(0, 2) - .5f;
            float randDirZ = UnityEngine.Random.Range(0, 2) - .5f;
            targetPosition = MyTransform.position + new Vector3(randDirX, 0, randDirZ) * 10;

        }

        // When arrived at position, chill for a bit then look again
        if (Vector3.Distance(MyTransform.position, targetPosition) < .1f)
        {
            StartCoroutine(DoNothingForRandomTime());
            currentState = AlienState.looking;
            targetPosition = Vector3.one * 1000;
        }

        // Check if new coordinate is within circle
        if (targetPosition.x * targetPosition.x + targetPosition.z * targetPosition.z > GameManager.SharedInstance.worldRadius * GameManager.SharedInstance.worldRadius)
        {
            targetPosition = Vector3.one * 1000;
        }
    }

    public void HandleFleeing(GameObject targetAlien)
    {
        // Add +1 so i is out of the lookradius
        if (!targetAlien.activeInHierarchy || Vector3.Distance(targetAlien.transform.position, MyTransform.position) > lookRadius + 1)
        {
            closestAlien = null;
            currentState = AlienState.looking;
        }

        HandleStateIcon(2); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = MyTransform.position + (MyTransform.position - targetAlien.transform.position);

        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(RandomAudioSelector(evadingAudioList, currentSpecies), 1f);
        }
        //Debug.Log("Escaping Vecotr: " + targetPosition);
        //Debug.DrawLine(this.transform.position, this.transform.position + (this.transform.position - targetAlien.transform.position), Color.green);
    } // Use this here on the player as well to scare the aliens away

    public void HandleAttacking(GameObject targetAlien) // Player makes them flee as well and by acting als targetAlien in PlayerManager
    {
        if (!targetAlien.activeInHierarchy)
        {
            closestAlien = null;
            currentState = AlienState.looking;
        }
        if (targetAlien.CompareTag("Player"))
        {
            HandleStateIcon(4); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(RandomAudioSelector(attackAudioList, currentSpecies), 1f);
            }
        }
        else
        {
            HandleStateIcon(1); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        }
        targetPosition = targetAlien.transform.position; // Update targetPosition only every tick update
    }

    private void HandleLoveApproach(GameObject targetAlien)
    {
        if (!targetAlien.activeInHierarchy)
        {
            closestAlien = null;
            currentState = AlienState.looking;
        }
        HandleStateIcon(3); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
        targetPosition = targetAlien.transform.position; // Update targetPosition only every tick update
    }

    private void HandleMating()
    {
        if (hasUterus)
        {
            int amountOfBabies = UnityEngine.Random.Range(1, maxAmountOfBabies);

            audioSource.PlayOneShot(RandomAudioSelector(lovemakingAudioList, currentSpecies), 1f);

            for (var i = 0; i < amountOfBabies; i++)
            {
                GameObject alienPoolGo = PoolManager.SharedInstance.GetPooledAliens();
                if (alienPoolGo != null)
                {
                    float randomOffSet = (UnityEngine.Random.Range(0, 5) - 2) / 4;

                    AlienHandler newBornAlien = alienPoolGo.GetComponent<AlienHandler>();
                    newBornAlien.currentSpecies = currentSpecies;
                    newBornAlien.transform.position = new Vector3(MyTransform.position.x + randomOffSet, 0.5f, MyTransform.position.z + randomOffSet) + Vector3.forward;
                    newBornAlien.gameObject.SetActive(true);
                }
            }
        }
        lustTimer = 0;
        DisgardClosestAlien();
    }

    public void HandleDeath()
    {
        isDead = true;
        anim[currentSpecies].Stop();
        AlienManager.SharedInstance.KillAlien(currentSpecies);
        StartCoroutine(Dissolve());
        // TODO: Add Coroutine & Ragdoll to show impact/force of bullets
        //EnableRagdoll();
    }

    private void HandleMovement(float step)
    {
        if (currentAge != AlienAge.resource && currentState != AlienState.looking)
        {
            if (anim[currentSpecies] != null)
            {
                anim[currentSpecies].Play("Armature|WALK");
            }

            MyTransform.position = Vector3.MoveTowards(MyTransform.position, targetPosition, step);

            if (Vector3.Distance(transform.position, targetPosition) < .1f)
            {
                if (currentState == AlienState.roaming)
                {
                    currentState = AlienState.looking;
                    DisgardClosestAlien();
                }
            }

            MyTransform.LookAt(targetPosition, Vector3.up);
        }
    }

    public void DeactivateAllModels()
    {
        foreach (var item in alienSpecies)
        {
            item.SetActive(false);
        }
    }

    public void ActivateCurrentModels(int currentSpeziesIndex)
    {
        DeactivateAllModels();
        alienSpecies[currentSpeziesIndex].SetActive(true);
    }

    private void HandleStateIcon(int index)
    {
        currentStateIcon.texture = allStateIcons[index]; // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader
    }

    void DisgardClosestAlien()
    {
        if (closestAlien != null)
        {
            lastClosestAlien = closestAlien;
        }
        closestAlien = null;
        closestAlienHandler = null;
        currentState = AlienState.looking;
        targetPosition = Vector3.one * 1000;
    }

    void ResetVariable()
    {
        timeToChild += UnityEngine.Random.Range(0, 10);
        isDead = false;
        canAct = true;
        alienHealth = alienLifeResource;
        currentAge = AlienAge.resource;
        lustTimer = 0;
        hungerTimer = 0;
        lifeTime = 0;
        hasUterus = UnityEngine.Random.Range(0, 2) == 1;
        HandleStateIcon(6); // 0: eye, 1: crosshair, 2: wind, 3: heart, 4: shield, 5: clock, 6: loader

        // TODO: Place this at better location
        ParticleSystem.MainModule ma = resourceSteam.main;
        if (currentSpecies == 0)
        {
            ma.startColor = Color.blue;
            alienMiniMapMarker.material = alienColors[0];
        }
        else if (currentSpecies == 1)
        {
            ma.startColor = Color.green;
            alienMiniMapMarker.material = alienColors[1];
        }
        else if (currentSpecies == 2)
        {
            ma.startColor = Color.red;
            alienMiniMapMarker.material = alienColors[2];
        }
        if (this.gameObject.activeInHierarchy)
        {
            StartCoroutine(HandleAge());
        }
    }

    void DisableRagdoll()
    {
        rb.isKinematic = true;
        rb.detectCollisions = false;
        //coll.isTrigger = true;
    }

    void EnableRagdoll()
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;
        coll.isTrigger = false;
    }

    IEnumerator DoNothingForRandomTime()
    {
        float lookTime = UnityEngine.Random.Range(10, 30) / 10;
        yield return new WaitForSeconds(lookTime);
        currentState = AlienState.looking;
    }

    IEnumerator Dissolve()
    {
        switch (currentSpecies)
        {
            case 0:
                skinRenderer1.material = dissolve;
                break;
            case 1:
                skinRenderer2.material = dissolve;
                break;
            default:
                skinRenderer3.material = dissolve;
                break;
        }

        float counter = 0;
        while (dissolve.GetFloat("_DissolveAmount") < 1)
        {
            counter += dissolveRate;
            for (int i = 0; i <= 10; i++)
            {
                dissolve.SetFloat("_DissolveAmount", counter);
                yield return new WaitForSeconds(refreshRate);
            }
        }
        dissolve.SetFloat("_DissolveAmount", 0);
        audioSource.PlayOneShot(RandomAudioSelector(dyingAudioList, currentSpecies), 1f);
        this.gameObject.SetActive(false);

        switch (currentSpecies)
        {
            case 0:
                skinRenderer1.material = orignalMaterial[0];
                break;
            case 1:
                skinRenderer2.material = orignalMaterial[1];
                break;
            default:
                skinRenderer3.material = orignalMaterial[2];
                break;
        }

    }

    IEnumerator PlayActionParticle(bool isLoving)
    {
        if (Vector3.Distance(MyTransform.position, GameManager.SharedInstance.CameraFollowSpot.position) > 50) { yield return null; }

        if (isLoving)
        {
            alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(Color.red, Color.magenta);
        }
        else
        {
            alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(Color.gray, Color.black);
        }
        alienActionParticles.SetActive(true);
        yield return new WaitForSeconds(1f);
        alienActionParticles.SetActive(false);

    }

    IEnumerator WaitForInteraction(float seconds)
    {
        canAct = false;
        yield return new WaitForSeconds(seconds);
        canAct = true;
    }

    IEnumerator HandleAge()
    {
        // Resource Life
        resourceSteamGO.SetActive(true);
        alienSpeciesChild[currentSpecies].SetActive(true);
        alienSpeciesAdult[currentSpecies].SetActive(false);
        currentAge = AlienAge.resource;
        alienHealth = alienLifeResource;
        MyTransform.localScale = Vector3.one * 0.7f;
        AlienManager.SharedInstance.AddToResourceList(this);
        yield return new WaitForSeconds(timeToChild);

        // Child Life
        resourceSteamGO.SetActive(false);
        currentAge = AlienAge.child;
        alienHealth = alienLifeChild;
        alienSpeciesChild[currentSpecies].SetActive(false);
        alienSpeciesAdult[currentSpecies].SetActive(true);
        MyTransform.localScale = Vector3.one * .6f;
        // TODO: Check if available in List?!
        AlienManager.SharedInstance.RemoveFromResourceList(this);
        yield return new WaitForSeconds(timeToSexual);

        // Sexual active Life
        currentAge = AlienAge.sexualActive;
        alienHealth = alienLifeSexual;
        alienSpeciesChild[currentSpecies].SetActive(false);
        alienSpeciesAdult[currentSpecies].SetActive(true);
        StartCoroutine(HandleGrowing(.6f, .8f));
        yield return new WaitForSeconds(timeToFullGrown);

        // Full Grown Life
        currentAge = AlienAge.fullyGrown;
        alienHealth = alienLifeFullGrown;
        alienSpeciesChild[currentSpecies].SetActive(false);
        alienSpeciesAdult[currentSpecies].SetActive(true);
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
        // Handle Alien interaction
        if (other.gameObject.CompareTag("Alien"))
        {
            AlienHandler otherAlien = other.gameObject.GetComponent<AlienHandler>();
            if (currentAge == AlienAge.resource)
            {
                if (currentSpecies != otherAlien.currentSpecies) // You the resource gets trampled
                {
                    AlienManager.SharedInstance.RemoveFromResourceList(this);
                    this.gameObject.SetActive(false);
                }
            }
            else
            {
                if (currentAge == AlienAge.sexualActive && currentSpecies == otherAlien.currentSpecies)
                {
                    if (lustTimer > lustTimerThreshold)
                    {
                        // Spawn new Species
                        HandleMating();
                        StartCoroutine(PlayActionParticle(true)); // Loving Partilce
                        StartCoroutine(WaitForInteraction(1f));
                        return;
                    }

                }
                else if (
                    hungerTimer > hungerTimerThreshold &&
                    (currentSpecies == otherAlien.currentSpecies + 1 ||
                    (currentSpecies == 0 && otherAlien.currentSpecies == 2)))
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
                    otherAlien.gameObject.SetActive(false);
                    StartCoroutine(PlayActionParticle(false)); // Eating Partilce
                    StartCoroutine(WaitForInteraction(1f));
                    DisgardClosestAlien();
                    return;

                }
                return;
            }
            return;
        }
        // Handle Bullet interaction
        else if (other.gameObject.CompareTag("Bullet"))
        {
            // Cannot shoot resource
            if (currentAge == AlienAge.resource) { return; }

            //Debug.Log("Handle Bullet damage to alien here");
            BulletHandler BH = other.gameObject.GetComponent<BulletHandler>();
            alienHealth -= BH.bulletDamage;
            // Needs to deactivate this here so it does not trigger multiple times
            // Maybe deactive the Collider on the Bullet and then make sure to enable it again if new spawned

            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(RandomAudioSelector(beingAttackedAudioList, currentSpecies), 1f);
            }

            other.gameObject.SetActive(false);


            // Handle Alien Death
            if (alienHealth <= 0 && isDead == false)
            {
                HandleDeath();
                return;
            };

            transform.position = GetComponent<Transform>().position;

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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AlienStateMachine : MonoBehaviour
{
    [SerializeField, TextArea] public string debugsString;

    public BaseState _currentState;
    public AlienClass alienClass;
    
    public enum AlienAge
    {
        resource,
        child,
        sexualActive,
        fullyGrown
    }
    
    
    [Header("Alien")]
    public NavMeshAgent agent;
    public GameObject[] alienSpecies; // 0:Sphere > 1:Square > 2:Triangle  
    public GameObject[] alienSpeciesChild; // 0:Sphere > 1:Square > 2:Triangle  
    public GameObject[] alienSpeciesAdult; // 0:Sphere > 1:Square > 2:Triangle  
    public ParticleSystem alienActionFog;
    public AlienAge currentAge;

    public AlienStateMachine otherAlien;
    // [SerializeField] private AlienState currentStateValue; //this holds the actual value, should be private

    [Header("General Alien Marker")]
    public Renderer alienMiniMapMarker;
    
    [Header("States")] 
    public LookingState lookingState;
    public HuntingState huntingState;
    public EvadingState evadingState;
    public LovingState lovingState;
    public DeathState deathState;
    public RoamingState roamingState;
    
    [Header("Looking State")]
    public List<Collider> aliensInRange;
    public Collider[] aliensInRangeCollider;
    public Collider[] aliensInRangeColliderOrdered;
    
    private void Awake()
    {
        if (alienClass == null)
        {
            alienClass = new AlienClass();
        }
        
        InitializeStates();
        ChangeState(roamingState);
        alienClass.rigidbody = GetComponent<Rigidbody>();
        alienClass.resourceSteamGO = GetComponentInChildren<ParticleSystem>().gameObject;
        alienClass.resourceSteamMain = alienClass.resourceSteamGO.GetComponent<ParticleSystem>().main;
        alienClass.agent = GetComponent<NavMeshAgent>();
        agent = alienClass.agent;
        
        if (!agent.enabled)
        {
            agent.enabled = true;
        }
    }
    
    private void Start()
    {
        alienClass.worldRadiusSquared = GameManager.Instance.worldRadius * GameManager.Instance.worldRadius;
        alienClass.alienActionFogMain = alienActionFog.gameObject.GetComponentInChildren<ParticleSystem>().main;
        if (alienClass.MyTransform == null) { alienClass.MyTransform = this.gameObject.GetComponent<Transform>(); }
    }

    private void Update()
    {
        _currentState.Update();
        
        debugsString = "Current State: " + _currentState + "\n";
        HandleRendering();
        
        if (alienClass.isRendered == false || alienClass.canAct == false || alienClass.isDead == true) { return; }

        alienClass.delta = Time.deltaTime;
        HandleUpdateVariables();

        if (currentAge == AlienAge.resource) { return; }

        // HandleUpdateTarget();
        // HandleAnimation();
        //
        // // Finaly move the alien if it can
        // HandleMovement();

    }

    private void FixedUpdate()
    {
        alienClass.hungerTimer += Time.fixedDeltaTime;
        alienClass.lustTimer += Time.fixedDeltaTime;
        alienClass.tickTimer += Time.fixedDeltaTime;
        alienClass.lifeTime += Time.fixedDeltaTime;
        
        // if(alienClass.tickTimer > alienClass.tickTimerMax && _currentState != lookingState)
        // {
        //     ChangeState(lookingState);
        //     alienClass.tickTimer = 0;
        // }
    }

    private void InitializeStates()
    {
        roamingState = new RoamingState(this);
        lookingState = new LookingState(this);
        huntingState = new HuntingState(this);
        evadingState = new EvadingState(this);
        lovingState = new LovingState(this);
        deathState = new DeathState(this);
    }

    public void ChangeState(BaseState newState)
    {
        _currentState = newState;
        _currentState.Enter();
        _currentState?.Exit();
    }
    
    public BaseState GetCurrentState()
    {
        return _currentState;
    }

    private void OnEnable()
    {
        if (AlienManager.Instance == null)
        {
            Debug.Log("AlienManager.Instance is null");
            return; 
        }
        StartCoroutine(HandleAge(alienClass.spawnAsAdults));
        ActivateCurrentModels(alienClass.currentSpecies);
    }

    private void HandleRendering()
    {
        if (alienClass.distanceToCameraSpot > AlienManager.Instance.renderDistance)
        {
            if (alienClass.isRendered == true)
            {
                DeactivateAllModels();
                alienClass.isRendered = false;
            }
        }
        else
        {
            if (alienClass.isRendered == false)
            {
                ActivateCurrentModels(alienClass.currentSpecies);
                alienClass.isRendered = true;
            }
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
        alienMiniMapMarker.material = AlienManager.Instance.alienColors[currentSpeziesIndex];

        alienClass.MyCollisionCollider = alienSpecies[currentSpeziesIndex].GetComponent<Collider>();
    }
    
    private void HandleUpdateVariables()
    {
        alienClass.lifeTime += alienClass.delta;
        alienClass.lustTimer += alienClass.delta;
        alienClass.hungerTimer += alienClass.delta;
        alienClass.tickTimer += alienClass.delta;
        
        if (_currentState == huntingState)
        {
            alienClass.huntingSpeed = (AlienManager.Instance.alienSpeedHunting + ((alienClass.lustTimer + alienClass.hungerTimer) / 100)) * alienClass.delta; // + ((2 * (lustTimer + hungerTimer)) / (lustTimer + hungerTimer)); TODO: make better?! Way too fast
        }
        else
        {
            alienClass.huntingSpeed = (AlienManager.Instance.alienSpeed + ((alienClass.lustTimer + alienClass.hungerTimer) / 100)) * alienClass.delta; // + ((2 * (lustTimer + hungerTimer)) / (lustTimer + hungerTimer)); TODO: make better?! Way too fast
        }

        alienClass.randomNumber = Random.Range(1, 11) / 10;
    }
    
    public void SetTarget(GameObject currentTargetGO)
    {
        if (alienClass.targetAlien != null)
        {
            alienClass.lastTargetAlien = alienClass.targetAlien;
        }

        alienClass.targetAlien = currentTargetGO;
    }
    
    public IEnumerator IdleSecsUntilNewState()
    {
        alienClass.canAct = false;
        alienClass.hasNewTarget = false;
        alienClass.targetPosition3D = Vector3.zero;
        alienClass.distanceToCurrentTarget = 999f;
        alienClass.lookTimeIdle = Random.Range(1, (alienClass.randomNumber + 1) * 10) / 10;
        yield return new WaitForSeconds(alienClass.lookTimeIdle);
        alienClass.canAct = true;
    }
    
    private IEnumerator HandleAge(bool isSpawningAsAdult)
    {
        yield return new WaitForSeconds(.5f);

        if (isSpawningAsAdult == false)
        {
            // Resource Life
            UpdateResourceSteam(alienClass.currentSpecies);
            alienClass.resourceSteamGO.SetActive(true);
            alienClass.rigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            currentAge = AlienAge.resource;
            alienClass.alienHealth = AlienManager.Instance.alienLifeResource;
            alienClass.MyTransform.localScale = Vector3.one * AlienManager.Instance.resourceScale;
            yield return new WaitForSeconds(alienClass.minTimeToChild);
        }

        // Child Life
        alienClass.resourceSteamGO.SetActive(false);
        alienClass.rigidbody.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
        alienClass.alienHealth = AlienManager.Instance.alienLifeChild;
        currentAge = AlienAge.child;
        ChangeState(roamingState);
        alienClass.MyTransform.localScale = Vector3.one * AlienManager.Instance.childScale;
        alienSpeciesChild[alienClass.currentSpecies].SetActive(false);
        alienSpeciesAdult[alienClass.currentSpecies].SetActive(true);
        if (AlienManager.Instance.resourceSphere.Count + AlienManager.Instance.resourceSquare.Count + AlienManager.Instance.resourceTriangle.Count > 0)
        {
            // AlienManager.Instance.RemoveFromResourceList(AlienStateMachine); // TODO: Check if available in List?!
        }
        yield return new WaitForSeconds(AlienManager.Instance.timeToSexual);

        // Sexual active Life
        alienClass.alienHealth = AlienManager.Instance.alienLifeSexual;
        currentAge = AlienAge.sexualActive;
        StartCoroutine(HandleGrowing(AlienManager.Instance.childScale, AlienManager.Instance.sexualActiveScale));
        yield return new WaitForSeconds(AlienManager.Instance.timeToFullGrown);

        // Full Grown Life
        alienClass.alienHealth = AlienManager.Instance.alienLifeFullGrown;
        currentAge = AlienAge.fullyGrown;
        StartCoroutine(HandleGrowing(AlienManager.Instance.sexualActiveScale, AlienManager.Instance.fullGrownScale));
    }
    
    private void UpdateResourceSteam(int currentIndex)
    {
        if (AlienManager.Instance == null) { return; }

        if (currentIndex == 0)
        {
            alienClass.resourceSteamMain.startColor = AlienManager.Instance.alienColors[currentIndex].color;
        }

        if (currentIndex == 1)
        {
            alienClass.resourceSteamMain.startColor = AlienManager.Instance.alienColors[currentIndex].color;
        }

        if (currentIndex == 2)
        {
            alienClass.resourceSteamMain.startColor = AlienManager.Instance.alienColors[currentIndex].color;
        }
    }

    private IEnumerator HandleGrowing(float oldFactor, float newFactor)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(.5f / 10); // Total duration of transform 0.5f seconds
            alienClass.MyTransform.localScale = Vector3.one * ((oldFactor + newFactor * i / 10) - (oldFactor * i / 10));
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Alien"))
        {
            AlienStateMachine prey = other.GetComponent<AlienStateMachine>();
            if (prey.alienClass.currentSpecies != alienClass.currentSpecies && _currentState == evadingState)
            {
                ChangeState(deathState);
                
                alienClass.hungerTimer = 0; // Reset hunger
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

//Death By bullet logic, Animation, Sound

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
        alienClass.rigidbody = GetComponent<Rigidbody>();
        alienClass.resourceSteamGO = GetComponentInChildren<ParticleSystem>().gameObject;
        alienClass.resourceSteamMain = alienClass.resourceSteamGO.GetComponent<ParticleSystem>().main;
        alienClass.agent = GetComponent<NavMeshAgent>();
        agent = alienClass.agent;
        
        if (!agent.enabled)
        {
            agent.enabled = true;
        }
        
        ChangeState(roamingState);
        Debug.Log(_currentState);
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
        // if (!alienClass.canAct || alienClass.isDead) return;
        
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    private void OnEnable()
    {
        if (AlienManager.Instance == null)
        {
            Debug.Log("AlienManager.Instance is null");
        }
        
        ResetVariable();
        
        StartCoroutine(HandleAge(alienClass.spawnAsAdults));
        ActivateCurrentModels(alienClass.currentSpecies);
    }

    private void OnDisable()
    {
        ResetVariable();
        StopAllCoroutines();
        alienClass.brainWashed = false;
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
    
    private IEnumerator PlayActionParticle(BaseState currentState)
    {
        if (alienClass.isRendered == false)
        {
            yield return null;
        }
        else
        {
            if (currentState == lovingState)
            {
                alienClass.alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(AlienManager.Instance.loveMakingColor1, AlienManager.Instance.loveMakingColor2);
            }
            else if (currentState == huntingState)
            {
                alienClass.alienActionFogMain.startColor = new ParticleSystem.MinMaxGradient(AlienManager.Instance.fightingColor1, AlienManager.Instance.fightingColor2);
            }

            alienClass.alienActionParticlesGO.SetActive(true);
            yield return new WaitForSeconds(1f);
            alienClass.alienActionParticlesGO.SetActive(false);
        }
    }

    private void ResetVariable()
    {
        alienClass.lustTimer = 0;
        alienClass.hungerTimer = 0;
        alienClass.lifeTime = 0;
        alienClass.rigidbody.velocity = Vector3.zero;
        currentAge = AlienAge.resource;
        alienClass.minTimeToChild += UnityEngine.Random.Range(0, 10); // This just get added on top of minTimeToChild 
        alienClass.hasUterus = UnityEngine.Random.Range(0, 2) == 1;
        alienClass.alienHealth = AlienManager.Instance.alienLifeResource;
        alienClass.brainWashed = false; // AKA tutuorial scene
        alienClass.canAct = true;
        alienClass.isDead = false;
        alienClass.spawnAsAdults = false;
        alienClass.gotAttackedByPlayer = false;
        alienClass.isAttackingPlayer = false;
        alienClass.isEvadingPlayer = false;
        alienClass.targetPosition3D = Vector3.zero;
        alienClass.targetAlien = null;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Alien"))
        {
            AlienStateMachine otherAlien = other.GetComponent<AlienStateMachine>();
            if (otherAlien.alienClass.currentSpecies != alienClass.currentSpecies && _currentState == evadingState)
            {
                ChangeState(deathState);
                
                alienClass.hungerTimer = 0; // Reset hunger
            }
            
            if (alienClass.currentSpecies == otherAlien.alienClass.currentSpecies && _currentState == lovingState && otherAlien._currentState == lovingState)
            {
                alienClass.lustTimer = 0;
                otherAlien.alienClass.lustTimer = 0;
                StartCoroutine(PlayActionParticle(lovingState));
                // audioSource.PlayOneShot(RandomAudioSelectorFoley(AlienManager.Instance.aliensLoving));
                ChangeState(lovingState);
            }
        }
        
        if (other.CompareTag("Bullet"))
        {
            if(currentAge == AlienAge.resource) return;
            
            alienClass.CurrentBH = other.gameObject.GetComponent<BulletHandler>();
            alienClass.currentBulletDamage = alienClass.CurrentBH.bulletDamage;
            
            alienClass.alienHealth -= alienClass.currentBulletDamage;
            alienClass.isPlayerBullet = alienClass.CurrentBH.isPlayerBullet;

            if (Random.Range(0, 2) == 1)
            {
                alienClass.gotAttackedByPlayer = true;
                ChangeState(huntingState);
            }
            else
            {
                alienClass.gotAttackedByPlayer = true;
                ChangeState(evadingState);
            }
            
            alienClass.damageUIGo = PoolManager.Instance.GetPooledDamageUI();
            if (alienClass.damageUIGo != null)
            {
                alienClass.damageUIGo.transform.position = alienClass.MyTransform.position;

                alienClass.DUIH = alienClass.damageUIGo.GetComponentInChildren<DamageUIHandler>();
                alienClass.DUIH.damageValue = alienClass.currentBulletDamage;

                alienClass.damageUIGo.SetActive(true);
            }
            
            if (alienClass.alienHealth <= 0 && alienClass.isDead == false)
            {
                ChangeState(deathState);
            };
            
        }
    }
}

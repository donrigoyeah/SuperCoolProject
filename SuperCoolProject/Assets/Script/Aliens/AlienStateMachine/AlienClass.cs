using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
[System.Serializable]
public class AlienClass
{
    [Header("NavMesh")] 
    public NavMeshAgent agent;
    
    //Alien enum age,
    
    // [SerializeField] private AlienState lastAlienState;
    public int layerMaskAlien = 1 << 9; // Lyer 9 is Alien
    public int aliensInRangeCount;
    public float worldRadiusSquared;
    public BulletHandler CurrentBH;
    public float currentBulletDamage;
    
    [Header("This Alien")]
    public Rigidbody rigidbody;
    public bool isRendered = true;
    public bool brainWashed = false;
    public bool canAct = true;
    public int currentSpecies = 0;
    public Transform MyTransform;
    public Vector2 MyTransform2D;
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
    public float currentShortestDistanceLooking;
    public float currentDistanceLooking;
    public float randDirXRoaming;
    public float randDirZRoaming;
    public GameObject newBornAlienPoolGo;
    public AlienHandler newBornAlien;
    public float randomOffSetBabySpawn;
    public Vector3 targetPosition3D;
    public Vector2 targetPosition2D;

    [Header("Target Alien")]
    public GameObject targetAlien;
    public GameObject lastTargetAlien;
    public AlienHandler targetAlienHandler;
    public AlienHandler otherAlienHandler;
    
    [Header("General Alien References")]
    public Animation[] anim;
    public GameObject resourceSteamGO;
    public GameObject alienActionParticlesGO;
    public ParticleSystem resourceSteam;
    public ParticleSystem.MainModule alienActionFogMain;
    public ParticleSystem.MainModule resourceSteamMain;
    public float delta;
    public float huntingSpeed;
    public int minTimeToChild = 5;
    
    [Header("More reference")]
    public float lookTimeIdle;
    private GameObject deadAlienGO;
    public Rigidbody deadAlienRB;
    public DeadAlienHandler deadAlien;
    public Vector2 CameraFollowSpot2D;
    public float randomNumber;
    public float distanceToCameraSpot;
    public bool isPlayerBullet;
    public GameObject damageUIGo;
    public DamageUIHandler DUIH;
    Collider[] tmpColliderArray;
    
    [Header("Tick stats")]
    public float tickTimer;
    public float tickTimerMax = .5f;
    public bool hasNewTarget = false;
}

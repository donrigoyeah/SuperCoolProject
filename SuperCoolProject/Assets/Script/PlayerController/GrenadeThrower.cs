using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GrenadeThrower : MonoBehaviour { 
    
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int PhysicsFrame = 62;
    [SerializeField] private Grenade grenadePrefab;
    [SerializeField] private float force = 1f;
    [SerializeField] private float maxForce = 10f;
    [SerializeField] private float currentForce = 0f;
    [SerializeField] private Transform spawnLocation;
    
    private Scene simulateScene;
    private PhysicsScene physicsScene;

    private void Start() {
        CreatePhysicsScene();
    }

    private void CreatePhysicsScene() {
        simulateScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        physicsScene = simulateScene.GetPhysicsScene();
    }

    private void Update() {
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            force = 0f;
        }
        
        if(Input.GetKeyUp((KeyCode.Q)))
        {  
            LaunchGrenade();
            lineRenderer.positionCount = 0;
        }
        
        if (Input.GetKey(KeyCode.Q))
        {
            force += 0.3f;
            currentForce = Mathf.Min(maxForce, force);
            UpdateTrajectory();
        }
    }
    
    private void LaunchGrenade() {
        var spawned = Instantiate(grenadePrefab, spawnLocation.position, spawnLocation.rotation); //this is used to spawn grenede at spawnlocation with alloted force
        spawned.Init(spawnLocation.forward * currentForce, false);
    }
    
    private void UpdateTrajectory() {
        
        if (lineRenderer.positionCount < PhysicsFrame) {
            lineRenderer.positionCount = PhysicsFrame;
        }

        Grenade grenadeTrajectory = Instantiate(grenadePrefab, spawnLocation.position, Quaternion.identity); // this is used to simulate trejectory
        SceneManager.MoveGameObjectToScene(grenadeTrajectory.gameObject, simulateScene);

        grenadeTrajectory.Init(spawnLocation.forward * currentForce, true);

        for (var i = 0; i < PhysicsFrame; i++) {
            physicsScene.Simulate(Time.fixedDeltaTime);
            if (i < lineRenderer.positionCount) {
                lineRenderer.SetPosition(i, grenadeTrajectory.transform.position);
            }
        }

        Destroy(grenadeTrajectory.gameObject);
    }
}
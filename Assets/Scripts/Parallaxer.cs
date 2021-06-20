using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxer : MonoBehaviour{
	
	//keep track of stars,clouds,pipes
	class PoolObject {
		
		public Transform transform;

		//determines if the object is available or not
		public bool inUse;

		//constructor
		public PoolObject(Transform t) { transform = t; }
		
		//called when using an object
		public void Use() { inUse = true; }
	
		//called when an object is not in use anymore
		public void Dispose() { inUse = false; }
	}

	[System.Serializable]
	public struct YSpawnRange {
		public float minY;
		public float maxY;
	}

	public GameObject Prefab;
	public int poolSize;
	
	//how fast are the objects moving
	public float shiftSpeed;
	public float spawnRate;

	public YSpawnRange ySpawnRange;
	public Vector3 defaultSpawnPos;
	public bool spawnImmediate;
	public Vector3 immediateSpawnPos;

	//make sure it will fit all the screen types
	public Vector2 targetAspectRatio;

	float spawnTimer;

	PoolObject[] poolObjects;

	//store the target aspect
	float targetAspect;

	GameManager game;

	void Awake() {
		Configure();
	}
    // Start is called before the first frame update(always called after Awake())
    void Start()
    {
        game = GameManager.Instance;
    }

    void OnEnable() {
		GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
	}

	void OnDisable() {
		GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
	}

    //handling disposing the poolObjects
    void OnGameOverConfirmed() {
		for (int i = 0; i < poolObjects.Length; i++) {
			poolObjects[i].Dispose();
			poolObjects[i].transform.position = Vector3.one * 1000;
		}
		Configure();
	}

    // Update is called once per frame
    void Update()
    {
        //we dont need to update if the game is over
        if (game.GameOver) return;

		Shift();
		spawnTimer += Time.deltaTime;
		if (spawnTimer > spawnRate) {
			Spawn();
			spawnTimer = 0;
		}
    }

    void Configure() {
		//spawning pool objects
		targetAspect = targetAspectRatio.x / targetAspectRatio.y;
        //creating the PoolObjects array
		poolObjects = new PoolObject[poolSize];
		for (int i = 0; i < poolObjects.Length; i++) {
			GameObject go = Instantiate(Prefab) as GameObject;
			Transform t = go.transform;
			t.SetParent(transform);
			t.position = Vector3.one * 1000;
			poolObjects[i] = new PoolObject(t);
		}

		if (spawnImmediate) {
			SpawnImmediate();
		}
	}

	void SpawnImmediate() {
		//moving pool objects into place
		Transform t = GetPoolObject();
        //if true, it means that poolSize is too small
		if (t == null) return;
		Vector3 pos = Vector3.zero;
		pos.y = Random.Range(ySpawnRange.minY, ySpawnRange.maxY);
		pos.x = (immediateSpawnPos.x * Camera.main.aspect) / targetAspect;
		t.position = pos;
	}

    //customizing the position where something is spawned initially
    //it is going to spawn 2 objects 
	void Spawn() {
		Transform t = GetPoolObject();
        //if true, it means that poolSize is too small
		if (t==null) return;
		Vector3 pos = Vector3.zero;
		pos.y = Random.Range(ySpawnRange.minY, ySpawnRange.maxY);
		pos.x = (defaultSpawnPos.x * Camera.main.aspect) / targetAspect;
		t.position = pos; 
	}

    void Shift() {
		//loop through pool objects 
		//moving them using the shiftSpeed
		//discarding them as they go off screen
		for (int i = 0; i < poolObjects.Length; i++) {
			poolObjects[i].transform.position += -Vector3.right * shiftSpeed * Time.deltaTime;
			CheckDisposeObject(poolObjects[i]);
		}
	}

    //checking if the object needs to be disposed
	void CheckDisposeObject(PoolObject poolObject) {
		//place objects off screen
		if (poolObject.transform.position.x < -immediateSpawnPos.x ) {
            //setting inUse to False
			poolObject.Dispose();
			poolObject.transform.position = Vector3.one * 1000;
		}
	}

	Transform GetPoolObject() {
		//retrieving first available pool object
		for (int i = 0; i < poolObjects.Length; i++) {
			if (!poolObjects[i].inUse) {
                //setting inUse to True
				poolObjects[i].Use();
				return poolObjects[i].transform;
			}
		}
		return null;
	}
}

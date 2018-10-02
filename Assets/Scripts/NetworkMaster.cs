using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkMaster : NetworkManager {

	// Use this for initialization
	void Start () {
		playerPrefab = spawnPrefabs[1];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

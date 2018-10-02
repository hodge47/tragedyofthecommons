using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Destroy(this.gameObject, 5f);
	}

	public void SetText(string _text){
		this.GetComponent<Text>().text = _text;
	}
}

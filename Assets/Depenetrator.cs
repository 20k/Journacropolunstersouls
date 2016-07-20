using UnityEngine;
using System.Collections;

public class Depenetrator : MonoBehaviour {

    public float maxVel = 0.1f;

	// Use this for initialization
	void Start () {
        Rigidbody body = GetComponent<Rigidbody>();

        body.maxDepenetrationVelocity = maxVel;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

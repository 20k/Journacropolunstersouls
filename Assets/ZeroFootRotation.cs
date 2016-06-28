using UnityEngine;
using System.Collections;

public class ZeroFootRotation : MonoBehaviour {

    public Transform trans;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
        Quaternion rot = trans.rotation;

        Vector3 euler = rot.eulerAngles;

        trans.rotation = Quaternion.Euler(euler.x, euler.y, euler.z);
	}
}

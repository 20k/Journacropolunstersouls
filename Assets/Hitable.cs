using UnityEngine;
using System.Collections;

public class Hitable : MonoBehaviour {

    private GameObject PhysCube;

	// Use this for initialization
	void Start () {
        PhysCube = GameObject.Find("PhysCube");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// Ruh roh!
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Damaging")
            return;

        GameObject me = gameObject;

        Vector3 half = (me.transform.position + other.transform.position) / 2f;

        GameObject obj = Instantiate(PhysCube, half, Quaternion.identity) as GameObject;

        obj.SetActive(true);
    }
}

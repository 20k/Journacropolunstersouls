using UnityEngine;
using System.Collections;

public class MagicSystem : MonoBehaviour {

    public Transform position;

    GameObject iceCube;
    int placed = 0;

	// Use this for initialization
	void Start () {
        iceCube = GameObject.Find("IceCube");
    }
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKeyDown("q"))
        {
            GameObject obj = Instantiate(iceCube, position.position, Quaternion.identity) as GameObject;

            obj.SetActive(true);
        }
    }
}

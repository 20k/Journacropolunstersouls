using UnityEngine;
using System.Collections;

public class PlayerDetector : MonoBehaviour {

    bool playerInside = false;
    bool justLeft = false;

	// Use this for initialization
	void Start () {
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();

        collider.center = new Vector3(0, 2, 0);
        collider.isTrigger = true;

        var vec = collider.size;
        vec.y = 10;
        collider.size = vec;
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    void LateUpdate()
    {
        ///justLeft = false; //????
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "PlayerTrigger")
            return;

        playerInside = true;

        Debug.Log("hello");
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag != "PlayerTrigger")
            return;

        if (!playerInside)
            return;

        playerInside = false;
        justLeft = true;

        Debug.Log("probably going to explode now");
    }

    public bool RequestPlayerJustLeftAction()
    {
        bool toRet = justLeft;

        justLeft = false;

        return toRet;
    }
}

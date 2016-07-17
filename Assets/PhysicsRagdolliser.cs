using UnityEngine;
using System.Collections;

public class PhysicsRagdolliser : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.F1))
        {
            ToRagdoll();

            WigglesMaster master = GetComponent<WigglesMaster>();

            master.enabled = false;
        }
	}

    void ToRagdoll()
    {
        Damageable[] toRagdoll = GetComponentsInChildren<Damageable>();
        
        for(int i=0; i<toRagdoll.Length; i++)
        {
            GameObject obj = toRagdoll[i].gameObject;

            obj.AddComponent<Rigidbody>();
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;


public class WigglesMaster : MonoBehaviour {

    public Transform wiggles;
    public float spiderSpeed = 1.0f;

    List<ProceduralLeg> legs = new List<ProceduralLeg>();

	// Use this for initialization
	void Start () {
	
	}

    float getMult()
    {
        float mult = 1f;

        if (Input.GetKey(KeyCode.H))
        {
            mult = 2f;
        }

        return mult;
    }

    Vector2 getDebugInput()
    {
        Vector2 ret = new Vector2(0,0);

        if (Input.GetKey(KeyCode.L))
            ret.x -= 1;

        if (Input.GetKey(KeyCode.J))
            ret.x += 1;

        if (Input.GetKey(KeyCode.I))
            ret.y += 1;

        if (Input.GetKey(KeyCode.K))
            ret.y -= 1;


        return ret * Time.deltaTime * getMult();
    }

    // Update is called once per frame
    void Update () {
        bool toPlant = Input.GetKeyDown(KeyCode.T);

        if(toPlant)
        {
            for(int i=0; i<legs.Count; i++)
            {
                legs[i].PlantFoot(i);
                legs[i].IKPlantFoot();
            }
        }

        Vector2 input = getDebugInput() * spiderSpeed;

        Vector3 translate = new Vector3(input.x, 0, -input.y);

        wiggles.transform.position += translate;

        if (Mathf.Abs(input.y) < Mathf.Epsilon)
            return;

        int moveDir = 0;

        if (input.y > 0)
            moveDir = 1;
        else
            moveDir = -1;

	    for(int i=0; i<legs.Count; i++)
        {
            legs[i].Tick(Time.deltaTime * moveDir * getMult());
        }
	}

    void FixedUpdate()
    {
        
    }

    public void Register(ProceduralLeg leg)
    {
        legs.Add(leg);
    }
}

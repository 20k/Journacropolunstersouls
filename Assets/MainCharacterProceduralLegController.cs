using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainCharacterProceduralLegController : MonoBehaviour {

    public LegHub legHub;

    List<ProceduralLeg> legs;

    Vector3 lastPosition;

    bool isIdle = false;

    Vector2 moveDir = new Vector2(0,0);

	// Use this for initialization
	void Start () {
        legs = legHub.GetLegs();
	}
	
	// Update is called once per frame
	void Update () {
        for(int i=0; i<legs.Count; i++)
        {
            if (!legs[i].IsPlanted())
                legs[i].PlantFoot(i);

            legs[i].enableRestTracking();
        }

        if (moveDir.magnitude < 0.01f)
        {
            SetIdling(true);
        }
        else
            SetIdling(false);

        for (int i=0; i<legs.Count; i++)
        {
            legs[i].Tick(Time.deltaTime, 1, 1);

            //float diff = (lastPosition - transform.position).magnitude;

            //float maxErr = 0.00001f;

            //legs[i].SetIdling(diff < maxErr);

            legs[i].SetIdling(isIdle);
        }

        lastPosition = transform.position;
	}

    public void SetMoveDir(Vector2 input)
    {
        moveDir = input;
    }

    void SetIdling(bool idle)
    {
        isIdle = idle;
    }
}

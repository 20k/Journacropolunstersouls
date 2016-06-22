using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainCharacterProceduralLegController : MonoBehaviour {

    public LegHub legHub;
    public float baseForwardOffset = -1;

    List<ProceduralLeg> legs;

    Vector3 lastPosition;

    bool isIdle = false;

    Vector2 moveDir = new Vector2(0,0);
    float runMult = 1f;
    bool isRunning = false;

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

            legs[i].SetIdling(isIdle);

            if(isRunning)
            {
                legs[i].forwardOffset = baseForwardOffset * runMult;
            }
            else
            {
                legs[i].forwardOffset = baseForwardOffset;
            }
        }

        float legTimeMult = 1f;

        if (isRunning)
            legTimeMult = 0.8f;

        legHub.setLegTimeMult(legTimeMult);

        lastPosition = transform.position;
	}

    public void SetMoveDir(Vector2 input, float pRunMult, bool running)
    {
        moveDir = input;

        runMult = pRunMult;

        isRunning = running;
    }

    void SetIdling(bool idle)
    {
        isIdle = idle;
    }
}

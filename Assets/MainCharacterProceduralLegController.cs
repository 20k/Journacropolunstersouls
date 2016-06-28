using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainCharacterProceduralLegController : MonoBehaviour
{
    public LegHub legHub;
    public float baseForwardOffset = -1;
    public float bobHeight = 1;
    public Transform body;
    public SwordAttack swordAttack;

    List<ProceduralLeg> legs;

    Vector3 lastPosition;

    bool isIdle = false;

    Vector2 moveDir = new Vector2(0, 0);
    float runMult = 1f;
    bool isRunning = false;

    Vector3 baseOffset;

    float interFeetDistance = 1;

    // Use this for initialization
    void Start()
    {
        legs = legHub.GetLegs();

        baseOffset = body.localPosition;

        if (legs.Count != 2)
            return;

        interFeetDistance = (legs[0].transform.GetChild(1).position - legs[1].transform.GetChild(1).position).magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < legs.Count; i++)
        {
            if (!legs[i].IsPlanted())
                legs[i].PlantFoot(i);

            legs[i].enableRestTracking();
        }

        if (moveDir.magnitude < 0.01f)
            SetIdling(true);
        
        else
            SetIdling(false);

        for (int i = 0; i < legs.Count; i++)
        {
            legs[i].Tick(Time.deltaTime, 1, 1);

            legs[i].SetIdling(isIdle);

            if (isRunning)
            {
                legs[i].forwardOffset = baseForwardOffset * runMult;
            }
            else
            {
                legs[i].forwardOffset = baseForwardOffset;
            }

            legs[i].forwardOffset *= swordAttack.getMovementMult();

            if(swordAttack.getMovementMult() < 0.1f)
            {
                float dir = ((float)i - 0.5f) * 2f;

                legs[i].forwardOffset = dir * baseForwardOffset;

                legs[i].SetIdling(false);
            }
        }

        float legTimeMult = 1f;

        if (isRunning)
            legTimeMult = 0.8f;

        legHub.setLegTimeMult(legTimeMult);

        lastPosition = transform.position;

        //body.Translate(new Vector3(0, getBob(), 0));


        //Vector3 npos = body.localPosition;
        Vector3 npos = new Vector3(0, 0, 0);

        npos.y = getBob();

        body.transform.localPosition = baseOffset + npos;

        //body.Translate(new Vector3(0, yval, 0));
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

    public float getBob()
    {
        ///...
        ///really we want to tilt body too
        float fdist = (legs[0].transform.GetChild(1).position - legs[1].transform.GetChild(1).position).magnitude;

        float frac = 1f - (interFeetDistance / fdist);

        return Mathf.Clamp(frac, 0f, 1f) * bobHeight;
    }

}

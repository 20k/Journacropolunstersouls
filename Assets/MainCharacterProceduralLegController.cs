using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainCharacterProceduralLegController : MonoBehaviour
{
    public LegHub legHub;
    public float baseForwardOffset = -1;
    public float bobHeight = 0.1f;
    public float headBobHeight = 0.05f;
    public float headLagTimeSeconds = 0.1f;
    public Transform body;
    public Transform head;
    public SwordAttack swordAttack;

    List<ProceduralLeg> legs;

    Vector3 lastPosition;

    bool isIdle = false;

    Vector2 moveDir = new Vector2(0, 0);
    float runMult = 1f;
    bool isRunning = false;

    Vector3 baseBodyOffset;
    Vector3 baseHeadOffset;

    float interFeetDistance = 1;

    float timeSincePop = 0;

    Queue<float> bobFracHistory = new Queue<float>();

    // Use this for initialization
    void Start()
    {
        legs = legHub.GetLegs();

        baseBodyOffset = body.localPosition;
        baseHeadOffset = head.localPosition;

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

        Vector3 npos = new Vector3(0, 0, 0);

        npos.y = getBob();

        body.transform.localPosition = baseBodyOffset + npos;

        Vector3 headNew = new Vector3(0, 0, 0);

        if(timeSincePop > headLagTimeSeconds && bobFracHistory.Count > 0)
        {
            headNew.y = bobFracHistory.Dequeue() * headBobHeight;

            head.transform.localPosition = baseHeadOffset + headNew;

            ///well, approximation here ;_;
            timeSincePop -= Time.deltaTime;
        }

        timeSincePop += Time.deltaTime;

        bobFracHistory.Enqueue(getBobFrac());
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

    ///...
    ///really we want to tilt body too
    public float getBobFrac()
    {
        float fdist = (legs[0].transform.GetChild(1).position - legs[1].transform.GetChild(1).position).magnitude;

        float frac = 1f - (interFeetDistance / fdist);

        return Mathf.Clamp(frac, 0f, 1f);
    }

    public float getBob()
    {
        return getBobFrac() * bobHeight;
    }

}

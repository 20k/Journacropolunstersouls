using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;



public class WigglesMaster : MonoBehaviour {

    public Transform wiggles;
    public float spiderSpeed = 1.0f;
    public float turnTimeSeconds = 0.5f;
    public float legShiftTimeSeconds = 0.2f;
    public float legShiftOffsetFrac = 0.333333333f;
    public Transform target;

    /// <summary>
    /// 0.5 is center, 1 is dist/2, 0 is -dist/2
    /// </summary>
    public AnimationCurve bodySlamZ;
    public float bodySlamSeconds = 2;
    public float bodySlamDistance = 5;

    float attackFrac = 0;
    bool isAttack = false;
    Vector3 startPosition;

    bool isTurning = false;
    float turnTimeFrac = 0f;
    float desiredRelativeTurnAngle = 0f;
    float startGlobalTurnAngle = 0f;

    List<ProceduralLeg> legs = new List<ProceduralLeg>();
    
    float AngleToTarget(Transform t)
    {
        Vector3 mypos = wiggles.position;
        Vector3 theirpos = t.position;

        Vector3 spiderReference = new Vector3(0, 0, -1);

        Vector3 spiderFace = wiggles.rotation * spiderReference;

        Vector3 toThem = theirpos - mypos;

        Vector2 toThem2d = new Vector2(toThem.x, toThem.z);
        Vector2 spiderFace2d = new Vector2(spiderFace.x, spiderFace.z);

        /*float angle = Mathf.Atan2(-toThem.z, toThem.x) - Mathf.PI/2f;

        if(angle < 0)
        {
            angle %= Mathf.PI * 2;
            angle = Mathf.PI + (Mathf.PI - Mathf.Abs(angle));
        }

        angle = angle * Mathf.Rad2Deg;*/

        float a1 = Mathf.Atan2(toThem2d.y, toThem2d.x);
        float a2 = Mathf.Atan2(spiderFace2d.y, spiderFace2d.x);

        float angle = JMaths.AngleDiff(a2, a1);

        //float angle = Mathf.Acos(Mathf.Clamp(Vector2.Dot(toThem2d.normalized, spiderFace2d.normalized), -1f, 1f));

        angle = angle * Mathf.Rad2Deg;

        Debug.Log("angle " + angle);

        return angle;
    }

    void TickAttack(float ftime)
    {
        if (!isAttack)
            return;

        float eval = - ((bodySlamZ.Evaluate(attackFrac) - 0.5f) * 2);

        Vector3 npos = new Vector3(0, 0, bodySlamDistance) * eval;

        Vector3 globalDiff = wiggles.rotation * npos;

        wiggles.position = startPosition + globalDiff;

        attackFrac += ftime / bodySlamSeconds;

        if(attackFrac >= 1f)
            isAttack = false;
    }

    void InitiateAttack()
    {
        if (isAttack)
            return;

        attackFrac = 0;
        isAttack = true;
        startPosition = wiggles.position;
    }

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

        TickTurn(Time.deltaTime);

        bool togglePlant = Input.GetKeyDown(KeyCode.T);

        if (togglePlant)
        {
            for (int i = 0; i < legs.Count; i++)
            {
                legs[i].TogglePlant(i);
            }
        }

        if(Input.GetKeyDown(KeyCode.Z))
        {
            FirmlyPlantAllFeet();
        }

        if(Input.GetKeyDown(KeyCode.X))
        {
            FirmlyUnplantAllFeet();
        }

        if(Input.GetKeyDown(KeyCode.C))
        {
            InitiateAttack();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            ExecuteTurn(90);
        }

        ExecuteTurn(AngleToTarget(target));

        Vector2 input = getDebugInput() * spiderSpeed;

        Vector3 translate = new Vector3(input.x, 0, -input.y);

        wiggles.position += translate;

        //if (Mathf.Abs(input.y) < Mathf.Epsilon)
        //    return;

        int moveDir = 0;

        if (input.y > 0)
            moveDir = 1;
        else
            moveDir = -1;

        if (input.magnitude < Mathf.Epsilon)
            moveDir = 0;

	    for(int i=0; i<legs.Count; i++)
        {
            legs[i].Tick(Time.deltaTime, moveDir, getMult());
        }

        TickAttack(Time.deltaTime);
	}

    void FirmlyPlantAllFeet()
    {
        for(int i=0; i<legs.Count; i++)
        {
            legs[i].FirmlyPlant();
        }
    }

    void FirmlyUnplantAllFeet()
    {
        for (int i = 0; i < legs.Count; i++)
        {
            legs[i].FirmlyUnplant();
        }
    }

    void PlantAllFeet()
    {
        for(int i=0; i<legs.Count; i++)
        {
            legs[i].PlantFoot(i);
        }
    }

    void TickTurn(float ftime)
    {
        if (!isTurning)
            return;

        float globalY = wiggles.rotation.eulerAngles.y;

        ///ie in a circle, the sensible kind of diff we want for angles that takes into account mod 2PI
        //float angleDiff = JMaths.AngleDiff(desiredGlobalTurnAngle, startGlobalTurnAngle);

        float calcAngle = startGlobalTurnAngle * (1f - turnTimeFrac) + (startGlobalTurnAngle + desiredRelativeTurnAngle) * turnTimeFrac;

        ///testing
        Vector3 rot = wiggles.rotation.eulerAngles;
        rot.y = calcAngle;

        Quaternion nquat;
        nquat = Quaternion.Euler(rot.x, rot.y, rot.z);

        wiggles.localRotation = nquat;

        turnTimeFrac += ftime / turnTimeSeconds;

        if (turnTimeFrac >= 1f)
            isTurning = false;
    }

    /// <summary>
    /// dis gun be gud
    /// </summary>
    void ExecuteTurn(float desiredAngle)
    {
        if (isTurning)
            return;

        isTurning = true;
        desiredRelativeTurnAngle = desiredAngle;
        turnTimeFrac = 0;
        startGlobalTurnAngle = wiggles.rotation.eulerAngles.y;
    }

    public void Register(ProceduralLeg leg)
    {
        legs.Add(leg);
    }
}

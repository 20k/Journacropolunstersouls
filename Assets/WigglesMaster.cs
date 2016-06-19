using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;


public class WigglesMaster : MonoBehaviour {

    public Transform wiggles;
    public float spiderSpeed = 1.0f;
    public float turnTimeSeconds = 0.5f;
    public float legShiftTimeSeconds = 0.2f;

    bool isTurning = false;
    float turnTimeFrac = 0f;
    float desiredGlobalTurnAngle = 0f;
    float startGlobalTurnAngle = 0f;

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

        TickTurn(Time.deltaTime);

        bool togglePlant = Input.GetKeyDown(KeyCode.T);

        if (togglePlant)
        {
            for (int i = 0; i < legs.Count; i++)
            {
                legs[i].TogglePlant(i);
            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            ExecuteTurn(90);
        }

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
            legs[i].Tick(Time.deltaTime * moveDir * getMult());
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
        float angleDiff = JMaths.AngleDiff(desiredGlobalTurnAngle, startGlobalTurnAngle);

        float calcAngle = startGlobalTurnAngle * (1f - turnTimeFrac) + desiredGlobalTurnAngle * turnTimeFrac;

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
        desiredGlobalTurnAngle = desiredAngle;
        turnTimeFrac = 0;
        startGlobalTurnAngle = wiggles.rotation.eulerAngles.y;
    }

    public void Register(ProceduralLeg leg)
    {
        legs.Add(leg);
    }
}

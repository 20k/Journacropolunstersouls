﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.CrossPlatformInput;
using System;

public enum waitSlotType
{
    attackFinished,
    turnFinished,
    extraTurn,
    COUNT
}

/// <summary>
/// have a doonce function that accepts delegates?
/// </summary>
public class WaitSlots
{
    public bool[] slots = new bool[(int)waitSlotType.COUNT];
    public bool[] everRequestedSlots = new bool[(int)waitSlotType.COUNT];
    bool once = false;

    public bool OnceOnly()
    {
        bool val = !once;

        once = true;

        return val;
    }

    public WaitSlots()
    {
        Reset();
    }

    public void ActivateWaitSlot(waitSlotType wait)
    {
        slots[(int)wait] = true;
        everRequestedSlots[(int)wait] = true;
    }

    public void TerminateWaitSlot(waitSlotType wait)
    {
        slots[(int)wait] = false;
    }

    public bool IsWaiting()
    {
        for(int i=0; i<(int)waitSlotType.COUNT; i++)
        {
            if (slots[i])
                return true;
        }

        return false;
    }

    public bool IsWaitingOn(waitSlotType wait)
    {
        return slots[(int)wait];
    }

    public bool CanGoAhead(waitSlotType wait)
    {
        if(IsWaiting())
            return false;

        if(EverRequested(wait))
            return false;

        return true;
    }

    public bool EverRequested(waitSlotType wait)
    {
        return everRequestedSlots[(int)wait];
    }

    public bool FullyTerminated()
    {
        for(int i=0; i<(int)waitSlotType.COUNT; i++)
        {
            ///if a slot been accessed
            if(everRequestedSlots[i])
            {
                ///if we're waiting on the slot, we're not fully terminated
                if (slots[i])
                    return false;
            }
        }

        return true;
    }

    public void Reset()
    {
        for (int i = 0; i < (int)waitSlotType.COUNT; i++)
        {
            slots[i] = false;
            everRequestedSlots[i] = false;
        }

        once = false;
    }
}

public class WigglesMaster : MonoBehaviour {

    public Transform wiggles;
    public float spiderSpeed = 1.0f;
    public float turnTimeSeconds = 0.5f;
    public float legShiftTimeSeconds = 0.2f;
    public float legShiftOffsetFrac = 0.333333333f;
    public Transform target;
    public Transform body;
    public bool aiEnabled = false;

    /// <summary>
    /// name, animation curve, time, distance
    /// </summary>
    [Serializable]
    public class MonsterMove
    {
        public String name;
        public AnimationCurve curve;
        public float timeSeconds = 2;

        /// <summary>
        /// 0.5 is center, 1 is dist/2, 0 is -dist/2
        /// </summary>
        public float distance = 5;
    }

    public MonsterMove[] moves;

    MonsterMove GetMove(String name)
    {
        for(int i=0; i<moves.Length; i++)
        {
            if (name == moves[i].name)
                return moves[i];
        }

        Debug.Log("No move with name " + name);

        return moves[0];
    }

    private String currentMove = "BodySlam";
    bool moveIsExecuting = false;

    float attackFrac = 0;
    bool isAttack = false;
    Vector3 startPosition;

    bool isTurning = false;
    float turnTimeFrac = 0f;
    float desiredRelativeTurnAngle = 0f;
    float startGlobalTurnAngle = 0f;

    List<ProceduralLeg> legs = new List<ProceduralLeg>();

    public delegate void moveset();

    WaitSlots currentWaitSlots = new WaitSlots();

    moveset currentMoveFunc;

    /// <summary>
    /// ? 
    /// </summary>
    void None()
    {
       
    }

    void FaceAndBodyslam()
    {
        ///plant all feet, and once only
        if(currentWaitSlots.OnceOnly())
        {
            PlantAllFeet();
        }

        ///all this skip
        ///If the angle to target less than const, skip the turn
        float angleToTarget = Mathf.Abs(AngleToTarget(target));

        bool skipTurn = angleToTarget < Mathf.Rad2Deg * Mathf.PI / 8f;

        if(skipTurn && !currentWaitSlots.EverRequested(waitSlotType.turnFinished))
        {
            currentWaitSlots.ActivateWaitSlot(waitSlotType.turnFinished);
            currentWaitSlots.TerminateWaitSlot(waitSlotType.turnFinished);
        }

        if (currentWaitSlots.CanGoAhead(waitSlotType.turnFinished))
        {
            currentWaitSlots.ActivateWaitSlot(waitSlotType.turnFinished);
            ExecuteTurn(AngleToTarget(target));
        }

        if (currentWaitSlots.CanGoAhead(waitSlotType.attackFinished))
        {
            currentWaitSlots.ActivateWaitSlot(waitSlotType.attackFinished);
            InitiateAttack("BodySlam");
        }
    }

    void Scuttle()
    {
        if (currentWaitSlots.CanGoAhead(waitSlotType.turnFinished))
        {
            currentWaitSlots.ActivateWaitSlot(waitSlotType.turnFinished);
            ExecuteTurn(AngleToTarget(target));

            ///only want to plant feet once, and not firmly
            PlantAllFeet();
        }

        if (currentWaitSlots.CanGoAhead(waitSlotType.attackFinished))
        {
            currentWaitSlots.ActivateWaitSlot(waitSlotType.attackFinished);

            float distanceToTarget = DistanceToTarget(target);

            GetMove("Scuttle").distance = distanceToTarget - GetSpiderLength()/2f;

            ExecuteTurn(AngleToTarget(target));

            InitiateAttack("Scuttle");
        }

        ///If hardmode. This makes it like, way more competent
        if(currentWaitSlots.IsWaitingOn(waitSlotType.attackFinished) && attackFrac >= 0.5f
            && !currentWaitSlots.EverRequested(waitSlotType.extraTurn))
        {
            ExecuteTurn(AngleToTarget(target));

            ///we don't want to wait, but we want to trigger this
            currentWaitSlots.ActivateWaitSlot(waitSlotType.extraTurn);
            currentWaitSlots.TerminateWaitSlot(waitSlotType.extraTurn);

            //Debug.Log("Extra turn");
        }
    }

    /// <summary>
    /// How do I terminate this
    /// </summary>
    void FootStomp()
    {
        if(currentWaitSlots.OnceOnly())
        {
            PlantAllFeet();
        }

        ///I think this doesnt work on like, the first tick or so
        ///possibly there's a big time.deltaTime?
        //if (currentWaitSlots.CanGoAhead(waitSlotType.attackFinished))
        {
            currentWaitSlots.ActivateWaitSlot(waitSlotType.attackFinished);

            foreach(Transform child in transform)
            {
                if(child.tag == "Leg")
                {
                    GameObject obj = child.gameObject;

                    ProceduralLeg leg = obj.GetComponent<ProceduralLeg>();

                    Vector3 randomVec = leg.getRandomFootPosition();

                    leg.setFootPlantTipTransition(randomVec);
                }
            }
        }
    }

        //we need a history of attacks to complete the AI puzzle
    void TickAI()
    {
        ///need to use delegate
        //FaceAndBodyslam();

        currentMoveFunc();

        if (currentWaitSlots.FullyTerminated())
        {
            //Debug.Log("Finished moveset");
            currentWaitSlots.Reset();

            if (DistanceToTarget(target) > GetMove("BodySlam").distance + GetSpiderLength()/2)
                currentMoveFunc = Scuttle;
            else
                currentMoveFunc = FaceAndBodyslam;

            //currentMoveFunc = FootStomp;
        }
    }

    // Use this for initialization
    void Start()
    {
        currentMoveFunc = None;
    }

    float AngleToTarget(Transform t)
    {
        Vector3 mypos = wiggles.position;
        Vector3 theirpos = t.position;

        Vector3 spiderReference = new Vector3(0, 0, -1);

        Vector3 spiderFace = wiggles.rotation * spiderReference;

        Vector3 toThem = theirpos - mypos;

        Vector2 toThem2d = new Vector2(toThem.x, toThem.z);
        Vector2 spiderFace2d = new Vector2(spiderFace.x, spiderFace.z);

        float a1 = Mathf.Atan2(toThem2d.y, toThem2d.x);
        float a2 = Mathf.Atan2(spiderFace2d.y, spiderFace2d.x);

        float angle = JMaths.AngleDiff(a2, a1);

        angle = angle * Mathf.Rad2Deg;

        return angle;
    }

    float DistanceToTarget(Transform t)
    {
        Vector2 tpos = new Vector2(t.position.x, t.position.z);
        Vector2 spos = new Vector2(wiggles.position.x, wiggles.position.z);

        return (tpos - spos).magnitude;
    }

    void TickAttack(float ftime)
    {
        if (!isAttack)
            return;

        MonsterMove mov = GetMove(currentMove);

        float eval = - ((mov.curve.Evaluate(attackFrac) - 0.5f) * 2);

        Vector3 npos = new Vector3(0, 0, mov.distance) * eval;

        Vector3 globalDiff = wiggles.rotation * npos;

        wiggles.position = startPosition + globalDiff;

        attackFrac += ftime / mov.timeSeconds;

        if(attackFrac >= 1f)
        {
            isAttack = false;
            currentWaitSlots.TerminateWaitSlot(waitSlotType.attackFinished);
        }
    }

    void InitiateAttack(String name)
    {
        if (isAttack)
            return;

        currentMove = name;
        attackFrac = 0;
        isAttack = true;
        startPosition = wiggles.position;
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
            InitiateAttack("BodySlam");
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            ExecuteTurn(90);
        }

        if(Input.GetKeyDown(KeyCode.V))
        {
            FaceAndBodyslam();
        }

        if(aiEnabled)
            TickAI();

        //ExecuteTurn(AngleToTarget(target));

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

        //float globalY = wiggles.rotation.eulerAngles.y;

        ///ie in a circle, the sensible kind of diff we want for angles that takes into account mod 2PI
        //float angleDiff = JMaths.AngleDiff(desiredGlobalTurnAngle, startGlobalTurnAngle);

        float calcAngle = startGlobalTurnAngle * (1f - turnTimeFrac) + (startGlobalTurnAngle + desiredRelativeTurnAngle) * turnTimeFrac;

        ///testing
        Vector3 rot = wiggles.rotation.eulerAngles;
        rot.y = calcAngle;

        Quaternion nquat;
        nquat = Quaternion.Euler(rot.x, rot.y, rot.z);

        wiggles.rotation = nquat;

        turnTimeFrac += ftime / turnTimeSeconds;

        if (turnTimeFrac >= 1f)
        {
            currentWaitSlots.TerminateWaitSlot(waitSlotType.turnFinished);
            isTurning = false;
        }
    }

    /// <summary>
    /// dis gun be gud
    /// so the problem is, we execute a turn
    /// but the enemy has moved
    /// so then we have to execute another one
    /// and thats slow
    /// we could make turns move at constant angular speed, instead of constant time?
    /// Actually, you know what, I think this is fine
    /// We're never constantly tracking the player only executing moves towards him
    /// we do need it to be able to deal with the player position changing while its moving
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

    float GetSpiderLength()
    {
        return body.transform.localScale.z;
    }
}

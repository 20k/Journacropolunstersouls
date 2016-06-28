﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

/// <summary>
/// use animation curves to define transition from start to end
/// we'll eventually need separate hand pos, and sword direction
/// or maybe we just need a curve to define essentially what is the smoothing func
/// </summary>
[Serializable]
public class movement
{
    public Vector3 startVec;
    public Vector3 endVec;
    /// <summary>
    /// time to execute this part of the attack
    /// </summary>
    public float timeSeconds = 1;
    public AnimationCurve movementMultiplierCurve;
    public AnimationCurve turnAmountMultiplierCurve;
    /// <summary>
    /// defines the interpolation between start and end
    /// </summary>
    public AnimationCurve interpolateCurve;
    public float maxTurncapDeg = 360;
    public float damage = 10;

    private Quaternion startQuat;
    private Quaternion endQuat;


    [HideInInspector]
    public float timeElapsed = 0;
    private bool going = false;
    private bool isInit = false;

    public void startAtFinishOfPrev(movement prev)
    {
        startVec = prev.endVec;
    }

    public movement(movement a)
    {
        //init(a.startVec, a.endVec, a.timeSeconds, a.movementMultiplierCurve);

        startVec = a.startVec;
        endVec = a.endVec;
        timeSeconds = a.timeSeconds;
        movementMultiplierCurve = a.movementMultiplierCurve;
        turnAmountMultiplierCurve = a.turnAmountMultiplierCurve;
        interpolateCurve = a.interpolateCurve;
        maxTurncapDeg = a.maxTurncapDeg;
        damage = a.damage;

        startQuat.SetLookRotation(startVec);
        endQuat.SetLookRotation(endVec);
        isInit = true;
    }

    public void fire()
    {
        going = true;
    }

    public bool isGoing()
    {
        return going;
    }

    public void tick(float ftime)
    {
        if(!isInit)
        {
            startQuat.SetLookRotation(startVec);
            endQuat.SetLookRotation(endVec);

            isInit = true;
        }

        timeElapsed += ftime;
    }

    float frac_smooth(float t)
    {
        return - t * (t - 2);
    }

    public Quaternion getRotation()
    {
        float t = frac_smooth(timeElapsed / timeSeconds);

        t = t * t;

        //float t = timeElapsed / timeSeconds;

        if (t >= 1)
            t = 1;

        float newt = interpolateCurve.Evaluate(t);

        ///t*t IS intentional here, its part of the smoothing
        Quaternion ipc = Quaternion.SlerpUnclamped(startQuat, endQuat, newt);

        return ipc;
    }

    public float getMovementMult()
    {
        float val = movementMultiplierCurve.Evaluate(timeElapsed / timeSeconds);

        val = val < 0 ? 0 : val;

        return val;
    }

    public float getTurnCapDeg()
    {
        float val = turnAmountMultiplierCurve.Evaluate(timeElapsed / timeSeconds) * maxTurncapDeg;

        val = val < 0 ? 0 : val;

        return val;
    }

    public bool isFinished()
    {
        return timeElapsed >= timeSeconds;
    }

    public float getDamage()
    {
        return damage;
    }
}

[Serializable]
public class attack
{
    //List<movement> moveList = new List<movement>();
    public List<movement> moveList;
    public bool loops = false;

    [HideInInspector]
    public int numPopped = 0;
    private float movementMult = 1;
    private float turnMult = 360;

    public attack(attack a)
    {
        moveList = new List<movement>();

        for (int i = 0; i < a.moveList.Count; i++)
        {
            movement m = new movement(a.moveList[i]);
            moveList.Add(m);
        }
    }

    public Quaternion tick(float ftime)
    {
        Quaternion Q = Quaternion.identity;

        if(moveList.Count <= 0)
        {
            return Q;
        }

        if(!moveList[0].isGoing())
        {
            moveList[0].fire();
        }

        moveList[0].tick(ftime);

        Q = moveList[0].getRotation();

        turnMult = moveList[0].getTurnCapDeg();
        movementMult = moveList[0].getMovementMult();

        if(moveList[0].isFinished())
        {
            moveList.RemoveAt(0);
            numPopped++;
        }

        return Q;
    }

    public bool isFinished()
    {
        return moveList.Count == 0;
    }

    public void fixUpConnectivity()
    {
        for (int i = 1; i < moveList.Count; i++)
        {
            moveList[i].startAtFinishOfPrev(moveList[i - 1]);
        }
        
        if(loops && moveList.Count > 0)
        {
            moveList[moveList.Count - 1].endVec = moveList[0].startVec;
        }
    }

    public float getMovementMult()
    {
        return movementMult;
    }

    public float getTurnCapDeg()
    {
        return turnMult;
    }

    public float getDamage()
    {
        if (moveList.Count == 0)
            return 0;

        return moveList[0].getDamage(); 
    }
}

/// <summary>
/// Ok. Make sword attacks completely definable throught the interface
/// what we want is the ability to define a curve in 2 dimensions (x/z) that defines the way the character moves when he attacks
/// we also need to define move speed decreases, and whether or not we're allowed to move while attacking
/// </summary>
public class SwordAttack : MonoBehaviour {

    public Transform swordTransform;
    public float baseTurnCapDegSeconds = 360;

    public attack slashAttack;
    public attack MH1;

    List<attack> attackList = new List<attack>();

    Damager damage;

	// Use this for initialization
	void Start () {
        //slash = new movement(new Vector3(1, 1, 1), new Vector3(-1, 0, 1), 0.3f);
        //slashRecoverP1 = new movement(slash.endVec, new Vector3(-0.4f, -0.3f, 1), 0.2f);
        //slashRecoverP2 = new movement(slashRecoverP1.endVec, slash.startVec, 0.5f);

        slashAttack.fixUpConnectivity();
        MH1.fixUpConnectivity();

        damage = GetComponent<Damager>();
    }

    bool isDamaging()
    {
        if (attackList.Count == 0)
            return false;

        return attackList[0].numPopped == 0;
    }

    void activateColliderIfDamaging()
    {
        if (damage.HasHit())
            return;

        damage.SetDamage(GetDamage());
        damage.SetActive(isDamaging());
    }

	// Update is called once per frame
	void Update () {

        if (attackList.Count == 0 && Input.GetMouseButtonDown(0))
        {
            attack atk = new attack(MH1);

            attackList.Add(atk);

            damage.ResetHit();
        }

        for(int i=0; i<attackList.Count; i++)
        {
            Quaternion Q = attackList[i].tick(Time.deltaTime);

            swordTransform.localRotation = Q;

            if(attackList[i].isFinished())
            {
                attackList.RemoveAt(i);
                i--;
            }
        }

        activateColliderIfDamaging();
    }

    public float getMovementMult()
    {
        if (attackList.Count == 0)
            return 1;

        return attackList[0].getMovementMult();
    }

    public float getTurnCapDeg()
    {
        if (attackList.Count == 0)
            return baseTurnCapDegSeconds;

        return attackList[0].getTurnCapDeg();
    }

    public float GetDamage()
    {
        if (attackList.Count == 0)
            return 0;

        return attackList[0].getDamage();
    }
}

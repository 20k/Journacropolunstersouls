using UnityEngine;
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

    //public bool canBeCharged = false;
    public int chargeLevels = 0;
    //public float chargeTime = 0.0f;

    [HideInInspector]
    public int currentChargeLevel = 0;

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

        //canBeCharged = a.canBeCharged;
        chargeLevels = a.chargeLevels;
        //chargeTime = a.chargeTime;
        currentChargeLevel = a.currentChargeLevel;

        if(startVec.magnitude > Mathf.Epsilon)
            startQuat.SetLookRotation(startVec);

        if(endVec.magnitude > Mathf.Epsilon)
            endQuat.SetLookRotation(endVec);

        isInit = true;
    }

    public void updateQuats()
    {
        startQuat.SetLookRotation(startVec);
        endQuat.SetLookRotation(endVec);
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

        if (t >= 1)
            t = 1;

        float newt = interpolateCurve.Evaluate(t);

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

    public bool isChargeable()
    {
        return currentChargeLevel < chargeLevels;
    }

    public bool isCharging()
    {
        return currentChargeLevel > 0;
    }
}

[Serializable]
public class attack
{
    //List<movement> moveList = new List<movement>();
    public List<movement> moveList;
    public bool loops = false;
    public float extraDamagePerChargeLevel = 10;

    [HideInInspector]
    public int numPopped = 0;
    private float movementMult = 1;
    private float turnMult = 360;
    private int chargeLevel = 0;

    public attack(attack a)
    {
        moveList = new List<movement>();

        for (int i = 0; i < a.moveList.Count; i++)
        {
            movement m = new movement(a.moveList[i]);
            moveList.Add(m);
        }
    }

    public Quaternion tick(float ftime, bool requestChargeup, movement chargeupMove)
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

        if(requestChargeup)
        {
            insertChargingIfAppropriate(chargeupMove);
        }
        else
        {
            removeChargingIfAppropriate();
        }

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

        return moveList[0].getDamage() + extraDamagePerChargeLevel * chargeLevel; 
    }

    public bool isChargeable()
    {
        if (moveList.Count == 0)
            return false;

        return moveList[0].isChargeable();
    }

    /// <summary>
    /// Ie we're a charging stage
    /// </summary>
    /// <returns></returns>
    public bool isCharging()
    {
        if(moveList.Count == 0)
            return false;

        return moveList[0].isCharging();
    }

    public void insertChargingIfAppropriate(movement m)
    {
        if (!isChargeable())
            return;

        if (moveList[0].isFinished())
        {
            chargeLevel++;

            movement c = new movement(m);

            c.startVec = moveList[0].endVec;
            c.endVec = moveList[0].endVec;
            c.currentChargeLevel = chargeLevel;
            c.chargeLevels = moveList[0].chargeLevels;
            c.updateQuats();

            moveList.Insert(1, c);
        }
    }

    public void removeChargingIfAppropriate()
    {
        if (!isCharging())
            return;

        chargeLevel--;

        moveList.RemoveAt(0);
    }

    ///ie if we can cancel because we're charging, do it
    ///void cancelifcharging
}

/// <summary>
/// Ok. Make sword attacks completely definable throught the interface
/// what we want is the ability to define a curve in 2 dimensions (x/z) that defines the way the character moves when he attacks
/// we also need to define move speed decreases, and whether or not we're allowed to move while attacking
/// </summary>
public class SwordAttack : MonoBehaviour {

    public Transform swordTransform;
    public float baseTurnCapDegSeconds = 360;
    public MainCharacterProceduralLegController legController;

    public attack slashAttack;
    public attack MH1;
    /// <summary>
    /// Start/end are irrelevant here, only movement curve/damage etc are important
    /// </summary>
    public attack chargeup;

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

        if(chargeup.moveList.Count < 1)
        {
            Debug.Log("Chargeup is definitely going to crash, movelist < 1");
            ///should find out how to throw c# exceptions and throw one
        }
    }

    bool isDamaging()
    {
        if (attackList.Count == 0)
            return false;

        return attackList[0].getDamage() > Mathf.Epsilon;
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

        bool lclick = Input.GetMouseButtonDown(0);
        bool lheld = Input.GetMouseButton(0);

        if (attackList.Count == 0 && lclick)
        {
            attack atk = new attack(MH1);

            attackList.Add(atk);

            damage.ResetHit();
        }

        for(int i=0; i<attackList.Count; i++)
        {
            Quaternion Q = attackList[i].tick(Time.deltaTime, lheld, chargeup.moveList[0]);

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

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

/// <summary>
/// use animation curves to define transition from start to end
/// we'll eventually need separate hand pos, and sword direction
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

    private Quaternion startQuat;
    private Quaternion endQuat;


    [HideInInspector]
    public float timeElapsed = 0;
    private bool going = false;
    private bool isInit = false;

    private void init(Vector3 start, Vector3 end, float time)
    {
        startVec = start;
        endVec = end;
        timeSeconds = time;

        startQuat.SetLookRotation(startVec);
        endQuat.SetLookRotation(endVec);

        isInit = true;
    }

    public void startAtFinishOfPrev(movement prev)
    {
        startVec = prev.endVec;
    }

    public movement(Vector3 start, Vector3 end, float time)
    {
        init(start, end, time);
    }

    public movement(movement a)
    {
        init(a.startVec, a.endVec, a.timeSeconds);
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

        Quaternion ipc = Quaternion.Slerp(startQuat, endQuat, t*t);

        return ipc;
    }

    public bool isFinished()
    {
        return timeElapsed >= timeSeconds;
    }
}

[Serializable]
public class attack
{
    //List<movement> moveList = new List<movement>();
    public List<movement> moveList;

    [HideInInspector]
    public int numPopped = 0;

    /*public attack(List<movement> moves)
    {
        moveList = new List<movement>();

        for(int i=0; i<moves.Count; i++)
        {
            movement m = new movement(moves[i]);
            moveList.Add(m);
        }
    }*/

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
}

/// <summary>
/// Ok. Make sword attacks completely definable throught the interface
/// what we want is the ability to define a curve in 2 dimensions (x/z) that defines the way the character moves when he attacks
/// we also need to define move speed decreases, and whether or not we're allowed to move while attacking
/// </summary>
public class SwordAttack : MonoBehaviour {

    public Transform swordTransform;

    public attack slashAttack;

    //public movement[] slashMoves;

    List<attack> attackList = new List<attack>();

    Damager damage;

	// Use this for initialization
	void Start () {
	    //slash = new movement(new Vector3(1, 1, 1), new Vector3(-1, 0, 1), 0.3f);
        //slashRecoverP1 = new movement(slash.endVec, new Vector3(-0.4f, -0.3f, 1), 0.2f);
        //slashRecoverP2 = new movement(slashRecoverP1.endVec, slash.startVec, 0.5f);

        for(int i=1; i<slashAttack.moveList.Count; i++)
        {
            slashAttack.moveList[i].startAtFinishOfPrev(slashAttack.moveList[i-1]);
        }

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
        /*List<movement> d1 = new List<movement>();

        for(int i=0; i<slashMoves.Length; i++)
        {
            d1.Add(slashMoves[i]);
        }*/

        if (Input.GetMouseButtonDown(0))
        {
            attack atk = new attack(slashAttack);

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

    public float GetDamage()
    {
        return 10f;
    }
}

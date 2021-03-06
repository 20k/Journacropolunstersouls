﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// true indicates that the onhit should be terminated
/// </summary>
/// <returns></returns>
public delegate float onHitter(float dam, GameObject obj);

public class Damageable : MonoBehaviour {

    public float HP = 100;
    public string whatTagCanHitMe = "Damaging";
    public float invulnTimeSeconds = 1;

    bool isInvuln = false;
    float invulnFrac = 0;

    [HideInInspector]
    public float maxHP = 0;


    List<onHitter> registeredNotifiers = new List<onHitter>();

	// Use this for initialization
	void Start () {
        maxHP = HP;
	}
	
	// Update is called once per frame
	void Update () {
	    if(!alive())
        {
            gameObject.SetActive(false);
        }

        invulnFrac += Time.deltaTime / invulnTimeSeconds;

        if (invulnFrac > 1f)
            isInvuln = false;
    }

    /// remember to do other->inactivate
    void OnTriggerEnter(Collider other)
    {
        if (other.tag != whatTagCanHitMe)
            return;

        if (isInvuln)
            return;

        GameObject gobj = other.gameObject;

        Damager sa = gobj.GetComponent<Damager>();

        if (!sa)
            return;

        float damage = sa.GetDamage();

        if (damage <= 0)
            return;

        float residual = damage;

        foreach(var notifier in registeredNotifiers)
        {
            residual = notifier(residual, other.gameObject);
        }

        //if(!anySkip)
        HP -= residual;

        Debug.Log("I am hit " + HP);

        sa.Hit();

        isInvuln = true;
        invulnFrac = 0;
    }

    bool alive()
    {
        return HP > 0;
    }

    public void register(onHitter onhit)
    {
        registeredNotifiers.Add(onhit);
    }
}

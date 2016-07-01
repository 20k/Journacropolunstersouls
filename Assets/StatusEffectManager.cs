using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// so, we hold all the different kinds of status effect in here?
/// and then wiggles master defines how we react
/// </summary>
public class StatusEffectManager : MonoBehaviour {

    public float stunTimeSeconds = 8;
    public int numberOfCycles = 8;
    public float orbitRad = 2;
    public Transform stunOrbit;

    float stunTimeRemaining = 0;
    GameObject stunCube;

    GameObject[] myStunCubes = new GameObject[2];

	// Use this for initialization
	void Start () {
        stunCube = GameObject.Find("StunCube");

        GameObject o1 = Instantiate(stunCube, new Vector3(0,0,0), Quaternion.identity) as GameObject;
        GameObject o2 = Instantiate(stunCube, new Vector3(0,0,0), Quaternion.identity) as GameObject;

        myStunCubes[0] = o1;
        myStunCubes[1] = o2;
    }
	
	// Update is called once per frame
	void Update () {
        ConditionalActivate();

        if (!IsStunned())
            return;

        float timePerCycle = stunTimeSeconds / numberOfCycles;

        float angle = (stunTimeRemaining / timePerCycle) * 2f * Mathf.PI;

        float xp = Mathf.Cos(angle) * orbitRad;
        float yp = Mathf.Sin(angle) * orbitRad;

        Vector3 offset = new Vector3(xp, 0, yp);

        myStunCubes[0].transform.position = offset + stunOrbit.transform.position;
        myStunCubes[1].transform.position = -offset + stunOrbit.transform.position;

        stunTimeRemaining -= Time.deltaTime;

        if (stunTimeRemaining < 0)
            stunTimeRemaining = 0;
	}

    public void Stun()
    {
        stunTimeRemaining += stunTimeSeconds;
    }

    public bool IsStunned()
    {
        return stunTimeRemaining > 0;
    }

    void ConditionalActivate()
    {
        for(int i=0; i<myStunCubes.Length; i++)
        {
            myStunCubes[i].SetActive(IsStunned());
        }
    }
}

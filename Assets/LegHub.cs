using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LegHub : MonoBehaviour {

    public float legShiftOffsetFrac = 0.33333333333f;
    public float baseLegShiftTimeSeconds = 0.1f;

    [HideInInspector]
    public float legShiftTimeSeconds = 0.1f;

    List<ProceduralLeg> legs = new List<ProceduralLeg>();


    // Use this for initialization
    void Start () {
        legShiftTimeSeconds = baseLegShiftTimeSeconds;
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void Register(ProceduralLeg leg)
    {
        legs.Add(leg);
    }

    public List<ProceduralLeg> GetLegs()
    {
        return legs;
    }

    public void setLegTimeMult(float mult)
    {
        legShiftTimeSeconds = baseLegShiftTimeSeconds * mult;
    }
}

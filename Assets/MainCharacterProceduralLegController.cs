using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainCharacterProceduralLegController : MonoBehaviour {

    public LegHub legHub;

    List<ProceduralLeg> legs;

	// Use this for initialization
	void Start () {
        legs = legHub.GetLegs();
	}
	
	// Update is called once per frame
	void Update () {
        for(int i=0; i<legs.Count; i++)
        {
            if (!legs[i].IsPlanted())
                legs[i].PlantFoot(i);
        }

	    for(int i=0; i<legs.Count; i++)
        {
            legs[i].Tick(Time.deltaTime, 1, 1);
        }
	}
}

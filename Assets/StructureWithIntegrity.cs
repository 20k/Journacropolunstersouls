using UnityEngine;
using System.Collections;

public class StructureWithIntegrity : MonoBehaviour {

    public float fracDestroyedToCollapse = 0.5f;
    public float timeBetweenSegmentCollapse = 1f;

    float timeSinceLastCollapse = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        timeSinceLastCollapse += Time.deltaTime;
	}

    public bool isStructurallySound()
    {
        int activeCount = 0;
        int totalCount = 0;

        activeCount = GetComponentsInChildren<Transform>().Length;
        totalCount = GetComponentsInChildren<Transform>(true).Length;

        if (totalCount <= 1)
            return false;

        if (activeCount == 0)
            return false;

        float activeFrac = (float)(activeCount) / (totalCount);

        if ((1f - activeFrac) >= fracDestroyedToCollapse)
            return false;

        return true;
    }

    public bool requestCollapse()
    {
        if(timeSinceLastCollapse >= timeBetweenSegmentCollapse)
        {
            timeSinceLastCollapse = 0;

            return true;
        }

        return false;
    }
}

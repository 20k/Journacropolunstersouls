using UnityEngine;
using System.Collections;

public class ProceduralLeg : MonoBehaviour {
    public Transform baseBody;
    public float walkCycleSeconds = 1f;
    public float walkSweepAngleDegrees = 10;
    public float fracOffset = 0;
    public float walkHeight = 1;
    public float side = 1; //1 is left, -1 is right
    public WigglesMaster legHub;

    private float walkCycleFrac = 0f;
    private Vector3 offset;
    //private Vector3 plantPosition; //we're planted in the ground
    private Vector3 plantPositionTip;
    private bool isPlanted = false; //should we use dynamic, or leave the foot alone and only move when waked?

    private Transform upperLeg;
    private Transform lowerLeg;

    private GameObject sphere;
    private GameObject sphere1;

    Vector3 lowerBaseOffset;

    /// <summary>
    /// for debugging
    /// </summary>
    private int whoAmI = -1;

	// Use this for initialization
	void Start () {
        offset = transform.position - baseBody.position;
        legHub.Register(this);

        upperLeg = this.gameObject.transform.GetChild(0);
        lowerLeg = this.gameObject.transform.GetChild(1);

        lowerBaseOffset = lowerLeg.position - baseBody.position;

        sphere = GameObject.Find("DebugSphere");
        sphere1 = GameObject.Find("DebugSphere1");
    }

    void Update()
    {

    }

    public void TogglePlant(int who)
    {
        isPlanted = !isPlanted;

        if(isPlanted)
        {
            PlantFoot(who);
        }
    }

    public void PlantFoot(int who)
    {
        isPlanted = true;

        Vector3 lowerPos = lowerLeg.position;

        Vector3 lowerReference = new Vector3(0, 1, 0);

        Vector3 lowerDir = lowerLeg.rotation * lowerReference;

        float llength = lowerLeg.localScale.y;

        Vector3 footTip = lowerPos - lowerDir * llength/2;

        plantPositionTip = footTip;

        whoAmI = who;
    }

    Vector3 nearestSkew(Vector3 p1, Vector3 p1d, Vector3 p2, Vector3 p2d)
    {
        Vector3 n = Vector3.Cross(p1d, p2d);

        Vector3 n2 = Vector3.Cross(p2d, n);

        Vector3 c1 = p1 + (Vector3.Dot(p2 - p1, n2) / Vector3.Dot(p1d, n2)) * p1d;

        return c1;
    }

    float clamp(float v, float minv, float maxv)
    {
        if (v < minv)
            return minv;
        if (v > maxv)
            return maxv;

        return v;
    }

    float getJointAngle(Vector3 endPos, Vector3 startPos, float s2, float s3)
    {
        float s1 = (endPos - startPos).magnitude;

        s1 = clamp(s1, 0f, s2 + s3);

        float ic = (s2 * s2 + s3 * s3 - s1 * s1) / (2 * s2 * s3);

        ic = clamp(ic, -1, 1);

        float angle = Mathf.Acos(ic);

        return angle;
    }

    public void IKPlantFoot()
    {
        Vector3 upperPos = upperLeg.position;
        Vector3 lowerPos = lowerLeg.position;

        Vector3 upperReference = new Vector3(1, 0, 0);

        Quaternion uRot = upperLeg.rotation;

        Vector3 upperDir = uRot * upperReference;

        Vector3 lowerReference = new Vector3(0, 1, 0);

        Vector3 lowerDir = lowerLeg.rotation * lowerReference;

        //Vector3 skew1 = nearestSkew(upperPos, upperDir, lowerPos, lowerDir);
        //Vector3 skew2 = nearestSkew(lowerPos, lowerDir, upperPos, upperDir);

        //Vector3 avgTop = (skew1 + skew2) / 2f;

        //upper is scale.x, lower is scale.y

        float ulength = upperLeg.localScale.x;
        float llength = lowerLeg.localScale.y;

        float dir = side;

        Vector3 requestedFootTip = plantPositionTip;
        Vector3 rootTip = upperPos - dir * upperDir * ulength/2;


        float joinAngle = getJointAngle(requestedFootTip, rootTip, ulength, llength);
        
        ///rest positions
        float s1 = ulength + llength;
        float s2 = ulength;
        float s3 = llength;


        float area = 0.5f * s2 * s3 * Mathf.Sin(joinAngle);

        float height = 2 * area / s1;

        Vector3 perp = baseBody.rotation * (new Vector3(0, 0, -1) * dir);

        Vector3 d1 = rootTip - requestedFootTip;
        Vector3 d2 = new Vector3(1, 0, 0);

        Vector3 d3 = Vector3.Cross(d1, perp);

        d3 = d3.normalized;

        Vector3 half = (requestedFootTip + rootTip) / 2f;

        ///why negative?
        Vector3 topPos = half + Mathf.Min(height, -5f) * d3;

        Quaternion lookUpper = Quaternion.FromToRotation(dir * upperReference, topPos - rootTip);
        Quaternion lookLower = Quaternion.FromToRotation(lowerReference, topPos - requestedFootTip);

        upperLeg.rotation = lookUpper;
        lowerLeg.rotation = lookLower * baseBody.rotation;
        lowerLeg.position = requestedFootTip + lookLower * (lowerReference * llength / 2);
    }

    bool ShouldMovePlanted()
    {
        if (!isPlanted)
            return false;

        Vector3 baseReference = new Vector3(0, 0, -1) * -side;

        Vector3 baseForward = baseBody.rotation * baseReference;

        Vector3 baseRight = Vector3.Cross(Vector3.up, baseForward);

        float dir = side;

        float ulength = upperLeg.localScale.x;

        Vector3 upperPos = upperLeg.position;

        Vector3 upperReference = new Vector3(1, 0, 0);

        Quaternion uRot = upperLeg.rotation;

        Vector3 upperDir = uRot * upperReference;

        Vector3 rootTip = upperPos - dir * upperDir * ulength / 2;

        Vector3 relFoot = (plantPositionTip - rootTip).normalized;

        Vector2 lFoot = new Vector2(relFoot.x, relFoot.z);
        Vector2 lBase = new Vector2(baseRight.x, baseRight.z);

        float angle = Mathf.Acos(clamp(Vector2.Dot(lFoot.normalized, lBase.normalized), -1f, 1f));

        angle = angle * Mathf.Rad2Deg;

        //Debug.Log("asdfsadf " + angle);

        ///this whole function isn't quite correct somewhere :[
        if(Mathf.Abs(angle) > walkSweepAngleDegrees*2)
        {
            return true;
        }

        float restDistance = lowerBaseOffset.magnitude;

        float extraFrac = 1.1f;

        float curDistance = (plantPositionTip - rootTip).magnitude;

        if(curDistance >= restDistance * extraFrac)
        {
            return true;
        }

        return false;
    }

    Vector3 getCurrentRestPosition()
    {
        Quaternion baseQuat = baseBody.rotation;

        Vector3 disp = baseQuat * lowerBaseOffset;

        return disp + baseBody.position;
    }

    /// <summary>
    /// Ok, this is broken too
    /// </summary>
    void updateFootPlantIfNecessary()
    {
        if (!ShouldMovePlanted())
            return;

        Vector3 lowerPos = getCurrentRestPosition();

        Vector3 lowerReference = new Vector3(0, 1, 0);

        Vector3 lowerDir = lowerLeg.rotation * lowerReference;

        float llength = lowerLeg.localScale.y;

        Vector3 footTip = lowerPos - lowerDir * llength / 2;

        plantPositionTip = footTip;
    }

    public void Tick (float ftime) {
        if (isPlanted)
        {
            IKPlantFoot();
            updateFootPlantIfNecessary();
            return;
        }

        float walkCycle = Mathf.Sin((walkCycleFrac + fracOffset) * 2f * Mathf.PI)/2f;
        float heightCycle = Mathf.Sin((walkCycleFrac + fracOffset + 0.25f) * 2f * Mathf.PI) / 2f;

        if (side > 0 && heightCycle < 0)
            heightCycle = 0;
        if (side < 0 && heightCycle > 0)
            heightCycle = 0;

        float baseY;
        Vector3 baseR = baseBody.rotation.eulerAngles;
        baseY = baseR.y;

        Vector3 euler = transform.localRotation.eulerAngles;

        euler.z = heightCycle * walkSweepAngleDegrees;
        euler.y = walkCycle * walkSweepAngleDegrees + baseY;

        Quaternion nQuat = Quaternion.Euler(euler.x, euler.y, euler.z);

        transform.rotation = nQuat;
        transform.position = nQuat * offset + baseBody.position;

        if(!isPlanted)
            walkCycleFrac += ftime / walkCycleSeconds;

        walkCycleFrac %= 1f;
	}
}

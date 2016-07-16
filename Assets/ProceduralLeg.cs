using UnityEngine;
using System.Collections;

public class ProceduralLeg : MonoBehaviour {
    public Transform baseBody;
    public float walkCycleSeconds = 1f;
    public float walkSweepAngleDegrees = 10;
    public float fracOffset = 0;
    public float walkHeight = 1;
    public float side = 1; //1 is left, -1 is right
    public LegHub legHub;
    public ProceduralLeg offsetLeg;

    public Vector3 upperReference = new Vector3(1, 0, 0);
    public Vector3 lowerReference = new Vector3(0, 1, 0);

    public float forwardOffset = 0;

    public bool useAngleConstraint = true;
    public bool idleAt0Offset = false;

    public float heightMin = 5f;
    public float moveDistanceMult = 0.4f;

    private float upperLength;
    private float lowerLength;

    private float walkCycleFrac = 0f;
    private Vector3 offset;
    //private Vector3 plantPosition; //we're planted in the ground
    private Vector3 oldPlantPositionTip;
    private Vector3 plantPositionTip;
    private Vector3 nextPlantPositionTip;
    private bool isTipTransitioning = false;
    private float transitionFrac = 0;
    private bool isPlanted = false; //should we use dynamic, or leave the foot alone and only move when waked?
    private bool isFirmlyPlanted = false; //cannot be moved by request, can only be toggled

    private bool restTracking = false;
    private bool isIdle = true;


    private Transform upperLeg;
    private Transform lowerLeg;

    private GameObject sphere;
    private GameObject sphere1;

    private Vector3 bodyReference = new Vector3(0, 0, -1);

    Quaternion lowerLegBaseRotation;

    Vector3 lowerBaseOffset;
    Vector3 currentIdealFootRestPosition;

    float legShiftTimeSeconds;

    [HideInInspector]
    public float maxRestDistance;

    /// <summary>
    /// for debugging
    /// </summary>
    private int whoAmI = -1;

    /// <summary>
    /// This is true if the leg in front of us has just moved
    /// preventing us from moving
    /// </summary>
    /// <returns></returns>
    bool constrainedByOtherLeg()
    {
        ///we're fine, their leg isn't doing much of anything
        if (!offsetLeg.isTipTransitioning)
            return false;

        ///oh bum, their leg is transitioning AND their within the danger zone
        if (offsetLeg.transitionFrac < legHub.legShiftOffsetFrac)
            return true;

        ///leg is transitioning, but we're clear to transition ourselves because they're out the 
        ///time danger zone
        return false;
    }

	// Use this for initialization
	void Start () {
        offset = transform.position - baseBody.position;
        legHub.Register(this);

        upperLeg = this.gameObject.transform.GetChild(0);
        lowerLeg = this.gameObject.transform.GetChild(1);

        lowerBaseOffset = lowerLeg.position - baseBody.position;
        lowerLegBaseRotation = lowerLeg.rotation;
        currentIdealFootRestPosition = lowerLeg.position;

        legShiftTimeSeconds = legHub.legShiftTimeSeconds;

        sphere = GameObject.Find("DebugSphere");
        sphere1 = GameObject.Find("DebugSphere1");

        maxRestDistance = lowerBaseOffset.magnitude * moveDistanceMult;

        lowerLength = Mathf.Max(lowerLeg.localScale.x, Mathf.Max(lowerLeg.localScale.y, lowerLeg.localScale.z));
        upperLength = Mathf.Max(upperLeg.localScale.x, Mathf.Max(upperLeg.localScale.y, upperLeg.localScale.z));

        //lowerLength = lowerLeg.localScale.y;
        //upperLength = upperLeg.localScale.x;
    }

    public void SetIdling(bool idle)
    {
        isIdle = idle;
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

    Vector3 getCurrentFootTip()
    {
        Vector3 lowerPos = lowerLeg.position;

        //Vector3 lowerReference = new Vector3(0, 1, 0);

        Vector3 lowerDir = lowerLeg.rotation * lowerReference;

        float llength = lowerLength;

        Vector3 footTip = lowerPos - lowerDir * llength / 2;

        return footTip;
    }

    public bool IsPlanted()
    {
        return isPlanted;
    }

    public void PlantFoot(int who)
    {
        isPlanted = true;

        Vector3 footTip = getCurrentFootTip();

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

        Quaternion uRot = upperLeg.rotation;

        Vector3 upperDir = uRot * upperReference;

        Vector3 lowerDir = lowerLeg.rotation * lowerReference;

        //upper length is scale.x, lower is scale.y for spider

        float ulength = upperLength;
        float llength = lowerLength;

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

        Vector3 d3 = Vector3.Cross(d1, perp);

        d3 = d3.normalized;

        Vector3 half = (requestedFootTip + rootTip) / 2f;

        ///why negative?
        Vector3 topPos = half + Mathf.Min(height, -heightMin) * d3;

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

        float ulength = upperLength;

        Vector3 upperPos = upperLeg.position;

        //Vector3 upperReference = new Vector3(1, 0, 0);

        Quaternion uRot = upperLeg.rotation;

        Vector3 upperDir = uRot * upperReference;

        Vector3 rootTip = upperPos - dir * upperDir * ulength / 2;

        Vector3 relFoot = (plantPositionTip - rootTip).normalized;

        Vector2 lFoot = new Vector2(relFoot.x, relFoot.z);
        Vector2 lBase = new Vector2(baseRight.x, baseRight.z);

        float angle = Mathf.Acos(clamp(Vector2.Dot(lFoot.normalized, lBase.normalized), -1f, 1f));

        angle = angle * Mathf.Rad2Deg;

        if(useAngleConstraint && Mathf.Abs(angle) > walkSweepAngleDegrees*2)
        {
            return true;
        }

        //float extraFrac = 1.1f;

        /*if(whoAmI == 3)
        {
            sphere.transform.position = plantPositionTip;
            sphere1.transform.position = currentIdealFootRestPosition;
        }*/

        float curDistance = (plantPositionTip - currentIdealFootRestPosition).magnitude;

        ///take distance from foot ideal rest position
        if(curDistance >= maxRestDistance)
        {
            return true;
        }

        return false;
    }

    public Vector3 getRandomFootPosition()
    {
        Vector3 rest = getRestFootPlant();

        float len = maxRestDistance;

        float angle = Random.RandomRange(-180, 180);
        float randomLen = Random.RandomRange(len / 4, len);

        float y = Mathf.Sin(angle * Mathf.Deg2Rad) * randomLen;
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * randomLen;

        Vector3 nvec = new Vector3(x, 0, y);

        return nvec + rest;
    }


    /// <summary>
    /// not foot
    /// </summary>
    /// <returns></returns>
    Vector3 getCurrentRestPosition()
    {
        Quaternion baseQuat = baseBody.rotation;

        Vector3 disp = baseQuat * lowerBaseOffset;

        return disp + baseBody.position;
    }

    Vector3 getRestFootPlant()
    {
        Vector3 lowerPos = getCurrentRestPosition();

        Vector3 lowerDir = baseBody.rotation * lowerLegBaseRotation * lowerReference;

        float llength = lowerLength;

        Vector3 footTip = lowerPos - lowerDir * llength / 2;

        Vector3 frontExtra = baseBody.rotation * bodyReference * forwardOffset;

        if(isIdle && idleAt0Offset)
        {
            frontExtra = new Vector3(0, 0, 0);
        }

        footTip = footTip + frontExtra;

        return footTip;
    }

    void requestFootTransition()
    {
        if (!isPlanted)
            return;

        if (!ShouldMovePlanted())
            return;

        if (isTipTransitioning)
            return;

        if (constrainedByOtherLeg())
            return;

        Vector3 newFoot = getRestFootPlant();

        oldPlantPositionTip = plantPositionTip;
        nextPlantPositionTip = newFoot;
        transitionFrac = 0;
        isTipTransitioning = true;
    }

    Vector3 interpolate(Vector3 cur, Vector3 start, Vector3 end, float fstart, float fend, float t)
    {
        float it = (t - fstart) / (fend - fstart);

        if (it < 0 || it >= 1)
            return cur;

        return start * (1f - it) + end * it;
    }

    public void setFootPlantTipTransition(Vector3 pos)
    {
        if (!isPlanted)
            return;

        if (isTipTransitioning)
            return;

        if (constrainedByOtherLeg())
            return;

        oldPlantPositionTip = plantPositionTip;
        nextPlantPositionTip = pos;
        transitionFrac = 0;
        isTipTransitioning = true;
    }

    public void enableRestTracking()
    {
        restTracking = true;
    }

    void tickFootTransition(float ftime)
    {
        if (!isTipTransitioning)
            return;

        Vector3 start = oldPlantPositionTip;
        Vector3 finish = nextPlantPositionTip;

        Vector3 intermediate = ((start + finish) / 2) + new Vector3(0, walkHeight, 0);

        float footTransitionTime = legShiftTimeSeconds;

        Vector3 ip = new Vector3(0,0,0);

        ip = interpolate(ip, start, intermediate, 0, 0.5f, transitionFrac);
        ip = interpolate(ip, intermediate, finish, 0.5f, 1f, transitionFrac);

        //Debug.Log(transitionFrac);

        plantPositionTip = ip;

        transitionFrac += ftime / footTransitionTime;

        if (transitionFrac >= 1f)
            isTipTransitioning = false;
    }

    void updateFootPlantIfNecessary()
    {
        Vector3 newFoot = getRestFootPlant();

        currentIdealFootRestPosition = newFoot;

        if (!ShouldMovePlanted())
            return;

        plantPositionTip = newFoot;
    }

    public void FirmlyPlant()
    {
        if (isFirmlyPlanted)
            return;

        isFirmlyPlanted = true;
        isPlanted = true;

        plantPositionTip = getCurrentFootTip();
    }

    public void FirmlyUnplant()
    {
        if (!isFirmlyPlanted)
            return;

        isFirmlyPlanted = false;
        isPlanted = false;
    }

    /// <summary>
    /// last two parameters only useful for ye olde style sinwalk
    /// </summary>
    /// <param name="ftime"></param>
    /// <param name="direction"></param>
    /// <param name="mult"></param>
    public void Tick (float ftime, float direction, float mult) {

        currentIdealFootRestPosition = getRestFootPlant();
        legShiftTimeSeconds = legHub.legShiftTimeSeconds;

        if (isPlanted)
        {
            if (restTracking)
            {
                nextPlantPositionTip = getRestFootPlant();
            }


            IKPlantFoot();
            //updateFootPlantIfNecessary();

            if (!isFirmlyPlanted && ShouldMovePlanted())
                requestFootTransition();

            tickFootTransition(ftime);

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
            walkCycleFrac += mult * direction * ftime / walkCycleSeconds;

        walkCycleFrac %= 1f;
	}
}

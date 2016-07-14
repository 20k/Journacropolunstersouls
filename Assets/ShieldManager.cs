using UnityEngine;
using System.Collections;

public class ShieldManager : MonoBehaviour {

    public float idleRestAngle = 45;
    public float defenseRestAngle = 0f;
    public float transitionTimeSeconds = 0.2f;
    public Damageable toProtect;

    public GameObject onBlockParticles;
    public GameObject spawnLoc;

    float transitionFrac = 0;

    bool active = false;

    public bool onHit()
    {
        if (!active)
            return false;

        GameObject obj = (GameObject)Object.Instantiate(onBlockParticles, spawnLoc.transform.position, spawnLoc.transform.rotation);

        obj.SetActive(false);
        obj.SetActive(true);

        /*ParticleSystem ps = obj.GetComponent<ParticleSystem>();

        ps.Stop();
        ps.Simulate(0, true, true);*/

        return true;
    }

	// Use this for initialization
	void Start () {
        toProtect.register(onHit);
	}
	
	// Update is called once per frame
	void Update () {
        bool rheld = Input.GetMouseButton(1);

        active = rheld;

        Vector3 rot = transform.localEulerAngles;

        if(rheld)
        {
            transitionFrac += Time.deltaTime / transitionTimeSeconds;

            transitionFrac = Mathf.Min(transitionFrac, 1f);
        }
        else
        {
            transitionFrac -= Time.deltaTime / transitionTimeSeconds;

            transitionFrac = Mathf.Max(transitionFrac, 0f);
        }

        rot.y = defenseRestAngle * transitionFrac + -idleRestAngle * (1f - transitionFrac);

        transform.localRotation = Quaternion.Euler(rot);
    }
}

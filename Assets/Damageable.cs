using UnityEngine;
using System.Collections;

public class Damageable : MonoBehaviour {

    public float HP = 100;
    public string whatTagCanHitMe = "Damaging";
    public float invulnTimeSeconds = 1;

    bool isInvuln = false;
    float invulnFrac = 0;

	// Use this for initialization
	void Start () {

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

        HP -= damage;

        Debug.Log("I am hit " + HP);

        sa.Hit();

        isInvuln = true;
        invulnFrac = 0;
    }

    bool alive()
    {
        return HP > 0;
    }
}

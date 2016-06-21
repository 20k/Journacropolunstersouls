using UnityEngine;
using System.Collections;

public class Damageable : MonoBehaviour {

    public float HP = 100;
    public string whatTagCanHitMe = "Damaging";

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(!alive())
        {
            gameObject.SetActive(false);
        }
	}

    /// remember to do other->inactivate
    void OnTriggerEnter(Collider other)
    {
        if (other.tag != whatTagCanHitMe)
            return;

        GameObject gobj = other.gameObject;

        SwordAttack sa = gobj.GetComponent<SwordAttack>();

        if (!sa)
            return;

        float damage = sa.GetDamage();

        HP -= damage * 10f;

        Debug.Log("I am hit " + HP);

        sa.Inactivate();
    }

    bool alive()
    {
        return HP > 0;
    }
}

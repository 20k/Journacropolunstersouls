using UnityEngine;
using System.Collections;

public class Damager : MonoBehaviour {
    public string tagCanHit = "Damaging";
    public float amount = 10;


    Collider col;
    bool hitObject = false;

	// Use this for initialization
	void Start () {
        tag = tagCanHit;
        col = GetComponent<Collider>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// I think we can replace these with c# get/set
    /// </summary>
    /// <returns></returns>
    public float GetDamage()
    {
        return amount;
    }

    public void SetDamage(float val)
    {
        amount = val;
    }

    public void Activate()
    {
        col.enabled = true;
        hitObject = false;
    }

    public void Inactivate()
    {
        col.enabled = false;
    }

    public void Hit()
    {
        hitObject = true;

        Inactivate();
    }

    public void ResetHit()
    {
        hitObject = false;
    }

    public bool HasHit()
    {
        return hitObject;
    }

    public void SetActive(bool val)
    {
        col.enabled = val;
    }

}

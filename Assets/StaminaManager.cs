using UnityEngine;
using System.Collections;

public class StaminaManager : MonoBehaviour {
    public float stamina = 50f;
    public float mandatoryStaminaRechargeTime = 1;
    public float staminaRegenPS = 25;

    public float sprintStaminaDrainPS = 5;
    public float dodgeStaminaDrainBulk = 15;

    [HideInInspector]
    public float maxStamina;

    public bool forcedExhaustionRegen = false;
    public bool forcedDodgeRegen = false;
    public bool shouldRegenStamina = true;

    public float dodgeFrac = 1f;

	// Use this for initialization
	void Start () {
        maxStamina = stamina;
	}
	
	// Update is called once per frame
	void Update () {

        if(shouldRegenStamina && dodgeFrac >= 1f)
            stamina += Time.deltaTime * staminaRegenPS;

        stamina = Mathf.Min(stamina, maxStamina);

        checkDepletion();

        shouldRegenStamina = true;
	}

    void checkDepletion()
    {
        if (stamina < 0)
        {
            forcedExhaustionRegen = true;
            stamina = 0;
        }

        if (forcedExhaustionRegen && stamina >= staminaRegenPS * mandatoryStaminaRechargeTime)
        {
            forcedExhaustionRegen = false;
        }
    }

    public void depleteAtRate(float ratePS)
    {
        stamina -= Time.deltaTime * ratePS;

        checkDepletion();

        shouldRegenStamina = false;
    }

    public void depleteBulk(float amount)
    {
        stamina -= amount;

        checkDepletion();

        ///I guess we should really terminate it for a certain amount of time
        shouldRegenStamina = false;
    }

    public bool doDodge()
    {
        if (!canDoStaminaAction())
            return false;

        depleteBulk(dodgeStaminaDrainBulk);

        return true;
    }

    public void tickDodge(float t)
    {
        dodgeFrac = t;
    }

    public bool doSprint()
    {
        if (!canDoStaminaAction())
            return false;

        depleteAtRate(sprintStaminaDrainPS);

        return true;
    }

    public bool canDoStaminaAction()
    {
        return !forcedExhaustionRegen;
    }
}

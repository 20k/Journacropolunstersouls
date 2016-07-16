using UnityEngine;
using System.Collections;

public class StaminaManager : MonoBehaviour {
    public float stamina = 50f;
    public float mandatoryStaminaRechargeTime = 1;
    public float staminaRegenPS = 25;

    public float sprintStaminaDrainPS = 5;
    public float dodgeStaminaDrainBulk = 15;
    public float healthDamageToStaminaDrainFrac = 0.5f;

    [HideInInspector]
    public float maxStamina;

    [HideInInspector]
    public bool forcedExhaustionRegen = false;
    public bool forcedDodgeRegen = false;
    public bool shouldRegenStamina = true;

    [HideInInspector]
    public float dodgeFrac = 1f;
    [HideInInspector]
    public bool isBlocking = false;

    bool isPaused = false;

	// Use this for initialization
	void Start () {
        maxStamina = stamina;
	}
	
	// Update is called once per frame
	void Update () {

        if(shouldRegenStamina && dodgeFrac >= 1f && !isBlocking && !isPaused)
            stamina += Time.deltaTime * staminaRegenPS;

        stamina = Mathf.Min(stamina, maxStamina);

        checkDepletion();

        shouldRegenStamina = true;

        isPaused = false;
	}

    void checkDepletion()
    {
        if (stamina < 0)
        {
            forcedExhaustionRegen = true;
            stamina = 0;
        }

        if (forcedExhaustionRegen && stamina >= Mathf.Min(staminaRegenPS * mandatoryStaminaRechargeTime, maxStamina))
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

    public float doDirectStaminaDamage(float damage, float staminaDamage)
    {
        stamina -= staminaDamage;

        if(stamina < 0f)
        {
            float predeplete = stamina;           

            checkDepletion();

            return (Mathf.Abs(predeplete) / staminaDamage) * damage;
        }

        checkDepletion();

        return 0f;
    }

    /// <summary>
    /// returns residual damage
    /// </summary>
    /// <param name="dam"></param>
    /// <returns></returns>
    public float doBlockAndGetDamageResidual(float dam)
    {
        //if (!canDoStaminaAction())
        //    return dam;

        float stam = dam * healthDamageToStaminaDrainFrac;

        stamina -= stam;

        float extra = Mathf.Abs(stamina) / healthDamageToStaminaDrainFrac;

        if (stamina >= 0)
            extra = 0;

        checkDepletion();

        return extra;
    }

    public void tickBlock(bool blocking)
    {
        isBlocking = blocking;
    }

    public bool canDoStaminaAction()
    {
        return !forcedExhaustionRegen;
    }

    public void pause()
    {
        isPaused = true;
    }
}

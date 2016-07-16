using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIUpdater : MonoBehaviour {
    public Damageable HPToDisplay;
    public StaminaManager staminaManager;
    public float barHoldTime = 1f;
    public float tempColourLossRatePS = 25f;
    public float activateDiff = 5f;

    GameObject staminaBack, staminaGray, stamina;
    GameObject healthBack, healthGray, health;

    float staminaHoldFrac = 1f;
    float healthHoldFrac = 1f;

    float lastHP = 0f;
    float lastStamina = 0f;

    float holdHP = 0f;
    float holdStamina = 0f;

    //GameObject staminaYellow;
    //GameObject healthYellow;

    GameObject[] tempColour = new GameObject[2];

    GameObject getChild(string name)
    {
        Transform[] ts = transform.GetComponentsInChildren<Transform>(true);

        foreach(var t in ts)
        {
            if (t.gameObject.name == name)
                return t.gameObject;
        }

        Debug.Log("Could not find game object, this is a fatal error");

        return null;
    }

    Vector4 GetColour(int i)
    {
        if (i == 0)
            return new Vector4(1, 0, 0, 1);
        if (i == 1)
            return new Vector4(255 / 255f, 253 / 255f, 0f, 1);

        return new Vector4(255, 0, 255, 1);
    }

    // Use this for initialization
    void Start () {
        staminaBack = getChild("StaminaBack");
        staminaGray = getChild("StaminaGray");
        stamina = getChild("Stamina");
        //staminaYellow = (GameObject)Instantiate(stamina, stamina.transform.position, stamina.transform.rotation);

        healthBack = getChild("HealthBack");
        healthGray = getChild("HealthGray");
        health = getChild("Health");

        for(int i=0; i<2; i++)
        {
            tempColour[i] = (GameObject)Instantiate(health, health.transform.position, health.transform.rotation);

            tempColour[i].SetActive(true);

            tempColour[i].transform.parent = transform;

            RectTransform r = tempColour[i].GetComponent<RectTransform>();

            r.localScale = stamina.GetComponent<RectTransform>().localScale;

            Image img = tempColour[i].GetComponent<Image>();

            img.color = GetColour(i) * 0.75f;

            if(i == 1)
            {
                tempColour[1].transform.position = stamina.transform.position;
            }

            tempColour[i].transform.SetSiblingIndex(4);
        }
    }

    void SetHP(float cur, float max, float border)
    {
        RectTransform r1 = healthBack.GetComponent<RectTransform>();
        RectTransform r2 = healthGray.GetComponent<RectTransform>();
        RectTransform r3 = health.GetComponent<RectTransform>();
        RectTransform r4 = tempColour[0].GetComponent<RectTransform>();

        float CurSize = cur;
        float MaxSize = max;
        float MaxWithBorder = max + border;

        r1.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxWithBorder);
        r2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxSize);
        r3.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurSize);
        //r4.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurSize);
    }

    void SetStamina(float cur, float max, float border)
    {
        RectTransform r1 = staminaBack.GetComponent<RectTransform>();
        RectTransform r2 = staminaGray.GetComponent<RectTransform>();
        RectTransform r3 = stamina.GetComponent<RectTransform>();
        RectTransform r4 = tempColour[1].GetComponent<RectTransform>();

        float CurSize = cur;
        float MaxSize = max;
        float MaxWithBorder = max + border;

        r1.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxWithBorder);
        r2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxSize);
        r3.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurSize);
        //r4.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurSize);
    }

    // Update is called once per frame
    void Update () {
        SetHP(HPToDisplay.HP, HPToDisplay.maxHP, 2);
        SetStamina(staminaManager.stamina,staminaManager.maxStamina, 2);

        float HPDiff = lastHP - HPToDisplay.HP;
        float staminaDiff = lastStamina - staminaManager.stamina;

        if(HPDiff > activateDiff)
        {
            healthHoldFrac = 0f;

            holdHP = lastHP;
        }

        if(staminaDiff > activateDiff)
        {
            staminaHoldFrac = 0f;

            holdStamina = lastStamina;
        }

        if(healthHoldFrac >= 1f && Mathf.Abs(holdHP - HPToDisplay.HP) > Mathf.Epsilon)
        {
            float cdiff = (HPToDisplay.HP - holdHP);

            if (cdiff > 0)
                holdHP = HPToDisplay.HP;

            if(cdiff < -tempColourLossRatePS * Time.deltaTime)
            {
                cdiff = -tempColourLossRatePS * Time.deltaTime;
            }

            holdHP += cdiff;

            RectTransform r4 = tempColour[0].GetComponent<RectTransform>();

            r4.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, holdHP);
        }

        if(staminaHoldFrac >= 1f && Mathf.Abs(holdStamina - staminaManager.stamina) > Mathf.Epsilon)
        {
            float cdiff = (staminaManager.stamina - holdStamina);

            if (cdiff > 0)
                holdStamina = staminaManager.stamina;

            if (cdiff < -tempColourLossRatePS * Time.deltaTime)
            {
                cdiff = -tempColourLossRatePS * Time.deltaTime;
            }

            holdStamina += cdiff;

            RectTransform r4 = tempColour[1].GetComponent<RectTransform>();

            r4.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, holdStamina);
        }

        healthHoldFrac += Time.deltaTime / barHoldTime;
        staminaHoldFrac += Time.deltaTime / barHoldTime;

        lastHP = HPToDisplay.HP;
        lastStamina = staminaManager.stamina;
    }
}

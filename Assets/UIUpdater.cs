using UnityEngine;
using System.Collections;

public class UIUpdater : MonoBehaviour {
    public Damageable HPToDisplay;
    public StaminaManager staminaManager;

    GameObject staminaBack, staminaGray, stamina;
    GameObject healthBack, healthGray, health;

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

    // Use this for initialization
    void Start () {
        staminaBack = getChild("StaminaBack");
        staminaGray = getChild("StaminaGray");
        stamina = getChild("Stamina");

        healthBack = getChild("HealthBack");
        healthGray = getChild("HealthGray");
        health = getChild("Health");
    }

    void SetHP(float cur, float max, float border)
    {
        RectTransform r1 = healthBack.GetComponent<RectTransform>();
        RectTransform r2 = healthGray.GetComponent<RectTransform>();
        RectTransform r3 = health.GetComponent<RectTransform>();

        float CurSize = cur;
        float MaxSize = max;
        float MaxWithBorder = max + border;

        r1.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxWithBorder);
        r2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxSize);
        r3.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurSize);
    }

    void SetStamina(float cur, float max, float border)
    {
        RectTransform r1 = staminaBack.GetComponent<RectTransform>();
        RectTransform r2 = staminaGray.GetComponent<RectTransform>();
        RectTransform r3 = stamina.GetComponent<RectTransform>();

        float CurSize = cur;
        float MaxSize = max;
        float MaxWithBorder = max + border;

        r1.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxWithBorder);
        r2.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, MaxSize);
        r3.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CurSize);
    }

    // Update is called once per frame
    void Update () {
        SetHP(HPToDisplay.HP, HPToDisplay.maxHP, 2);
        SetStamina(staminaManager.stamina,staminaManager.maxStamina, 2);

    }
}

using UnityEngine;
using System.Collections;

public class StatusEffect : MonoBehaviour
{
    public string activateTag;

    void Start()
    {

    }

    void Update()
    {

    }


    /// <summary>
    /// won't be a wiggles master that enters because its just a collider
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.tag != activateTag)
            return;

        Debug.Log("Something we want entered");

        StatusEffectManager manage = other.gameObject.GetComponentInChildren<StatusEffectManager>();

        if (manage == null)
            return;

        manage.Stun();

        gameObject.SetActive(false);

        /*WigglesMaster wg = other.gameObject.GetComponent<WigglesMaster>();

        if (wg == null)
            return;

        Debug.Log("wiggles enter");*/
    }
}



/// <summary>
/// rename me to icewall, magicsystem is a level up from here
/// EnemyDamaging is the tag that can be affected by IceWall
/// </summary>
public class IceWall : MonoBehaviour {

    public Transform position;
    public string activateTag = "EnemyDamaging"; //bossmonster type

    GameObject iceCube;
    int placed = 0;

    Vector3 firstPos = new Vector3(0, 0, 0);

    //BoxCollider col;

	// Use this for initialization
	void Start () {
        iceCube = GameObject.Find("IceCube");

        /*col = gameObject.AddComponent<BoxCollider>();

        col.size = new Vector3(0, 0, 0);
        col.isTrigger = true;
        col.enabled = false;*/
    }

    Quaternion GetQuat(Vector3 p1, Vector3 p2)
    {
        Vector3 diff = (p2 - p1);

        float yangle = -Mathf.Atan2(diff.z, diff.x);

        return Quaternion.Euler(0, yangle * Mathf.Rad2Deg, 0);
    }
	
    void Spawn(Vector3 pos, float yangle)
    {
        float mangle = yangle * Mathf.Rad2Deg;

        Quaternion quat;
        quat = Quaternion.Euler(0, mangle, 0);

        GameObject obj = Instantiate(iceCube, pos, quat) as GameObject;

        obj.SetActive(true);
    }

    void SpawnBetween(Vector3 p1, Vector3 p2, float hsep, float vsep, int height)
    {
        float dist = (p2 - p1).magnitude;

        float num = Mathf.Floor(dist / hsep);

        Vector3 cur = p1;
        Vector3 dir = (p2 - p1) / num;

        ///cubes are symmetric, so who cares?
        float yangle = -Mathf.Atan2(dir.z, dir.x);

        for(int i=0; i<(int)num; i++)
        {
            Vector3 hpos = cur;

            for(int h=0; h<height; h++)
            {
                Spawn(hpos, yangle);

                hpos.y += vsep;
            }

            cur = cur + dir;
        }
    }

	// Update is called once per frame
	void Update () {
        bool q = Input.GetKeyDown("q");

        if (q && placed == 0)
        {
            firstPos = position.position;

            placed = 1;

            return;
        }

        if (q && placed == 1)
        {
            ///initial separation
            float heightSep = 3;
            ///total height in cubes
            int heightNum = 5;

            float awidth = iceCube.transform.localScale.x * 1.5f;

            float height = iceCube.transform.localScale.y * 1.5f * heightNum;

            Vector3 localPos = new Vector3(-awidth / 2f, height / 2 - awidth / 2f, 0);

            GameObject gobj = new GameObject();

            StatusEffect effect = gobj.AddComponent<StatusEffect>();
            effect.activateTag = activateTag;

            gobj.SetActive(true);
            gobj.transform.position = (firstPos + position.position) / 2f + GetQuat(firstPos, position.position) * localPos;

            BoxCollider col = gobj.AddComponent<BoxCollider>();
            col.isTrigger = true;

            col.size = new Vector3((firstPos - position.position).magnitude, height, awidth);

            col.transform.rotation = GetQuat(firstPos, position.position);
            col.enabled = true;

            ///+ front distance offset
            SpawnBetween(firstPos, position.position, awidth, heightSep, heightNum);

            placed = 0;

            return;
        }
    }

}

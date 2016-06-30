using UnityEngine;
using System.Collections;

public class MagicSystem : MonoBehaviour {

    public Transform position;

    GameObject iceCube;
    int placed = 0;

    Vector3 firstPos = new Vector3(0, 0, 0);

	// Use this for initialization
	void Start () {
        iceCube = GameObject.Find("IceCube");
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
            ///+ front distance offset
            SpawnBetween(firstPos, position.position, iceCube.transform.localScale.x*1.5f, 3, 5);

            placed = 0;

            return;
        }

    }
}

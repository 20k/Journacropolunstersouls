using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Fragmenter : MonoBehaviour {

    MeshFilter filter;
    Mesh mesh;
    Collider col;

	// Use this for initialization
	void Start () {
        filter = GetComponent<MeshFilter>();
        mesh = filter.mesh;
        col = GetComponent<Collider>();
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.F2))
        {
            FragmentNonDelaunay(mesh);
            return;
        }

        StructureWithIntegrity structure = GetComponentInParent<StructureWithIntegrity>();

        if(structure != null)
        {
            if(!structure.isStructurallySound())
            {
                FragmentNonDelaunay(mesh);
            }
        }
	}

    void OnTriggerEnter(Collider col)
    {
        GameObject obj = col.gameObject;

        // BuildingDestroyer dest = obj.GetComponentInParent<BuildingDestroyer>();

        if (obj.tag != "BuildingDestroyer")
            return;

        FragmentNonDelaunay(mesh);
    }

    public class fragment
    {
        public bool boundary = false;
        public Vector3 pos;
    }

    float fragAngle(fragment f1)
    {
        float a1 = Mathf.Atan2(f1.pos.z, f1.pos.x);

        return a1;
    }

    bool fragSort(fragment f1, fragment f2)
    {
        float a1 = Mathf.Atan2(f1.pos.z, f1.pos.x);
        float a2 = Mathf.Atan2(f2.pos.z, f2.pos.x);

        return a1 < a2;
    }

    Vector3 getNearestVertex(Mesh m, Vector3 point)
    {
        point = transform.InverseTransformPoint(point);
        float minDistanceSqr = Mathf.Infinity;
        Vector3 nearestVertex = Vector3.zero;
        // scan all vertices to find nearest
        foreach (Vector3 vertex in mesh.vertices)
        {
            Vector3 diff = point - vertex;
            float distSqr = diff.sqrMagnitude;
            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearestVertex = vertex;
            }
        }
        // convert nearest vertex back to world space
        return transform.TransformPoint(nearestVertex);
    }

    Vector3 getRandomWithinCollider(Collider col, float height)
    {
        //bool within = false;

        Bounds bound = col.bounds;

        Debug.Log("mm " + bound.min + " " + bound.max);

        Vector3 min = bound.min;
        Vector3 max = bound.max;

        //min.Scale(transform.localScale);
        //max.Scale(transform.localScale);

        //min += transform.position;
        //max += transform.position;

        Debug.Log(min + " " + max);

        int c = 0;

        while(c < 100)
        {
            Vector3 pos = new Vector3(Random.Range(min.x, max.x), height, Random.Range(min.z, max.z));

            if (Physics.CheckSphere(pos, 0.001f))
            {
                return pos;
            }

            c++;
        }

        return new Vector3(0, 0, 0);
    }

    public struct triList
    {
        public int[] indices;
        //public Vector3[] vertices;
    }

    bool shareEdge(int[] t1, int[] t2)
    {
        int numShared = 0;

        for(int i=0; i<3; i++)
        {
            for(int j=0; j<3; j++)
            {
                if (t1[i] == t2[j])
                    numShared++;
            }
        }

        return numShared == 2;
    }

    int[] getSharedEdge(int[] t1, int[] t2)
    {
        int[] shared = new int[2];

        int numShared = 0;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (t1[i] == t2[j])
                {
                    shared[numShared] = t1[i];

                    numShared++;
                }
            }
        }

        return shared;
    }

    int[] getUnsharedPoints(int[] t1, int[] t2)
    {
        int[] shared = getSharedEdge(t1, t2);

        int[] notShared = new int[2];

        for(int i=0; i<3; i++)
        {
            bool t1any = false;
            bool t2any = false;

            for (int j=0; j<3; j++)
            {
                if (t1[i] == shared[j])
                    t1any = true;
                if (t2[i] == shared[j])
                    t2any = true;
            }

            if(!t1any)
            {
                notShared[0] = t1[i];
            }

            if(!t2any)
            {
                notShared[1] = t2[i];
            }
        }

        return notShared;
    }

    public bool isDelaunay(int[] t1, int[] t2, List<Vector3> verts)
    {
        int[] sharedEdge = getSharedEdge(t1, t2);
        int[] notShared = getUnsharedPoints(t1, t2);

        int arbitrary = notShared[0];

        int Ai = notShared[0];
        int Bi = sharedEdge[0];
        int Ci = notShared[1];
        int Di = sharedEdge[1];

        float Ax = verts[Ai].x;
        float Ay = verts[Ai].x;

        float Bx = verts[Bi].x;
        float By = verts[Bi].x;

        float Cx = verts[Ci].x;
        float Cy = verts[Ci].x;

        float Dx = verts[Di].x;
        float Dy = verts[Di].x;

        ///In hindsight, I could have avoided all this duplicated code
        ///sigh, no 3x3 matrices
        Matrix4x4 mat = new Matrix4x4();
        mat.SetRow(0, new Vector4(Ax - Dx, Ay - Dy, (Ax * Ax - Dx * Dx) + (Ay * Ay - Dy * Dy), 0));
        mat.SetRow(1, new Vector4(Bx - Dx, By - Dy, (Bx * Bx - Dx * Dx) + (By * By - Dy * Dy), 0));
        mat.SetRow(2, new Vector4(Cx - Dx, Cy - Dy, (Cx * Cx - Dx * Dx) + (Cy * Cy - Dy * Dy), 0));
        mat.SetRow(3, new Vector4(0, 0, 0, 1));

        ///If this works, Ill... uuh... be very surprised
        float det = mat.determinant;

        return det > 0;
    }

    void makeDelaunay(int[] t1, int[] t2, List<Vector3> verts)
    {
        ///t1, t2 share edge

        int[] sharedEdge = getSharedEdge(t1, t2);
        int[] notShared = getUnsharedPoints(t1, t2);

        bool delaunay = isDelaunay(t1, t2, verts);

        if(!delaunay)
        {
            int A, B, C, D;

            A = notShared[0];
            B = sharedEdge[0];
            C = notShared[1];
            D = sharedEdge[1];

            t1[0] = A;
            t1[1] = C;
            t1[2] = D;

            t2[0] = A;
            t2[1] = B;
            t2[2] = C;
        }
    }

    /*float calc_area(float3 x, float3 y)
    {
        
    }
    */

    float calcThirdAreas(float x1, float x2, float x3, float y1, float y2, float y3, float x, float y)
    {
        return (Mathf.Abs(x2*y-x*y2+x3*y2-x2*y3+x*y3-x3*y) + Mathf.Abs(x*y1-x1*y+x3*y-x*y3+x1*y3-x3*y1) + Mathf.Abs(x2*y1-x1*y2+x*y2-x2*y+x1*y-x*y1)) * 0.5f;
    }

    float calcArea(Vector3 x, Vector3 y)
    {
        return Mathf.Abs((x.x * (y.y - y.z) + x.y * (y.z - y.x) + x.z * (y.x - y.y)) * 0.5f);
    }

    List<triList> insertIntoTri(triList tri, int vIndex)
    {
        int i1, i2, i3;

        i1 = tri.indices[0];
        i2 = tri.indices[1];
        i3 = tri.indices[2];

        int i4 = vIndex;

        triList t1 = new triList();
        triList t2 = new triList();
        triList t3 = new triList();

        t1.indices = new int[3];
        t2.indices = new int[3];
        t3.indices = new int[3];

        t1.indices[0] = i1;
        t1.indices[1] = i4;
        t1.indices[2] = i2;

        t2.indices[0] = i2;
        t2.indices[1] = i4;
        t2.indices[2] = i3;

        t3.indices[0] = i3;
        t3.indices[1] = i4;
        t3.indices[2] = i1;

        List<triList> ret = new List<triList>();

        ret.Add(t1);
        ret.Add(t2);
        ret.Add(t3);

        return ret;
    }

    void addVertex(List<triList> ret, List<Vector3> verts, Vector3 vertex, int vIndex)
    {
        for(int i=0; i<ret.Count; i++)
        {
            triList t = ret[i];

            Vector3 v1, v2, v3;

            v1 = verts[t.indices[0]];
            v2 = verts[t.indices[1]];
            v3 = verts[t.indices[2]];

            Vector3 xs = new Vector3(v1.x, v2.x, v3.x);
            Vector3 ys = new Vector3(v1.y, v2.y, v3.y);

            float area = calcArea(xs, ys);

            float calcedArea = calcThirdAreas(v1.x, v2.x, v3.x, v1.y, v2.y, v3.y, vertex.x, vertex.y);

            if(calcedArea <= area + 0.01f)
            {
                List<triList> toInsert = insertIntoTri(ret[i], vIndex);

                ret.AddRange(toInsert);

                return;
            }
        }
    }

    struct returnDelaunay
    {
        public List<triList> tris;
        public List<Vector3> points;
    }

    returnDelaunay getDelaunay(List<Vector3> centres)
    {
        //centres.Sort((x, y) => x.x.CompareTo(y.x));

        List<triList> ret = new List<triList>();

        List<Vector3> mpoints = centres;

        float minx = float.MaxValue;
        float minz = float.MaxValue;

        float maxx = -float.MaxValue;
        float maxz = -float.MaxValue;

        for(int i=0; i<centres.Count; i++)
        {
            Vector3 c = centres[i];

            minx = Mathf.Min(minx, c.x);
            minz = Mathf.Min(minz, c.z);
            maxx = Mathf.Max(maxx, c.x);
            maxz = Mathf.Max(maxz, c.z);
        }

        float height = centres[0].y;

        mpoints.Insert(0, new Vector3(minx, height, minz));
        mpoints.Insert(1, new Vector3(minx, height, maxz));
        mpoints.Insert(2, new Vector3(maxx, height, minz));
        mpoints.Insert(3, new Vector3(maxx, height, maxz));

        triList tri1;
        tri1.indices = new int[3]{ 0, 1, 2 };
        //tri1.vertices = new Vector3[3] { centres[0], centres[1], centres[3] };

        triList tri2;
        tri2.indices = new int[3] { 3, 2, 1 };
        //tri2.vertices = new Vector3[3] { centres[3], centres[2], centres[1] };

        ret.Add(tri1);
        ret.Add(tri2);

        if (centres.Count < 2)
        {
            return new returnDelaunay();
        }

        for(int i=4; i<mpoints.Count; i++)
        {
            addVertex(ret, mpoints, mpoints[i], i);
        }

        returnDelaunay rd;

        rd.points = mpoints;
        rd.tris = ret;

        /*for(int i=0; i<mpoints.Count; i++)
        {
            ret[i].indices[0] -= 3;
            ret[i].indices[1] -= 3;
            ret[i].indices[2] -= 3;
        }

        ret.RemoveAt(0);
        ret.RemoveAt(0);*/

        return rd;
    }

    void Fragment(Mesh m)
    {
        float fragmentHeight = 0.8f;

        Bounds bound = mesh.bounds;

        Vector3 min = bound.min;
        Vector3 max = bound.max;

        ///Maybe one day i'll understand why this isn't the * operator
        ///and also why its called scale
        min.Scale(transform.localScale);
        max.Scale(transform.localScale);

        int fragmentsPerLayer = 8;

        List<List<Vector3>> yFragments = new List<List<Vector3>>();

        int cy = 0;
        for(float y = min.y; y <= max.y; y += fragmentHeight)
        {
            yFragments.Add(new List<Vector3>());

            for(int i=0; i<fragmentsPerLayer; i++)
            {
                Vector3 rpos = getRandomWithinCollider(col, y + transform.position.y);

                Debug.Log(rpos);

                yFragments[cy].Add(rpos);
            }

            cy++;
        }

        foreach(var layer in yFragments)
        {
            returnDelaunay rd = getDelaunay(layer);

            for(int t = 0; t < rd.tris.Count; t++)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                Mesh omesh = obj.GetComponent<MeshFilter>().mesh;

                List<Vector3> vs = new List<Vector3>();

                triList triangle = rd.tris[t];

                for(int i=0; i<3; i++)
                    vs.Add(rd.points[triangle.indices[i]]);

                int[] indices = new int[3];

                for(int i=0; i<3; i++)
                {
                    indices[i] = i;
                }

                omesh.SetTriangles(indices, 0);
                omesh.SetVertices(vs);
                omesh.RecalculateBounds();
                omesh.RecalculateNormals();

                /*BoxCollider c = obj.GetComponent<BoxCollider>();

                c.enabled = false;

                MeshCollider mcollider = obj.AddComponent<MeshCollider>();

                mcollider.convex = true;

                obj.AddComponent<Rigidbody>();*/

                obj.SetActive(true);
            }
        }

        gameObject.SetActive(false);
    }

    void FragmentNonDelaunay(Mesh m)
    {
        Bounds bound = mesh.bounds;

        Vector3 nums = new Vector3(4, 3, 4);

        //float fragmentWidth = 0.5f;
        //float fragmentHeight = 0.8f;
        //float fragmentDepth = 0.5f;

        Vector3 min = bound.min;
        Vector3 max = bound.max;

        ///ffs unity, lacking basic vector features
        //Vector3 step = (max - min) / nums;


        ///Maybe one day i'll understand why this isn't the * operator
        ///and also why its called scale
        min.Scale(transform.lossyScale);
        max.Scale(transform.lossyScale);

        Vector3 step = new Vector3();

        step.x = Mathf.Abs(max.x - min.x) / nums.x;
        step.y = Mathf.Abs(max.y - min.y) / nums.y;
        step.z = Mathf.Abs(max.z - min.z) / nums.z;

        for (float y = min.y; y < max.y; y += step.y)
        {
            for (float z = min.z; z < max.z; z += step.z)
            {
                for (float x = min.x; x < max.x; x += step.x)
                {
                    Vector3 point = new Vector3(x, y, z);

                    Collider[] found = Physics.OverlapSphere(point + gameObject.transform.position, 0.0001f);

                    bool foundMine = false;

                    for(int i=0; i<found.Length; i++)
                    {
                        if(found[i] == col)
                        {
                            foundMine = true;
                            break;
                        }
                    }

                    if (!foundMine)
                        continue;

                    ///SIGH, THANKS UNITY
                    //if (Physics.CheckSphere())
                    {
                        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                        obj.transform.position = point + transform.position + step/2f;
                        obj.transform.localScale = step * 0.99f;

                        Rigidbody body = obj.AddComponent<Rigidbody>();

                        obj.AddComponent<BoxCollider>();

                        //body.AddExplosionForce(10f, transform.position, 10 * (max - min).magnitude / 2f);
                        body.AddExplosionForce(100f, transform.position, 100f);
                    }
                }
            }
        }
        
        gameObject.SetActive(false);
    }


    void FragmentNonDelaunayOld(Mesh m)
    {
        Bounds bound = mesh.bounds;

        float fragmentWidth = 0.4f;
        float fragmentHeight = 0.8f;
        float fragmentDepth = 0.4f;

        Vector3 min = bound.min;
        Vector3 max = bound.max;

        ///Maybe one day i'll understand why this isn't the * operator
        ///and also why its called scale
        min.Scale(transform.localScale);
        max.Scale(transform.localScale);

        List<fragment> fragments = new List<fragment>();

        bool iAmWithin = false;

        for (float y = min.y; y <= max.y; y += fragmentHeight)
        {
            for (float z = min.z; z <= max.z; z += fragmentDepth)
            {
                for (float x = min.x; x <= max.x; x += fragmentWidth)
                {
                    Vector3 point = new Vector3(x, y, z);

                    ///SIGH, THANKS UNITY
                    if (Physics.CheckSphere(point + gameObject.transform.position, 0.001f))
                    {
                        bool boundary = false;

                        if (!iAmWithin)
                        {
                            boundary = true;
                        }

                        fragment frag = new fragment();
                        frag.boundary = boundary;
                        frag.pos = point;

                        fragments.Add(frag);

                        iAmWithin = true;
                    }
                    else
                    {
                        ///the last particle was not inside!!!!
                        if (iAmWithin)
                        {
                            fragments[fragments.Count - 1].boundary = true;
                        }

                        iAmWithin = false;
                    }
                }
            }
        }

        List<fragment> boundPoints = new List<fragment>();

        List<List<fragment>> yLayeredBounds = new List<List<fragment>>();

        int cy = -1;

        float currentY = float.MaxValue;

        Vector3 globalPos = transform.position;


        for (int i = 0; i < fragments.Count; i++)
        {
            //if(!fragments[i].boundary)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

                obj.transform.position = fragments[i].pos + globalPos;
                obj.transform.localScale = new Vector3(fragmentWidth, fragmentHeight, fragmentDepth) * 0.98f;

                obj.AddComponent<Rigidbody>();
                obj.AddComponent<BoxCollider>();
            }

            if (fragments[i].boundary)
            {
                boundPoints.Add(fragments[i]);
            }

            if (currentY != fragments[i].pos.y && fragments[i].boundary)
            {
                yLayeredBounds.Add(new List<fragment>());
                cy++;
                currentY = fragments[i].pos.y;
            }

            ///so basically we end up with x/z pounds, with y structure
            if (fragments[i].boundary)
            {
                yLayeredBounds[cy].Add(fragments[i]);
            }
        }

        ///objListOrder.Sort((x, y) => x.OrderDate.CompareTo(y.OrderDate));


        /*for(int l = 0; l < yLayeredBounds.Count; l++)
        {
            yLayeredBounds[l].Sort((x, y) => fragAngle(x).CompareTo(fragAngle(y)));
        }

        for(int l = 0; l < yLayeredBounds.Count; l++)
        {
            List<fragment> layer = yLayeredBounds[l];

            for(int i = 0; i < layer.Count; i++)
            {
                int next = (i + 1) % layer.Count;

                fragment f1 = layer[i];
                fragment f2 = layer[next];

                //Vector3 p1 = getNearestVertex(m, f1.pos);
                //Vector3 p2 = getNearestVertex(m, f2.pos);
            }
        }*/

        gameObject.SetActive(false);
    }
}

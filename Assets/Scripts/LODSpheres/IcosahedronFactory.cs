using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class IcosahedronFactory : MonoBehaviour {

    private struct TriangleFace
    {
        public Vector3 v1, v2, v3;
        public TriangleFace(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    private struct TriangleFace2D{
        public Vector2 v1, v2, v3;
        public TriangleFace2D(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }
        
    private List<Vector3> vertexList; // our vertices list, has every triangle (face) of the mesh
    // consequently the first vertices are duplicated 5 times, and subdivided vertices are duplicated 6 times
    private float t = (1f + Mathf.Sqrt(5f)) / 2f; // golden ratio
    private List<TriangleFace> faces;
    private List<TriangleFace2D> uvFaces;
    private Mesh mesh;

    public float radius = 2f;
    public int depth = 2;

    private void addFace(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        faces.Add(new TriangleFace(v1, v2, v3));
    }

    private void addUVFace(Vector2 v1, Vector2 v2, Vector2 v3)
    {
        uvFaces.Add(new TriangleFace2D(v1, v2, v3));
    }

    //build the first faces making our icosahedron
    public void BuildIcosahedron()
    {
        List<Vector3> vertList = new List<Vector3>();

        //create the 12 vertices of a icosahedron
        Vector3[] vecs = new Vector3[12];
        vecs[0] = new Vector3(-1f, t, 0f).normalized * radius;
        vecs[1] = new Vector3(1f, t, 0f).normalized * radius;
        vecs[2] = new Vector3(-1f, -t, 0f).normalized * radius;
        vecs[3] = new Vector3(1f, -t, 0f).normalized * radius;

        vecs[4] = new Vector3(0f, -1f, t).normalized * radius;
        vecs[5] = new Vector3(0f, 1f, t).normalized * radius;
        vecs[6] = new Vector3(0f, -1f, -t).normalized * radius;
        vecs[7] = new Vector3(0f, 1f, -t).normalized * radius;

        vecs[8] = new Vector3(t, 0f, -1f).normalized * radius;
        vecs[9] = new Vector3(t, 0f, 1f).normalized * radius;
        vecs[10] = new Vector3(-t, 0f, -1f).normalized * radius;
        vecs[11] = new Vector3(-t, 0f, 1f).normalized * radius;

        // create 20 triangles of the icosahedron
        faces = new List<TriangleFace>();

        // 5 faces around point 0
        addFace(vecs[0], vecs[11], vecs[5]);
        addFace(vecs[0], vecs[5], vecs[1]);
        addFace(vecs[0], vecs[1], vecs[7]);
        addFace(vecs[0], vecs[7], vecs[10]);
        addFace(vecs[0], vecs[10], vecs[11]);

        // 5 adjacent faces
        addFace(vecs[1], vecs[5], vecs[9]);
        addFace(vecs[5], vecs[11], vecs[4]);
        addFace(vecs[11], vecs[10], vecs[2]);
        addFace(vecs[10], vecs[7], vecs[6]);
        addFace(vecs[7], vecs[1], vecs[8]);

        // 5 faces around point 3
        addFace(vecs[3], vecs[9], vecs[4]);
        addFace(vecs[3], vecs[4], vecs[2]);
        addFace(vecs[3], vecs[2], vecs[6]);
        addFace(vecs[3], vecs[6], vecs[8]);
        addFace(vecs[3], vecs[8], vecs[9]);

        // 5 adjacent faces
        addFace(vecs[4], vecs[9], vecs[5]);
        addFace(vecs[2], vecs[4], vecs[11]);
        addFace(vecs[6], vecs[2], vecs[10]);
        addFace(vecs[8], vecs[6], vecs[7]);
        addFace(vecs[9], vecs[8], vecs[1]);

    }

    //find middle point of an edge for subdivision
    private Vector3 getMiddle(Vector3 v1, Vector3 v2)
    {
        Vector3 res = new Vector3();
        // compute the middle
        res = (v2 - v1) * 0.5f + v1;

        //offset back on the sphere
        res.Normalize();
        //res *= Mathf.Sqrt(t * t + 1f) * radius;
        res *= radius;
        return res;
    }

    //subdivides every face of the icosahedron, later add rules to make LOD sphere
    private void subdivideIcosahedron()
    {
        List<TriangleFace> newFaces = new List<TriangleFace>();
        Vector3 v1, v2, v3;
        Vector3 v4, v5, v6;

        for (int i = 0; i < faces.Count; i++)
        {
            v1 = faces[i].v1;
            v2 = faces[i].v2;
            v3 = faces[i].v3;
            //find middle points
            v4 = getMiddle(v1, v2);
            v5 = getMiddle(v2, v3);
            v6 = getMiddle(v3, v1);

            //add the new 4 faces
            newFaces.Add(new TriangleFace(v6, v1, v4));
            newFaces.Add(new TriangleFace(v4, v2, v5));
            newFaces.Add(new TriangleFace(v5, v3, v6));
            newFaces.Add(new TriangleFace(v4, v5, v6));
        }
        //replace old list with new faces
        faces = newFaces;
    }

    //build UVmap for the basic icosahedron
    private void buildUVMap()
    {
        float w = 5.5f; //horizontal length
        float h = 3f; // vertical

        //build first points of the icosahedron
        Vector2[] UVPoints = new Vector2[22];
        UVPoints[0] = new Vector2(0.5f / w, 0);
        UVPoints[1] = new Vector2(1.5f / w, 0);
        UVPoints[2] = new Vector2(2.5f / w, 0);
        UVPoints[3] = new Vector2(3.5f / w, 0);
        UVPoints[4] = new Vector2(4.5f / w, 0);

        UVPoints[5] = new Vector2(0, 1 / h);
        UVPoints[6] = new Vector2(1f / w, 1 / h);
        UVPoints[7] = new Vector2(2f / w, 1 / h);
        UVPoints[8] = new Vector2(3f / w, 1 / h);
        UVPoints[9] = new Vector2(4f / w, 1 / h);
        UVPoints[10] = new Vector2(5f / w, 1 / h);

        UVPoints[11] = new Vector2(0.5f / w, 2 / h);
        UVPoints[12] = new Vector2(1.5f / w, 2 / h);
        UVPoints[13] = new Vector2(2.5f / w, 2 / h);
        UVPoints[14] = new Vector2(3.5f / w, 2 / h);
        UVPoints[15] = new Vector2(4.5f / w, 2 / h);
        UVPoints[16] = new Vector2(1, 2 / h);

        UVPoints[17] = new Vector2(1f / w, 1);
        UVPoints[18] = new Vector2(2f / w, 1);
        UVPoints[19] = new Vector2(3f / w, 1);
        UVPoints[20] = new Vector2(4f / w, 1);
        UVPoints[21] = new Vector2(5f / w, 1);

        uvFaces = new List<TriangleFace2D>();
        //create the 20 faces of the icosahedron on the UVMap
        //first row
        addUVFace(UVPoints[0], UVPoints[5], UVPoints[6]);
        addUVFace(UVPoints[1], UVPoints[6], UVPoints[7]);
        addUVFace(UVPoints[2], UVPoints[7], UVPoints[8]);
        addUVFace(UVPoints[3], UVPoints[8], UVPoints[9]);
        addUVFace(UVPoints[4], UVPoints[9], UVPoints[10]);

        //second row
        addUVFace(UVPoints[7], UVPoints[6], UVPoints[12]);
        addUVFace(UVPoints[6], UVPoints[5], UVPoints[11]);
        addUVFace(UVPoints[10], UVPoints[9], UVPoints[15]);
        addUVFace(UVPoints[9], UVPoints[8], UVPoints[14]);
        addUVFace(UVPoints[8], UVPoints[7], UVPoints[13]);

        //fourth row
        addUVFace(UVPoints[17], UVPoints[12], UVPoints[11]);
        addUVFace(UVPoints[21], UVPoints[16], UVPoints[15]);
        addUVFace(UVPoints[20], UVPoints[15], UVPoints[14]);
        addUVFace(UVPoints[19], UVPoints[14], UVPoints[13]);
        addUVFace(UVPoints[18], UVPoints[13], UVPoints[12]);

        //third row
        addUVFace(UVPoints[11], UVPoints[12], UVPoints[6]);
        addUVFace(UVPoints[15], UVPoints[16], UVPoints[10]);
        addUVFace(UVPoints[14], UVPoints[15], UVPoints[9]);
        addUVFace(UVPoints[13], UVPoints[14], UVPoints[8]);
        addUVFace(UVPoints[12], UVPoints[13], UVPoints[7]);
    }

    //the UVMap subdivision is the same as for 3D vertices
    private Vector2 getMiddle(Vector2 v1, Vector2 v2)
    {
        Vector2 res = new Vector2();
        res = (v2 - v1) * 0.5f + v1;
        return res;
    }

    private void subdivideUVMap()
    {
        List<TriangleFace2D> newUVFaces = new List<TriangleFace2D>();
        Vector2 v1, v2, v3;
        Vector2 v4, v5, v6;
        for (int i = 0; i < uvFaces.Count; i++)
        {
            v1 = uvFaces[i].v1;
            v2 = uvFaces[i].v2;
            v3 = uvFaces[i].v3;
            //find middle points
            v4 = getMiddle(v1, v2);
            v5 = getMiddle(v2, v3);
            v6 = getMiddle(v3, v1);

            //add 4 new faces
            newUVFaces.Add(new TriangleFace2D(v6, v1, v4));
            newUVFaces.Add(new TriangleFace2D(v4, v2, v5));
            newUVFaces.Add(new TriangleFace2D(v5, v3, v6));
            newUVFaces.Add(new TriangleFace2D(v4, v5, v6));
        }
        //replace old faces
        uvFaces = newUVFaces;
    }

    //create mesh from our data
    void CreateMesh()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        vertexList = new List<Vector3>();
        for (int i = 0; i < faces.Count; i++)
        {
            vertexList.Add(faces[i].v1);
            vertexList.Add(faces[i].v2);
            vertexList.Add(faces[i].v3);
        }

        int[] triangles = new int[vertexList.Count];
        for (int i = 0; i < triangles.Length; i++)
        {
            triangles[i] = i;
        }

        List<Vector2> uvs = new List<Vector2>();
        for (int i = 0; i < uvFaces.Count; i++)
        {
            uvs.Add(uvFaces[i].v1);
            uvs.Add(uvFaces[i].v2);
            uvs.Add(uvFaces[i].v3);
        }

        mesh.vertices = vertexList.ToArray();
        mesh.triangles = triangles;
        mesh.uv = uvs.ToArray();

    }

    private struct PatchConfig
    {
        public string name;
        public Vector3 v1, v2, v3;
        public Vector2 uv1, uv2, uv3;
        public PatchConfig(string name, TriangleFace face, TriangleFace2D uvface)
        {
            this.name = name;
            v1 = face.v1;
            v2 = face.v2;
            v3 = face.v3;
            uv1 = uvface.v1;
            uv2 = uvface.v2;
            uv3 = uvface.v3;
        }

    }
    //bypass 65000 vertices limit
    void CreatePatch(PatchConfig conf)
    {
        GameObject patch = new GameObject("Patch_" + conf.name);
        MeshFilter mf = patch.AddComponent<MeshFilter>();
        MeshRenderer rend = patch.AddComponent<MeshRenderer>();
        rend.sharedMaterial = GetComponent<MeshRenderer>().material;
        Mesh m = mf.sharedMesh = new Mesh();
        patch.transform.parent = transform;
        patch.transform.localEulerAngles = Vector3.zero;
        patch.transform.localPosition = Vector3.zero;

        List<Vector3> patchVert = new List<Vector3>();
        patchVert.Add(conf.v1);
        patchVert.Add(conf.v2);
        patchVert.Add(conf.v3);

        int[] patchTriangles = new int[3];
        for (int i = 0; i < patchTriangles.Length; i++)
        {
            patchTriangles[i] = i;
        }

        List<Vector2> patchUV = new List<Vector2>();
        patchUV.Add(conf.uv1);
        patchUV.Add(conf.uv2);
        patchUV.Add(conf.uv3);

        m.vertices = patchVert.ToArray();
        m.triangles = patchTriangles;
        m.uv = patchUV.ToArray();

        m.RecalculateNormals();
        m.RecalculateBounds();
    }
    void CreatePatches()
    {
        int count = 0;
        for (int i = 0; i < faces.Count; i++)
        {
            PatchConfig patch = new PatchConfig(i.ToString(), faces[i], uvFaces[i]);
            CreatePatch(patch);
            count++;
        }
        Debug.Log("Number of patches: " + count);
    }
    // Use this for initialization
    void Start () {

        //build starting polygon
        BuildIcosahedron();
        buildUVMap();

        //subdivision process
        for (int i = 0; i < depth; i++)
        {
            subdivideIcosahedron();
            subdivideUVMap();
        }

        //create mesh
        //CreateMesh();
        CreatePatches();
	}
	
	// Update is called once per frame
	void Update () {

	}
}

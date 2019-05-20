using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Icosahedron : MonoBehaviour {

    private struct TriangleIndices
    {
        public int v1;
        public int v2;
        public int v3;

        public TriangleIndices(int v1, int v2, int v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }
    }

    private Mesh mesh;

    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uvs;
    private List<TriangleIndices> faces;
    private List<TriangleIndices> uvfaces;
    private Dictionary<long, int> middlePointIndexCache;

    float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;//golden ratio

    public float radius = 100f; //radius of the sphere
    public int depth = 4;

    public static Vector3[] GetIcosahedronPositions(float radius)
    {
        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;//golden ratio
        //create the 12 vertices of a icosahedron
        Vector3[] verts = new Vector3[12];
        verts[0] = new Vector3(-1f, t, 0f).normalized * radius;
        verts[1] = new Vector3(1f, t, 0f).normalized * radius;
        verts[2] = new Vector3(-1f, -t, 0f).normalized * radius;
        verts[3] = new Vector3(1f, -t, 0f).normalized * radius;

        verts[4] = new Vector3(0f, -1f, t).normalized * radius;
        verts[5] = new Vector3(0f, 1f, t).normalized * radius;
        verts[6] = new Vector3(0f, -1f, -t).normalized * radius;
        verts[7] = new Vector3(0f, 1f, -t).normalized * radius;

        verts[8] = new Vector3(t, 0f, -1f).normalized * radius;
        verts[9] = new Vector3(t, 0f, 1f).normalized * radius;
        verts[10] = new Vector3(-t, 0f, -1f).normalized * radius;
        verts[11] = new Vector3(-t, 0f, 1f).normalized * radius;

        return verts;
    }

    public static int[] GetIcosahedronIndices()
    {
        int[] res = new int[]{
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,

            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,

            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,

            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1
        };
        return res;
    }

    void BuildIcosahedron()
    {
        //create the 12 vertices of a icosahedron
        Vector3[] verts = new Vector3[12];
        verts[0] = new Vector3(-1f, t, 0f).normalized * radius;
        verts[1] = new Vector3(1f, t, 0f).normalized * radius;
        verts[2] = new Vector3(-1f, -t, 0f).normalized * radius;
        verts[3] = new Vector3(1f, -t, 0f).normalized * radius;

        verts[4] = new Vector3(0f, -1f, t).normalized * radius;
        verts[5] = new Vector3(0f, 1f, t).normalized * radius;
        verts[6] = new Vector3(0f, -1f, -t).normalized * radius;
        verts[7] = new Vector3(0f, 1f, -t).normalized * radius;

        verts[8] = new Vector3(t, 0f, -1f).normalized * radius;
        verts[9] = new Vector3(t, 0f, 1f).normalized * radius;
        verts[10] = new Vector3(-t, 0f, -1f).normalized * radius;
        verts[11] = new Vector3(-t, 0f, 1f).normalized * radius;

        vertices = new List<Vector3>();
        for(int i = 0; i<verts.Length;i++)
        {
            vertices.Add(verts[i]);
        }

        // create 20 triangles of the icosahedron
        faces = new List<TriangleIndices>();

        // 5 faces around point 0
        faces.Add(new TriangleIndices(0, 11, 5));
        faces.Add(new TriangleIndices(0, 5, 1));
        faces.Add(new TriangleIndices(0, 1, 7));
        faces.Add(new TriangleIndices(0, 7, 10));
        faces.Add(new TriangleIndices(0, 10, 11));

        // 5 adjacent faces 
        faces.Add(new TriangleIndices(1, 5, 9));
        faces.Add(new TriangleIndices(5, 11, 4));
        faces.Add(new TriangleIndices(11, 10, 2));
        faces.Add(new TriangleIndices(10, 7, 6));
        faces.Add(new TriangleIndices(7, 1, 8));

        // 5 faces around point 3
        faces.Add(new TriangleIndices(3, 9, 4));
        faces.Add(new TriangleIndices(3, 4, 2));
        faces.Add(new TriangleIndices(3, 2, 6));
        faces.Add(new TriangleIndices(3, 6, 8));
        faces.Add(new TriangleIndices(3, 8, 9));

        // 5 adjacent faces 
        faces.Add(new TriangleIndices(4, 9, 5));
        faces.Add(new TriangleIndices(2, 4, 11));
        faces.Add(new TriangleIndices(6, 2, 10));
        faces.Add(new TriangleIndices(8, 6, 7));
        faces.Add(new TriangleIndices(9, 8, 1));
        
    }

    void BuildUVMap()
    {
        float w = 5.5f;// number of points horizontally
        float h = 3f;// number of points vertically

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

        //create 20 faces (triangles) on uvmap
        uvfaces = new List<TriangleIndices>();
        //first row
        uvfaces.Add(new TriangleIndices(0, 5, 6));
        uvfaces.Add(new TriangleIndices(1, 6, 7));
        uvfaces.Add(new TriangleIndices(2, 7, 8));
        uvfaces.Add(new TriangleIndices(3, 8, 9));
        uvfaces.Add(new TriangleIndices(4, 9, 10));

        //second row
        uvfaces.Add(new TriangleIndices(7, 6, 12));
        uvfaces.Add(new TriangleIndices(6, 5, 11));
        uvfaces.Add(new TriangleIndices(10, 9, 15));
        uvfaces.Add(new TriangleIndices(9, 8, 14));
        uvfaces.Add(new TriangleIndices(8, 7, 13));

        //fourth row
        uvfaces.Add(new TriangleIndices(17, 12, 11));
        uvfaces.Add(new TriangleIndices(21, 16, 15));
        uvfaces.Add(new TriangleIndices(20, 15, 14));
        uvfaces.Add(new TriangleIndices(19, 14, 13));
        uvfaces.Add(new TriangleIndices(18, 13, 12));

        //third row
        uvfaces.Add(new TriangleIndices(11, 12, 6));
        uvfaces.Add(new TriangleIndices(15, 16, 10));
        uvfaces.Add(new TriangleIndices(14, 15, 9));
        uvfaces.Add(new TriangleIndices(13, 14, 8));
        uvfaces.Add(new TriangleIndices(12, 13, 7));
    }

    void CreateMesh()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.vertices = vertices.ToArray();

        triangles = new List<int>();
        for (int i = 0; i < faces.Count; i++)
        {
            triangles.Add(faces[i].v1);
            triangles.Add(faces[i].v2);
            triangles.Add(faces[i].v3);
        }
        mesh.triangles = triangles.ToArray();
        
        //mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();
    }

    // return index of point in the middle of p1 and p2
    private static int getMiddlePoint(int p1, int p2, ref List<Vector3> vertices, ref Dictionary<long, int> cache, float radius)
    {
        // first check if we have it already
        bool firstIsSmaller = p1 < p2;
        long smallerIndex = firstIsSmaller ? p1 : p2;
        long greaterIndex = firstIsSmaller ? p2 : p1;
        long key = (smallerIndex << 32) + greaterIndex;

        int ret;
        if (cache.TryGetValue(key, out ret))
        {
            return ret;
        }

        // not in cache, calculate it
        Vector3 point1 = vertices[p1];
        Vector3 point2 = vertices[p2];
        Vector3 middle = new Vector3
        (
            (point1.x + point2.x) / 2f,
            (point1.y + point2.y) / 2f,
            (point1.z + point2.z) / 2f
        );

        // add vertex makes sure point is on unit sphere
        int i = vertices.Count;
        vertices.Add(middle.normalized * radius);

        // store it, return index
        cache.Add(key, i);

        return i;
    }

    void SubdivideIcosahedron()
    {
        middlePointIndexCache = new Dictionary<long, int>();
        for (int i = 0; i < depth; i++)
        {
            List<TriangleIndices> faces2 = new List<TriangleIndices>();
            foreach(var tri in faces)
            {
                //replace parent triangle by 4 triangles
                int a = getMiddlePoint(tri.v1, tri.v2, ref vertices, ref middlePointIndexCache, radius);
                int b = getMiddlePoint(tri.v2, tri.v3, ref vertices, ref middlePointIndexCache, radius);
                int c = getMiddlePoint(tri.v3, tri.v1, ref vertices, ref middlePointIndexCache, radius);

                faces2.Add(new TriangleIndices(tri.v1, a, c));
                faces2.Add(new TriangleIndices(tri.v2, b, a));
                faces2.Add(new TriangleIndices(tri.v3, c, b));
                faces2.Add(new TriangleIndices(a, b, c));
            }
            faces = faces2;
        }
    }

	// Use this for initialization
	void Start () {
        BuildIcosahedron();
        BuildUVMap();

        SubdivideIcosahedron();

        CreateMesh();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

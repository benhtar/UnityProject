using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum TriNext
{
    CULL,
    LEAF,
    SPLIT,
    SPLITCULL
}

class Tri
{
    Tri parent;
    Tri c1, c2, c3, c4;

    TriNext state;
    public readonly int level;
    public readonly Vector3 a, b, c;

    public Tri(Vector3 A, Vector3 B, Vector3 C, Tri Parent, int Level)
    {
        a = A; b = B; c = C;
        parent = Parent;
        level = Level;
    }
}

public class Triangulator {

    private List<Tri> m_icosahedron;
    private List<float> m_distanceTable;
    private List<float> m_triLevelDotTable;
    private List<float> m_heightMultTable;

    private List<Tri> m_leaves;
    private Planet planet;
    private List<PatchInstance> m_positions;

    private float m_allowedTriPx = 300f;
    private int m_maxLevel = 10;

    public bool m_frustumCull;

    public Triangulator(Planet planet)
    {

    }

    public void Init()
    {
        Vector3[] icoVerts = Icosahedron.GetIcosahedronPositions(planet.GetRadius());
        int[] indices = Icosahedron.GetIcosahedronIndices();
        for (int i = 0; i < indices.Length; i+=3)
        {
            m_icosahedron.Add(new Tri(icoVerts[indices[i]], icoVerts[indices[i + 1]], icoVerts[indices[i + 2]], null, 0));
        }

        Precalculate();
        //first geometry generation
        GenerateGeometry();
    }
    
    public bool Update()
    {
        Precalculate(); //recompute table if the camera has moved

        return true;
    }

    //Precalculates Distance Table
    //The distances at any level should keep the generated triangles smaller
    //than m_allowedTriPx
    public void GenerateGeometry()
    {
        //Precalculate distance table
        Camera cam = Camera.main;
        m_distanceTable.Clear();
        float size = (m_icosahedron[0].a - m_icosahedron[0].b).magnitude;
        float frac = Mathf.Tan((m_allowedTriPx * Mathf.Deg2Rad *cam.fieldOfView) / Screen.width);
        for (int level = 0; level < m_maxLevel + 5; level++)
        {
            m_distanceTable.Add(size / frac);
            size *= 0.5f;
        }

        //Start subdivision
        m_positions.Clear();
        foreach(var face in m_icosahedron)
        {
            RecursiveTriangle(face.a, face.b, face.c, face.level, true);
        }
    }
    
    private void Precalculate()
    {
        //Culling angle behind the planet based on max height
        float cullingAngle = Mathf.Acos(planet.GetRadius() / (planet.GetRadius() + planet.GetMaxHeight()));
        //Precalculate Dot product table
        m_triLevelDotTable.Clear();
        m_triLevelDotTable.Add(0.5f + Mathf.Sin(cullingAngle));
        float angle = Mathf.Acos(0.5f);
        for (int i = 1; i <= m_maxLevel; i++)
        {
            angle *= 0.5f;
            m_triLevelDotTable.Add(Mathf.Sin(angle + cullingAngle));
        }
        //Precalculate height multipliers table
        m_heightMultTable.Clear();
        Vector3 a = m_icosahedron[0].a;
        Vector3 b = m_icosahedron[0].b;
        Vector3 c = m_icosahedron[0].c;
        Vector3 center = (a + b + c) / 3f;
        center = center.normalized * planet.GetRadius();
        m_heightMultTable.Add(1 / (Vector3.Dot(a.normalized, center.normalized)));
        float normMaxHeight = planet.GetMaxHeight() / planet.GetRadius();
        for (int i = 1; i <= m_maxLevel; i++)
        {
            Vector3 A = (c - b) * 0.5f + b;
            //Vector3 B = (a - c) * 0.5f + c;
            //Vector3 C = (b - a) * 0.5f + a;
            a = A.normalized * planet.GetRadius();
            //b = B.normalized * planet.GetRadius();
            //c = C.normalized * planet.GetRadius();
            m_heightMultTable.Add(1 / (Vector3.Dot(a.normalized, center.normalized)) + normMaxHeight);
        }
    }

    //----------------------------
    //
    // SPLITTING STUFF
    //
    //----------------------------
    //Defines if a triangle can be split into lower level or not
    private TriNext SplitHeuristic(Vector3 a, Vector3 b, Vector3 c, int level, bool frustumCull)
    {
        Camera cam = Camera.main;
        Vector3 cam_pos = cam.transform.position;
        //Backface culling (dot product triangle normal * cam to center vector)
        Vector3 center = (a + b + c) / 3f;
        Vector3 camtocenter = center - cam_pos;
        if (Vector3.Dot(center.normalized, camtocenter.normalized) >= m_triLevelDotTable[level])
            return TriNext.CULL;

        //Frustum culling
        if (frustumCull)
        {
            Bounds bound = new Bounds(center, center - a);
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
            if (!GeometryUtility.TestPlanesAABB(planes, bound))
                return TriNext.CULL;
            else
            {
                //check if new splits are allowed
                if (level >= m_maxLevel)
                    return TriNext.LEAF;
                //split according to the distance to camera
                float aDist = (a - cam_pos).magnitude;
                float bDist = (b - cam_pos).magnitude;
                float cDist = (c - cam_pos).magnitude;
                if (Mathf.Min(aDist, Mathf.Min(bDist, cDist)) < m_distanceTable[level])
                    return TriNext.SPLIT;
                return TriNext.LEAF;
            }
        }
        //Check if new splits are allowed
        if (level >= m_maxLevel)
            return TriNext.LEAF;
        //split according to the distance to camera
        float aaDist = (a - cam_pos).magnitude;
        float bbDist = (b - cam_pos).magnitude;
        float ccDist = (c - cam_pos).magnitude;
        if (Mathf.Min(aaDist, Mathf.Min(bbDist, ccDist)) < m_distanceTable[level])
            return TriNext.SPLITCULL;
        return TriNext.LEAF;
    }
    
    //Performs the recursive subdivision of the icosahedron faces
    //following the split rules above. Also rules out culled triangles
    private void RecursiveTriangle(Vector3 a, Vector3 b, Vector3 c, int level, bool frustumCull)
    {
        TriNext next = SplitHeuristic(a, b, c, level, frustumCull);
        //If culled, proceed to next triangle
        if (next == TriNext.CULL)
            return;
        //If can be split, split and add new triangles
        else if(next == TriNext.SPLIT || next == TriNext.SPLITCULL)
        {
            //find middle points
            Vector3 d = (b - a) * 0.5f + a;
            Vector3 e = (c - b) * 0.5f + b;
            Vector3 f = (a - c) * 0.5f + c;
            //push back on the sphere
            d = d.normalized * planet.GetRadius();
            e = e.normalized * planet.GetRadius();
            f = f.normalized * planet.GetRadius();
            //make the 4 new triangles
            int newLevel = level + 1;
            RecursiveTriangle(a, d, f, newLevel, next == TriNext.SPLITCULL);
            RecursiveTriangle(d, b, e, newLevel, next == TriNext.SPLITCULL);
            RecursiveTriangle(e, c, f, newLevel, next == TriNext.SPLITCULL);
            RecursiveTriangle(d, e, f, newLevel, next == TriNext.SPLITCULL);
        }
        //Else we have a leaf ready for the buffer
        else
        {
            m_positions.Add(new PatchInstance(level, a, b - a, c - a));
        }
    }
}

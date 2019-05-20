using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchVertex
{
    Vector2 pos;
    Vector2 morph;

    public PatchVertex(Vector2 position, Vector2 morphVec)
    {
        pos = position;
        morph = morphVec;
    }
}

public class PatchInstance
{
    int level;
    Vector3 a, r, s;

    public PatchInstance(int Level, Vector3 A, Vector3 R, Vector3 S)
    {
        level = Level;
        a = A;
        r = R;
        s = S;
    }
}

public class Patch {

    private List<PatchVertex> m_vertices;
    private List<int> m_indices;

    private Planet m_planet;
    private int m_numInstances = 0;
    private int m_levels;
    private int m_RC;

    private float m_morphRange = 0.5f;

    public Patch(int levels = 5)
    {
        m_levels = levels;
    }

    public int GetVertexCount()
    {
        return m_vertices.Count;
    }

    public void SetPlanet(Planet planet)
    {
        m_planet = planet;
    }

    public void Init()
    {
        GenerateGeometry(m_levels);
    }

    public void GenerateGeometry(int levels)
    {
        //clear buffers
        m_vertices.Clear();
        m_indices.Clear();
        //Generate buffers
        m_levels = levels;
        m_RC = 1 + (int)Mathf.Pow(2, m_levels);

        float delta = 1 / (float)(m_RC - 1);
        int rowIdx = 0;
        int nextIdx = 0;
        for (int row = 0; row < m_RC; row++)
        {
            int numCols = m_RC - row;
            nextIdx += numCols;
            for (int col = 0; col < numCols; col++)
            {
                //compute position and morph
                Vector2 pos = new Vector2(col * delta, row * delta);
                Vector2 morph = Vector2.zero;
                if (row%2 == 0)
                {
                    if (col % 2 == 1)
                        morph.x = -delta;// (-delta, 0);
                }
                else
                {
                    if (col % 2 == 0)
                        morph.y = delta;// (0, delta);
                    else
                        morph.x = delta; morph.y = -delta;//(delta, -delta);
                }
                //create vertex
                m_vertices.Add(new PatchVertex(pos, morph));
                //compute indices
                if(row < m_RC - 1 && col < numCols - 1)
                {
                    m_indices.Add(rowIdx + col);
                    m_indices.Add(nextIdx + col);
                    m_indices.Add(1 + rowIdx + col);
                    if(col<numCols - 2)
                    {
                        m_indices.Add(nextIdx + col);
                        m_indices.Add(1 + nextIdx + col);
                        m_indices.Add(1 + rowIdx + col);
                    }
                }
            }
            rowIdx = nextIdx;
            //at this point we have our vertices and indices
        }
    }

    public void BindInstances(List<PatchInstance> instances)
    {
        m_numInstances = instances.Count;
        var materialProperty = new MaterialPropertyBlock();
        //materialProperty.SetFloatArray("levels", m_levels);
        m_planet.gameObject.GetComponent<Renderer>().SetPropertyBlock(materialProperty);
        //uniform float arrayName[size]
    }

    public void UploadDistanceTable(List<float> distances)
    {
        var distanceTableProp = new MaterialPropertyBlock();
        distanceTableProp.SetFloatArray("distanceTable", distances);
        m_planet.gameObject.GetComponent<Renderer>().SetPropertyBlock(distanceTableProp);
    }

    public void Draw()
    {
        //pass all uniforms to the shader
        var radiusProp = new MaterialPropertyBlock();
        var morphRangeProp = new MaterialPropertyBlock();
        var deltaProp = new MaterialPropertyBlock();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour {

    private Triangulator m_triangulator;
    private Patch m_patch;

    private float m_radius = 1700f;
    private float m_maxHeight = 10f;

    public float GetRadius()
    {
        return m_radius;
    }

    public float GetMaxHeight()
    {
        return m_maxHeight;
    }

	// Use this for initialization
	void Start () {
        m_triangulator = new Triangulator(this);
        m_patch = new Patch(4);
        m_patch.SetPlanet(this);

        //load textures

        //init triangulator and patch
        m_triangulator.Init();
        m_patch.Init();
	}
	
	// Update is called once per frame
	void Update () {

        //Change planet geometry
        if (m_triangulator.Update())
        {
            //change vertex positions
            m_triangulator.GenerateGeometry();
        }
	}
}

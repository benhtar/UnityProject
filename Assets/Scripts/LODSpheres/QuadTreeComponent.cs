using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeComponent : MonoBehaviour {

    public float size = 5;
    public int depth = 2;

    public Transform[] points = new Transform[0];

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        Vector3 distance = new Vector3(0, 0, 0);
        var quadtree = new QuadTree<bool>(this.transform.position, size, depth);
        foreach(var point in points){
            quadtree.Insert(point.position, true);
        }
        DrawNode(quadtree.GetRoot());
    }

    private Color minColor = new Color(1,1,1,1f);
    private Color maxColor = new Color(0, 0.5f, 1, 0.25f);

    private void DrawNode(QuadTree<bool>.QuadTreeNode<bool> node, int nodeDepth = 0)
    {
        if (!node.IsLeaf())
        {
            foreach(var subnode in node.Nodes)
            {
                if(subnode != null)
                {
                    DrawNode(subnode, nodeDepth + 1);
                }
            }
        }
        Gizmos.color = Color.Lerp(minColor, maxColor, nodeDepth / (float)depth);
        Gizmos.DrawWireCube(node.Position, new Vector3(1,1,0.1f) * node.Size);
    }
}

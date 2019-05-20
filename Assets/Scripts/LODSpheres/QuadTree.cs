using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuadTreeIndex
{
    TopLeft = 0,    //00    first bit -> left(0) or right(1)
    TopRight = 2,   //10    second bit -> top(0) or bottom(1)
    BottomRight = 3,//11
    BottomLeft = 1  //01
}

public class QuadTree<TType> {

    private QuadTreeNode<TType> node;
    private int depth;

    public QuadTree(Vector2 position, float size, int depth)
    {
        node = new QuadTreeNode<TType>(position, size);
        this.depth = depth;
        //node.Subdivide(depth);
    }
	
    public void Insert(Vector2 position, TType value)
    {
        node.Subdivide(position, value, depth);
    }
    
    public class QuadTreeNode<TType>
    {
        Vector2 position;
        float size;
        QuadTreeNode<TType>[] subNodes;
        TType value;

        public QuadTreeNode(Vector2 pos, float size)
        {
            position = pos;
            this.size = size;
        }

        public IEnumerable<QuadTreeNode<TType>> Nodes
        {
            get { return subNodes; }
        }

        public Vector2 Position
        {
            get { return position; }
        }

        public float Size
        {
            get { return size; }
        }

        public void Subdivide(Vector2 targetPosition, TType type, int depth = 0)
        {

            var subdivIndex = GetIndexOfPosition(targetPosition, position);
            if (subNodes == null)
            {
                subNodes = new QuadTreeNode<TType>[4];
                for (int i = 0; i < subNodes.Length; i++)
                {
                    Vector2 newPosition = position;
                    if ((i & 2) == 2) // top/bottom
                    {
                        newPosition.y += size * 0.25f;
                    }
                    else
                    {
                        newPosition.y -= size * 0.25f;
                    }
                    if ((i & 1) == 1) // left/right
                    {
                        newPosition.x += size * 0.25f;
                    }
                    else
                    {
                        newPosition.x -= size * 0.25f;
                    }

                    subNodes[i] = new QuadTreeNode<TType>(newPosition, size * 0.5f);
                    if (depth > 0 && subdivIndex == i)
                    {
                        subNodes[i].Subdivide(targetPosition, value, depth - 1);
                    }
                }
            }

            if (depth > 0)
            {
                subNodes[subdivIndex].Subdivide(targetPosition, value, depth - 1);
            }            
        }

        public bool IsLeaf()
        {
            return subNodes == null;
        }
    }

    private static int GetIndexOfPosition(Vector2 lookupPosition, Vector2 nodePosition)
    {
        int index = 0;

        index |= lookupPosition.y > nodePosition.y ? 2 : 0;
        index |= lookupPosition.x > nodePosition.x ? 1 : 0;

        return index;
    }

    public QuadTreeNode<TType> GetRoot()
    {
        return node;
    }
}

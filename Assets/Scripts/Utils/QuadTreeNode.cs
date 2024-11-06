using UnityEngine;
using System.Collections.Generic;

public class QuadtreeNode
{
    public Rect Bounds { get; private set; }
    public List<Unit> Units { get; private set; }
    public QuadtreeNode[] Children { get; private set; }
    public bool IsLeaf => Children == null;

    public QuadtreeNode(Rect bounds)
    {
        Bounds = bounds;
        Units = new List<Unit>();
        Children = null;
    }

    public void Subdivide()
    {
        float halfWidth = Bounds.width / 2f;
        float halfHeight = Bounds.height / 2f;
        Children = new QuadtreeNode[4];
        Children[0] = new QuadtreeNode(new Rect(Bounds.x, Bounds.y, halfWidth, halfHeight));
        Children[1] = new QuadtreeNode(new Rect(Bounds.x + halfWidth, Bounds.y, halfWidth, halfHeight));
        Children[2] = new QuadtreeNode(new Rect(Bounds.x, Bounds.y + halfHeight, halfWidth, halfHeight));
        Children[3] = new QuadtreeNode(new Rect(Bounds.x + halfWidth, Bounds.y + halfHeight, halfWidth, halfHeight));
    }

    public bool ChildrenAreEmpty()
    {
        // Check if all children are either null or contain no units
        foreach (var child in Children)
        {
            if (child != null && child.Units.Count > 0)
            {
                return false;
            }
        }
        return true;
    }

    public void ClearChildren()
    {
        // Clear references to child nodes to free up memory
        for (int i = 0; i < Children.Length; i++)
        {
            Children[i] = null;
        }
        Children = null;
    }

    public bool Contains(Vector3 position)
    {
        return Bounds.Contains(position);
    }
}
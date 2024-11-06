using UnityEngine;
using System.Collections.Generic;

public class Quadtree
{
    private QuadtreeNode root;
    private int maxUnitsPerNode;
    private int maxDepth;

    public Quadtree(Rect bounds, int maxUnitsPerNode, int maxDepth)
    {
        root = new QuadtreeNode(bounds);
        this.maxUnitsPerNode = maxUnitsPerNode;
        this.maxDepth = maxDepth;
    }

    public void Insert(Unit unit)
    {
        Insert(unit, root, 0);
    }

    private void Insert(Unit unit, QuadtreeNode node, int depth)
    {
        if (!node.Bounds.Contains(unit.transform.position))
        {
            return;
        }

        if (node.IsLeaf && (node.Units.Count < maxUnitsPerNode || depth >= maxDepth))
        {
            node.Units.Add(unit);
        }
        else
        {
            if (node.IsLeaf)
            {
                node.Subdivide();
                List<Unit> unitsToReinsert = new List<Unit>(node.Units);
                node.Units.Clear();
                foreach (var u in unitsToReinsert)
                {
                    Insert(u, node, depth + 1);
                }
            }

            foreach (var child in node.Children)
            {
                Insert(unit, child, depth + 1);
            }
        }
    }

    public void Remove(Unit unit)
    {
        Remove(unit, root);
    }

    private void Remove(Unit unit, QuadtreeNode node)
    {
        // Exit early if the unit's position is not within the node's bounds or if unit is null
        if (unit == null || !node.Bounds.Contains(unit.transform.position))
        {
            return;
        }

        // If this node is a leaf, remove the unit directly if it exists
        if (node.IsLeaf)
        {
            node.Units.Remove(unit);
        }
        else
        {
            // Recursively check child nodes
            foreach (var child in node.Children)
            {
                Remove(unit, child);
            }

            // Clean up empty children
            if (node.ChildrenAreEmpty())
            {
                node.ClearChildren();
            }
        }
    }

    public Unit FindClosest(Vector3 position)
    {
        return FindClosest(position, root, float.MaxValue, null);
    }

    private Unit FindClosest(Vector3 position, QuadtreeNode node, float closestDistance, Unit closestUnit)
    {
        if (!node.Bounds.Overlaps(new Rect(position.x - closestDistance, position.y - closestDistance, closestDistance * 2, closestDistance * 2)))
        {
            return closestUnit;
        }

        foreach (var unit in node.Units)
        {
            float distance = Vector3.Distance(position, unit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestUnit = unit;
            }
        }

        if (!node.IsLeaf)
        {
            foreach (var child in node.Children)
            {
                closestUnit = FindClosest(position, child, closestDistance, closestUnit);
            }
        }

        return closestUnit;
    }

    public List<Unit> FindUnitsInRange(Vector3 position, float range)
    {
        List<Unit> unitsInRange = new List<Unit>();
        FindUnitsInRange(position, range, root, unitsInRange);
        return unitsInRange;
    }

    private void FindUnitsInRange(Vector3 position, float range, QuadtreeNode node, List<Unit> unitsInRange)
    {
        float rangeSquared = range * range;

        // Circular range check based on center distance to node's bounds
        Vector2 circleCenter = new Vector2(position.x, position.z);
        float nodeRadius = Mathf.Sqrt(node.Bounds.width * node.Bounds.width + node.Bounds.height * node.Bounds.height) / 2;

        // Skip nodes entirely outside the circular range
        Vector2 nodeCenter = new Vector2(node.Bounds.center.x, node.Bounds.center.y);
        if (Vector2.SqrMagnitude(nodeCenter - circleCenter) > (range + nodeRadius) * (range + nodeRadius))
        {
            return;
        }

        foreach (var unit in node.Units)
        {
            // Get collider radius
            Collider collider = unit.GetComponent<Collider>();
            if (collider == null) continue;

            // Find the nearest point on the collider’s bounds to the center of the search range
            Vector3 closestPoint = collider.ClosestPoint(position);
            float distanceSquared = (position - closestPoint).sqrMagnitude;

            // If the closest point on the collider is within range, add the unit to the list
            if (distanceSquared <= rangeSquared)
            {
                unitsInRange.Add(unit);
            }
        }

        // Recursive search for child nodes
        if (!node.IsLeaf)
        {
            foreach (var child in node.Children)
            {
                FindUnitsInRange(position, range, child, unitsInRange);
            }
        }
    }

    public void UpdateUnitPosition(Unit unit, Vector3 oldPosition, Vector3 newPosition)
    {
        if (!root.Contains(oldPosition) || !root.Contains(newPosition))
        {
            Remove(unit);  // Remove the unit from the old position
            Insert(unit);  // Reinsert it at the new position
        }
    }

    public void DrawGizmos()
    {
        DrawGizmos(root);
    }

    private void DrawGizmos(QuadtreeNode node)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(node.Bounds.x + node.Bounds.width / 2, 0, node.Bounds.y + node.Bounds.height / 2), new Vector3(node.Bounds.width, 0, node.Bounds.height));

        if (!node.IsLeaf)
        {
            foreach (var child in node.Children)
            {
                DrawGizmos(child);
            }
        }
    }
}
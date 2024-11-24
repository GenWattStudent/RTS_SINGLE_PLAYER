using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuadTree
{
    private int MAX_OBJECTS = 7;
    private int MAX_LEVELS = 6;

    private int level;
    private Rect bounds;
    private List<Unit> objects;
    private QuadTree[] nodes;

    public QuadTree(int level, Rect bounds)
    {
        this.level = level;
        this.bounds = bounds;
        objects = new List<Unit>();
        nodes = new QuadTree[4];
    }

    public void Clear()
    {
        objects.Clear();

        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i].Clear();
                nodes[i] = null;
            }
        }
    }

    public void Split()
    {
        float subWidth = bounds.width / 2;
        float subHeight = bounds.height / 2;
        float x = bounds.x;
        float y = bounds.y;

        nodes[0] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight));
        nodes[1] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight));
        nodes[2] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight));
        nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight));
    }

    public void Insert(Unit obj)
    {
        if (nodes[0] != null)
        {
            int index = GetIndex(obj.transform.position);

            if (index != -1)
            {
                nodes[index].Insert(obj);
                return;
            }
        }

        objects.Add(obj);

        if (objects.Count > MAX_OBJECTS && level < MAX_LEVELS)
        {
            if (nodes[0] == null)
            {
                Split();
            }

            int i = 0;
            while (i < objects.Count)
            {
                int index = GetIndex(objects[i].transform.position);
                if (index != -1)
                {
                    nodes[index].Insert(objects[i]);
                    objects.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    public List<Unit> FindUnitsInRange(Vector3 position, float range)
    {
        List<Unit> found = new List<Unit>();
        FindUnitsInRange(position, range, found);
        return found;
    }

    private void FindUnitsInRange(Vector3 position, float range, List<Unit> found)
    {
        if (!bounds.Contains(position))
            return;

        foreach (Unit obj in objects)
        {
            if (Vector3.Distance(obj.transform.position, position) <= range)
            {
                found.Add(obj);
            }
        }

        if (nodes[0] != null)
        {
            for (int i = 0; i < 4; i++)
            {
                nodes[i].FindUnitsInRange(position, range, found);
            }
        }
    }

    public Unit FindClosestUnitInRange(Vector3 position, float range, TeamType team)
    {
        Unit closest = null;
        float minDistance = float.MaxValue;
        FindClosestUnitInRange(position, range, ref closest, ref minDistance, team);
        return closest;
    }

    private void FindClosestUnitInRange(Vector3 position, float range, ref Unit closest, ref float minDistance, TeamType team)
    {
        if (!bounds.Overlaps(new Rect(position.x - range, position.z - range, range * 2, range * 2)))
            return;

        foreach (Unit obj in objects)
        {
            if (obj.Damagable.teamType.Value != team)
            {
                if (obj == null)
                {
                    continue;
                }

                Vector3 closestPoint = GetClosestPointOnCollider(obj, position);
                float distance = Vector3.Distance(closestPoint, position);

                if (distance <= range && distance < minDistance)
                {
                    closest = obj;
                    minDistance = distance;
                }
            }
        }

        if (nodes[0] != null)
        {
            for (int i = 0; i < 4; i++)
            {
                nodes[i].FindClosestUnitInRange(position, range, ref closest, ref minDistance, team);
            }
        }
    }

    private Vector3 GetClosestPointOnCollider(Unit unit, Vector3 position)
    {
        Collider collider = unit.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.ClosestPoint(position);
        }
        return unit.transform.position;
    }

    public void Remove(Unit unit)
    {
        if (!bounds.Contains(unit.transform.position))
            return;

        if (objects.Contains(unit))
        {
            objects.Remove(unit);
            return;
        }

        if (nodes[0] != null)
        {
            int index = GetIndex(unit.transform.position);
            if (index != -1)
            {
                nodes[index].Remove(unit);
            }
        }
    }

    private int GetIndex(Vector3 position)
    {
        int index = -1;
        float verticalMidpoint = bounds.x + bounds.width / 2;
        float horizontalMidpoint = bounds.y + bounds.height / 2;

        bool topQuadrant = position.z < horizontalMidpoint && position.z >= bounds.y;
        bool bottomQuadrant = position.z >= horizontalMidpoint && position.z < bounds.y + bounds.height;

        if (position.x < verticalMidpoint && position.x >= bounds.x)
        {
            if (topQuadrant)
            {
                index = 1;
            }
            else if (bottomQuadrant)
            {
                index = 2;
            }
        }
        else if (position.x >= verticalMidpoint && position.x < bounds.x + bounds.width)
        {
            if (topQuadrant)
            {
                index = 0;
            }
            else if (bottomQuadrant)
            {
                index = 3;
            }
        }

        return index;
    }

    public void UpdateUnit(Dictionary<ulong, List<Unit>> units)
    {
        Clear();
        foreach (var unitList in units.Values)
        {
            foreach (var unit in unitList)
            {
                Insert(unit);
            }
        }
    }

    public void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(bounds.x + bounds.width / 2, 0, bounds.y + bounds.height / 2), new Vector3(bounds.width, 0, bounds.height));

#if UNITY_EDITOR
        Handles.Label(new Vector3(bounds.x + bounds.width / 2, 0, bounds.y + bounds.height / 2), objects.Count.ToString(), new GUIStyle() { normal = new GUIStyleState() { textColor = Color.red } });
#endif
        if (nodes[0] != null)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].DrawGizmos();
            }
        }
    }
}
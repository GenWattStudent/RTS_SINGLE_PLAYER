using System.Collections.Generic;
using UnityEngine;

public class QuadTree
{
    private int MAX_OBJECTS = 7;
    private int MAX_LEVELS = 5;

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

                float distance = Vector3.Distance(obj.transform.position, position);
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

    public void RemoveUnit(Unit unit)
    {
        if (nodes[0] != null)
        {
            int index = GetIndex(unit.transform.position);

            if (index != -1)
            {
                nodes[index].RemoveUnit(unit);
                return;
            }
        }

        objects.Remove(unit);
    }

    public void UpdateUnit(Unit unit)
    {
        RemoveUnit(unit);
        Insert(unit);
    }

    private int GetIndex(Vector3 position)
    {
        float subWidth = bounds.width / 2;
        float subHeight = bounds.height / 2;
        float x = bounds.x;
        float y = bounds.y;

        int index = -1;

        if (position.x <= x + subWidth)
        {
            if (position.z <= y + subHeight)
            {
                index = 1;
            }
            else
            {
                index = 2;
            }
        }
        else
        {
            if (position.z <= y + subHeight)
            {
                index = 0;
            }
            else
            {
                index = 3;
            }
        }

        return index;
    }

    public void DrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(new Vector3(bounds.x + bounds.width / 2, 0, bounds.y + bounds.height / 2), new Vector3(bounds.width, 0, bounds.height));

        if (nodes[0] != null)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].DrawGizmos();
            }
        }
    }
}
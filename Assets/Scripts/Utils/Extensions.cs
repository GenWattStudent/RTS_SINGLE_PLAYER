using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static void GetIndex<T>(this List<T> list, T item, out int index)
    {
        index = list.IndexOf(item);
    }
}

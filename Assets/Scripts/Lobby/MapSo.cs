using UnityEngine;

[CreateAssetMenu(fileName = "Map", menuName = "Map")]
public class MapSo : ScriptableObject
{
    public string MapName;
    // image of the map
    public Texture2D MapImage;
}

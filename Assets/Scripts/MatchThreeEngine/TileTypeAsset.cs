using UnityEngine;


[CreateAssetMenu(menuName = "Match 3 Engine/Tile Type Asset")]
public sealed class TileTypeAsset : ScriptableObject
{
    public int id;

    public int value;

    public bool isSpecial;

    public bool canBeSelected;

    public Sprite sprite;
}


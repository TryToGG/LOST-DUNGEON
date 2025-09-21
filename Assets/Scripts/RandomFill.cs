using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;  // 注意：需要在 Editor 里显示按钮

public class RandomFill : MonoBehaviour
{
    public Tilemap tilemap;      // 目标 Tilemap
    public TileBase[] tiles;     // 随机候选 Tile

    public void FillWholeMap()
    {
        if (tilemap == null || tiles.Length == 0) return;

        // 获取 Tilemap 的范围（包含所有格子）
        BoundsInt bounds = tilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
            // 只在空白格子填充（如果想覆盖现有格子，可以删掉 if 判断）
            if (!tilemap.HasTile(pos))
            {
                TileBase randomTile = tiles[Random.Range(0, tiles.Length)];
                tilemap.SetTile(pos, randomTile);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RandomFill))]
public class RandomFillEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RandomFill script = (RandomFill)target;
        if (GUILayout.Button("随机填充整个 Tilemap"))
        {
            script.FillWholeMap();
        }
    }
}
#endif

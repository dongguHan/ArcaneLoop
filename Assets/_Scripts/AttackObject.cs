using UnityEngine;
using UnityEngine.Tilemaps;

public class AttackObject : MonoBehaviour
{
    [HideInInspector] public int attackId;  // PoolManager∞° º≥¡§«ÿ¡‹
    public bool isBlackAttack;

    public Tilemap breakableTilemap;
    public Vector2 boxSize = new Vector2(1f, 1f);
    public LayerMask tilemapLayer;

    public void BreakTiles(Vector2 center, Vector2 dir)
    {
        var hits = Physics2D.OverlapBoxAll(center, boxSize, 0, tilemapLayer);

        foreach (var hit in hits)
        {
            Tilemap tilemap = hit.GetComponent<Tilemap>();
            if (tilemap == null) continue;

            Vector3Int cellPos = tilemap.WorldToCell(center);
            tilemap.SetTile(cellPos, null);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxSize);
    }
}

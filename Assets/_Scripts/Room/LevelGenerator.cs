using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public RoomDatabase roomDatabase;
    public Vector2 roomSize = new Vector2(16, 9);

    public int width = 13;
    public int height = 13;
    public int targetRooms = 10;

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        RandomRoomGenerator gen = new RandomRoomGenerator(width, height, targetRooms);
        Dictionary<Vector2Int, RandomRoomGenerator.Room> rooms = gen.Generate();

        foreach (var kv in rooms)
        {
            Vector2Int pos = kv.Key;
            int roomId = kv.Value.roomId;

            GameObject prefab = roomDatabase.GetRoomPrefab(roomId);
            if (prefab == null)
            {
                Debug.LogWarning($"No prefab for roomId {roomId}");
                continue;
            }

            Vector3 worldPos =
                new Vector3(pos.x * roomSize.x, pos.y * roomSize.y, 0);

            Instantiate(prefab, worldPos, Quaternion.identity, transform);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class RandomRoomGenerator
{
    public struct Room
    {
        public int x;
        public int y;
        public bool start;
        public bool end;
        public bool boss;

        public int roomId; // ScriptableObject RoomDatabase에서 사용할 ID
    }

    private int width;
    private int height;
    private int targetRooms;

    private System.Random rng = new System.Random();

    public RandomRoomGenerator(int width, int height, int targetRooms)
    {
        this.width = width;
        this.height = height;
        this.targetRooms = targetRooms;
    }

    public Dictionary<Vector2Int, Room> Generate()
    {
        Dictionary<Vector2Int, Room> rooms = new Dictionary<Vector2Int, Room>();

        int startX = width / 2;
        int startY = height / 2;

        // Start Room
        rooms[new Vector2Int(startX, startY)] = new Room
        {
            x = startX,
            y = startY,
            start = true,
            end = false,
            boss = false,
            roomId = 1 // start room id
        };

        List<Vector2Int> frontier = new List<Vector2Int>
        {
            new Vector2Int(startX, startY)
        };

        int[] dx = { 0, 1, 0, -1 };
        int[] dy = { -1, 0, 1, 0 };

        // === ROOM GENERATION ===
        while (rooms.Count < targetRooms && frontier.Count > 0)
        {
            int idx = rng.Next(frontier.Count);
            Vector2Int current = frontier[idx];

            List<int> dirs = new List<int> { 0, 1, 2, 3 };
            Shuffle(dirs);

            int newBranches = 0;

            foreach (int d in dirs)
            {
                if (rooms.Count >= targetRooms) break;

                // 50% prune if already branched at least once
                if (newBranches > 0 && rng.NextDouble() < 0.5) continue;

                int nx = current.x + dx[d];
                int ny = current.y + dy[d];

                // Out of bounds
                if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                    continue;

                Vector2Int np = new Vector2Int(nx, ny);

                // Already exists
                if (rooms.ContainsKey(np))
                    continue;

                // Create room
                rooms[np] = new Room
                {
                    x = nx,
                    y = ny,
                    start = false,
                    end = false,
                    boss = false,
                    roomId = 2 // 일반 방 id (이후 확장 가능)
                };

                frontier.Add(np);
                newBranches++;
            }

            if (newBranches == 0)
                frontier.RemoveAt(idx);
        }

        // === BFS to find furthest room (boss) ===
        Dictionary<Vector2Int, int> dist = new Dictionary<Vector2Int, int>();
        foreach (var kv in rooms)
            dist[kv.Key] = int.MaxValue;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        dist[new Vector2Int(startX, startY)] = 0;
        queue.Enqueue(new Vector2Int(startX, startY));

        while (queue.Count > 0)
        {
            Vector2Int cur = queue.Dequeue();
            for (int d = 0; d < 4; d++)
            {
                int nx = cur.x + dx[d];
                int ny = cur.y + dy[d];
                Vector2Int np = new Vector2Int(nx, ny);

                if (!rooms.ContainsKey(np)) continue;
                if (dist[np] != int.MaxValue) continue;

                dist[np] = dist[cur] + 1;
                queue.Enqueue(np);
            }
        }

        // Find max-dist room
        Vector2Int bossRoom = new Vector2Int(startX, startY);
        int maxDist = 0;

        foreach (var kv in dist)
        {
            Room r = rooms[kv.Key];
            if (r.start) continue;

            if (kv.Value > maxDist)
            {
                maxDist = kv.Value;
                bossRoom = kv.Key;
            }
        }

        // Assign boss
        Room br = rooms[bossRoom];
        br.boss = true;
        br.roomId = 3; // boss room id
        rooms[bossRoom] = br;

        // === Leaf rooms (end rooms) ===
        foreach (var key in new List<Vector2Int>(rooms.Keys))
        {
            Room r = rooms[key];
            if (r.start || r.boss) continue;

            int exits = 0;
            for (int d = 0; d < 4; d++)
            {
                Vector2Int np = new Vector2Int(r.x + dx[d], r.y + dy[d]);
                if (rooms.ContainsKey(np)) exits++;
            }

            if (exits == 1)
            {
                r.end = true;
                r.roomId = 4; // end room id
            }

            rooms[key] = r;
        }

        return rooms;
    }

    private void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = rng.Next(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}

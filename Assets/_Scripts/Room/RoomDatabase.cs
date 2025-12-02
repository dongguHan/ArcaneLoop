using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Map/Room Database")]
public class RoomDatabase : ScriptableObject
{
    [Serializable]
    public class RoomEntry
    {
        public int id;
        public GameObject prefab;
    }

    public List<RoomEntry> rooms = new List<RoomEntry>();

    private Dictionary<int, GameObject> _lookup;

    // Build lookup dictionary on first use
    public GameObject GetRoomPrefab(int id)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<int, GameObject>();
            foreach (var entry in rooms)
            {
                if (entry.prefab == null) continue;

                if (_lookup.ContainsKey(entry.id))
                {
                    Debug.LogWarning($"Duplicate room id {entry.id} in RoomDatabase.");
                    continue;
                }

                _lookup.Add(entry.id, entry.prefab);
            }
        }

        if (_lookup.TryGetValue(id, out var prefab))
        {
            return prefab;
        }

        Debug.LogWarning($"No room prefab with id {id} in RoomDatabase.");
        return null;
    }
}
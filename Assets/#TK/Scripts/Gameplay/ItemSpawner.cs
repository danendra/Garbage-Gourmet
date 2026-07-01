using System.Collections.Generic;
using UnityEngine;

namespace TK.Gameplay
{

    public class ItemSpawner : MonoBehaviour
    {
        [Header("Spawn Amount")]
        [SerializeField] private int totalItems = 40;

        [Header("Spacing")]
        [SerializeField] private float spacing = 2.5f;
        [SerializeField] private int maxAttempts = 150;

        [Header("Spawn Bounds")]
        [SerializeField] private Transform xmin;
        [SerializeField] private Transform xmax;
        [SerializeField] private Transform topY;
        [SerializeField] private Transform bottomY;

        [Header("Item Pools")]
        [SerializeField] private GameObject[] commonPool;
        [SerializeField] private GameObject[] uncommonPool;
        [SerializeField] private GameObject[] rarePool;
        [SerializeField] private GameObject[] epicPool;
        [SerializeField] private GameObject[] trashPool;

        private List<Vector2> usedPositions = new List<Vector2>();

        void Start()
        {
            SpawnAllItems();
        }

        // =====================================================
        // MAIN SPAWN
        // =====================================================

        private void SpawnAllItems()
        {
            usedPositions.Clear();

            for (int i = 0; i < totalItems; i++)
            {
                Vector2 spawnPos;

                if (!TryFindSpawnPosition(out spawnPos))
                    continue;

                GameObject prefabToSpawn = GetPrefabForDepth(spawnPos.y);

                if (prefabToSpawn == null)
                    continue;
                float randomZ = Random.Range(-18f, 18f);
                Quaternion rotation = Quaternion.Euler(0f, 0f, randomZ);

                Instantiate(prefabToSpawn, spawnPos, rotation);

                usedPositions.Add(spawnPos);
            }
        }

        // =====================================================
        // POSITION CHECK
        // =====================================================

        private bool TryFindSpawnPosition(out Vector2 result)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                float x = Random.Range(xmin.position.x, xmax.position.x);
                float y = Random.Range(topY.position.y, bottomY.position.y);

                Vector2 candidate = new Vector2(x, y);

                bool valid = true;

                foreach (Vector2 used in usedPositions)
                {
                    float dynamicSpacing = GetSpacingForDepth(candidate.y);
                    if (Vector2.Distance(candidate, used) < dynamicSpacing)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    result = candidate;
                    return true;
                }
            }

            result = Vector2.zero;
            return false;
        }

        // =====================================================
        // DEPTH LOOT LOGIC
        // =====================================================

        private GameObject GetPrefabForDepth(float y)
        {
            float top = topY.position.y;
            float bottom = bottomY.position.y;

            float t = Mathf.InverseLerp(top, bottom, y);
            // t = 0 near top
            // t = 1 near bottom

            int roll = Random.Range(0, 100);

            // TOP ZONE
            if (t < 0.25f)
            {
                if (roll < 20) return GetRandomFromPool(trashPool);
                return GetRandomFromPool(commonPool);
            }

            // MID ZONE
            if (t < 0.55f)
            {
                if (roll < 20) return GetRandomFromPool(trashPool);
                if (roll < 65) return GetRandomFromPool(commonPool);
                return GetRandomFromPool(uncommonPool);
            }

            // DEEP ZONE
            if (t < 0.80f)
            {
                if (roll < 15) return GetRandomFromPool(trashPool);
                if (roll < 45) return GetRandomFromPool(uncommonPool);
                return GetRandomFromPool(rarePool);
            }

            // BOTTOM ZONE
            if (roll < 10) return GetRandomFromPool(trashPool);
            if (roll < 35) return GetRandomFromPool(rarePool);
            return GetRandomFromPool(epicPool);
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private GameObject GetRandomFromPool(GameObject[] pool)
        {
            if (pool == null || pool.Length == 0)
                return null;

            return pool[Random.Range(0, pool.Length)];
        }

        private float GetSpacingForDepth(float y)
        {
            float top = topY.position.y;
            float bottom = bottomY.position.y;

            float t = Mathf.InverseLerp(top, bottom, y);

            // top = full spacing
            // bottom = tighter spacing

            return Mathf.Lerp(spacing, spacing * 0.7f, t);
        }
    }
}
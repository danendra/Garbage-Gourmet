using System.Collections.Generic;
using UnityEngine;

using Anoa.Module;

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
        [SerializeField] private PoolerContainer _poolCommon;
        [SerializeField] private PoolerContainer _poolUncommon;
        [SerializeField] private PoolerContainer _poolRare;        
        [SerializeField] private PoolerContainer _poolBad;

        private List<Vector2> _usedPositions = new List<Vector2>();

        void Start()
        {
            SpawnAllItems();
        }

        // =====================================================
        // MAIN SPAWN
        // =====================================================

        private void SpawnAllItems()
        {
            _usedPositions.Clear();

            for (int i = 0; i < totalItems; i++)
            {
                Vector2 _spawnPos;

                if (!TryFindSpawnPosition(out _spawnPos))
                    continue;

                Collectible _collectibleToSpawn = GetPrefabForDepth(_spawnPos.y);

                if (_collectibleToSpawn == null)
                    continue;

                float _randomZ = Random.Range(-18f, 18f);
                Quaternion _rotation = Quaternion.Euler(0f, 0f, _randomZ);

                _collectibleToSpawn.Initialize(_spawnPos, _rotation);                

                _usedPositions.Add(_spawnPos);
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

                foreach (Vector2 used in _usedPositions)
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

        private Collectible GetPrefabForDepth(float y)
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
                if (roll < 20) return _poolBad.Pop<Collectible>(true);
                return _poolCommon.Pop<Collectible>(true);
            }

            // MID ZONE
            if (t < 0.55f)
            {
                if (roll < 20) return _poolBad.Pop<Collectible>(true);
                if (roll < 65) return _poolCommon.Pop<Collectible>(true);
                return _poolUncommon.Pop<Collectible>(true);
            }

            // DEEP ZONE
            if (t < 0.80f)
            {
                if (roll < 15) return _poolBad.Pop<Collectible>(true);
                if (roll < 45) return _poolUncommon.Pop<Collectible>(true);
                return _poolRare.Pop<Collectible>(true);
            }

            // BOTTOM ZONE
            if (roll < 10) return _poolBad.Pop<Collectible>(true);
            if (roll < 35) return _poolRare.Pop<Collectible>(true);
            return _poolBad.Pop<Collectible>(true);
        }

        // =====================================================
        // HELPERS
        // =====================================================

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
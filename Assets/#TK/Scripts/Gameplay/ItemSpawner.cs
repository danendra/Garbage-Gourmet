using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

using Anoa.Module;

namespace TK.Gameplay
{
    [Serializable]
    public class WeightedPoolEntry
    {
        public PoolerObject Prefab;
        public float Weight;
    }

    [Serializable]
    public class SubLevelPool
    {
        [Range(0f, 1f)] public float MinDepth;   
        [Range(0f, 1f)] public float MaxDepth;  
        public PoolerContainer Pool;
        public int Count;
        public WeightedPoolEntry[] Entries;
        [Range(0f, 100f)] public float TrashWeight;
    }

    public class ItemSpawner : MonoBehaviour
    {
        [Header("Dynamic Spawning")]
        [SerializeField] private int _itemsPerChunk = 3;
        [SerializeField] private float _chunkHeight = 6f;
        [SerializeField] private float _spawnLookaheadDistance = 20f;
        [SerializeField] private float _despawnDistance = 25f;

        [Header("Spacing")]
        [SerializeField] private float _spacing = 2.5f;
        [SerializeField] private int _maxAttempts = 150;

        [Header("Spawn Bounds")]
        [SerializeField] private Transform _xmin;
        [SerializeField] private Transform _xmax;
        [SerializeField] private Transform _topY;
        [SerializeField] private Transform _bottomY;

        [Header("Sub-Level Pools")]
        [SerializeField] private SubLevelPool[] _subLevels;

        [Header("Trash")]
        [SerializeField] private PoolerContainer _poolTrash;

        [Header("Event Items")]
        [SerializeField] private CollectibleController _eventItemPrefab;

        [Header("References")]
        [SerializeField] private HandMovement _hand;


        // ── Runtime ──────────────────────────────────────────────────────────
        private float _spawnHorizonY;
        private bool  _isInitialized;
        private bool  _eventItemSpawned;
        private int[] _remainingCounts;

        private int TotalItems
        {
            get
            {
                if (_subLevels == null) return 0;
                int total = 0;
                foreach (SubLevelPool sl in _subLevels) total += sl.Count;
                return total;
            }
        }

        private float _cachedTop;
        private float _cachedBottom;
        private float _cachedXMin;
        private float _cachedXMax;

        private List<CollectibleController> _activeItems;
        private Dictionary<Vector2Int, List<Vector2>> _spatialGrid;
        private float _cellSize;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            CacheBounds();
            ResetState();
        }

        // =====================================================
        // PUBLIC API
        // =====================================================

        public void TickSpawn(float playerY)
        {
            if (!_isInitialized) return;

            // if (!_eventItemSpawned && _hand.GetDepth() >= _hand.GetMaxArmReach() - _chunkHeight)
            // {
            //     _eventItemSpawned = true;
            //     SpawnEventItem();
            // }

            AdvanceHorizon(playerY);
        }

        public void Respawn()
        {
            ReturnAllToPool();
            ResetState();
        }

        // =====================================================
        // STATE RESET
        // =====================================================

        private void ResetState()
        {
            // Rebuild the per-level budget array from Inspector data
            if (_subLevels != null && _subLevels.Length > 0)
            {
                _remainingCounts = new int[_subLevels.Length];
                for (int i = 0; i < _subLevels.Length; i++)
                    _remainingCounts[i] = _subLevels[i].Count;
            }
            else
            {
                _remainingCounts = Array.Empty<int>();
            }

            _spawnHorizonY    = _cachedTop;
            _eventItemSpawned = false;

            if (_activeItems == null)
                _activeItems = new List<CollectibleController>(TotalItems);

            if (_spatialGrid == null)
                _spatialGrid = new Dictionary<Vector2Int, List<Vector2>>();
            else
                _spatialGrid.Clear();

            _isInitialized = true;
        }

        // =====================================================
        // POOL RETURN
        // =====================================================

        private void ReturnAllToPool()
        {
            if (_activeItems == null) return;

            foreach (CollectibleController item in _activeItems)
            {
                if (item != null && item.gameObject.activeSelf)
                    item.gameObject.SetActive(false);
            }

            _activeItems.Clear();
        }

        // =====================================================
        // CACHED BOUNDS
        // =====================================================

        private void CacheBounds()
        {
            _cachedTop    = _topY.position.y;
            _cachedBottom = _bottomY.position.y;
            _cachedXMin   = _xmin.position.x;
            _cachedXMax   = _xmax.position.x;

            // Cell size = tightest spacing (bottom zone = spacing * 0.7)
            // so a 3×3 neighbourhood always covers the full exclusion radius.
            _cellSize = _spacing * 0.7f;
        }

        // =====================================================
        // DYNAMIC SPAWN LOGIC
        // =====================================================

        private void AdvanceHorizon(float playerY)
        {
            while (HasRemainingBudget()
                   && _spawnHorizonY > _cachedBottom
                   && playerY - _spawnLookaheadDistance < _spawnHorizonY)
            {
                float chunkTop    = _spawnHorizonY;
                float chunkBottom = Mathf.Max(_spawnHorizonY - _chunkHeight, _cachedBottom);

                SpawnChunk(chunkTop, chunkBottom);
                _spawnHorizonY = chunkBottom;
            }
        }

        private void SpawnChunk(float chunkTop, float chunkBottom)
        {
            for (int i = 0; i < _itemsPerChunk; i++)
            {
                if (!HasRemainingBudget()) break;

                if (!TryFindSpawnPosition(chunkTop, chunkBottom, out Vector2 spawnPos))
                    continue;

                // t = 0 → top of play area   |   t = 1 → bottom of play area
                float t = Mathf.InverseLerp(_cachedTop, _cachedBottom, spawnPos.y);

                float      randomZ = Random.Range(-18f, 18f);
                Quaternion rot     = Quaternion.Euler(0f, 0f, randomZ);
                int levelIndex = FindSubLevelIndex(t);

                float trashChance = levelIndex >= 0 ? _subLevels[levelIndex].TrashWeight : 0f;

                if (Random.Range(0f, 100f) < trashChance)
                {
                    SpawnTrash(spawnPos, rot);
                }
                else
                {
                    CollectibleController collectible = levelIndex >= 0
                        ? PopWeighted(levelIndex) ?? TryPopAny()
                        : TryPopAny();

                    if (collectible == null) break;

                    collectible.Initialize(spawnPos, rot);
                    _activeItems.Add(collectible);
                }

                AddToGrid(spawnPos);
            }
        }

        // =====================================================
        // POSITION CHECK  (Spatial grid — O(neighbours) per attempt)
        // =====================================================

        private bool TryFindSpawnPosition(float yTop, float yBottom, out Vector2 result)
        {
            for (int attempt = 0; attempt < _maxAttempts; attempt++)
            {
                float x = Random.Range(_cachedXMin, _cachedXMax);
                float y = Random.Range(yBottom, yTop);

                Vector2 candidate    = new Vector2(x, y);
                float dynamicSpacing = GetSpacingForDepth(y);

                if (IsPositionValid(candidate, dynamicSpacing))
                {
                    result = candidate;
                    return true;
                }
            }

            result = Vector2.zero;
            return false;
        }

        // Checks only the 3×3 grid cells surrounding the candidate
        private bool IsPositionValid(Vector2 candidate, float minDist)
        {
            Vector2Int centerCell = WorldToCell(candidate);

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int neighbourCell = new Vector2Int(
                        centerCell.x + dx,
                        centerCell.y + dy
                    );

                    if (!_spatialGrid.TryGetValue(neighbourCell, out List<Vector2> positions))
                        continue;

                    foreach (Vector2 pos in positions)
                    {
                        if (Vector2.Distance(candidate, pos) < minDist)
                            return false;
                    }
                }
            }

            return true;
        }

        private void AddToGrid(Vector2 position)
        {
            Vector2Int cell = WorldToCell(position);

            if (!_spatialGrid.TryGetValue(cell, out List<Vector2> list))
            {
                list = new List<Vector2>();
                _spatialGrid[cell] = list;
            }

            list.Add(position);
        }

        private Vector2Int WorldToCell(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / _cellSize),
                Mathf.FloorToInt(worldPos.y / _cellSize)
            );
        }

        // =====================================================
        // DEPTH LOOT LOGIC
        // =====================================================

        private int FindSubLevelIndex(float t)
        {
            if (_subLevels == null) return -1;

            for (int i = 0; i < _subLevels.Length; i++)
            {
                if (t >= _subLevels[i].MinDepth && t <= _subLevels[i].MaxDepth)
                    return i;
            }

            return -1;
        }

        private CollectibleController PopWeighted(int levelIndex)
        {
            if (_remainingCounts[levelIndex] <= 0) return null;

            SubLevelPool        level   = _subLevels[levelIndex];
            WeightedPoolEntry[] entries = level.Entries;

            if (entries == null || entries.Length == 0) return null;

            // Sum all weights in this table
            float totalWeight = 0f;
            foreach (WeightedPoolEntry e in entries)
                totalWeight += e.Weight;

            if (totalWeight <= 0f) return null;

            // Weighted roll — works with any float weights, not just 0–100
            float roll        = Random.Range(0f, totalWeight);
            float accumulated = 0f;

            foreach (WeightedPoolEntry e in entries)
            {
                accumulated += e.Weight;
                if (roll < accumulated)
                {
                    if (e.Prefab == null) return null;

                    // Use the prefab's own name as the pool key — no string typing needed
                    CollectibleController item =
                        level.Pool.Pop<CollectibleController>(e.Prefab.name);

                    if (item != null)
                        _remainingCounts[levelIndex]--;

                    return item;
                }
            }

            return null;
        }

        // =====================================================
        // POP HELPERS
        // =====================================================

        private CollectibleController TryPopAny()
        {
            for (int i = 0; i < _subLevels.Length; i++)
            {
                if (_remainingCounts[i] <= 0) continue;

                CollectibleController item = PopWeighted(i);
                if (item != null) return item;
            }

            return null;
        }

        private bool HasRemainingBudget()
        {
            if (_remainingCounts == null) return false;

            foreach (int remaining in _remainingCounts)
                if (remaining > 0) return true;

            return false;
        }

        // =====================================================
        // TRASH SPAWNING
        // =====================================================

        private void SpawnTrash(Vector2 pos, Quaternion rot)
        {
            GameObject _objTrash = _poolTrash.Pop(true);
            if (_objTrash == null) return;

            _objTrash.SetActive(true);
            _objTrash.transform.position = new Vector3(pos.x, pos.y, 0f);
            _objTrash.transform.rotation = rot;
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private float GetSpacingForDepth(float y)
        {
            // Uses cached bounds — no Transform property access in hot path
            float t = Mathf.InverseLerp(_cachedTop, _cachedBottom, y);

            // top = full spacing  |  bottom = tighter spacing
            return Mathf.Lerp(_spacing, _spacing * 0.7f, t);
        }

        // =====================================================
        // EVENT SPAWNING
        // =====================================================

        public void SpawnEventItem()
        {
            if (_eventItemPrefab == null) return;

            // Target Y = just past the player's arm reach limit
            float targetY = _hand.transform.position.y - _spawnLookaheadDistance;

            float yTop    = targetY + _spacing;
            float yBottom = targetY - _spacing;

            if (!TryFindSpawnPosition(yTop, yBottom, out Vector2 spawnPos))
            {
                Debug.LogWarning("SpawnEventItem: no empty space found near arm limit — skipping spawn.");
                return;
            }

            Quaternion rot = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));

            CollectibleController instance = Instantiate(_eventItemPrefab);
            instance.Initialize(spawnPos, rot);

            Debug.Log($"Spawned event item at {spawnPos}");
        }
    }
}
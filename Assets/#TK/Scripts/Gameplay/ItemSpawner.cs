using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Anoa.Module;

namespace TK.Gameplay
{
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Dynamic Spawning")]
        [SerializeField] private int   _itemsPerChunk          = 3;
        [SerializeField] private float _chunkHeight            = 6f;
        [SerializeField] private float _spawnLookaheadDistance = 20f;
        [SerializeField] private float _despawnDistance        = 25f;

        [Header("Spacing")]
        [SerializeField] private float spacing    = 2.5f;
        [SerializeField] private int   maxAttempts = 150;

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

        [Header("Rarity Counts")]
        [SerializeField] private int _commonCount   = 18;
        [SerializeField] private int _uncommonCount = 10;
        [SerializeField] private int _rareCount     = 7;
        [SerializeField] private int _badCount      = 5;

        [Header("Event Items")]
        [SerializeField] private CollectibleController _eventItemPrefab;

        [Header("References")]
        [SerializeField] private PlayerMovement player;

        private float _spawnHorizonY;
        private bool  _isInitialized;
        private bool  _eventItemSpawned;

        private int _remainingCommon;
        private int _remainingUncommon;
        private int _remainingRare;
        private int _remainingBad;
        private int TotalItems => _commonCount + _uncommonCount + _rareCount + _badCount;

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

            // Bug 4 fixed: restore despawn so the pool keeps recycling
            DespawnFarItems(playerY);

            // Bug 1 fixed: one-time threshold cross instead of exact float equality
            if (!_eventItemSpawned && player.DistanceTravelled >= player.MaximumArmReach - _chunkHeight)
            {
                _eventItemSpawned = true;
                SpawnEventItem();
            }

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
            _remainingCommon   = _commonCount;
            _remainingUncommon = _uncommonCount;
            _remainingRare     = _rareCount;
            _remainingBad      = _badCount;

            // Horizon starts at the very top; chunks advance downward
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
            _cachedTop    = topY.position.y;
            _cachedBottom = bottomY.position.y;
            _cachedXMin   = xmin.position.x;
            _cachedXMax   = xmax.position.x;

            // Cell size = tightest spacing (bottom zone = spacing * 0.7)
            // so a 3×3 neighbourhood always covers the full exclusion radius.
            _cellSize = spacing * 0.7f;
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

                float t = Mathf.InverseLerp(_cachedTop, _cachedBottom, spawnPos.y);

                CollectibleController collectible = GetPoolForDepth(t);
                if (collectible == null) break;

                float randomZ  = Random.Range(-18f, 18f);
                Quaternion rot = Quaternion.Euler(0f, 0f, randomZ);

                collectible.Initialize(spawnPos, rot);

                AddToGrid(spawnPos);
                _activeItems.Add(collectible);
            }
        }

        // =====================================================
        // DESPAWN
        // =====================================================

        // (INI GA KEpAkE, ga di apus incase butuh)
        private void DespawnFarItems(float playerY)
        {
            for (int i = _activeItems.Count - 1; i >= 0; i--)
            {
                CollectibleController item = _activeItems[i];

                if (item == null || !item.gameObject.activeSelf)
                {
                    _activeItems.RemoveAt(i);
                    continue;
                }

                if (item.transform.position.y > playerY + _despawnDistance)
                {
                    item.gameObject.SetActive(false);
                    _activeItems.RemoveAt(i);
                }
            }
        }

        // =====================================================
        // POSITION CHECK  (Spatial grid — O(neighbours) per attempt)
        // =====================================================

        private bool TryFindSpawnPosition(float yTop, float yBottom, out Vector2 result)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
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

        // t = 0 bottom ; t = 1 top
        private CollectibleController GetPoolForDepth(float t)
        {
            int roll = Random.Range(0, 100);

            // TOP ZONE
            if (t < 0.25f)
            {
                if (roll < 20)
                    return TryPop(_poolBad, ref _remainingBad)
                        ?? TryPop(_poolCommon, ref _remainingCommon)
                        ?? TryPopAny();

                return TryPop(_poolCommon, ref _remainingCommon)
                    ?? TryPop(_poolBad, ref _remainingBad)
                    ?? TryPopAny();
            }

            // MID ZONE
            if (t < 0.55f)
            {
                if (roll < 20)
                    return TryPop(_poolBad, ref _remainingBad)
                        ?? TryPop(_poolCommon, ref _remainingCommon)
                        ?? TryPopAny();

                if (roll < 65)
                    return TryPop(_poolCommon, ref _remainingCommon)
                        ?? TryPop(_poolUncommon, ref _remainingUncommon)
                        ?? TryPopAny();

                return TryPop(_poolUncommon, ref _remainingUncommon)
                    ?? TryPop(_poolCommon, ref _remainingCommon)
                    ?? TryPopAny();
            }

            // DEEP ZONE
            if (t < 0.80f)
            {
                if (roll < 15)
                    return TryPop(_poolBad, ref _remainingBad)
                        ?? TryPop(_poolUncommon, ref _remainingUncommon)
                        ?? TryPopAny();

                if (roll < 45)
                    return TryPop(_poolUncommon, ref _remainingUncommon)
                        ?? TryPop(_poolRare, ref _remainingRare)
                        ?? TryPopAny();

                return TryPop(_poolRare, ref _remainingRare)
                    ?? TryPop(_poolUncommon, ref _remainingUncommon)
                    ?? TryPopAny();
            }

            // BOTTOM ZONE
            if (roll < 10)
                return TryPop(_poolBad, ref _remainingBad)
                    ?? TryPop(_poolRare, ref _remainingRare)
                    ?? TryPopAny();

            return TryPop(_poolRare, ref _remainingRare)
                ?? TryPop(_poolUncommon, ref _remainingUncommon)
                ?? TryPopAny();
        }

        // =====================================================
        // POP HELPERS
        // =====================================================

        private CollectibleController TryPop(PoolerContainer pool, ref int remaining)
        {
            if (remaining <= 0) return null;

            CollectibleController item = pool.Pop<CollectibleController>(true);
            if (item != null) remaining--;

            return item;
        }

        // Nek entek pool e
        private CollectibleController TryPopAny()
        {
            return TryPop(_poolCommon,   ref _remainingCommon)
                ?? TryPop(_poolUncommon, ref _remainingUncommon)
                ?? TryPop(_poolRare,     ref _remainingRare)
                ?? TryPop(_poolBad,      ref _remainingBad);
        }

        private bool HasRemainingBudget()
        {
            return _remainingCommon   > 0
                || _remainingUncommon > 0
                || _remainingRare     > 0
                || _remainingBad      > 0;
        }

        // =====================================================
        // HELPERS
        // =====================================================

        private float GetSpacingForDepth(float y)
        {
            // Uses cached bounds — no Transform property access in hot path
            float t = Mathf.InverseLerp(_cachedTop, _cachedBottom, y);

            // top = full spacing  |  bottom = tighter spacing
            return Mathf.Lerp(spacing, spacing * 0.7f, t);
        }

        // =====================================================
        // EVENT SPAWNING
        // =====================================================

        public void SpawnEventItem()
        {
            if (_eventItemPrefab == null) return;

            float targetY = player.transform.position.y - _spawnLookaheadDistance;

            float yTop    = targetY + spacing;
            float yBottom = targetY - spacing;

            Vector2 spawnPos = new Vector2(
                Random.Range(_cachedXMin, _cachedXMax),
                Random.Range(yBottom, yTop)
            );

            Quaternion rot = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));

            CollectibleController instance = Instantiate(_eventItemPrefab);
            instance.Initialize(spawnPos, rot);

            Debug.Log($"Spawned event item at {spawnPos}");
        }
    }
}
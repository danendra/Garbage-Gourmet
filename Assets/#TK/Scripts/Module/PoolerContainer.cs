using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anoa.Module
{
    public class PoolerContainer : MonoBehaviour
    {
        [SerializeField] protected PoolerObject[] poolPrefabs;
        [SerializeField] protected Transform parent;

        [SerializeField] protected int poolSize = 5;

        protected List<string> listStrKey = new List<string>();
        protected Dictionary<string, Queue<PoolerObject>> dictPool = new Dictionary<string, Queue<PoolerObject>>();
        protected Dictionary<string, PoolerObject> dictPrefabs = new Dictionary<string, PoolerObject>();

        // Start is called before the first frame update
        void Awake()
        {
            if (!parent)
                parent = transform;

            FillPool();
        }

        void Start()
        {

        }

        protected virtual void FillPool()
        {
            // Initialize the dictionary for each prefab
            foreach (PoolerObject _poolPrefab in poolPrefabs)
            {
                listStrKey.Add(_poolPrefab.name);
                dictPool.Add(_poolPrefab.name, new Queue<PoolerObject>());
                dictPrefabs.Add(_poolPrefab.name, _poolPrefab);
            }

            StartCoroutine(IEFillPool());
        }

        protected IEnumerator IEFillPool()
        {
            foreach (PoolerObject _pool in poolPrefabs)
            {
                for (int i = 0; i < poolSize; i++)
                {
                    CreateNewObject(_pool);

                    yield return new WaitForEndOfFrame();
                }
            }
        }

        public PoolerObject FillPool(PoolerObject _pool)
        {
            if (!listStrKey.Contains(_pool.name))
            {
                PoolerObject _poolObj = null;

                listStrKey.Add(_pool.name);
                dictPool.Add(_pool.name, new Queue<PoolerObject>());
                dictPrefabs.Add(_pool.name, _pool);

                for (int i = 0; i < poolSize; i++)
                {
                    _poolObj = CreateNewObject(_pool);
                }

                return _poolObj;
            }

            return null;
        }

        public PoolerObject CreateNewObject(PoolerObject _pool)
        {
            PoolerObject _poolTemp = Instantiate(_pool, parent);
            _poolTemp.name = _pool.name;
            _poolTemp.Init(this);
            _poolTemp.gameObject.SetActive(false);

            return _poolTemp;
        }

        public virtual T Pop<T>(bool randomize = false) where T : Component
        {
            GameObject _poolSpawn = Pop(randomize);

            if (_poolSpawn)
                return _poolSpawn.GetComponent<T>();
            else
                return null;
        }

        public virtual GameObject Pop(bool _isRandomize = false)
        {
            if (_isRandomize)
            {
                return Pop(listStrKey[Random.Range(0, listStrKey.Count)]);
            }
            else
            {
                for (int i = 0; i < listStrKey.Count; i++)
                {
                    if (Check(listStrKey[i]))
                    {
                        return Pop(listStrKey[i]);
                    }
                }
            }

            return Pop(true);
        }

        public virtual T Pop<T>(string _strName) where T : Component
        {
            GameObject _poolSpawn = Pop(_strName);

            if (_poolSpawn)
                return _poolSpawn.GetComponent<T>();
            else
                return null;
        }

        public virtual GameObject Pop(string _strName)
        {
            if (!dictPool.ContainsKey(_strName))
            {
                Debug.LogError($"Prefab {_strName} is not registered in the pool!");
                return null;
            }

            Queue<PoolerObject> _qPool = dictPool[_strName];

            PoolerObject _poolSpawn;
            
            if (_qPool.Count == 0)
            {
                PoolerObject _poolOri = dictPrefabs[_strName];

                if (_poolOri)
                {                    
                    CreateNewObject(_poolOri);
                }
                else
                {
                    Debug.LogError($"Prefab {_strName} is not registered in the pool!");
                    return null;
                }
            }

            _poolSpawn = _qPool.Dequeue();
            _poolSpawn.Init(this);

            return _poolSpawn.gameObject;
        }

        public virtual bool Check(string _strName)
        {
            return dictPool.ContainsKey(_strName);
        }

        public virtual void ReturnToPool(PoolerObject _pooler)
        {
            if (dictPool.ContainsKey(_pooler.name))
            {
                dictPool[_pooler.name].Enqueue(_pooler);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
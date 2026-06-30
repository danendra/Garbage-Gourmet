using UnityEngine;

namespace Anoa.Module
{
    public class PoolerObject : MonoBehaviour
    {
        public PoolerContainer container { get; protected set; }

        public virtual void Init(PoolerContainer pool)
        {
            container = pool;
            gameObject.SetActive(true);
        }

        protected virtual void OnDisable()
        {
            if (container)
                container.ReturnToPool(this);
        }
    }
}
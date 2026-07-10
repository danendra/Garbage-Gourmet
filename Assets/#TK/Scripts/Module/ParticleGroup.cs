using UnityEngine;

namespace TK.Module
{
    public class ParticleGroup : MonoBehaviour
    {
        [SerializeField] ParticleSystem[] particles;

        public void Play()
        {
            foreach (var ps in particles)
                ps.Play();
        }

        public void ChangeOrderInLayer(int order)
        {
            foreach (var ps in particles)
            {
                var renderer = ps.GetComponent<ParticleSystemRenderer>();
                if (renderer) renderer.sortingOrder = order;
            }
        }
    }
}


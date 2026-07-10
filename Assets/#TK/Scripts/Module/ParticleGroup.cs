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
    }
}


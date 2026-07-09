using UnityEngine;
using UnityEngine.Audio;

namespace TK.Audio
{
    [CreateAssetMenu(fileName = "SFXData", menuName = "TK/Audio/SFXData")]
    public class SFXData : ScriptableObject
    {
        [SerializeField] private SFXId id;
        [SerializeField] private AudioResource audioResource;
        
        public SFXId Id => id;
        public AudioResource AudioResource => audioResource;

        public void SetId(SFXId newId) => id = newId;
        public void SetAudioResource(AudioResource resource) => audioResource = resource;
    }
}

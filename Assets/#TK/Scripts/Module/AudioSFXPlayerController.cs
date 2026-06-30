using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anoa.Module
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSFXPlayerController : MonoBehaviour
    {
        [SerializeField] protected AudioClip[] arrClips;
        [SerializeField] protected float fltMinPitch = 1.0f;
        [SerializeField] protected float fltMaxPitch = 1.0f;
        [SerializeField] protected bool isRemoveParent = true;

        protected Transform transRefference;
        protected new AudioSource audio;

        private void Awake()
        {
            audio = GetComponent<AudioSource>();
            transRefference = transform.parent;
        }

        public void PlaySFX()
        {
            gameObject.SetActive(true);

            if (transRefference && isRemoveParent)
            {
                transform.parent = null;
                transform.position = transRefference.position;
            }

            audio.clip = arrClips[Random.Range(0, arrClips.Length)];
            audio.pitch = Random.Range(fltMinPitch, fltMaxPitch);
            audio.Play();

            if (isRemoveParent)
                StartCoroutine(IEHideOnStop());
        }

        protected IEnumerator IEHideOnStop()
        {
            yield return new WaitForEndOfFrame();

            while (audio.isPlaying)
            {
                yield return null;
            }

            gameObject.SetActive(false);
        }

        public void Mute(bool _isOn)
        {
            audio.mute = _isOn;
        }
    }
}
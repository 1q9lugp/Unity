using UnityEngine;

namespace Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;
            _audioSource.clip = clip;
            _audioSource.loop = loop;
            _audioSource.Play();
        }

        public void StopMusic()
        {
            _audioSource.Stop();
        }

        public void SetVolume(float vol)
        {
            _audioSource.volume = Mathf.Clamp01(vol);
        }
    }
}

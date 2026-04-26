using UnityEngine;

public class ArenaMusic : MonoBehaviour
{
    public enum PlayMode { LoopOne, Rotate }

    [Header("Songs - drag clips in order")]
    public AudioClip[] songs;
    public PlayMode    mode   = PlayMode.LoopOne;
    public float       volume = 0.8f;

    AudioSource _src;
    int         _idx;

    void Start()
    {
        _src        = gameObject.AddComponent<AudioSource>();
        _src.loop   = false;
        _src.volume = volume;
        if (songs != null && songs.Length > 0) { _src.clip = songs[0]; _src.Play(); }
    }

    void Update()
    {
        if (_src.isPlaying || songs == null || songs.Length == 0) return;
        if (mode == PlayMode.Rotate) _idx = (_idx + 1) % songs.Length;
        _src.clip = songs[_idx];
        _src.Play();
    }
}

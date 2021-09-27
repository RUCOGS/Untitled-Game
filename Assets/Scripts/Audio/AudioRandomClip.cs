using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioRandomClip : MonoBehaviour
{

    public List<AudioClip> clips = new List<AudioClip>();
    [HideInInspector]
    public AudioSource source;

    public void Start() {
        source = GetComponent<AudioSource>();
    }

    public void Play() {
        int index = Random.Range(0, clips.Count);

        source.PlayOneShot(clips[index]);
    }
}

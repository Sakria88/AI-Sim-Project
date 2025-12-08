using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicRandomScript : MonoBehaviour
{
    void Start()
    {
        AudioSource[] musicAudio = GetComponents<AudioSource>();
        musicAudio[Random.Range(0, musicAudio.Length - 1)].Play();
    }
}

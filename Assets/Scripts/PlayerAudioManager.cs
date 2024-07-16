using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    private AudioSource PlayerAudio
    {  get; set; }
    [field: SerializeField] public AudioClip PushBackPickedUp
    {  get; set; }
    [field: SerializeField] public AudioClip PushBack
    { get; set; }

    [field: SerializeField] public AudioClip Dash
    { get; set; }

    [field: SerializeField] public AudioClip SmashPickedUp
    { get; set; }
    [field: SerializeField] public AudioClip Smash
    { get; set; }
    [field: SerializeField] public AudioClip PreSmashRise
    { get; set; }

    [field: SerializeField] public AudioClip ProjectilePickedUp
    { get; set; }
    [field: SerializeField] public AudioClip Projectile
    { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        PlayerAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAudioClip(AudioClip clipToPlay, float volume = 5f)
    {
        PlayerAudio.PlayOneShot(clipToPlay, volume);
    }
}

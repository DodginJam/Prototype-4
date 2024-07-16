using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource BackgroundMusic
    {  get; set; }
    [field: SerializeField] public AudioClip DefaultMusic
    { get; set; }
    [field: SerializeField] public AudioClip AltMusic
    { get; set; }
    [field: SerializeField] public AudioClip BossMusic
    { get; set; }

    private SpawnManager SpawnManagerScript
    { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        BackgroundMusic = GetComponent<AudioSource>();
        BackgroundMusic.clip = DefaultMusic;
        BackgroundMusic.Play();

        SpawnManagerScript = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
    }

    // Update is called once per frame
    void Update()
    {
        ChangeMusicOnBossLevel();
    }

    void ChangeAudioClip(AudioClip newClip)
    {
        if (!BackgroundMusic.clip.Equals(newClip))
        {
            float currentAudioTime = BackgroundMusic.time;

            BackgroundMusic.clip = newClip;

            BackgroundMusic.Play();

            BackgroundMusic.time = currentAudioTime;
        }
    }

    void ChangeMusicOnBossLevel()
    {
        if (SpawnManagerScript.IsBossLevel())
        {
            ChangeAudioClip(BossMusic);
        }
        else
        {
            if (SpawnManagerScript.WaveNumber <= 4)
            {
                ChangeAudioClip(DefaultMusic);
            }
            else
            {
                ChangeAudioClip(AltMusic);
            }
        }
    }
}

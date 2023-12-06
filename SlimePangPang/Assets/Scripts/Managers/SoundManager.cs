using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum SFXType { Pop, Drop, Button, Over, Unlock };

public class SoundManager : Singleton<SoundManager>
{
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer;
    public SFXClip[] sfxClip;

    private int sfxCursor;

    private void Start()
    {
        GameManager gm = GameManager.Instance;

        bgmPlayer.volume = gm.bgmVolume;
        foreach(AudioSource source in sfxPlayer)
        {
            source.volume = gm.sfxVolume;
        }

        bgmPlayer.Play();
    }

    public void SFXPlay(SFXType type, int i)
    {
        AudioClip[] clips = FindSFXClip(type);

        // i�� 0�Ͻ� �ش� Ÿ���� ������ Ŭ�� ���
        if (i == 0)
            sfxPlayer[sfxCursor].clip = clips[UnityEngine.Random.Range(0, clips.Length)];

        // i�� 1�̻� �Ͻ� �迭�� �ش� index���� i-1�� Ŭ�� ���
        else
            sfxPlayer[sfxCursor].clip = clips[i - 1];

        sfxPlayer[sfxCursor].Play();
        sfxCursor = (sfxCursor + 1) % sfxPlayer.Length;
    }

    private AudioClip[] FindSFXClip(SFXType type)
    {
        return Array.Find(sfxClip, clip => clip.type == type).sfxClip;
    }
}

[Serializable]
public struct SFXClip
{
    public SFXType type;
    public AudioClip[] sfxClip;
}

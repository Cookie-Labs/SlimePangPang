using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

// Magic: ��� �������� ��ġ�� ����
// Slime: �������� �������� �ٸ� ���������� �ٲ�
// Needle: ������ �� ���� �������� �Ͷ߸�
// Sword: ���� ū �������� �Ͷ߸�
public enum ItemType { Magic, Slime, Needle, Sword }

public class GameManager : Singleton<GameManager>
{
    [Title("Value")]
    public bool isOver;
    public float bgmVolume; // ���̺�
    public float sfxVolume; // ���̺�

    [Title("Save")]
    public Item[] items; // ���̺�
    public Moeny money; // ���̺� (total)
    public int maxScore; // ���̺�

    private void Start()
    {
        Application.targetFrameRate = 60; // ��������ȭ

        Load(); // ���� ������ �ҷ���
    }

    public void Save()
    {
        // �� ���̺�
        ES3.Save("BGM", bgmVolume, "Value.es3"); // BGM
        ES3.Save("SFX", sfxVolume, "Value.es3"); // SFX

        // �⺻ �� ���̺�
        ES3.Save("Money", money.total, "Value.es3"); // ��
        ES3.Save("MaxScore", maxScore, "Value.es3"); // �ִ� ����

        // ������ ���̺�
        ES3.Save("Items", items, "Items.es3");
    }

    public void Load()
    {
        // �� ���̺�
        bgmVolume = ES3.Load("BGM", "Value.es3", 0.2f);
        sfxVolume = ES3.Load("SFX", "Value.es3", 0.3f);

        // �⺻�� �ε�
        money.total = ES3.Load("Money", "Value.es3", 0);
        maxScore = ES3.Load("MaxScore", "Value.es3", 0);

        // ������ �ε�
        items = ES3.Load("Items", "Items.es3", items);

        // ���� �ε�
        DecoManager.Instance.LoadDeco();
    }

    public void GameOver()
    {
        if (isOver)
            return;

        isOver = true;

        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        SpawnManager sm = SpawnManager.Instance;
        UIManager um = UIManager.Instance;

        Slime[] slimes = sm.slimeList.ToArray();

        // ����ȿ�� ��Ȱ��ȭ
        foreach (Slime slime in slimes)
            slime.rigid.simulated = false;

        // ������ ����Ʈ �ϳ��� �����ؼ� �����
        foreach (Slime slime in slimes)
        {
            sm.DeSpawnSlime(slime);
            sm.SpawnPopAnim(slime);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1f);

        // �ִ� ���� ������
        maxScore = um.score.curScore > maxScore ? um.score.curScore : maxScore;

        SoundManager.Instance.SFXPlay(SFXType.Over, 1); // ����
        BtnManager.Instance.Play(false); // ����
        um.gameOver.TabGameOver(um.score.curScore, maxScore, money.cur); // ���ӿ��� UI Ȱ��ȭ
        money.SettleMoney(); // �� ����

        Save(); // ���̺�
    }
}

[Serializable]
public class Item
{
    public ItemType type;
    public int count;

    public void UseItem()
    {
        count--;

        UIManager.Instance.itemUI.SetItemUI((int)type, count);
    }
}

[Serializable]
public class Moeny
{
    public int total, cur;
    
    public void EarnMoney(int i) // �ΰ���
    {
        cur += i;
        UIManager.Instance.moneyUI.inGameUI.text = cur.ToString();
    }

    public void SettleMoney() // ���� ���� ��
    {
        total += cur;
        cur = 0;
    }

    public void UseMoney(int i)
    {
        // ���� ����� ���� �� ��δٸ� return (�Ŀ� �˸�UI �߰�)
        if (i > total)
            return;

        total -= i;
    }
}

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
        sm.lastSlime = null;

        BtnManager.Instance.isTouching = false;

        yield return new WaitForSeconds(1f);

        // �ִ� ���� ������
        if(um.score.curScore > maxScore)
        {
            maxScore = um.score.curScore;
            /*JSManager.Instance.SetMaxScore(maxScore, true);*/
        }
        else
            /*JSManager.Instance.SetMaxScore(um.score.curScore, false);*/

        SoundManager.Instance.SFXPlay(SFXType.Over, 1); // ����
        um.gameOver.TabGameOver(um.score.curScore, maxScore, money.cur); // ���ӿ��� UI Ȱ��ȭ
        money.SettleMoney(); // �� ����

        Save(); // ���̺�

        BtnManager.Instance.Play(false); // ����
    }
}

[Serializable]
public class Item
{
    public ItemType type;
    public int count;

    public void UseItem()
    {
        ItemUI item = UIManager.Instance.itemUI;
        int id = (int)type;

        if (item.itemBtn[id].isUse)
            return;

        count--;

        item.SetItemUI((int)type, count);
        item.itemBtn[id].isUse = true;
    }

    public void GainItem()
    {
        count++;
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

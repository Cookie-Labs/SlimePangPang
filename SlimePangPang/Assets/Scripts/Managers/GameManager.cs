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

    [Title("Save")]
    public Item[] items; // ���̺�
    public Moeny money;
    public int maxScore; // ���̺�

    protected override void Awake()
    {
        base.Awake();

        Application.targetFrameRate = 60; // ��������ȭ
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

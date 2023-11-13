using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Magic: ��� �������� ��ġ�� ����
// Slime: �������� �������� �ٸ� ���������� �ٲ�
// Needle: ������ �� ���� �������� �Ͷ߸�
// Sword: ���� ū �������� �Ͷ߸�
public enum ItemType { Magic, Slime, Needle, Sword }

public class GameManager : Singleton<GameManager>
{
    public bool isOver;
    public Item[] items; // ���̺�
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
        um.gameOver.TabGameOver(um.score.curScore, maxScore, 0); // ���� ���߿�
    }
}

[System.Serializable]
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

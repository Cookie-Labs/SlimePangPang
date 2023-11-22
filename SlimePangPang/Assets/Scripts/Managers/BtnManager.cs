using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Linq;

public class BtnManager : Singleton<BtnManager>
{
    [HideInInspector] public bool isTouching; // ��Ŭ�� üũ

    public void TouchDown()
    {
        isTouching = true;

        SpawnManager sm = SpawnManager.Instance;

        if (sm.lastSlime == null)
            return;

        sm.lastSlime.Drag();
    }

    public void TouchUp()
    {
        isTouching = false;

        SpawnManager sm = SpawnManager.Instance;

        if (sm.lastSlime == null || !sm.lastSlime.gameObject.activeSelf)
            return;

        sm.lastSlime.Drop();
        sm.lastSlime = null;
    }

    public void Tab(GameObject obj)
    {
        if (isTouching)
            return;

        if (!obj.activeSelf)
        {
            obj.SetActive(true);
            UIManager.Instance.raycastPannel.SetActive(true);
            obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            obj.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InExpo).SetEase(Ease.OutBounce).SetUpdate(true);
        }
        else
        {
            UIManager.Instance.raycastPannel.SetActive(false);
            obj.transform.DOScale(new Vector3(0.05f, 0.05f, 0.05f), 0.25f).SetEase(Ease.InOutExpo).SetUpdate(true).OnComplete(() => obj.SetActive(false));
        }
    }

    public void Tab_NoRayCast(GameObject obj)
    {
        if (isTouching)
            return;

        if (!obj.activeSelf)
        {
            obj.SetActive(true);
            obj.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            obj.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f).SetEase(Ease.InExpo).SetEase(Ease.OutBounce).SetUpdate(true);
        }
        else
        {
            obj.transform.DOScale(new Vector3(0.05f, 0.05f, 0.05f), 0.25f).SetEase(Ease.InOutExpo).SetUpdate(true).OnComplete(() => obj.SetActive(false));
        }
    }

    public void TabShop()
    {
        UIManager.Instance.moneyUI.shopUI.text = GameManager.Instance.money.total.ToString();
    }

    public void TabSetting()
    {
        UIManager.Instance.settingUI.SetSettingUI();
    }

    public void TabItem(string _type)
    {
        ItemType type = (ItemType)System.Enum.Parse(typeof(ItemType), _type);

        if (GameManager.Instance.items[(int)type].count <= 0)
            return;

        ItemUI ui = UIManager.Instance.itemUI;

        ui.TabUI(type);
        Tab(ui.pannel);
    }

    public void SetBGMVolume()
    {
        UIManager.Instance.settingUI.SetBGMVolume();
    }
    
    public void SetSFXVolume()
    {
        UIManager.Instance.settingUI.SetSFXVolume();
    }

    public void Play(bool isPlay)
    {
        if (isPlay)
            Time.timeScale = 1f; // ���
        else
            Time.timeScale = 0f; // ����
    }

    public void StartBtn()
    {
        SceneManager.sceneLoaded += OnInGameSceneLoaded;
        SceneManager.LoadScene(1);
    }

    private void OnInGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(SceneManager.GetActiveScene().buildIndex == 1)
        {
            // ù ��° ���� �ε�� �Ŀ� ������ �ڵ带 ���⿡ �߰��մϴ�.
            UIManager.Instance.StartItemUI();

            // �ʿ信 ���� �̺�Ʈ ��� ����
            SceneManager.sceneLoaded -= OnInGameSceneLoaded;
        }
    }

    public void RestartBtn()
    {
        GameManager.Instance.isOver = false;
        Play(true);

        SceneManager.sceneLoaded += OnInGameSceneLoaded;
        SceneManager.LoadScene(1);
    }

    public void RobbyBtn()
    {
        GameManager.Instance.isOver = false;
        Play(true);

        SceneManager.LoadScene(0);
    }

    public void UseItemBtn()
    {
        GameManager gm = GameManager.Instance;
        ItemUI ui = UIManager.Instance.itemUI;

        switch(ui.curSelect)
        {
            case ItemType.Magic: // ��� �������� ��ġ�� ����
                Magic_Item();
                break;
            case ItemType.Slime: // ������ �������� �������� �Ͷ߸�
                Slime_Item();
                break;
            case ItemType.Needle: // ������ �� ���� �������� �Ͷ߸�
                Needle_Item();
                break;
            case ItemType.Sword: // ���� ū �������� �Ͷ߸�
                Sword_Item();
                break;
        }

        gm.items[(int)ui.curSelect].UseItem();
    }

    #region Item
    private void Magic_Item()
    {
        SpawnManager sm = SpawnManager.Instance;

        List<Slime> slimeList = new List<Slime>(sm.slimeList); // ������ ������ ������ ������ ����Ʈ
        List<Vector3> transList = new List<Vector3>(); // ������ ����Ʈ���� ��ġ ����

        // ������ ������ ����
        slimeList.Remove(sm.lastSlime);

        // ��ġ ���� ����Ʈ ����
        foreach (Slime slime in slimeList)
        {
            transList.Add(slime.transform.position);
        }

        // ������ ����Ʈ �������� ����
        System.Random rng = new System.Random();
        slimeList = slimeList.OrderBy(x => rng.Next()).ToList();

        // ������ ����Ʈ ��ġ ����
        int length = slimeList.Count;
        for (int i = 0; i < length; i++)
        {
            sm.SpawnPopAnim(slimeList[i]);
            slimeList[i].transform.position = transList[i];
        }
    }

    private void Slime_Item()
    {
        SpawnManager sm = SpawnManager.Instance;

        sm.SpawnPopAnim(sm.lastSlime);
        sm.DeSpawnSlime(sm.lastSlime);
        sm.lastSlime = null;
    }

    private void Needle_Item()
    {
        SpawnManager sm = SpawnManager.Instance;

        List<Slime> slimeList = new List<Slime>(sm.slimeList);
        slimeList.Remove(sm.lastSlime);

        for(int i = 0; i < 2; i++)
        {
            int ranInt = Random.Range(0, slimeList.Count);
            Slime selSlime = slimeList[ranInt];

            sm.SpawnPopAnim(selSlime);
            sm.DeSpawnSlime(selSlime);
            slimeList.Remove(selSlime);
        }
    }

    private void Sword_Item()
    {
        SpawnManager sm = SpawnManager.Instance;

        List<Slime> slimeList = new List<Slime>(sm.slimeList);
        Slime maxSlime = null; // �ִ� ���� ������
        slimeList.Remove(sm.lastSlime);

        int max = 0;
        foreach(Slime slime in slimeList)
        {
            if(slime.level > max)
            {
                maxSlime = slime;
                max = slime.level;
            }
        }

        sm.SpawnPopAnim(maxSlime);
        sm.DeSpawnSlime(maxSlime);
    }
    #endregion
}

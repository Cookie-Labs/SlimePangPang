using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Redcode.Pools;
using UnityEngine.Localization.Components;

public class DecoUI : MonoBehaviour, IPoolObject
{
    public Deco deco;

    public TextMeshProUGUI title;
    public LocalizeStringEvent titleString;
    public Image pannelImg;
    public Image mainImg;
    public Button btn;
    public CanvasGroup btnAlpha;
    public TextMeshProUGUI btnTxt;

    const string X = "X";
    const string EQUIP = "Equip";
    const string EQUIPPED = "Equipped";

    public void OnCreatedInPool()
    {
        name = name.Replace("(Clone)", "");
    }

    public void OnGettingFromPool()
    {
        
    }

    public void SetUI(Deco _deco)
    {
        deco = _deco;

        titleString.StringReference.SetReference("DecoTable", deco.name);
        title.color = DecoManager.Instance.pallate[1].color[(int)deco.tier]; // Ƽ�� ��
        mainImg.sprite = deco.sprite;

        SetButtonUI();
    }

    public void SetButtonUI()
    {
        DecoManager dm = DecoManager.Instance;

        deco = dm.FindByID(deco.ID); // ����

        // ���� X
        if(!deco.isHave)
        {
            btn.interactable = false;
            btnAlpha.alpha = 0.6f;
            btnTxt.text = X;
        }

        // ���� O
        else
        {
            btn.interactable = true;
            btnAlpha.alpha = 1f;

            // ���� X
            if (deco.equipID < 0)
            {
                btnTxt.text = EQUIP;
                pannelImg.color = dm.pallate[0].color[8]; // �� ����
            }

            // ���� O
            else
            {
                btnTxt.text = EQUIPPED;
                pannelImg.color = dm.pallate[0].color[deco.equipID]; // �� ����
            }
        }
    }

    public void ChangeDeco()
    {
        DecoManager dm = DecoManager.Instance;
        UIManager um = UIManager.Instance;

        // ����
        SoundManager.Instance.SFXPlay(SFXType.Button, 0);

        // ���� ����
        ShopUI shopUI = um.shopUI;
        int currentSlimeID = shopUI.ID;
        int decoID = deco.ID;

        // ���� ������ �������� ������ �ִ� ����� ID
        int currentSlimeEquipID = deco.equipID;

        // ���� �������� ������ �����ߴ� ��� ��������
        Deco prevDeco = dm.FindByTypeIndex(deco.type, currentSlimeID);

        // ���� �������� ������ �ִ� ����� �ε��� ��������
        int currentDecoIndex = dm.GetIndex(decoID);

        // ���� �������� ��� ����ID ����
        dm.deco[currentDecoIndex].SetEquipID(currentSlimeID);

        // ������ ������ ����� ���� ���
        if (prevDeco != null)
        {
            int prevIndex = dm.GetIndex(prevDeco.ID);

            // ������ ������ ����� ����ID�� -1�� ����
            dm.deco[prevIndex].SetEquipID(-1);

            // ���� �������� ������ ������ ��İ� �ٸ� ���
            if (currentSlimeEquipID != -1 && currentDecoIndex != prevIndex)
            {
                // ���� �������� ������ ��İ� ������ ������ ����� ���� ���
                dm.deco[prevIndex].SetEquipID(-1);
            }

            // ������ ������ ��Ŀ� ���� Button UI ����
            dm.decoUIs[prevIndex].SetButtonUI();
        }

        // ���� �������� Button UI ����
        SetButtonUI();

        // �ٸ� �������� UI ����
        if (currentSlimeEquipID != -1)
            shopUI.slimeUIs[currentSlimeEquipID].SetDecoSlimeUI();

        // ���� �������� UI ����
        shopUI.slimeUIs[currentSlimeID].SetDecoSlimeUI();
    }
}

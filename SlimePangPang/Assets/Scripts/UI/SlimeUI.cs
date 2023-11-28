using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SlimeUI : MonoBehaviour
{
    public int id;
    [HideInInspector] public Image[] decoImg;

    private void Awake()
    {
        decoImg = GetComponentsInChildren<Image>(true);
        decoImg = decoImg.Skip(1).ToArray(); // ù ��° �ε��� �� ����
    }

    private void Start()
    {
        SetDecoSlimeUI();
    }

    public void SetDecoSlimeUI()
    {
        int length = decoImg.Length;
        for (int i = 0; i < length; i++)
        {
            // ������ ���� �ҷ�����
            Deco deco = DecoManager.Instance.FindByTypeIndex((DecoType)i, id);

            // ������ ���� ���� ��
            if(deco == null)
                decoImg[i].gameObject.SetActive(false);
            // ������ ���� ���� ��
            else
            {
                decoImg[i].gameObject.SetActive(true);
                decoImg[i].sprite = deco.GetSprite();
            }
        }
    }
}

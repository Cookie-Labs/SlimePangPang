using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DecoManager : Singleton<DecoManager>
{
    public List<Deco> deco; // ���̺�
    public DecoUI[] decoUIs;
    public Pallate[] pallate; // 0: ���� ������ ��    1: Ƽ��(���)

    public void SetDecoUI()
    {
        SpawnManager sm = SpawnManager.Instance;

        int length = deco.Count;
        decoUIs = new DecoUI[length];
        for(int i = 0; i < length; i++)
        {
            decoUIs[i] = sm.SpawnDecoUI(deco[i]);
        }
    }

    public void SaveDeco()
    {
        ES3.Save("Decos", deco, "Decos.es3");
    }

    public void LoadDeco()
    {
        List<Deco> loadDeco = ES3.Load("Decos", "Decos.es3", deco); // Load

        // �������� �ʴ� ���̵��� ���� ����
        loadDeco.RemoveAll(existingDeco => !deco.Any(d => d.ID == existingDeco.ID));

        // Sprite ������� ���� ����
        foreach (Deco existingDeco in loadDeco)
        {
            Deco newDeco = deco.FirstOrDefault(d => d.ID == existingDeco.ID);
            if (newDeco != null)
            {
                existingDeco.sprite = newDeco.sprite;
            }
        }

        // �߰��� ���̵��� ���� �߰�
        loadDeco.AddRange(deco.Where(updatedDeco => !loadDeco.Any(d => d.ID == updatedDeco.ID)));

        // ������
        deco = loadDeco;

        // Ƽ� ���� ����
        deco.Sort((deco1, deco2) =>
        {
            int typeComparison = deco1.type.CompareTo(deco2.type);
            if (typeComparison != 0)
            {
                return typeComparison;
            }

            return deco1.tier.CompareTo(deco2.tier);
        });

        // UI����
        SetDecoUI();
    }

    public Deco FindByIndex(int _index)
    {
        return deco[_index];
    }

    public Deco FindByID(int _id)
    {
        return deco.Find(deco => deco.ID == _id);
    }

    public Deco FindByTypeIndex(DecoType type, int slimeIndex)
    {
        return deco.Find(deco => deco.equipID == slimeIndex && deco.type == type);
    }

    public int GetIndex(int id)
    {
        return deco.FindIndex(deco => deco.ID == id);
    }
}

[Serializable]
public class Deco
{
    public string name;
    public DecoType type;
    public Sprite sprite;
    public Tier tier;

    public int ID;

    public int equipID;
    public bool isHave;

    public void SetEquipID(int changeID)
    {
        equipID = changeID;
    }

    public void SetHave(bool isTrue)
    {
        isHave = isTrue;
    }

    public Sprite GetSprite()
    {
        return sprite;
    }
}

[Serializable]
public struct Pallate
{
    public Color[] color;
}

public enum Tier { Common, Rare, Epic, Legend }
public enum DecoType { Hat = 0, Eye, Cheek, Neck }
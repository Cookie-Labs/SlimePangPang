using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Redcode.Pools;
using TMPro;
using DG.Tweening;

public class ScorePopUp : MonoBehaviour, IPoolObject
{
    private TextMeshPro text;

    private const float MIN_SIZE = 0.2f;
    private const float MAX_SIZE = 1.5f;

    public void OnCreatedInPool()
    {
        name = name.Replace("(Clone)", "");

        text = GetComponent<TextMeshPro>();
    }

    public void OnGettingFromPool()
    {

    }

    public IEnumerator PopRoutine(int i, Slime slime)
    {
        // ����
        transform.position = slime.transform.position;
        transform.localScale = Vector3.one * MIN_SIZE;
        text.text = i.ToString();

        // �� ����

        text.color = slime.defColor;

        // �ִϸ��̼� ȿ��(�˾�)
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOScale(MAX_SIZE, 0.7f).SetEase(Ease.InExpo).SetEase(Ease.OutBounce))
            .Append(transform.DOScale(MIN_SIZE, 0.3f).SetEase(Ease.InOutExpo))
            .OnComplete(() => SpawnManager.Instance.DeSpawnScore(this));

        yield return null;
    }
}

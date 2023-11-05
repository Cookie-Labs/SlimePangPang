using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Redcode.Pools;
using DG.Tweening;

public class Slime : MonoBehaviour, IPoolObject
{
    public int level;

    private bool isDrag, isMerge, isAttach;
    private float leftBorder, rightBorder, topBorder;
    private float defSize;
    private float deadTime;

    [HideInInspector] public Rigidbody2D rigid;
    private SpriteRenderer sr;
    private CircleCollider2D circle;

    public void OnCreatedInPool()
    {
        name = name.Replace("(Clone)", "");

        rigid = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        circle = GetComponent<CircleCollider2D>();
        defSize = transform.localScale.x;
    }

    public void OnGettingFromPool()
    {
        SetStat();
        SetBorder();

        Pop();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StartCoroutine(AttachRoutine());
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Slime"))
        {
            Slime other = collision.gameObject.GetComponent<Slime>();

            // ������ ��ġ��
            if(level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                // �ڽŰ� ����� ��ġ ��������
                Vector2 meTrans = transform.position;
                Vector2 otherTrans = other.transform.position;

                // 1. �ڽ��� �Ʒ��� ���� ��
                // 2. ������ ������ ��, �ڽ��� �����ʿ� ���� ��
                if(meTrans.y < otherTrans.y || (meTrans.y == otherTrans.y && meTrans.x > otherTrans.x))
                    StartCoroutine(LevelUp(other));
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Finish"))
        {
            deadTime += Time.deltaTime;

            // 2�� ���� ���ο� �ӹ� ��
            if(deadTime > 2)
                GameManager.Instance.GameOver(); // ���� ����
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Finish"))
        {
            // ���� ���� ���� �ʱ�ȭ
            deadTime = 0;
        }
    }

    private void SetStat()
    {
        isDrag = false;
        isMerge = false;
        isAttach = false;
        rigid.simulated = false;
        circle.enabled = true;
        rigid.velocity = Vector2.zero;
        rigid.angularVelocity = 0;
        sr.sortingOrder = 0;
    }

    private void SetBorder()
    {
        CameraBound camBound = MapManager.Instance.camBound;
        SpawnManager sm = SpawnManager.Instance;

        // ���
        leftBorder = camBound.Left + transform.localScale.x / 2f;
        rightBorder = camBound.Right - transform.localScale.x / 2f;
        topBorder = sm.map.spawnPoint.position.y;
    }

    private IEnumerator LevelUp(Slime other)
    {
        SpawnManager sm = SpawnManager.Instance;
        GameManager gm = GameManager.Instance;

        // ���� ����
        int upLevel = level + 1;
        isMerge = true;
        other.isMerge = true;
        other.rigid.simulated = false;
        other.circle.enabled = false;
        other.sr.sortingOrder = 1;

        // ������ ���� ������(20 ������)
        int frameCount = 0;

        // �������� ��
        while(frameCount < 20)
        {
            other.transform.position = Vector3.Lerp(other.transform.position, transform.position, 10f * Time.deltaTime);
            frameCount++;
            yield return null;
        }

        // ��ħ �Ϸ�
        gm.GetScore((int)Mathf.Pow(2, level)); // ���� ������ 2���� ��ŭ ���� �߰�
        sm.curMaxLv = Mathf.Max(upLevel, sm.curMaxLv); // �ִ� ���� ���� ����
        // ���� ����
        other.isMerge = false;
        isMerge = false;
        // ������ ����
        sm.DeSpawnSlime(other);
        sm.DeSpawnSlime(this);

        // �� �����Ӱ� ����Ʈ ����
        sm.SpawnPopParticle(transform);
        Slime newSlime = sm.SpawnSlime(upLevel, transform);
        newSlime.rigid.simulated = true;
    }

    IEnumerator AttachRoutine()
    {
        if (isAttach)
            yield break;

        isAttach = true;
        SoundManager.Instance.SFXPlay(SFXType.Attach, 0);

        yield return new WaitForSeconds(0.2f);

        isAttach = false;
    }

    private void Pop()
    {
        transform.localScale = Vector3.zero;

        transform.DOScale(defSize, 0.2f);
    }

    public void Drag()
    {
        if (isDrag)
            return;

        isDrag = true;
        StartCoroutine(SpawnMove());
    }

    public void Drop()
    {
        isDrag = false;
        rigid.simulated = true;
    }

    private IEnumerator SpawnMove()
    {
        while(isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            mousePos.x = Mathf.Clamp(mousePos.x, leftBorder, rightBorder);
            mousePos.y = topBorder;
            mousePos.z = 0;
            transform.position = Vector3.Lerp(transform.position, mousePos, 0.2f);

            yield return null;
        }
    }
}

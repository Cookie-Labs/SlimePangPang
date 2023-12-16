using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Redcode.Pools;
using DG.Tweening;
using System.Linq;

public enum State { Idle, Cute, Surprise }

public class Slime : MonoBehaviour, IPoolObject
{
    public int level;
    public Color defColor;
    public State state;

    private bool isDrag, isMerge;
    private float leftBorder, rightBorder, topBorder;
    [HideInInspector] public float defSize;
    private float deadTime;
    private Sprite defSprite;
    private SpriteRenderer[] decoSprites;

    [HideInInspector] public Rigidbody2D rigid;
    private SpriteRenderer sr;
    private PolygonCollider2D circle;

    public void OnCreatedInPool()
    {
        name = name.Replace("(Clone)", "");

        rigid = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        circle = GetComponent<PolygonCollider2D>();
        decoSprites = GetComponentsInChildren<SpriteRenderer>();
        decoSprites = decoSprites.Skip(1).ToArray();

        defSprite = sr.sprite;
        defSize = transform.localScale.x;
    }

    public void OnGettingFromPool()
    {
        SetDeco();
        SetStat();
        SetBorder();

        Pop();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Slime"))
        {
            Slime other = collision.gameObject.GetComponent<Slime>();

            // ������ ��ġ��
            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                // �ڽŰ� ����� ��ġ ��������
                Vector2 meTrans = transform.position;
                Vector2 otherTrans = other.transform.position;

                // 1. �ڽ��� �Ʒ��� ���� ��
                // 2. ������ ������ ��, �ڽ��� �����ʿ� ���� ��
                if (meTrans.y < otherTrans.y || (meTrans.y == otherTrans.y && meTrans.x > otherTrans.x))
                    StartCoroutine(LevelUp(other));
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Finish"))
        {
            deadTime += Time.deltaTime;

            // 1�� ���� ���ο� �ӹ� ��
            if(deadTime > 1)
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

    private void SetDeco()
    {
        DecoManager dm = DecoManager.Instance;

        int length = decoSprites.Length;
        for(int i = 0; i < length; i++)
        {
            Deco deco = dm.FindByTypeIndex((DecoType)i, level);

            if (deco == null)
                continue;

            decoSprites[i].sprite = deco.GetSprite();
        }
    }

    private void SetStat()
    {
        sr.sprite = defSprite;
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-360, 360));

        isDrag = false;
        isMerge = false;
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
        leftBorder = camBound.Left + transform.localScale.x;
        rightBorder = camBound.Right - transform.localScale.x;
        topBorder = sm.map.spawnPoint.position.y;
    }

    public void SetState(State _state)
    {
        StopCoroutine(ChangeSprite(state));

        state = _state;

        StartCoroutine(ChangeSprite(state));
    }

    private IEnumerator ChangeSprite(State _state)
    {
        SpriteData sd = SpriteData.Instance;

        switch(_state)
        {
            case State.Idle:
                yield break;
            case State.Cute:
                sr.sprite = sd.newSlimeSprite[level].cute;
                break;
            case State.Surprise:
                sr.sprite = sd.newSlimeSprite[level].surprise;
                break;
        }

        yield return new WaitForSeconds(1.5f);

        state = State.Idle;
        sr.sprite = defSprite;
    }

    private IEnumerator LevelUp(Slime other)
    {
        SpawnManager sm = SpawnManager.Instance;
        UIManager um = UIManager.Instance;
        GameManager gm = GameManager.Instance;

        // ���� ����
        int upLevel = level + 1;
        isMerge = true;
        other.isMerge = true;
        other.rigid.simulated = false;
        other.circle.enabled = false;
        other.sr.sortingOrder = 1;

        // ���� �Ⱥ��̰�
        foreach (SpriteRenderer decoSprite in decoSprites)
            decoSprite.sprite = null;

        // ������ ���� ������(10 ������)
        int frameCount = 0;

        // �������� ��
        while(frameCount < 10)
        {
            other.transform.position = Vector3.Lerp(other.transform.position, transform.position, 10f * Time.deltaTime);
            frameCount++;
            yield return null;
        }

        // ��ħ �Ϸ�
        // ���� ����
        other.isMerge = false;
        isMerge = false;
        // ������ ����
        sm.DeSpawnSlime(other);
        sm.DeSpawnSlime(this);

        // �� �����Ӱ� ����Ʈ ����
        Slime newSlime = sm.SpawnSlime(upLevel, transform, State.Cute);
        newSlime.rigid.simulated = true;

        um.score.GetScore((int)Mathf.Pow(upLevel, 2), newSlime); // ���������� 2���� ��ŭ ���� �߰�
        gm.money.EarnMoney(upLevel); // ����������ŭ �� �߰�
        sm.curMaxLv = Mathf.Max(upLevel, sm.curMaxLv); // �ִ� ���� ���� ����

        sm.SpawnPopAnim(newSlime);
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
        rigid.AddForce(Vector2.down * 5f, ForceMode2D.Impulse);
        SetState(State.Surprise);

        SoundManager.Instance.SFXPlay(SFXType.Drop, 1);
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

using Cinemachine.Utility;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Pipe_top : MonoBehaviour
{
    public bool isActive = false;
    LineRenderer lineRenderer;
    public Transform MyTransform;
    public Transform myTransform { get { return MyTransform; } }
    public Vector3 linkObjectPos { get; set; } = new Vector3(0, 0, -100);

    public GameObject linkObject;
    public bool lineActive { get; set; } = false;

    [SerializeField] float lineWidth = 0.15f;

    public int dirInfo = 0;     //(위 : 0, 오른 : 1, 아래 : 2, 왼 : 3)

    Rigidbody2D rb;
    BoxCollider2D bc;
    SpriteRenderer sr;

    public GameObject Player;
    //플레이어 숫자 변수
    int playerNum;
    //기존 파이프 vector2값
    public Vector3 oriPipeVec;
    //이어진 파이프 vector2값
    public Vector3 connectPipeVec;

    int num;

    int marionum;

    private void Awake()
    {

        bc = GetComponent<BoxCollider2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        if (PhotonNetwork.IsMasterClient || 1==1)
        {
            //Debug.Log(linkObjectPos + " master");
            //GetComponent<PhotonView>().RPC("Sync_Pos", RpcTarget.All, linkObjectPos);
        }
    }

    [PunRPC]
    public void Sync_Pip(int a, Vector3 b, int c, bool e, bool f)
    {
        var obect = PhotonView.Find(a).gameObject;

        obect.GetComponent<Pipe_top>().linkObjectPos = b;

        obect.GetComponent<Pipe_top>().dirInfo = c;

        obect.GetComponent<Pipe_top>().lineActive = e;
        obect.GetComponent<Pipe_top>().isActive = f;
    }

    //[PunRPC]
    //public void Sync_Pos(Vector3 _pos)
    //{
    //    Debug.Log(linkObjectPos + " rpc");
    //    linkObjectPos = _pos;
    //}

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.SetPosition(0, myTransform.position);
        lineRenderer.SetPosition(1, myTransform.position);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(lineActive);
        //라인 연결 코드
        if (lineActive)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (linkObjectPos == new Vector3(0, 0, -100))
            {
                lineRenderer.SetPosition(1, mousePos);
            }
            else
            {
                lineRenderer.SetPosition(1, linkObjectPos);
            }
        }

        //동작 OnOff, 라인렌더러 OffOn
        if (!isActive)
        {
            lineRenderer.enabled = true;
            return;
        }
        else if (lineRenderer.enabled)
        {
            lineRenderer.enabled = false;
        }

        //Vector2 mouseposition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //if (Input.GetMouseButtonDown(0))
        //{
        //    connectPipeVec = mouseposition;
        //}

        //if (Input.GetMouseButtonDown(1))
        //{
        //    oriPipeVec = mouseposition;
        //}
    }
    Coroutine tmp;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            marionum = collision.gameObject.GetComponent<Mario>().marioMode;
                //Debug.Log("flag bbb");
            if (Input.GetKeyDown(KeyCode.DownArrow) && num == 0 && tmp == null)
            {
                //Debug.Log("flag aaa");
                AudioManager.instance.PlayerOneShot(MARIO_SOUND.POWER_DOWN, false, 1);
                Player = collision.gameObject;
                tmp = StartCoroutine(moveDown());
                num++;
            }
        }
    }

    IEnumerator moveDown()
    {
        Debug.Log("flag1");
        if (bc.isTrigger == false)
        {
            Debug.Log("flag2");
            bc.isTrigger = true;
        }
        sr.sortingOrder = 2;


        for (int i = 0; i < 8; i++)
        {
            Debug.Log("flag3");
            if (marionum == 0)
            {
            Debug.Log("flag4-1");
                Player.transform.Translate(Vector3.down * 0.1f);
            }
            else if (marionum == 1 || marionum == 2)
            {
            Debug.Log("flag5-1");
                Player.transform.Translate(Vector3.down * 0.2f);
            }
            Debug.Log("flag6-1");
        }
        yield return new WaitForSeconds(0.6f);

            Debug.Log("flag7-1");
        pipeMovement();
    }

    private void pipeMovement()
    {
        Debug.Log("flag5");
        StartCoroutine("moveUp");

    }

    IEnumerator moveUp()
    {
        yield return new WaitForSeconds(0.6f);
        
        AudioManager.instance.PlayerOneShot(MARIO_SOUND.POWER_DOWN, false, 1);
        if (Player.transform.position.x - myTransform.position.x < 1.2f
    && Player.transform.position.x - myTransform.position.x > -1.2f)
        {
            if (marionum == 0)
            {
                Player.transform.position
                    = new Vector3(linkObjectPos.x, linkObjectPos.y);
            }
            else if (marionum == 1 || marionum == 2)
            {

                Player.transform.position
                    = new Vector3(linkObjectPos.x, linkObjectPos.y - 0.4f);
            }
        }

        else if (Player.transform.position.x - linkObjectPos.x < 1.2f
    && Player.transform.position.x - linkObjectPos.x > -1.2f)
        {
            if (marionum == 0)
            {
                Player.transform.position
                    = new Vector3(myTransform.position.x, myTransform.position.y);
            }

            else if (marionum == 1 || marionum == 2)
            {
                Player.transform.position
                    = new Vector3(myTransform.position.x, myTransform.position.y - 0.4f);
            }
        }
        Debug.Log("flag11");
        yield return new WaitForSeconds(0.6f);

        for (int i = 0; i < 8; i++)
        {
            if (marionum == 0)
            {
                Player.transform.Translate(Vector3.up * 0.1f);
            }

            else if (marionum == 1 || marionum == 2)
            {
                Player.transform.Translate(Vector3.up * 0.2f);
            }
        }
        Debug.Log("flag12");
        if (bc.isTrigger == true)
        {
            bc.isTrigger = false;
        }

        Debug.Log("flag13");
        sr.sortingOrder = 2;
        num--;

        tmp = null;
    }
}
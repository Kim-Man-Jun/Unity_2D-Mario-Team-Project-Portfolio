using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mario_die : Mario_state
{
    private float dieJumpPower = 5;
    public Vector3 respawnPos = new Vector3(0, 0, 0);

    public Mario_die(Mario _mario, Mario_stateMachine _stateMachine, string _animBoolName) : base(_mario, _stateMachine, _animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (GameObject.Find("InGame") != null)
        {
            respawnPos = GameObject.Find("InGame").GetComponent<InGame>().spawnPos;
        }

        if (AudioManager.instance != null && PV.IsMine)
            AudioManager.instance.PlayerOneShot(MARIO_SOUND.MARIO_DIE, false, 2);
        mario.rb.AddForce(new Vector2(0, dieJumpPower), ForceMode2D.Impulse);
        Debug.Log(PV.name + " DieState AddForce ��");

        // collider ���� => ������ �ž� �ϴϱ� ������ �Ǹ� �ٽ� ����� ��
        mario.collider.enabled = false;
        mario.collider_big.enabled = false;
        //mario.GetComponent<CapsuleCollider2D>().enabled = false;
        //Debug.Log(PV.name + " DieState Collider2D ��");

        stateTimer = 5f;
    }

    public override void Exit()
    {
        base.Exit();
        Debug.Log(PV.name + " DieState ����");
    }

    public override void Update()
    {
        base.Update();
        stateTimer -= Time.deltaTime;

        // Respawn
        if (stateTimer <= 0)
        {
            mario.transform.position = respawnPos;
            mario.GetComponent<CapsuleCollider2D>().enabled = true;
            mario.stateMachine.ChangeState(mario.idleState);
        }

    }

    //void MarioJumpDie()
    //{

    //}

}

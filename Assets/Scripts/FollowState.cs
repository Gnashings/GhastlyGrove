using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AISTATE;
using UnityEngine.InputSystem.DualShock;
using System.Runtime.InteropServices;
using System.IO;
using TMPro;
using Microsoft.Win32.SafeHandles;
/// <summary>
/// State that engages the ghost to chase the player 
/// Should swap into AttackState or DefensiveState
/// </summary>
public class FollowState : State<AI>
{
    private static FollowState _instance;
    public GhostController ghost;

    private float huntingTimer = 0f;
    private float totalHuntingTimer = 5f; //use wallhax

    //original stats
    private float originalSpeed = 0f;
    private float originalAngle = 0f;
    
    
    private float speedBuff;
    float distance;

    private FollowState()
    {
        // first and ONLY time this gets constructed, we set 'this' instance = state.
        if (_instance != null)
        {
            return;
        }
        _instance = this;
    }

    //function to access a static instance of this state
    public static FollowState Instance
    {
        get
        {
            if (_instance == null)
            {
                new FollowState();
            }
            return _instance;
        }
    }

    public override void EnterState(AI _owner)
    {
        Debug.Log("ENTERING FOLLOW STATE");
        ghost = _owner.ghost;
        huntingTimer = 0f;
        speedBuff += _owner.speedStack + 0.35f ;
        BuffStats();

        
    }

    public override void ExitState(AI _owner)
    {
        Debug.Log("EXITING FOLLOW STATE");
        ResetStats();
    }

    public override void UpdateState(AI _owner)
    {

        if (_owner.InAttackRadius() == true)
        {
            huntingTimer = 0f;
        }

        //Debug.Log(huntingTimer);
        if(huntingTimer >= totalHuntingTimer)
        {
            _owner.stateMachine.ChangeState(PatrolState.Instance);
        }

        if (_owner.InSpotRangeFront() == true)
        {
            huntingTimer = 0f;
            ChasePlayer();
        }
        else
        {
            ChasePlayer();
            huntingTimer += Time.deltaTime;
        }

        
    }
    void BuffStats()
    {
        //resets timers
        huntingTimer = 0f;

        //movement speed
        originalSpeed = ghost.navAgent.speed;
        ghost.navAgent.speed = ghost.navAgent.speed * 1.5f + speedBuff;

        //rotational look speed
        originalAngle = ghost.navAgent.angularSpeed;
        ghost.navAgent.angularSpeed = 360f;
    }
    void ResetStats()
    {
        //returning orignal movement speed
        ghost.navAgent.speed = originalSpeed;

        //returning original look speed
        ghost.navAgent.angularSpeed = originalAngle;

    }

    //Calculate the distance between the player and ghost
    void GetDistance()
    {
        distance = Vector3.Distance(ghost.getPlayerPosition(), ghost.transform.position);
        //Debug.Log(distance);
    }

    //debug log to see elapsed time
    void OutputTime()
    {
        Debug.Log(Time.time);
    }

    void ChasePlayer()
    {
        //GetDistance();
        //TODO test and change if required
        ghost.MoveAt(ghost.getPlayerPosition());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AISTATE;
using System.Runtime.InteropServices;
using System.IO;
using TMPro;
using Microsoft.Win32.SafeHandles;
/// <summary>
/// Default state after 
/// Should swap into FollowState or PatrolState
/// </summary>
public class SearchState : State<AI>
{
    private static SearchState _instance;
    public GhostController ghost;

    private float totaldetectionMeter = 4f; //timer before he swaps to chasing you
    private float detectionMeter = 0f;      //timer before he swaps to chasing you

    private float searchingTimer = 0;       //after you get spotted in this mode, allows ghost to keep tracking you
    private float totalSearchingTimer = 1.5f;

    //original stats
    private float originalSpeed = 0f;
    private float originalAngle = 0f;



    private SearchState()
    {
        // first and ONLY time this gets constructed, we set 'this' instance = state.
        if (_instance != null)
        {
            return;
        }
        _instance = this;
    }

    //function to access a static instance of this state
    public static SearchState Instance
    {
        get
        {
            if (_instance == null)
            {
                new SearchState();
            }
            return _instance;
        }

    }

    public override void EnterState(AI _owner)
    {
        Debug.Log("ENTERING SEARCH STATE");
        ghost = _owner.ghost;
        totalSearchingTimer -= 0.25f;
        slowStats();
        _owner.StopAndStare();

    }

    public override void ExitState(AI _owner)
    {
        Debug.Log("EXITING SEARCH STATE");
        resetStats();

    }

    public override void UpdateState(AI _owner)
    {
        /**
        if (_owner.InSpotRangeFront() == true)
        {
            _owner.StopAndStare();
            //play a sound here!
        }*/

        if (_owner.InAttackRadius() == true)
        {
            _owner.stateMachine.ChangeState(FollowState.Instance);
        }


        //leave this state after you are unable to see the player
        //relook at this variable
        if (searchingTimer >= totalSearchingTimer && _owner.InSpotRangeFront() == false)
        {
            _owner.stateMachine.ChangeState(PatrolState.Instance);
        }

        //while ghost spots you
        if (_owner.InSpotRangeFront() == true)
        {
            detectionMeter += Time.deltaTime;
            searchingTimer = 0f;
            _owner.StopAndStare();
        }

        //if you leave line of sight
        if (_owner.InSpotRangeFront() == false && searchingTimer <= totalSearchingTimer)
        {
            searchingTimer += Time.deltaTime;
            //Debug.Log("im searching");
            //Debug.Log(searchingTimer);
            ghost.MoveAt(ghost.getPlayerPosition());
        }

        //while you have exceeded the amount of staredown time
        if (detectionMeter >= totaldetectionMeter)
        {
            //Debug.Log("i think i see you");
            //possible issue with moving forward
            //ghost.MoveAt(ghost.getPlayerPosition());
            _owner.stateMachine.ChangeState(FollowState.Instance);
        }
    }


    void slowStats()
    {
        //resets timers
        searchingTimer = 0f;
        detectionMeter = 0f;

        //movement speed
        originalSpeed = ghost.navAgent.speed;
        ghost.navAgent.speed = ghost.navAgent.speed * 0.10f;

        //rotational look speed
        originalAngle = ghost.navAgent.angularSpeed;
        ghost.navAgent.angularSpeed = ghost.navAgent.angularSpeed * 0.25f;
    }

    void resetStats()
    {
        //returning orignal movement speed
        ghost.navAgent.speed = originalSpeed;

        //returning original look speed
        ghost.navAgent.angularSpeed = originalAngle;

    }

}

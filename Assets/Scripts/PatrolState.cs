using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AISTATE;
/// <summary>
/// Default state after 
/// Should swap into SearchState or FollowState
/// </summary>
public class PatrolState : State<AI>
{
    private static PatrolState _instance;
    public GhostController ghost;
    float distance;
    private PatrolState()
    {
        // first and ONLY time this gets constructed, we set 'this' instance = state.
        if (_instance != null)
        {
            return;
        }
        _instance = this;
    }

    //function to access a static instance of this state
    public static PatrolState Instance
    {
        get
        {
            if (_instance == null)
            {
                new PatrolState();
            }
            return _instance;
        }
    }

    public override void EnterState(AI _owner)
    {
        ghost = _owner.ghost;
        Debug.Log("ENTERING PATROL STATE");
        //check to see if there are sufficent waypoints

    }

    public override void ExitState(AI _owner)
    {
        Debug.Log("EXITING PATROL STATE");
    }

    public override void UpdateState(AI _owner)
    {
        if (_owner.InAttackRadius() == true)
        {
            _owner.stateMachine.ChangeState(SearchState.Instance);
        }

        if (_owner.InSpotRangeFront() == true)
        {
            //Debug.Log("SPOTTED PLAYER");
            //ghost.MurderPlayer();
            _owner.stateMachine.ChangeState(SearchState.Instance);
        }

        if (_owner.navMeshAgent.remainingDistance != 0)
        {
            _owner._travelling = true;
        }

        //see if we are close to our destination
        if (_owner._travelling && _owner.navMeshAgent.remainingDistance <= 1.0f)
        {
            _owner._travelling = false;

            //f waiting to patrol, then wait
            if (_owner._patrolWaiting)
            {
                _owner._waiting = true;
                _owner._waitTimer = 0f;
            }
            else
            {
                _owner.ChangePatrolPoint();
                _owner.SetDestination();
            }

        }

        //Instead if we're waiting, set a timer
        if(_owner._waiting)
        {
            _owner._waitTimer += Time.deltaTime;
            
            //When finished waiting
            if (_owner._waitTimer >= _owner._totalWaitTime)
            {

                _owner._waiting = false;

                _owner.ChangePatrolPoint();
                _owner.SetDestination();
            }
        }

    }


    //Calculate the distance between the player and ghost
    void GetDistance()
    {
        distance = Vector3.Distance(ghost.getPlayerPosition(), ghost.transform.position);
    }

    //debug log to see elapsed time
    void OutputTime()
    {
        Debug.Log(Time.time);
    }



    //void ChasePlayer()
    //{
    //    GetDistance();
    //    //TODO test and change if required
    //    ghost.SetDestination();
    //}
}

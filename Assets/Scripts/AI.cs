using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AISTATE;
using System.IO;
using UnityEngine.AI;
/// <summary>
/// A collection of methods and booleans designed to influence the ghosts choice in state
/// </summary>
public class AI : MonoBehaviour
{
    [Header("Modifiers")]
    [SerializeField] public float gameTimer;
    private float distance;
    [SerializeField] public int seconds = 0;
    [SerializeField] public float radius = 10f;

    [Header("Pathfinding")]
    [SerializeField] public bool _patrolWaiting;
    [SerializeField] public float _totalWaitTime = 3f;
    [SerializeField] public float _switchProbability;
    [SerializeField] public List<Waypoint> _patrolPoints;
    [SerializeField] public NavMeshAgent navMeshAgent;
    [SerializeField] public int _currentPatrolIndex;
    [SerializeField] public bool _travelling;
    [SerializeField] public bool _waiting;
    [SerializeField] public bool _patrolForward;
    [SerializeField] public float _waitTimer;

    public float speedStack = 0.0f;

    public GhostController ghost;


    public StateMachine<AI> stateMachine { get; set; }

    private void Start()
    {
        stateMachine = new StateMachine<AI>(this);
        ghost = GetComponent<GhostController>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        if (ghost.enableAI)
        {
            //inits a starting state for the GhostB
            stateMachine.ChangeState(NeutralState.Instance);
            gameTimer = Time.time;
            if (_patrolPoints != null && _patrolPoints.Count >= 2)
            {
                _currentPatrolIndex = 0;
                SetDestination();
                _travelling = true;
                _travelling = false;
            }
            else
            {
                Debug.LogError("Not enough control points for patrolling");
            }

        }
    }

    private void Update()
    {
        if (ghost.enableAI)
        {
            stateMachine.Update();
        }
    }

    //If there is nothing to attack, reposition. Potentially obsolete
    public bool SpottedPlayer()
    {
        if (InSpotRangeFront() == false)
        {
            //Debug.LogWarning("not in attack range, can start following");
            return true;
        }
        else
        {
            //Debug.LogWarning("in attack range, can start following");
            return false;
        }

    }

    //returns true if player is within the ghosts interaction zone
    public bool InAttackRadius()
    {
        if (GetDistance() <= radius)
        {
            //Debug.LogWarning("Player in chase range");
            return true;
        }
        else
        {
            //Debug.LogWarning("Player NOT in attack range");
            return false;
        }
    }

    //returns true if the player is directly in front of the ghost
    public bool InSpotRangeFront()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        //if (Physics.Raycast(transform.position + (Vector3.up * 1.05f), fwd, out hit, 30, ghost.PlayerLayer))

        if (Physics.Raycast(transform.position + (Vector3.up * 1.00f), fwd, out hit, 100) || Physics.Raycast(transform.position + (Vector3.down * 1f), fwd, out hit, 100))
        {
            Debug.DrawRay(transform.position + (Vector3.up * 1.00f), fwd * hit.distance, Color.red);
            Debug.DrawRay(transform.position + (Vector3.down * 0.5f), fwd * hit.distance, Color.green);
            if (hit.collider.name.Equals("Character"))
            {
                return true;
            }
            else
            {
                //Debug.Log(hit);
                return false;
            }
        }
        return false;
    }

    public bool InHuntingRange()
    {
        RaycastHit hit;
        Vector3 fwd = transform.TransformDirection(Vector3.forward);

        if (Physics.Raycast(transform.position + (Vector3.up * 1.00f), fwd, out hit, 100, ghost.PlayerLayer))
        {
            Debug.DrawRay(transform.position + (Vector3.up * 1.00f), fwd * hit.distance, Color.blue);
            if (hit.collider.name.Equals("Character"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }


    public void LookAt()
    {
        ghost.FacePlayer();
    }

    public void StopAndStare()
    {
        ghost.StopMoving();
        ghost.FacePlayer();
    }

    public bool OnGround()
    {

        Vector3 origin = transform.position + (Vector3.up * ghost.distanceToGround);
        Vector3 dir = -Vector3.up;
        float dis = ghost.distanceToGround + 0.5f;
        RaycastHit hit;

        Debug.DrawRay(origin, dir * ghost.distanceToGround); //Debug Line to Show Raycasting Distance

        //If the Raycast is hitting the ground return true and alter the player's position to stay above ground.
        if (Physics.Raycast(origin, dir, out hit, dis, ghost.GroundLayer))
        {

            Vector3 targetPosition = hit.point;
            transform.position = targetPosition;
            return true;
        }

        return false;
    }

    //distance between player and ghost
    public float GetDistance()
    {
        return distance = Vector3.Distance(ghost.getPlayerPosition(), ghost.detectPlayerSphere.position);
    }

    //creates a sphere around the ghost that can be used for detection
    void OnDrawGizmosSelected()
    {
        if (ghost.detectPlayerSphere == null)
        {
            ghost.detectPlayerSphere = transform;
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(ghost.detectPlayerSphere.position, radius);
    }

    public IEnumerator SpotTimer()
    {
        //Debug.Log("Starting Attack Timer");
        yield return new WaitForSeconds(5f);
    }

    public void StartAttackTimer()
    {
        StartCoroutine(SpotTimer());
    }

    // accesses and sets a waypoint destination, turns on travelling
    public void SetDestination()
    {
        if(_patrolPoints != null)
        {
            Vector3 targetVector = _patrolPoints[_currentPatrolIndex].transform.position;
            //navMeshAgent.SetDestination(targetVector);
            ghost.navAgent.SetDestination(targetVector);
            //Debug.Log(targetVector);
            _travelling = true;
        }
    }

    // Randomly chooses a waypoint from a list of known points.
    public void ChangePatrolPoint()
    {
        if (UnityEngine.Random.Range(0f, 1f) <= _switchProbability)
        {
            //moves either forward or backwards
            _patrolForward = !_patrolForward;
        }

        if (_patrolForward)
        {
            _currentPatrolIndex++;

            if(_currentPatrolIndex >= _patrolPoints.Count)
            {
                _currentPatrolIndex = 0;
            }    

            // the chad version of the above code. 
            //_currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Count;
        }
        else
        {
            _currentPatrolIndex--;

            if(_currentPatrolIndex < 0)
            {
                _currentPatrolIndex = _patrolPoints.Count - 1;
            }

            //if (--_currentPatrolIndex < 0)
            //{
            //    _currentPatrolIndex = _patrolPoints.Count - 1;
            //}
        }
    }

}

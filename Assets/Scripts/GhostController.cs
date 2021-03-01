using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AI;


public class GhostController : MonoBehaviour
{
    [SerializeField] public NavMeshAgent navAgent;
    [SerializeField] public Transform detectPlayerSphere;
    [SerializeField] public LayerMask GroundLayer;
    [SerializeField] private Transform player = null;
    [SerializeField] public LayerMask PlayerLayer;
    [SerializeField] public Rigidbody rbody;
    //public GameObject eObj;
    public float distanceToGround = 0.5f;

    [Header("States")]
    public bool enableAI = false;
    public AI currentAI;
    public bool onGround;


    //void Awake()
    //{
    //    navAgent = GetComponent<NavMeshAgent>();
    //    //GroundLayer = (1 << 10);
    //    eObj = GameObject.Find("enemybody");
    //    Debug.Log(eObj);
    //}

    // Start is called before the first frame update
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        rbody = GetComponent<Rigidbody>();
        //GroundLayer = (1 << 10);
        //eObj = GameObject.Find("enemybody");
        //Debug.Log(eObj);
    }


    // Update is called once per frame
    void Update()
    {
        if (currentAI.OnGround() == true)
        {
            rbody.isKinematic = true;
            onGround = true;
        }
        else
        {
            onGround = false;
            rbody.isKinematic = false;
        }
    }
    public void FacePlayer()
    {
        transform.LookAt(player.position);
    }
    public void FaceTarget()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
    public Vector3 getPlayerPosition()
    {
        return player.position;
    }
    public void SetDestination()
    {
        navAgent.SetDestination(player.position);

        //Debug.Log(player.position);
    }

    public void MoveAt(Vector3 Direction)
    {
        navAgent.SetDestination(Direction);
    }

    public void StopMoving()
    {
        navAgent.SetDestination(rbody.position);
    }
}

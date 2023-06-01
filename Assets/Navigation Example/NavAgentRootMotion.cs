using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentRootMotion : MonoBehaviour
{
    // Inspector Assigned Variable
    public AIWaypointNetwork WaypointNetwork = null;
    public int CurrentIndex = 0;
    public bool HasPath = false;
    public bool PathPending = false;
    public bool PathStale = false;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve JumpCurve = new AnimationCurve();
    public bool MixedMode = true;
    // Private Members
    private NavMeshAgent _navAgent = null;
    private Animator _animator = null;
    private float _smoothAngle = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        // Cache NavMeshAgent Reference
        _navAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();

        // Turn off auto-update
        //_navAgent.updatePosition = false;
        _navAgent.updateRotation = false;

        // If not valid Waypoint Network has been assigned then return
        if (WaypointNetwork == null) return;

        // Set first waypoint
        SetNextDestination(false);
    }

    void SetNextDestination(bool increment)
    {
        if (!WaypointNetwork) return;
        int incStep = increment ? 1 : 0;
        Transform nextWaypointTransform = null;
        
        int nextWaypoint = (CurrentIndex + incStep >= WaypointNetwork.Waypoints.Count)?0:CurrentIndex+incStep;
        nextWaypointTransform = WaypointNetwork.Waypoints[nextWaypoint];
        if (nextWaypointTransform != null)
        {
            CurrentIndex = nextWaypoint;
            _navAgent.destination = nextWaypointTransform.position;
            return;
        }
        CurrentIndex++;            
    }
    // Update is called once per frame
    void Update()
    {
        // Copy NavMeshAgents state into inspector visible variables
        HasPath = _navAgent.hasPath;
        PathPending = _navAgent.pathPending;
        PathStale = _navAgent.isPathStale;
        PathStatus = _navAgent.pathStatus;

        Vector3 localDesiredVelocity = transform.InverseTransformVector(_navAgent.desiredVelocity);
        float angle = Mathf.Atan2(localDesiredVelocity.x,localDesiredVelocity.z)*Mathf.Rad2Deg;
        _smoothAngle = Mathf.MoveTowardsAngle(_smoothAngle,angle,80.0f*Time.deltaTime);
        float speed = localDesiredVelocity.z;
        _animator.SetFloat("Angle", _smoothAngle);
        _animator.SetFloat("Speed", speed, 0.1f,Time.deltaTime);

        if(_navAgent.desiredVelocity.sqrMagnitude>Mathf.Epsilon)
        {
            if (!MixedMode ||
                (MixedMode && Mathf.Abs(angle)<80.0f && _animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion")))
            {
                Quaternion lookRotation = Quaternion.LookRotation(_navAgent.desiredVelocity, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
            }            
        }

        // If agent is on an offmesh link then perform a jump 
        //if(_navAgent.isOnOffMeshLink)
        //{
        //    StartCoroutine(Jump(2.0f));
        //    return;
        //}

        // If we don't have a path and one isn't pending then set the next
        if (_navAgent.remainingDistance.Equals(0.0f)&&!PathPending||PathStatus==NavMeshPathStatus.PathInvalid/*||PathStatus==NavMeshPathStatus.PathPartial*/)
            SetNextDestination(true);
        else if (_navAgent.isPathStale)        
            SetNextDestination(false);       
    }
    void OnAnimatorMove()
    {
        if (MixedMode && !_animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion"))
            transform.rotation = _animator.rootRotation;
        _navAgent.velocity = _animator.deltaPosition / Time.deltaTime;
    }
    IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + (_navAgent.baseOffset*Vector3.up);
        float time = 0.0f;

        while(time<=duration)
        {
            float t = time / duration;
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t);
            time += Time.deltaTime;
            yield return null;
        }
    }
}

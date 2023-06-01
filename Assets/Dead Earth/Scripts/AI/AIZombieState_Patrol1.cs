using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AIZombieState_Patrol1 : AIZombieState
{
    // Inspector Assigned

    [SerializeField] float _turnOnSpotThreshold = 80.0f;
    [SerializeField] float _slerpSpeed = 5.0f;
    [SerializeField][Range(0.0f,3.0f)] float _speed = 1.0f;
    public override AIStateType GetStateType()
    {
        return AIStateType.Patrol;
    }

    public override void OnEnterState()
    {
        base.OnEnterState();
        if (_zombieStateMachine == null)
            return;
        // Configure State Machine
        _zombieStateMachine.NavAgentControl(true, false);
        _zombieStateMachine.speed = _speed;
        _zombieStateMachine.seeking = 0;
        _zombieStateMachine.feeding = false;
        _zombieStateMachine.attackType = 0;
        // Set Destination
        _zombieStateMachine.navAgent.SetDestination(_zombieStateMachine.GetWaypointPosition(false));
        // Make sure NavAgent is switched on
        _zombieStateMachine.navAgent.Resume();
    }
    public override AIStateType OnUpdate()
    {
        // Do we have a visual threat that is the player
        if(_zombieStateMachine.VisualThreat.type==AITargetType.Visual_Player)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Pursuit;
        }
        if(_zombieStateMachine.VisualThreat.type==AITargetType.Visual_Light)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.VisualThreat);
            return AIStateType.Alerted;
        }
        // Sound is the third highest priority
        if(_zombieStateMachine.AudioThreat.type==AITargetType.Audio)
        {
            _zombieStateMachine.SetTarget(_zombieStateMachine.AudioThreat);
            return AIStateType.Alerted;
        }
        // We have seen a dead body so lets pursue that if we are hungry enough
        if(_zombieStateMachine.VisualThreat.type==AITargetType.Visual_Food)
        {
            // If the distance to hunger ratio means we are hungry enough to stray off the path that far
            if ((1.0f-_zombieStateMachine.satisfaction)>(_zombieStateMachine.VisualThreat.distance/_zombieStateMachine.sensorRadius))
            {
                _stateMachine.SetTarget(_stateMachine.VisualThreat);
                return AIStateType.Pursuit;
            }
        }

        float angle = Vector3.Angle(_zombieStateMachine.transform.forward,(_zombieStateMachine.navAgent.steeringTarget-_zombieStateMachine.transform.position));
        
        if(Mathf.Abs(angle)>_turnOnSpotThreshold)
        {
            return AIStateType.Alerted;
        }

        if(!_zombieStateMachine.useRootRotation)
        {
            Quaternion newRot = Quaternion.LookRotation(_zombieStateMachine.navAgent.desiredVelocity);
            _zombieStateMachine.transform.rotation = Quaternion.Slerp(_zombieStateMachine.transform.rotation, newRot, Time.deltaTime * _slerpSpeed);

        }
        if(_zombieStateMachine.navAgent.isPathStale||
            !_zombieStateMachine.navAgent.hasPath||
            _zombieStateMachine.navAgent.pathStatus!=NavMeshPathStatus.PathComplete)
        {
            _zombieStateMachine.GetWaypointPosition(true);
        }
        // Stay in Patrol State
        return AIStateType.Patrol;
    }
    public override void onDestinationReached(bool isReached)
    {
        if(_zombieStateMachine==null||!isReached)
        {
            return;
        }
        if(_zombieStateMachine.targetType==AITargetType.Waypoint)
        {
            _zombieStateMachine.GetWaypointPosition(true);
        }
    }
    //public override void OnAnimatorIKUpdated() 
    //{
    //    if (_zombieStateMachine == null)
    //        return;
    //    _zombieStateMachine.animator.SetLookAtPosition(_zombieStateMachine.targetPosition+Vector3.up);
    //    _zombieStateMachine.animator.SetLookAtWeight(0.55f);
    //}
}

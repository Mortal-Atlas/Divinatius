using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Divinatius.Dialogue;

namespace Divinatius.NPC
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NPCWanderer : MonoBehaviour
    {
        [Header("Wander Settings")]
        [SerializeField] private float wanderRadius = 15.0f;
        [SerializeField] private float walkSpeed = 2.0f;
        [SerializeField] private float minIdleTime = 3.0f;
        [SerializeField] private float maxIdleTime = 7.0f;

        private Vector3 _homePosition;
        private NavMeshAgent _navMeshAgent;
        private bool _isDialoguePaused = false;
        private Transform _interactingPlayer;

        private void Start()
        {
            _homePosition = transform.position;
            _navMeshAgent = GetComponent<NavMeshAgent>();

            if (_navMeshAgent == null)
            {
                _navMeshAgent = gameObject.AddComponent<NavMeshAgent>();
            }

            _navMeshAgent.speed = walkSpeed;
            _navMeshAgent.stoppingDistance = 0.5f;
            _navMeshAgent.radius = 0.5f;
            _navMeshAgent.height = 2.0f;
            _navMeshAgent.acceleration = 8.0f;
            _navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

            StartCoroutine(WanderRoutine());
        }

        private IEnumerator WanderRoutine()
        {
            while (true)
            {
                if (_isDialoguePaused)
                {
                    yield return null;
                    continue;
                }

                // 1. Idle Phase
                if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
                {
                    _navMeshAgent.isStopped = true;
                }

                float waitTime = Random.Range(minIdleTime, maxIdleTime);
                float timer = 0f;
                while (timer < waitTime && !_isDialoguePaused)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (_isDialoguePaused) continue;

                // 2. Pick NavMesh Destination
                Vector3 newTarget = GetRandomNavMeshDestination();

                if (_navMeshAgent != null && _navMeshAgent.enabled)
                {
                    if (!_navMeshAgent.isOnNavMesh)
                    {
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
                        {
                            _navMeshAgent.Warp(hit.position);
                        }
                    }

                    if (_navMeshAgent.isOnNavMesh)
                    {
                        _navMeshAgent.isStopped = false;
                        _navMeshAgent.SetDestination(newTarget);
                    }
                }

                // 3. Move until destination reached or timeout
                float moveTimeout = 15.0f;
                float moveTimer = 0f;

                while (!_isDialoguePaused && moveTimer < moveTimeout)
                {
                    moveTimer += Time.deltaTime;

                    if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
                    {
                        if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                        {
                            break; // Arrived at destination
                        }
                    }
                    else
                    {
                        // Fallback movement with SphereCast Obstacle Avoidance
                        Vector3 dir = (newTarget - transform.position);
                        dir.y = 0;
                        if (dir.magnitude < 0.6f) break;

                        Vector3 moveForward = dir.normalized;

                        // Raycast/SphereCast obstacle check in front of NPC (e.g. fountain, buildings)
                        if (Physics.SphereCast(transform.position + Vector3.up * 0.8f, 0.45f, moveForward, out RaycastHit obstacleHit, 1.2f))
                        {
                            if (obstacleHit.collider != null && obstacleHit.collider.gameObject != gameObject && !obstacleHit.collider.isTrigger)
                            {
                                // Steer away from obstacle or cancel current path
                                Vector3 avoidDir = Vector3.Reflect(moveForward, obstacleHit.normal);
                                avoidDir.y = 0;
                                newTarget = transform.position + avoidDir.normalized * 5f;
                                break;
                            }
                        }

                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveForward), Time.deltaTime * 6f);
                        transform.position += transform.forward * walkSpeed * Time.deltaTime;
                    }

                    yield return null;
                }
            }
        }

        private Vector3 GetRandomNavMeshDestination()
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += _homePosition;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return _homePosition + new Vector3(randomDirection.x, 0, randomDirection.z);
        }

        private Vector3 _externalPushVelocity = Vector3.zero;

        public void ReceivePush(Vector3 force)
        {
            force.y = 0;
            _externalPushVelocity = force;
        }

        private void Update()
        {
            if (_externalPushVelocity.sqrMagnitude > 0.001f)
            {
                Vector3 moveDelta = _externalPushVelocity * Time.deltaTime;
                if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
                {
                    _navMeshAgent.Move(moveDelta);
                }
                else
                {
                    transform.position += moveDelta;
                }
                _externalPushVelocity = Vector3.Lerp(_externalPushVelocity, Vector3.zero, Time.deltaTime * 6.0f);
            }

            if (_isDialoguePaused && _interactingPlayer != null)
            {
                // Smoothly face the player during dialogue
                Vector3 lookDir = _interactingPlayer.position - transform.position;
                lookDir.y = 0;
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    Quaternion rot = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 8.0f);
                }
            }
        }

        public void PauseWandering(Transform player)
        {
            _isDialoguePaused = true;
            _interactingPlayer = player;

            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.isStopped = true;
            }
        }

        public void ResumeWandering()
        {
            _isDialoguePaused = false;
            _interactingPlayer = null;

            if (_navMeshAgent != null && _navMeshAgent.enabled && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.isStopped = false;
            }
        }
    }
}

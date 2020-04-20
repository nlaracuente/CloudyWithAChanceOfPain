using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Wolf : MonoBehaviour
{   
    enum State
    {
        Stalking,
        Walking,
        Running,
        Attacking,
        Diying,
    };

    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] float walkingSpeed = 5f;
    [SerializeField] float runningSpeed = 5f;
    [SerializeField] Animator animController;
    [SerializeField] Camera killCamera;

    // Particle Effects
    [SerializeField] ParticleSystem fireParticleSystem;

    // Sounds Clips
    [SerializeField] List<AudioClipInfo> howlingsClipInfos;
    [SerializeField] List<AudioClipInfo> attacksClipInfo;
    [SerializeField] List<AudioClipInfo> deathsClipInfo;
    [SerializeField] List<AudioClipInfo> laughsClipInfo;

    IEnumerator activeCoroutine;
    State state;

    Sheep targetSheep;

    private void Awake()
    {
        if (navMeshAgent == null)
            navMeshAgent = GetComponent<NavMeshAgent>();

        killCamera.enabled = false;
    }

    void Start()
    {
        state = State.Stalking;
        targetSheep = FindObjectOfType<Sheep>();
        fireParticleSystem?.Stop();
    }

    private void Update()
    {
        if (LevelController.Instance.IsGameOver || targetSheep.IsNotMoving)
        {
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.isStopped = true;

            animController.SetBool("Walking", true);
            animController.SetBool("Running", false);
            return;
        }

        navMeshAgent.isStopped = false;
        animController.SetBool("Walking", !targetSheep.IsRunning);
        animController.SetBool("Running", targetSheep.IsRunning);

        navMeshAgent.speed = targetSheep.IsRunning ? runningSpeed : walkingSpeed;
        navMeshAgent.SetDestination(targetSheep.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if(other.gameObject.GetComponent<Sheep>())
    }
}

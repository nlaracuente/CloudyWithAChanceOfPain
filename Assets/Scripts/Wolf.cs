using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Wolf : MonoBehaviour, IAttackable, IBurnable, IShockable
{   
    enum State
    {
        Idling,
        Chasing,
        Attacking,
        Diying,
    };

    [SerializeField] NavMeshAgent navMeshAgent;
    [SerializeField] float chasingSpeed = 8f;
    [SerializeField] float distanceToSheep = 1f;
    [SerializeField] float attackDuration = 3f;
    [SerializeField] float deathDuration = 2f;
    [SerializeField] Animator animController;
    [SerializeField] Camera killCamera;
    [SerializeField] WolfMouthTrigger mouthTrigger;

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

        if (mouthTrigger == null)
            mouthTrigger = GetComponentInChildren<WolfMouthTrigger>();

        killCamera.enabled = false;
    }

    void Start()
    {
        ChangeRoutine(FindTargetRoutine());
    }

    void ChangeRoutine(IEnumerator routine)
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = routine;
        StartCoroutine(activeCoroutine);
    }

    IEnumerator FindTargetRoutine()
    {
        if (LevelController.Instance.IsGameOver)
            ChangeRoutine(IdleRoutine());

        state = State.Idling;
        animController.SetBool("Idling", true);
        fireParticleSystem?.Stop();

        while (targetSheep == null && !targetSheep.IsDead)
        {
            yield return new WaitForEndOfFrame();
            targetSheep = SheepManager.Instance.GetTargetSheep();

            if (LevelController.Instance.IsGameOver)
            {
                ChangeRoutine(IdleRoutine());
                break;
            }   
        }

        ChangeRoutine(ChaseRoutine());
    }

    IEnumerator ChaseRoutine()
    {
        state = State.Chasing;
        animController.SetBool("Running", true);
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = chasingSpeed;

        while (!LevelController.Instance.IsGameOver
               && (targetSheep != null || Vector3.Distance(transform.position, targetSheep.transform.position) > distanceToSheep))
            yield return new WaitForEndOfFrame();

        if (targetSheep == null)
            ChangeRoutine(FindTargetRoutine());
        else
            ChangeRoutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        state = State.Attacking;
        animController.SetTrigger("Attack");
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;

        yield return new WaitForSeconds(attackDuration);

        var sheep = mouthTrigger?.SheepInTrigger;
        if (sheep == null || sheep.IsDead)
            ChangeRoutine(FindTargetRoutine());

        SheepManager.Instance.SheepDied(sheep);

        ChangeRoutine(FindTargetRoutine());
    }

    IEnumerator IdleRoutine()
    {
        state = State.Idling;
        animController.SetBool("Idling", true);
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        yield return null;
    } 

    IEnumerator DeathRoutine()
    {
        state = State.Diying;
        animController.SetTrigger("Death");
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        yield return new WaitForSeconds(deathDuration);
        Destroy(gameObject);

    }

    public void StruckedByLightning()
    {
        TriggerDeath();
    }

    public void Shocked()
    {
        TriggerDeath();
    }

    public void Burn()
    {
        TriggerDeath();
    }

    void TriggerDeath()
    {
        if (state != State.Diying)
            ChangeRoutine(DeathRoutine());
    }
}

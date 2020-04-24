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
    [SerializeField] float attackDuration = 1f;
    [SerializeField] float attackSoundDelay = .25f;
    [SerializeField] float deathDuration = 2f;
    [SerializeField] float coolDownDuration = 2f;
    [SerializeField] Animator animController;
    [SerializeField] Camera killCamera;
    [SerializeField] WolfMouthTrigger mouthTrigger;

    // Particle Effects
    [SerializeField] ParticleSystem fireParticleSystem;
    [SerializeField] ParticleSystem smokeParticleSystem;

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

    private void Start()
    {
        state = State.Idling;
        animController.SetBool("Idling", true);
        fireParticleSystem?.Stop();
        smokeParticleSystem?.Stop();
        ChangeRoutine(FindTargetRoutine());
        AudioManager.Instance.PlayRandom2DClip(howlingsClipInfos);
    }

    public void SetInitialTarget(Sheep sheep)
    {
        targetSheep = sheep;
        ChangeRoutine(ChaseRoutine());
        AudioManager.Instance.PlayRandom2DClip(howlingsClipInfos);
    }

    public void SetKillCamera()
    {
        killCamera.enabled = true;
        LevelController.Instance.MainCamera = killCamera;
        Camera.main.enabled = false;
        AudioManager.Instance.PlayRandom2DClip(laughsClipInfo);
    }

    void ChangeRoutine(IEnumerator routine)
    {
        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = routine;
        StartCoroutine(activeCoroutine);
        SetAnimatorToCurrentState();
    }

    void SetAnimatorToCurrentState()
    {
        animController.SetBool("Idling", state == State.Idling);
        animController.SetBool("Running", state == State.Chasing);
    }

    IEnumerator FindTargetRoutine()
    {
        if (LevelController.Instance.IsGameOver)
            ChangeRoutine(GameOverRoutine()); 
        else
        {
            state = State.Idling;
            animController.SetBool("Idling", true);
            smokeParticleSystem?.Stop();

            targetSheep = null;
            while (targetSheep == null)
            {
                yield return new WaitForEndOfFrame();
                targetSheep = SheepManager.Instance.GetSheepToTarget();

                if (LevelController.Instance.IsGameOver)
                    ChangeRoutine(GameOverRoutine());
                else if (targetSheep != null && targetSheep.IsDead)
                    ChangeRoutine(AttackCoolDownRoutine());
                else if (targetSheep != null)
                    ChangeRoutine(ChaseRoutine());
            }
        }
    }

    IEnumerator ChaseRoutine()
    {
        state = State.Chasing;
        animController.SetBool("Running", true);
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = chasingSpeed;

        while (targetSheep != null && Vector3.Distance(targetSheep.transform.position, transform.position) > distanceToSheep){
            if(LevelController.Instance.IsGameOver || targetSheep.IsDead)
                break;

            navMeshAgent.SetDestination(targetSheep.transform.position);
            yield return new WaitForEndOfFrame();
        }

        if (LevelController.Instance.IsGameOver)
            ChangeRoutine(GameOverRoutine());
        else if(targetSheep == null || targetSheep.IsDead)
            ChangeRoutine(FindTargetRoutine());
        else
            ChangeRoutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        if (targetSheep.IsDead)
            ChangeRoutine(FindTargetRoutine());
        else if (!LevelController.Instance.IsGameOver)
        {
            state = State.Attacking;
            animController.SetTrigger("Attack");
            navMeshAgent.isStopped = true;
            navMeshAgent.velocity = Vector3.zero;

            transform.LookAt(targetSheep.transform);

            yield return new WaitForSeconds(attackSoundDelay);
            AudioManager.Instance.PlayRandom2DClip(attacksClipInfo);
            yield return new WaitForSeconds(attackDuration);

            if (LevelController.Instance.IsGameOver)
                ChangeRoutine(GameOverRoutine());
            else 
            {
                var sheep = mouthTrigger.SheepInTrigger;
                if (sheep != null && !sheep.IsDead)
                    SheepManager.Instance.SheepDied(sheep, this);

                ChangeRoutine(AttackCoolDownRoutine());
            }            
        }
    }    

    IEnumerator AttackCoolDownRoutine()
    {
        state = State.Idling;
        animController.SetBool("Idling", true);
        animController.SetBool("Running", false);
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        SheepManager.Instance.AddSheep(targetSheep);
        yield return new WaitForSeconds(coolDownDuration);
        ChangeRoutine(FindTargetRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        state = State.Idling;
        animController.SetBool("Idling", true);
        animController.SetBool("Running", false);

        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        yield return null;
    }

    IEnumerator DeathRoutine()
    {
        SheepManager.Instance.AddSheep(targetSheep);
        smokeParticleSystem?.Play();

        state = State.Diying;
        animController.SetTrigger("Death");
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.enabled = false;
        AudioManager.Instance.PlayRandom2DClip(deathsClipInfo);
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

    public void SetNewTarget(Sheep sheep)
    {
        if (sheep == targetSheep || sheep.IsDead || LevelController.Instance.IsGameOver)
            return;

        if (sheep == null || targetSheep == null)
            return;

        var curDis = Vector3.Distance(targetSheep.transform.position, transform.position);
        var newDis = Vector3.Distance(sheep.transform.position, transform.position);

        if (newDis < curDis)
        {
            SheepManager.Instance.AddSheep(targetSheep);
            targetSheep = sheep;
        }
            
    }
}

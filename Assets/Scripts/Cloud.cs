using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cloud : Singleton<Cloud>
{
    enum State
    {
        Idle,
        Looking,
        Lightning,
        Raining,
        Happy,
        Angry,
    }

    enum Effect
    {
        Lightining,
        Rain,
    }

    [System.Serializable]
    struct StateSprites
    {
        public State state;
        public Sprite sprite;
    }

    [SerializeField]
    LayerMask clickableLayer;

    [SerializeField]
    SpriteRenderer spriteRenderer;

    [SerializeField]
    float trackingSpeed;

    [SerializeField, Tooltip("How long to play lightining effect"), Range(0.1f, 1f)]
    float lightningTime = .5f;

    [SerializeField]
    CloudParticleEffect lightningEffect;

    [SerializeField]
    CloudParticleEffect rainEffect;

    [SerializeField]
    List<AudioClipInfo> lightningClipInfos;

    [SerializeField]
    AudioClipInfo rainClipInfo;    

    [SerializeField]
    List<StateSprites> sprites;

    State state;

    int leftClick = 0;
    int rightClick = 1;

    float lightningMaxTime;

    AudioSource rainAudioSource;

    /// <summary>
    /// Mouse current position in world space
    /// </summary>
    /// <returns></returns>
    public Vector3 MouseWorldPosition
    {
        get
        {
            var pos = Input.mousePosition;
            pos.z = LevelController.Instance.MainCamera.WorldToScreenPoint(transform.position).z;
            return LevelController.Instance.MainCamera.ScreenToWorldPoint(pos);
        }
    }

    /// <summary>
    /// Default state to idle
    /// </summary>
    private void Start()
    {
        ChangeState(State.Idle);
    }

    private void Update()
    {
        if (LevelController.Instance.IsGameOver ||
            LevelController.Instance.IsPaused)
            return;

        FollowMouse();        

        if (Input.GetMouseButtonDown(leftClick))
        {
            if (GetClickableObjectUnderMouse() == null)
                return;

            ChangeState(State.Lightning);            
            PlayEffect(Effect.Lightining);
            TriggerEffect(Effect.Lightining);
        }
        else if (Input.GetMouseButton(rightClick))
        {
            if (GetClickableObjectUnderMouse() == null)
                return;

            ChangeState(State.Raining);
            PlayEffect(Effect.Rain);
            TriggerEffect(Effect.Rain);
        }
        else if (lightningMaxTime < Time.time && state != State.Idle && state != State.Looking)
        {
            // Enough time has passed for the ligthning effect
            // Randomly choose
            if (Random.Range(0, 2) == 1)
                ChangeState(State.Looking);
            else
                ChangeState(State.Idle);

            lightningEffect.Stop();
            rainEffect.Stop();
        }
    }

    void ChangeState(State newState)
    {
        state = newState;
        var sprite = sprites.Where(s => s.state == newState)
                            .Select(ss => ss.sprite)
                            .First();

        // Can be called multiple times for the same state
        // Avois resetting gif animation
        if (sprite != spriteRenderer.sprite)
            spriteRenderer.sprite = sprite;

        // Reset lightning timer
        if (state == State.Lightning)
            lightningMaxTime = Time.time + lightningTime;
        else
            lightningMaxTime = 0;

        // Stop rain
        if (state != State.Raining && rainAudioSource != null)
            rainAudioSource.Stop();
    }

    void FollowMouse()
    {
        var dest = MouseWorldPosition;
        dest.x = Mathf.Clamp(dest.x, -35f, 35f);
        dest.z = transform.position.z;
        dest.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, dest, trackingSpeed * Time.deltaTime);
    }

    void PlayEffect(Effect effect)
    {
        var go = GetClickableObjectUnderMouse();

        if (go == null)
            return;

        var xPos = go.transform.position;
        var position = new Vector3(xPos.x, 0, xPos.z);

        if (effect == Effect.Lightining)
        {
            position.y = lightningEffect.transform.position.y;
            lightningEffect.transform.position = position;

            var info = AudioManager.Instance.GetRandomAudioClipInfo(lightningClipInfos);
            AudioManager.Instance.Play2DSound(info.clip, info.volume);
            lightningEffect.Play();
        } 
        else 
        {
            position.y = rainEffect.transform.position.y;
            rainEffect.transform.position = position;

            if (rainAudioSource == null)
            {
                // Since audio and effect loop
                // Only need to play them once
                rainAudioSource = AudioManager.Instance.Play2DSound(rainClipInfo.clip, rainClipInfo.volume);
                rainAudioSource.loop = true;
                rainEffect.Play();
            }   
        }
    }

    public GameObject GetClickableObjectUnderMouse()
    {
        Ray ray = LevelController.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
            return null;

        return hit.transform?.gameObject;
    }

    public GrassFieldTile GetGrassUnderMouse()
    {
        Ray ray = LevelController.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
        var hits = Physics.RaycastAll(ray, Mathf.Infinity, clickableLayer);

        foreach (var hit in hits)
        {
            if (hit.transform?.GetComponentInParent<GrassFieldTile>())
                return hit.transform.GetComponentInParent<GrassFieldTile>();
        }

        return null;
    }

    void TriggerEffect(Effect effect)
    {
        Ray ray = LevelController.Instance.MainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayer))
            return;

        if(effect == Effect.Lightining)
        {
            IAttackable attackable = hit.collider.GetComponent<IAttackable>();
            attackable?.StruckedByLightning();
        } else
        {
            IDousable dousable = hit.collider.GetComponent<IDousable>();
            dousable?.RainedOn();
        }        
    }
}

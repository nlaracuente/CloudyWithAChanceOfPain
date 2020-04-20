using UnityEngine;

public class WaterResource : BaseResource
{
    enum State
    {
        Empty,
        Full,
    }

    State state;

    [SerializeField]
    State defaultState;

    [SerializeField]
    GameObject emptyPrefab;

    [SerializeField]
    GameObject fullPrefab;

    [SerializeField]
    AudioClipInfo waterZappedClipInfo;

    [SerializeField]
    AudioClipInfo fillingUpClipInfo;

    bool IsSetAsUnconsumable { get; set; }

    override public bool IsConsumable { get { return !IsSetAsUnconsumable && state == State.Full; } }
    override public Resource ResourceType { get { return Resource.Water; } }

    /// <summary>
    /// Initialize
    /// </summary>
    protected override void Start()
    {
        base.Start();
        state = defaultState;
        emptyPrefab.SetActive(state == State.Empty);
        fullPrefab.SetActive(state == State.Full);
    }

    void ChangeState(State newState)
    {
        // Already it
        if (state == newState)
            return;

        state = newState;

        // Only play effect on state change
        emptyPrefab.SetActive(newState == State.Empty);
        fullPrefab.SetActive(newState == State.Full);
        if (newState == State.Empty)
            AudioManager.Instance.Play2DSound(waterZappedClipInfo.clip, waterZappedClipInfo.volume);
        else
            AudioManager.Instance.Play2DSound(fillingUpClipInfo.clip, fillingUpClipInfo.volume);
    }

    override public void Consume()
    {
        ChangeState(State.Empty);
        IsSetAsUnconsumable = false;
    }

    override public void RainedOn()
    {
        if (!IsConsumable)
            ChangeState(State.Full);
    }

    override public void StruckedByLightning()
    {
        if (IsConsumable)
            ChangeState(State.Empty);
    }

    public override void SetAsNotConsumable()
    {
        IsSetAsUnconsumable = true;
    }
}

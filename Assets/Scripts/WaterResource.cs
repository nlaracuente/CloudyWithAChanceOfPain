using System.Collections;
using UnityEngine;

public class WaterResource : MonoBehaviour, IConsumable
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

    public bool IsConsumable { get { return state == State.Full; } }
    public Resource ResourceType { get { return Resource.Water; } }

    /// <summary>
    /// Initialize
    /// </summary>
    private void Start()
    {
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

    public void Consume()
    {
        if(IsConsumable)
            ChangeState(State.Empty);
    }

    public void RainedOn()
    {
        if (!IsConsumable)
            ChangeState(State.Full);
    }

    public void StruckedByLightning()
    {
        if (IsConsumable)
            ChangeState(State.Empty);
    }
}

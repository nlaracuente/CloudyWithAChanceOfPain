﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CropTile : BaseResource, IConsumable, IBurnable
{
    enum State 
    { 
        Soil,
        SoilWatered,
        SoilBurning,
        SoilBurned,
        Seeded,
        SeededWatered,
        Food,
        FoodWatered,
        FoodBurning,
        FoodBurned,
        FoodBurnedWatered,
    }

    [System.Serializable]
    struct StateGameObjects
    {
        public State state;
        public GameObject prefab;
    }

    [SerializeField]
    State defaultState = State.Food;

    [SerializeField]
    float transitionDelay = 5f;

    [SerializeField]
    ParticleSystem smokeParticles;

    [SerializeField]
    List<AudioClipInfo> firePutOutInfoClips;

    [SerializeField]
    List<AudioClipInfo> flameInfoClips;

    [SerializeField]
    List<StateGameObjects> gameObjects = new List<StateGameObjects>();

    State state;

    IEnumerator curRoutine;

    AudioSource burningAudioSource;

    /// <summary>
    /// States that trigger a routine to move to the next/prev state
    /// </summary>
    List<State> routineStates = new List<State>() 
    {
        State.Soil,
        State.SoilWatered,
        State.SoilBurning,
        State.SeededWatered,
        State.FoodWatered,
        State.FoodBurning,
        State.FoodBurnedWatered,
    };

    List<State> burningStates = new List<State>()
    {
        State.SoilBurning,
        State.FoodBurning,
    };

    List<State> burnableStates = new List<State>()
    {
        State.Soil,
        State.Food,
    };

    override public Resource ResourceType { get { return Resource.Food; } }

    override public bool IsConsumable
    {
        get { return !IsSetAsUnconsumable && (state == State.Food || state == State.FoodWatered); }
    }

    bool IsSetAsUnconsumable { get; set; }

    protected override void Start()
    {
        base.Start();
        ChangeState(defaultState);
    }

    void ChangeState(State newState)
    {
        // Kill the current before starting a new one
        if (curRoutine != null)
            StopCoroutine(curRoutine);

        // Fire is out - show smokes
        if (burningStates.Contains(state))
            smokeParticles?.Play();

        state = newState;

        // Disable all first
        gameObjects?.ForEach(g => g.prefab.SetActive(false));

        // Enable the one that matches the new state
        gameObjects?.Where(g => g.state == newState)
                    .First().prefab.SetActive(true);

        // Tigger a routine to change state if it qualifies for one
        if (routineStates.Contains(newState))
        {
            curRoutine = StateRoutine();
            StartCoroutine(curRoutine);
        }
    }

    /// <summary>
    /// Transitions into the next state after a while
    /// </summary>
    /// <returns></returns>
    IEnumerator StateRoutine()
    {
        yield return new WaitForSeconds(transitionDelay);

        // Based on the current state determines which state to move to
        var newState = state; // Default to current

        switch (state)
        {
            case State.Soil:
            case State.SoilWatered:
                newState = State.Seeded;
                break;

            case State.SoilBurning:
                newState = State.SoilBurned;
                break;

            case State.SeededWatered:
                newState = State.Food;
                break;

            case State.FoodWatered:
                newState = State.Food;
                break;

            case State.FoodBurning:
                newState = State.FoodBurned;
                break;

            case State.FoodBurned:
            case State.FoodBurnedWatered:
                newState = State.Soil;
                break;
        }

        //var next = (int)state + 1;
        //var total = Enum.GetNames(typeof(State)).Length;
        //var index = next < total ? next : 0;

        ChangeState(newState);
    }

    override public void RainedOn()
    {
        if (burningStates.Contains(state))
            AudioManager.Instance.PlayRandom2DClip(firePutOutInfoClips);

        switch (state)
        {
            case State.Soil:
                ChangeState(State.SoilWatered);
                break;

            case State.SoilBurning:
            case State.SoilBurned:
                ChangeState(State.Soil);
                break;

            case State.Seeded:
                ChangeState(State.SeededWatered);
                break;
           
            case State.Food:
                ChangeState(State.FoodWatered);
                break;

            case State.FoodBurning:
                ChangeState(State.Food);
                break;

            case State.FoodBurned:
                ChangeState(State.FoodBurnedWatered);
                break;
        }        
    }

    override public void StruckedByLightning()
    {
        if (burnableStates.Contains(state) && burningAudioSource == null)
            AudioManager.Instance.PlayRandom2DClip(flameInfoClips);

        switch (state)
        {
            case State.Soil:
                ChangeState(State.SoilBurning);
                break;

            case State.Seeded:
            case State.SeededWatered:
                ChangeState(State.Soil);
                break;

            case State.Food:
                ChangeState(State.FoodBurning);
                break;

            case State.FoodWatered:
                ChangeState(State.Food);
                break;

            case State.FoodBurnedWatered:
                ChangeState(State.FoodBurned);
                break;
        }
    }

    override public void Consume()
    {
        if (state == State.Food)
            ChangeState(State.Soil);
        else if (state == State.FoodWatered)
            ChangeState(State.SoilWatered);

        IsSetAsUnconsumable = false;
    }

    public void Burn()
    {
        StruckedByLightning();
    }

    public override void SetAsNotConsumable()
    {
        IsSetAsUnconsumable = true;
    }
}

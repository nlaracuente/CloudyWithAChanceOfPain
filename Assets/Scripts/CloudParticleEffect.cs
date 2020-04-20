using UnityEngine;

public class CloudParticleEffect : MonoBehaviour
{
    [SerializeField]
    ParticleSystem pSystem;

    private void Awake()
    {
        if (pSystem == null)
            pSystem = GetComponentInChildren<ParticleSystem>();
    }

    public void Play()
    {
        pSystem?.Play();
    }

    public void Stop()
    {
        pSystem?.Stop();
    }
}

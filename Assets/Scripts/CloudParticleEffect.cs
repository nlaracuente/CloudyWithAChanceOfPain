using UnityEngine;

public class CloudParticleEffect : MonoBehaviour
{
    [SerializeField]
    ParticleSystem particleSystem;

    private void Awake()
    {
        if (particleSystem == null)
            particleSystem = GetComponentInChildren<ParticleSystem>();
    }

    public void Play()
    {
        particleSystem?.Play();
    }

    public void Stop()
    {
        particleSystem?.Stop();
    }
}

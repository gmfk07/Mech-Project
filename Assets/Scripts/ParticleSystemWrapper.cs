using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemWrapper : MonoBehaviour
{
    public ParticleSystem givenParticleSystem;

    public void PlayParticleSystem()
    {
        givenParticleSystem.Play();
    }

    public void StopParticleSystem()
    {
        givenParticleSystem.Stop();
    }
}

using SaintsField.Playa;
using UnityEngine;

public class SoundTest : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [Button]
    private void PlaySound()
    {
        _audioSource.Play();
    }
}

using UnityEngine;

public class DoorSoundManager : MonoBehaviour
{
    public AudioSource audioSource;
    
    // Açılma ve kapanma sesleri için iki ayrı dizi (array)
    public AudioClip[] openSounds;
    public AudioClip[] closeSounds;

    // Kapı açılmaya başladığında bu fonksiyon çağrılacak
    public void PlayOpenSound()
    {
        if (audioSource != null && openSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, openSounds.Length);
            
            // Kapı seslerinde de pitch randomizasyonu çok iyi gider, robotik hissi kırar
            audioSource.pitch = Random.Range(0.95f, 1.05f); 
            audioSource.PlayOneShot(openSounds[randomIndex]);
        }
    }

    // Kapı kapandığında veya çarptığında bu fonksiyon çağrılacak
    public void PlayCloseSound()
    {
        if (audioSource != null && closeSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, closeSounds.Length);
            audioSource.pitch = Random.Range(0.95f, 1.05f);
            audioSource.PlayOneShot(closeSounds[randomIndex]);
        }
    }
}
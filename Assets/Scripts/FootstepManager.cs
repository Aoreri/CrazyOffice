using UnityEngine;

public class FootstepManager : MonoBehaviour
{
    public AudioSource audioSource;
    // Artık tek bir ses değil, birden fazla ses alabilen bir liste/dizi yaptık
    public AudioClip[] footstepSounds;

    public void PlayFootstep()
    {
        // Eğer AudioSource var ve listemizin içinde en az 1 ses varsa çalışsın
        if (audioSource != null && footstepSounds.Length > 0)
        {
            // Listenin içinden rastgele bir sayı (index) seçiyoruz
            int randomIndex = Random.Range(0, footstepSounds.Length);
            
            // Seçilen o rastgele sesi çalıyoruz
            audioSource.PlayOneShot(footstepSounds[randomIndex]);
        }
    }
}
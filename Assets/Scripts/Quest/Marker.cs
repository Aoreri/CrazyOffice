using UnityEngine;

public class Marker : MonoBehaviour
{
    [Header("Görev Ayarlarý")]
    [Tooltip("Ýçinde görevleri (Quest scriptleri) barýndýran Ana UseCase objesi")]
    public GameObject useCaseObject;

    
    public void SelectAndStartRandomQuest()
    {
        if (useCaseObject == null)
        {
            Debug.LogWarning("Marker'da UseCase objesi atanmamýþ!");
            return;
        }

        
        Quest[] quests = useCaseObject.GetComponentsInChildren<Quest>();

        if (quests.Length > 0)
        {
            
            int randomIndex = Random.Range(0, quests.Length);
            Quest selectedQuest = quests[randomIndex];

            
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.StartQuest(selectedQuest);
                Debug.Log("Marker rastgele görev baþlattý: " + selectedQuest.questName);
            }
            else
            {
                Debug.LogError("Sahnede QuestManager bulunamadý!");
            }
        }
        else
        {
            Debug.LogWarning(useCaseObject.name + " objesinin içinde hiç Quest bulunamadý!");
        }
    }

    // OPSIYONEL: Eðer oyuncu marker'ýn içine (collider'ýna) girince 
    // görev otomatik baþlasýn istiyorsanýz aþaðýdaki yorum satýrlarýný kaldýrýn.
    // (Marker objesinde 'Is Trigger' açýk bir Collider olmalýdýr)

    /*
    private void OnTriggerEnter(Collider other)
    {
        // Oyuncunun Tag'i "Player" olarak ayarlanmýþ olmalý
        if (other.CompareTag("Player"))
        {
            SelectAndStartRandomQuest();
            
            // Eðer marker sadece 1 kere çalýþsýn isterseniz, çalýþtýktan sonra bu scripti kapatabilirsiniz:
            // this.enabled = false; 
        }
    }
    */
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ConsentManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Panel Ayarý")]
    [Tooltip("Gizlenecek olan ana krem rengi panel (ConsentPanel)")]
    public GameObject consentPanel;

    [Header("Neon Hover Ayarlarý")]
    public Color neonGlowColor = new Color(0.5f, 1f, 0.5f, 1f); // Parlak neon yeþili
    public float scaleMultiplier = 1.05f;
    public float animationSpeed = 12f;

    private Image buttonImage;
    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovering = false;

    void Start()
    {
        // Eðer kullanýcý daha önce onay verdiyse paneli en baþtan gizle
        if (PlayerPrefs.GetInt("TermsApproved", 0) == 1)
        {
            if (consentPanel != null) consentPanel.SetActive(false);
        }

        // Animasyon için butonun ilk deðerlerini al
        buttonImage = GetComponent<Image>();
        originalScale = transform.localScale;
        originalColor = buttonImage.color;
    }

    void Update()
    {
        // Yumuþak neon geçiþ animasyonu (Her karede çalýþýr)
        Vector3 targetScale = isHovering ? originalScale * scaleMultiplier : originalScale;
        Color targetColor = isHovering ? neonGlowColor : originalColor;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
    }

    // --- FARE BUTONUN ÜSTÜNE GELDÝÐÝNDE ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    // --- FARE BUTONDAN ÇIKTIÐINDA ---
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
    }

    // --- BUTONA TIKLANDIÐINDA ÇALIÞACAK FONKSÝYON ---
    public void OnApproveClicked()
    {
        // Onayý kaydet (Oyuna bir dahaki giriþte sormamasý için)
        PlayerPrefs.SetInt("TermsApproved", 1);
        PlayerPrefs.Save();

        // Paneli ekrandan tamamen kaldýr
        if (consentPanel != null)
        {
            consentPanel.SetActive(false);
        }

        Debug.Log("Onay alýndý, panel kapatýldý ve oyun baþlýyor!");
    }
}
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro; // Klavye inputlarý için TextMeshPro kütüphanesi eklendi

[RequireComponent(typeof(Image))]
public class ConsentManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Panel Ayarlarý")]
    [Tooltip("En dýþtaki krem rengi ana panel (Backplate)")]
    public GameObject consentPanel;
    [Tooltip("Ýlk ekranda gözüken onay metni ve bu butonun bulunduðu UI grubu")]
    public GameObject agreementGroup;
    [Tooltip("Onay verdikten sonra açýlacak isim ve ders kodu girme UI grubu")]
    public GameObject inputGroup;

    [Header("Klavye Giriþ Alanlarý (Input Fields)")]
    [Tooltip("Kullanýcýnýn ismini gireceði opsiyonel alan")]
    public TMP_InputField nameInputField;
    [Tooltip("Kullanýcýnýn ders kodunu gireceði ZORUNLU alan")]
    public TMP_InputField courseCodeInputField;

    [Header("Neon Hover Ayarlarý")]
    public Color neonGlowColor = new Color(0.5f, 1f, 0.5f, 1f);
    public float scaleMultiplier = 1.05f;
    public float animationSpeed = 12f;

    // --- OYUNUN HER YERÝNDEN ERÝÞÝLEBÝLECEK VERÝLER ---
    // Baþka bir scriptten direkt "ConsentManager.StudentName" yazarak bu verilere ulaþabilirsin.
    public static string StudentName { get; private set; } = "";
    public static string CourseCode { get; private set; } = "";

    private Image buttonImage;
    private Vector3 originalScale;
    private Color originalColor;
    private bool isHovering = false;

    void Start()
    {
        // Eðer daha önce onaylanmýþsa, hem paneli kapat hem de eski verileri belleðe yükle
        if (PlayerPrefs.GetInt("TermsApproved", 0) == 1)
        {
            StudentName = PlayerPrefs.GetString("SavedName", "");
            CourseCode = PlayerPrefs.GetString("SavedCourse", "");

            if (consentPanel != null) consentPanel.SetActive(false);
        }
        else
        {
            // Ýlk açýlýþta form kýsmý gizli, onay kýsmý açýk olmalý
            if (agreementGroup != null) agreementGroup.SetActive(true);
            if (inputGroup != null) inputGroup.SetActive(false);
        }

        // Hover animasyonu için orijinal deðerleri al
        buttonImage = GetComponent<Image>();
        originalScale = transform.localScale;
        originalColor = buttonImage.color;
    }

    void Update()
    {
        // Yumuþak neon geçiþ animasyonu
        Vector3 targetScale = isHovering ? originalScale * scaleMultiplier : originalScale;
        Color targetColor = isHovering ? neonGlowColor : originalColor;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData) { isHovering = true; }
    public void OnPointerExit(PointerEventData eventData) { isHovering = false; }

    // --- 1. AÞAMA: "AGREE" BUTONUNA TIKLANINCA ---
    public void OnApproveClicked()
    {
        // Ana paneli kapatmýyoruz, sadece onay grubunu gizleyip form grubunu açýyoruz
        if (agreementGroup != null) agreementGroup.SetActive(false);
        if (inputGroup != null) inputGroup.SetActive(true);
    }

    // --- 2. AÞAMA: BÝLGÝLER GÝRÝLÝP "BAÞLA" BUTONUNA TIKLANINCA ---
    public void OnSubmitDataClicked()
    {
        // Ders kodu boþ mu diye kontrol et (Trim() boþluk karakterlerini siler)
        if (courseCodeInputField == null || string.IsNullOrWhiteSpace(courseCodeInputField.text))
        {
            Debug.LogWarning("Ders kodu zorunludur! Lütfen doldurun.");
            return; // Kod boþsa fonksiyonu burada kes, paneli kapatma
        }

        // Verileri static deðiþkenlere aktar (Ýsim boþ girilse bile sorun yok)
        StudentName = nameInputField != null ? nameInputField.text.Trim() : "";
        CourseCode = courseCodeInputField.text.Trim();

        // Verileri kalýcý belleðe (PlayerPrefs) kaydet
        PlayerPrefs.SetInt("TermsApproved", 1);
        PlayerPrefs.SetString("SavedName", StudentName);
        PlayerPrefs.SetString("SavedCourse", CourseCode);
        PlayerPrefs.Save();

        Debug.Log($"Veriler Alýndý! Ýsim: {StudentName} | Ders Kodu: {CourseCode}");

        // Tüm iþlemler bitti, artýk arka planý (Backplate) tamamen kapatabiliriz
        if (consentPanel != null)
        {
            consentPanel.SetActive(false);
        }
    }
}
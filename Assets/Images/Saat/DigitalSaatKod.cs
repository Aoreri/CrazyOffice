using UnityEngine;
using TMPro; // TextMeshPro kullanmak için bu kütüphane şart

public class IleriyeSayimKontrol : MonoBehaviour
{
    [Header("Görsel Bileşen")]
    public TMP_Text dijitalYaziNesnesi; // Ekrandaki TextMeshPro yazısı

    // Arka planda sıfırdan başlayıp artacak olan saniye sayacı
    private float gecenSure = 0f; 

    void Update()
    {
        // 1. Süreyi her karede ileriye doğru (zamanın akış hızıyla) arttır
        gecenSure += Time.deltaTime;

        // 2. Yazıyı ekranda güncelle (00:01, 00:02... formatında)
        YaziyiGuncelle(gecenSure);
    }

    // Saniyeyi "Dakika:Saniye" (00:00) formatına çeviren fonksiyon
    void YaziyiGuncelle(float toplamSaniye)
    {
        int dakikalar = Mathf.FloorToInt(toplamSaniye / 60);
        int saniyeler = Mathf.FloorToInt(toplamSaniye % 60);

        // string.Format yapısı sayıların hep iki basamaklı (01, 02 gibi) görünmesini sağlar
        dijitalYaziNesnesi.text = string.Format("{0:00}:{1:00}", dakikalar, saniyeler);
    }
}
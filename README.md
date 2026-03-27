# ▮ F O C U S  t o  A T L A S ⏱️

Modern, şık ve tamamen kişiselleştirilebilir bir masaüstü zaman takip ve verimlilik (dashboard) uygulaması. 

Bu proje, zaman yönetimini sıkıcı bir sayaç olmaktan çıkarıp profesyonel bir veri analitiği deneyimine dönüştürmek amacıyla geliştirilmiştir. Uygulama içerisindeki tüm grafik motorları (Pasta, Kombo ve Isı Haritası) hiçbir dış kütüphane (3rd party library) kullanılmadan, tamamen C# ve matematiksel algoritmalar ile sıfırdan inşa edilmiştir.

## 🚀 Öne Çıkan Özellikler

* **Gelişmiş Veri Görselleştirme:** Çalışma sürelerinizi 3 farklı profesyonel grafikte analiz edin:
  * **Oransal Pasta Grafik (Donut Chart):** Kategorilerin renklerine göre dış etiketli dağılımı.
  * **Haftalık Kombo Grafik:** Sütun (Bar) ve Çizgi (Trend Line) grafiklerinin aynı eksende birleşimi.
  * **Aylık Isı Haritası (Heatmap):** GitHub'ın katkı takvimine benzer, çalışma yoğunluğuna göre renk değiştiren akıllı takvim.
* **Odak Modu (Focus Mode):** İstatistik ekranına geçildiğinde arka plan zarifçe kararır ve veriler cam gibi netleşir.
* **Mini Widget (Her Zaman Üstte):** Ana ekranı kapatıp ekranın köşesinde duran, yeniden boyutlandırılabilir şık bir mini sayaca geçiş yapabilme.
* **Akıllı Gece Yarısı Sıfırlaması:** Gün döndüğünde (00:00) sayaç otomatik olarak sıfırlanır ve yeni güne hazır hale gelir.
* **Tam Kontrol (CRUD):** Geçmişe dönük manuel süre ekleme, yanlış girilen kayıtları anında silme veya düzenleme imkanı.
* **Modern Toast Bildirimleri:** Sıkıcı Windows mesaj kutuları yerine, ekranda zarifçe belirip kaybolan özel bildirimler.

## 🛠️ Kullanılan Teknolojiler

* **Dil:** C#
* **Arayüz (UI):** WPF (Windows Presentation Foundation)
* **Veritabanı:** SQLite (Yerel, internet gerektirmeyen hızlı veri depolama)
* **Mimari:** XAML ile modern tasarım, arka planda olay güdümlü (event-driven) programlama.

## 📥 Kurulum ve Kullanım

Uygulamayı kaynak kodlarından derlemek (build) veya doğrudan kullanmak oldukça basittir:

1. Bu depoyu (repository) bilgisayarınıza indirin veya klonlayın:
   ```bash
   git clone [https://github.com/KullaniciAdiniz/FocusToAtlas.git](https://github.com/KullaniciAdiniz/FocusToAtlas.git)

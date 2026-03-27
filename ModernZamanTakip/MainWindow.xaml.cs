using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ModernZamanTakip
{
    public class KategoriVerisi
    {
        public string Kategori { get; set; } = "";
        public string SureMetni { get; set; } = "";
        public SolidColorBrush RenkFirca { get; set; } = Brushes.White;
        public int ToplamSaniye { get; set; }
        public string RenkHex { get; set; } = "#FFFFFF";
    }

    public class PastaDilimEtiketModel
    {
        public Geometry DilimGeometrisi { get; set; } = Geometry.Empty;
        public SolidColorBrush RenkFirca { get; set; } = Brushes.White;
        public Geometry CizgiGeometrisi { get; set; } = Geometry.Empty;
        public string EtiketMetni { get; set; } = "";
        public double EtiketX { get; set; }
        public double EtiketY { get; set; }
    }

    public partial class MainWindow : Window
    {
        private System.Windows.Threading.DispatcherTimer zamanlayici;
        private System.Windows.Threading.DispatcherTimer toastZamanlayici;
        private int saniye = 0;
        private int dakika = 0;
        private bool calisiyorMu = false;

        private string dbYol = "Data Source=AtlasFocus.db;Version=3;";
        private string seciliRenk = "#007AFF";
        private string aktifIstatistikSekmesi = "GUN";
        private string duzenlenenKategori = "";

        private DateTime _gecerliTarih;
        private MiniZamanlayiciPenceresi? miniPencere;

        public MainWindow()
        {
            InitializeComponent();
            VeritabaniKur();

            lblTarih.Text = DateTime.Now.ToString("dd MMMM yyyy, dddd", new CultureInfo("tr-TR"));
            _gecerliTarih = DateTime.Now.Date;

            zamanlayici = new System.Windows.Threading.DispatcherTimer();
            zamanlayici.Interval = TimeSpan.FromSeconds(1);
            zamanlayici.Tick += Zamanlayici_Tick;

            toastZamanlayici = new System.Windows.Threading.DispatcherTimer();
            toastZamanlayici.Interval = TimeSpan.FromSeconds(3);
            toastZamanlayici.Tick += ToastGizle_Tick;
        }

        private void VeritabaniKur()
        {
            using (var baglanti = new SQLiteConnection(dbYol))
            {
                baglanti.Open();
                string tabloOlustur = "CREATE TABLE IF NOT EXISTS FocusGecmisi (Id INTEGER PRIMARY KEY AUTOINCREMENT, ToplamSaniye INTEGER, Tarih DATETIME, Kategori TEXT, Renk TEXT)";
                SQLiteCommand komut = new SQLiteCommand(tabloOlustur, baglanti);
                komut.ExecuteNonQuery();
            }
        }

        // ==========================================
        // KRONOMETRE & KAYIT & TARİH SIFIRLAMA
        // ==========================================
        private void Zamanlayici_Tick(object? sender, EventArgs e)
        {
            if (DateTime.Now.Date != _gecerliTarih)
            {
                dakika = 0;
                saniye = 0;
                lblSayac.Text = "00:00";

                if (calisiyorMu)
                {
                    zamanlayici.Stop();
                    calisiyorMu = false;
                    btnBaslat.Content = "B A Ş L A T";
                    miniPencere?.OynatDurdurSimgeGuncelle(false);
                }

                _gecerliTarih = DateTime.Now.Date;
                lblTarih.Text = DateTime.Now.ToString("dd MMMM yyyy, dddd", new CultureInfo("tr-TR"));
                ToastGoster("Gün bitti, kronometre sıfırlandı.");
                return;
            }

            saniye++;
            if (saniye >= 60)
            {
                dakika++;
                saniye = 0;
            }
            lblSayac.Text = string.Format("{0:00}:{1:00}", dakika, saniye);
            miniPencere?.ZamanMetniGuncelle(lblSayac.Text);
        }

        private void btnBaslat_Click(object sender, RoutedEventArgs e)
        {
            if (calisiyorMu)
            {
                zamanlayici.Stop();
                calisiyorMu = false;
                btnBaslat.Content = "D E V A M  E T";

                int toplamGecenSaniye = (dakika * 60) + saniye;
                if (toplamGecenSaniye > 0)
                {
                    Kaydet(toplamGecenSaniye, "Serbest Çalışma", "#FFFFFF");
                }
            }
            else
            {
                zamanlayici.Start();
                calisiyorMu = true;
                btnBaslat.Content = "D U R D U R";
            }
            miniPencere?.OynatDurdurSimgeGuncelle(calisiyorMu);
        }

        public void ToggleTimer()
        {
            btnBaslat_Click(null, null);
        }

        private void Kaydet(int gecenSaniye, string kategori, string renk)
        {
            using (var baglanti = new SQLiteConnection(dbYol))
            {
                baglanti.Open();
                string ekle = "INSERT INTO FocusGecmisi (ToplamSaniye, Tarih, Kategori, Renk) VALUES (@saniye, @tarih, @kategori, @renk)";
                SQLiteCommand komut = new SQLiteCommand(ekle, baglanti);
                komut.Parameters.AddWithValue("@saniye", gecenSaniye);
                komut.Parameters.AddWithValue("@tarih", DateTime.Now);
                komut.Parameters.AddWithValue("@kategori", kategori);
                komut.Parameters.AddWithValue("@renk", renk);
                komut.ExecuteNonQuery();
            }
        }

        private void ToastGoster(string mesaj)
        {
            lblToastMesaj.Text = mesaj;
            ToastBildirim.Visibility = Visibility.Visible;
            toastZamanlayici.Stop();
            toastZamanlayici.Start();
        }

        private void ToastGizle_Tick(object? sender, EventArgs e)
        {
            ToastBildirim.Visibility = Visibility.Collapsed;
            toastZamanlayici.Stop();
        }

        // ==========================================
        // MİNİ EKRANA GEÇİŞ
        // ==========================================
        private void btnMiniEkranaGec_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();

            if (miniPencere == null || !miniPencere.IsVisible)
            {
                miniPencere = new MiniZamanlayiciPenceresi(this);
                miniPencere.Topmost = true;
            }

            miniPencere.ZamanMetniGuncelle(lblSayac.Text);
            miniPencere.OynatDurdurSimgeGuncelle(calisiyorMu);
            miniPencere.Show();
        }

        public void RestoreMainWindow()
        {
            miniPencere?.Hide();
            this.Show();
        }

        // ==========================================
        // EKSTRA EKLE POP-UP
        // ==========================================
        private void btnManuelEkleAc_Click(object sender, MouseButtonEventArgs e)
        {
            txtManuelKategori.Text = "";
            txtManuelSure.Text = "";
            ManuelKayitOverlay.Visibility = Visibility.Visible;
        }

        private void btnManuelIptal_Click(object sender, RoutedEventArgs e)
        {
            ManuelKayitOverlay.Visibility = Visibility.Collapsed;
        }

        private void RenkSec_Click(object sender, RoutedEventArgs e)
        {
            Button secilenButon = (Button)sender;
            seciliRenk = secilenButon.Tag?.ToString() ?? "#FFFFFF";
            txtManuelKategori.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(seciliRenk));
        }

        private void btnManuelKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtManuelKategori.Text)) return;

            if (int.TryParse(txtManuelSure.Text, out int girilenDakika))
            {
                int saniyeKarsiligi = girilenDakika * 60;
                Kaydet(saniyeKarsiligi, txtManuelKategori.Text, seciliRenk);

                ManuelKayitOverlay.Visibility = Visibility.Collapsed;
                ToastGoster($"{txtManuelKategori.Text} eklendi!");
                if (IstatistiklerView.Visibility == Visibility.Visible) IstatistikleriGetir(aktifIstatistikSekmesi);
            }
            else
            {
                ToastGoster("Hata: Süreye sadece sayı giriniz.");
            }
        }

        // ==========================================
        // DÜZENLE / SİL
        // ==========================================
        private void btnKategoriSil_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string kategori = btn.Tag?.ToString() ?? "";

            using (var baglanti = new SQLiteConnection(dbYol))
            {
                baglanti.Open();
                string sql = $"DELETE FROM FocusGecmisi WHERE Kategori = @kat AND {TarihFiltresiSorgusu(aktifIstatistikSekmesi)}";
                SQLiteCommand cmd = new SQLiteCommand(sql, baglanti);
                cmd.Parameters.AddWithValue("@kat", kategori);
                cmd.ExecuteNonQuery();
            }
            ToastGoster($"{kategori} silindi.");
            IstatistikleriGetir(aktifIstatistikSekmesi);
        }

        private void btnKategoriDuzenle_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            duzenlenenKategori = btn.Tag?.ToString() ?? "";
            lblDuzenleBaslik.Text = duzenlenenKategori + " Düzenle";
            txtDuzenleSure.Text = "";
            DuzenleOverlay.Visibility = Visibility.Visible;
        }

        private void btnDuzenleIptal_Click(object sender, RoutedEventArgs e)
        {
            DuzenleOverlay.Visibility = Visibility.Collapsed;
        }

        private void btnDuzenleKaydet_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtDuzenleSure.Text, out int yeniDakika))
            {
                using (var baglanti = new SQLiteConnection(dbYol))
                {
                    baglanti.Open();
                    string renkBul = $"SELECT Renk FROM FocusGecmisi WHERE Kategori = @kat AND {TarihFiltresiSorgusu(aktifIstatistikSekmesi)} LIMIT 1";
                    SQLiteCommand cmdRenk = new SQLiteCommand(renkBul, baglanti);
                    cmdRenk.Parameters.AddWithValue("@kat", duzenlenenKategori);
                    string mevcutRenk = cmdRenk.ExecuteScalar()?.ToString() ?? "#FFFFFF";

                    string sqlSil = $"DELETE FROM FocusGecmisi WHERE Kategori = @kat AND {TarihFiltresiSorgusu(aktifIstatistikSekmesi)}";
                    SQLiteCommand cmdSil = new SQLiteCommand(sqlSil, baglanti);
                    cmdSil.Parameters.AddWithValue("@kat", duzenlenenKategori);
                    cmdSil.ExecuteNonQuery();

                    string sqlEkle = "INSERT INTO FocusGecmisi (ToplamSaniye, Tarih, Kategori, Renk) VALUES (@saniye, @tarih, @kategori, @renk)";
                    SQLiteCommand cmdEkle = new SQLiteCommand(sqlEkle, baglanti);
                    cmdEkle.Parameters.AddWithValue("@saniye", yeniDakika * 60);
                    cmdEkle.Parameters.AddWithValue("@tarih", DateTime.Now);
                    cmdEkle.Parameters.AddWithValue("@kategori", duzenlenenKategori);
                    cmdEkle.Parameters.AddWithValue("@renk", mevcutRenk);
                    cmdEkle.ExecuteNonQuery();
                }
                DuzenleOverlay.Visibility = Visibility.Collapsed;
                ToastGoster($"{duzenlenenKategori} güncellendi!");
                IstatistikleriGetir(aktifIstatistikSekmesi);
            }
            else ToastGoster("Sayı giriniz.");
        }

        // ==========================================
        // İSTATİSTİKLER (GÖRÜNÜM & SEKMELER)
        // ==========================================
        private void btnIstatistikSayfasi_Click(object sender, MouseButtonEventArgs e)
        {
            AnaEkranView.Visibility = Visibility.Collapsed;
            IstatistiklerView.Visibility = Visibility.Visible;

            // YENİ: Grafikleri net görmek için arka planı karart
            StatsArkaPlanKarartma.Visibility = Visibility.Visible;

            IstatistikleriGetir("GUN");
        }

        private void btnAnaEkranaDon_Click(object sender, MouseButtonEventArgs e)
        {
            IstatistiklerView.Visibility = Visibility.Collapsed;
            AnaEkranView.Visibility = Visibility.Visible;

            // YENİ: Ana ekrana dönünce karartmayı kaldır
            StatsArkaPlanKarartma.Visibility = Visibility.Collapsed;
        }

        private void Tab_Click(object sender, MouseButtonEventArgs e)
        {
            Border tiklananTab = (Border)sender;
            aktifIstatistikSekmesi = tiklananTab.Tag?.ToString() ?? "GUN";

            tabGun.Background = Brushes.Transparent;
            tabHafta.Background = Brushes.Transparent;
            tabAy.Background = Brushes.Transparent;
            tabTum.Background = Brushes.Transparent;

            tiklananTab.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4AFFFFFF"));

            if (aktifIstatistikSekmesi == "HAFTA" || aktifIstatistikSekmesi == "AY")
                btnGorunum3.Visibility = Visibility.Visible;
            else
                btnGorunum3.Visibility = Visibility.Collapsed;

            GorunumAyarla("1");
            IstatistikleriGetir(aktifIstatistikSekmesi);
        }

        private void GorunumDegistir_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            GorunumAyarla(btn.Tag?.ToString() ?? "1");
        }

        private void GorunumAyarla(string gorunum)
        {
            btnGorunum1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2AFFFFFF"));
            btnGorunum2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2AFFFFFF"));
            btnGorunum3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2AFFFFFF"));

            viewListe.Visibility = Visibility.Collapsed;
            viewGrafikPasta.Visibility = Visibility.Collapsed;
            viewGrafikHaftalik.Visibility = Visibility.Collapsed;
            viewTakvim.Visibility = Visibility.Collapsed;

            if (gorunum == "1") { btnGorunum1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4AFFFFFF")); viewListe.Visibility = Visibility.Visible; }
            else if (gorunum == "2") { btnGorunum2.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4AFFFFFF")); viewGrafikPasta.Visibility = Visibility.Visible; }
            else if (gorunum == "3")
            {
                btnGorunum3.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4AFFFFFF"));
                if (aktifIstatistikSekmesi == "HAFTA")
                {
                    viewGrafikHaftalik.Visibility = Visibility.Visible;
                    viewGrafikHaftalik.UpdateLayout();
                    HaftalikKomboGrafikCiz();
                }
                else if (aktifIstatistikSekmesi == "AY") { viewTakvim.Visibility = Visibility.Visible; AylikTakvimOlustur(); }
            }
        }

        private string TarihFiltresiSorgusu(string zamanDilimi)
        {
            switch (zamanDilimi)
            {
                case "GUN": return "date(Tarih) = date('now', 'localtime')";
                case "HAFTA": return "date(Tarih) >= date('now', '-6 days', 'localtime')";
                case "AY": return "date(Tarih) >= date('now', 'start of month', 'localtime')";
                default: return "1=1";
            }
        }

        private void IstatistikleriGetir(string zamanDilimi)
        {
            string tarihFiltresi = TarihFiltresiSorgusu(zamanDilimi);
            lblTabloBaslik.Text = zamanDilimi == "GUN" ? "BUGÜN TOPLAM" : (zamanDilimi == "HAFTA" ? "BU HAFTA TOPLAM" : (zamanDilimi == "AY" ? "BU AY TOPLAM" : "TÜM ZAMANLAR TOPLAM"));

            List<KategoriVerisi> listeVerileri = new List<KategoriVerisi>();

            using (var baglanti = new SQLiteConnection(dbYol))
            {
                baglanti.Open();

                string sorguToplam = $"SELECT SUM(ToplamSaniye) FROM FocusGecmisi WHERE {tarihFiltresi}";
                SQLiteCommand cmdToplam = new SQLiteCommand(sorguToplam, baglanti);
                var sonuc = cmdToplam.ExecuteScalar();

                if (sonuc != DBNull.Value && sonuc != null)
                {
                    TimeSpan ts = TimeSpan.FromSeconds(Convert.ToInt32(sonuc));
                    lblSeciliToplam.Text = string.Format("{0:00}s {1:00}d", (int)ts.TotalHours, ts.Minutes);
                }
                else lblSeciliToplam.Text = "00s 00d";

                string sorguGrup = $"SELECT Kategori, Renk, SUM(ToplamSaniye) as Sure FROM FocusGecmisi WHERE {tarihFiltresi} GROUP BY Kategori, Renk ORDER BY Sure DESC";
                SQLiteCommand cmdGrup = new SQLiteCommand(sorguGrup, baglanti);
                SQLiteDataReader okuyucu = cmdGrup.ExecuteReader();

                while (okuyucu.Read())
                {
                    string kat = okuyucu["Kategori"].ToString();
                    string renk = okuyucu["Renk"].ToString();
                    int san = Convert.ToInt32(okuyucu["Sure"]);
                    TimeSpan ts = TimeSpan.FromSeconds(san);

                    listeVerileri.Add(new KategoriVerisi
                    {
                        Kategori = kat,
                        SureMetni = $"{(int)ts.TotalHours:00}s {ts.Minutes:00}d",
                        ToplamSaniye = san,
                        RenkHex = renk,
                        RenkFirca = new SolidColorBrush((Color)ColorConverter.ConvertFromString(renk))
                    });
                }
            }

            listeVerileri = listeVerileri.OrderBy(x => x.Kategori != "Serbest Çalışma").ThenByDescending(x => x.ToplamSaniye).ToList();
            viewListe.ItemsSource = listeVerileri;
            PastaGrafikCiz(listeVerileri);
        }

        // ==========================================
        // PASTA GRAFİK ÇİZİMİ
        // ==========================================
        private void PastaGrafikCiz(List<KategoriVerisi> veriler)
        {
            icPastaDilimleri.ItemsSource = null;

            if (veriler.Count == 0) return;

            ObservableCollection<PastaDilimEtiketModel> dilimModelleri = new ObservableCollection<PastaDilimEtiketModel>();
            double toplam = veriler.Sum(x => x.ToplamSaniye);
            double mevcutAci = 0;
            double radius = 120;
            Point center = new Point(200, 150);

            foreach (var veri in veriler)
            {
                double dilimAcisi = (veri.ToplamSaniye / toplam) * 360;
                double baslangicRad = (mevcutAci - 90) * Math.PI / 180.0;
                double bitisRad = (mevcutAci + dilimAcisi - 90) * Math.PI / 180.0;

                Point pt1 = new Point(center.X + radius * Math.Cos(baslangicRad), center.Y + radius * Math.Sin(baslangicRad));
                Point pt2 = new Point(center.X + radius * Math.Cos(bitisRad), center.Y + radius * Math.Sin(bitisRad));

                PathGeometry pathGeometry = new PathGeometry();
                PathFigure pathFigure = new PathFigure() { StartPoint = center, IsClosed = true };
                pathFigure.Segments.Add(new LineSegment() { Point = pt1 });
                pathFigure.Segments.Add(new ArcSegment() { Point = pt2, Size = new Size(radius, radius), SweepDirection = SweepDirection.Clockwise, IsLargeArc = dilimAcisi > 180 });
                pathGeometry.Figures.Add(pathFigure);

                double ortaRad = (mevcutAci + (dilimAcisi / 2) - 90) * Math.PI / 180.0;
                Point kenarPt = new Point(center.X + radius * Math.Cos(ortaRad), center.Y + radius * Math.Sin(ortaRad));
                Point intermediatePt = new Point(center.X + (radius + 20) * Math.Cos(ortaRad), center.Y + (radius + 20) * Math.Sin(ortaRad));

                TimeSpan ts = TimeSpan.FromSeconds(veri.ToplamSaniye);
                string sureEtiketi = string.Format("{0}: {1:0}s {2:00}d", veri.Kategori, (int)ts.TotalHours, ts.Minutes);

                double etiketGenislikTahmin = sureEtiketi.Length * 7;
                Point finalPt = new Point();
                double lblX;

                if (kenarPt.X > center.X)
                {
                    finalPt = new Point(intermediatePt.X + 20, intermediatePt.Y);
                    lblX = finalPt.X + 5;
                }
                else
                {
                    finalPt = new Point(intermediatePt.X - 20, intermediatePt.Y);
                    lblX = finalPt.X - etiketGenislikTahmin - 5;
                }

                PathGeometry lineGeometry = new PathGeometry();
                PathFigure lineFigure = new PathFigure() { StartPoint = kenarPt };
                lineFigure.Segments.Add(new LineSegment() { Point = intermediatePt });
                lineFigure.Segments.Add(new LineSegment() { Point = finalPt });
                lineGeometry.Figures.Add(lineFigure);

                dilimModelleri.Add(new PastaDilimEtiketModel
                {
                    DilimGeometrisi = pathGeometry,
                    RenkFirca = veri.RenkFirca,
                    CizgiGeometrisi = lineGeometry,
                    EtiketMetni = sureEtiketi,
                    EtiketX = lblX,
                    EtiketY = finalPt.Y - 8
                });

                mevcutAci += dilimAcisi;
            }
            icPastaDilimleri.ItemsSource = dilimModelleri;
        }

        // ==========================================
        // HAFTALIK KOMBO GRAFİK ÇİZİMİ
        // ==========================================
        private void HaftalikKomboGrafikCiz()
        {
            viewCanvasHaftalik.Children.Clear();
            gridSaatEkseni.Children.Clear();
            gridGunEkseni.Children.Clear();

            int maxSaniye = 1;
            Dictionary<int, int> haftalikVeri = new Dictionary<int, int>();

            using (var baglanti = new SQLiteConnection(dbYol))
            {
                baglanti.Open();
                string sorgu = "SELECT strftime('%w', Tarih) as Gun, SUM(ToplamSaniye) as Sure FROM FocusGecmisi WHERE date(Tarih) >= date('now', '-6 days', 'localtime') GROUP BY Gun";
                SQLiteCommand cmd = new SQLiteCommand(sorgu, baglanti);
                SQLiteDataReader okuyucu = cmd.ExecuteReader();

                while (okuyucu.Read())
                {
                    int gunId = Convert.ToInt32(okuyucu["Gun"]);
                    int sure = Convert.ToInt32(okuyucu["Sure"]);
                    haftalikVeri[gunId] = sure;
                    if (sure > maxSaniye) maxSaniye = sure;
                }
            }

            int maxSaat = (maxSaniye / 3600) + 1;
            if (maxSaat < 1) maxSaat = 1;

            for (int saatDegeri = maxSaat; saatDegeri >= 0; saatDegeri--)
            {
                TextBlock txtSaat = new TextBlock() { Text = string.Format("{0:00}s", saatDegeri), Foreground = Brushes.Gray, FontSize = 12, TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
                gridSaatEkseni.Children.Add(txtSaat);
            }
            gridSaatEkseni.Rows = gridSaatEkseni.Children.Count;

            string[] gunIsimleri = { "Pa", "Pt", "Sa", "Ça", "Pe", "Cu", "Ct" };
            for (int i = 0; i < 7; i++)
            {
                int gunId = (i + 1) % 7;
                TextBlock txtGun = new TextBlock() { Text = gunIsimleri[gunId], Foreground = Brushes.Gray, FontSize = 12, TextAlignment = TextAlignment.Center };
                gridGunEkseni.Children.Add(txtGun);
            }

            double w = viewCanvasHaftalik.ActualWidth;
            double h = viewCanvasHaftalik.ActualHeight;
            if (w == 0) w = 380;
            if (h == 0) h = 250;

            double dilimGenislik = w / 7;
            double barGenislik = dilimGenislik * 0.6;
            PointCollection cizgiNoktalari = new PointCollection();

            for (int i = 0; i < 7; i++)
            {
                int gunId = (i + 1) % 7;
                int gunSuresi = haftalikVeri.ContainsKey(gunId) ? haftalikVeri[gunId] : 0;

                double barYukseklik = Math.Max(2, (gunSuresi / (double)(maxSaat * 3600)) * h);
                double xPos = (i * dilimGenislik) + (dilimGenislik * 0.2);
                double yPos = h - barYukseklik;

                Rectangle bar = new Rectangle() { Width = barGenislik, Height = barYukseklik, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AFF")), RadiusX = 3, RadiusY = 3, ToolTip = $"{gunIsimleri[gunId]}: {gunSuresi / 3600}s {(gunSuresi % 3600) / 60}d" };
                Canvas.SetLeft(bar, xPos);
                Canvas.SetTop(bar, yPos);
                viewCanvasHaftalik.Children.Add(bar);

                cizgiNoktalari.Add(new Point(xPos + (barGenislik / 2), Math.Max(0, yPos - 10)));
            }

            Path cizgiPath = new Path() { Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9500")), StrokeThickness = 3, Data = new PathGeometry(new PathFigureCollection { new PathFigure(cizgiNoktalari[0], new PathSegmentCollection { new PolyLineSegment(cizgiNoktalari.Skip(1).ToArray(), true) }, false) }) };
            viewCanvasHaftalik.Children.Add(cizgiPath);

            foreach (var pt in cizgiNoktalari)
            {
                Ellipse nokta = new Ellipse() { Width = 8, Height = 8, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9500")) };
                Canvas.SetLeft(nokta, pt.X - 4);
                Canvas.SetTop(nokta, pt.Y - 4);
                viewCanvasHaftalik.Children.Add(nokta);
            }

            for (int hIndex = 0; hIndex <= maxSaat; hIndex++)
            {
                double lineY = h - (hIndex / (double)maxSaat * h);
                Line gridLine = new Line() { X1 = 0, X2 = w, Y1 = lineY, Y2 = lineY, Stroke = Brushes.Gray, StrokeThickness = 0.5, Opacity = 0.2 };
                viewCanvasHaftalik.Children.Add(gridLine);
            }
        }

        private void AylikTakvimOlustur()
        {
            gridTakvimGunleri.Children.Clear();
            int yil = DateTime.Now.Year;
            int ay = DateTime.Now.Month;
            int buAydakiGunSayisi = DateTime.DaysInMonth(yil, ay);

            DateTime ayinIlkGunu = new DateTime(yil, ay, 1);
            int baslangicBosluk = (int)ayinIlkGunu.DayOfWeek;
            if (baslangicBosluk == 0) baslangicBosluk = 6; else baslangicBosluk -= 1;

            Dictionary<int, int> gunlukSureler = new Dictionary<int, int>();
            int ayinEnYuksekSuresi = 1;

            using (var baglanti = new SQLiteConnection(dbYol))
            {
                baglanti.Open();
                string sorgu = "SELECT strftime('%d', Tarih) as Gun, SUM(ToplamSaniye) as Sure FROM FocusGecmisi WHERE date(Tarih) >= date('now', 'start of month', 'localtime') GROUP BY strftime('%d', Tarih)";
                SQLiteCommand cmd = new SQLiteCommand(sorgu, baglanti);
                SQLiteDataReader okuyucu = cmd.ExecuteReader();

                while (okuyucu.Read())
                {
                    int gun = Convert.ToInt32(okuyucu["Gun"]);
                    int sure = Convert.ToInt32(okuyucu["Sure"]);
                    gunlukSureler[gun] = sure;
                    if (sure > ayinEnYuksekSuresi) ayinEnYuksekSuresi = sure;
                }
            }

            for (int i = 0; i < baslangicBosluk; i++)
            {
                gridTakvimGunleri.Children.Add(new Border() { Background = Brushes.Transparent, Margin = new Thickness(2) });
            }

            for (int i = 1; i <= buAydakiGunSayisi; i++)
            {
                Border gunKutusu = new Border() { CornerRadius = new CornerRadius(5), Margin = new Thickness(2), Padding = new Thickness(5) };
                TextBlock txtGun = new TextBlock() { Text = i.ToString(), Foreground = Brushes.White, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

                if (gunlukSureler.ContainsKey(i))
                {
                    double oran = (double)gunlukSureler[i] / ayinEnYuksekSuresi;
                    double parlaklik = Math.Max(0.2, oran);
                    gunKutusu.Background = new SolidColorBrush(Color.FromArgb((byte)(255 * parlaklik), 0, 230, 118));
                    TimeSpan ts = TimeSpan.FromSeconds(gunlukSureler[i]);
                    gunKutusu.ToolTip = $"{i} {DateTime.Now.ToString("MMMM")}\n{(int)ts.TotalHours}s {ts.Minutes}d çalışıldı.";
                }
                else
                {
                    gunKutusu.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1AFFFFFF"));
                }

                gunKutusu.Child = txtGun;
                gridTakvimGunleri.Children.Add(gunKutusu);
            }
        }

        private void btnKapat_Click(object sender, MouseButtonEventArgs e) => this.Close();
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { base.OnMouseLeftButtonDown(e); if (e.ButtonState == MouseButtonState.Pressed) this.DragMove(); }
    }
}
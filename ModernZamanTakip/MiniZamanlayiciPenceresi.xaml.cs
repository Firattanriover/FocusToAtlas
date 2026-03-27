using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System;
using System.Windows;
using System.Windows.Controls.Primitives; // DragDelta için gerekli
using System.Windows.Input;

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernZamanTakip
{
    public partial class MiniZamanlayiciPenceresi : Window
    {
        private MainWindow _anaPencere;

        public MiniZamanlayiciPenceresi(MainWindow anaPencere)
        {
            InitializeComponent();
            _anaPencere = anaPencere;
        }

        public void ZamanMetniGuncelle(string zaman) { lblMiniSayac.Text = zaman; }

        public void OynatDurdurSimgeGuncelle(bool calisiyorMu)
        {
            btnMiniOynat.Content = calisiyorMu ? "⏸" : "▶";
            btnMiniOynat.Foreground = calisiyorMu ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 230, 118)) : System.Windows.Media.Brushes.White;
        }

        private void btnMiniOynat_Click(object sender, RoutedEventArgs e) { _anaPencere.ToggleTimer(); }

        private void btnBuyut_Click(object sender, RoutedEventArgs e) { _anaPencere.RestoreMainWindow(); }

        // YENİ: UYGULAMAYI TAMAMEN KAPATIR
        private void btnKompleKapat_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double enBoyOrani = 3.0;
            double yeniGenislik = this.Width + e.HorizontalChange;
            if (yeniGenislik > 150 && yeniGenislik < 800)
            {
                this.Width = yeniGenislik;
                this.Height = yeniGenislik / enBoyOrani;
            }
        }
    }
}
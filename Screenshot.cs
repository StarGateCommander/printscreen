// Zrzut ekranu ze wszystkich monitorow do jednego pliku PNG,
// w takim ukladzie, w jakim sa rozmieszczone w ustawieniach Windows.
// Program okienkowy (bez konsoli), wiec nic nie pojawia sie na zrzucie.
//
// Uzycie:
//   Screenshot.exe                      -> zapis do Obrazy\Screenshots\Screenshot_<data>.png
//   Screenshot.exe -Folder "D:\Zrzuty"  -> zapis do wskazanego folderu
//   Screenshot.exe -Open                -> po zapisaniu otwiera plik
//   Screenshot.exe -Clipboard           -> dodatkowo kopiuje obraz do schowka
//   Screenshot.exe -Delay 5             -> odliczanie 5 s zamiast domyslnych 10 (0 = bez odliczania)
//
// Kompilacja:
//   C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:Screenshot.exe Screenshot.cs

using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Male okienko z odliczaniem na srodku glownego monitora.
// Nie zabiera fokusu aktywnemu oknu i zamyka sie przed zrobieniem zrzutu.
class CountdownForm : Form
{
    int left;
    readonly Label label = new Label();
    readonly Timer timer = new Timer();

    public CountdownForm(int seconds)
    {
        left = seconds;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.Black;
        Opacity = 0.8;
        StartPosition = FormStartPosition.Manual;
        Rectangle p = Screen.PrimaryScreen.Bounds;
        int size = p.Height / 5;
        Bounds = new Rectangle(p.Left + (p.Width - size) / 2, p.Top + (p.Height - size) / 2, size, size);

        label.Dock = DockStyle.Fill;
        label.ForeColor = Color.White;
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Font = new Font("Segoe UI", size * 0.45f, FontStyle.Bold, GraphicsUnit.Pixel);
        label.Text = left.ToString();
        Controls.Add(label);

        timer.Interval = 1000;
        timer.Tick += delegate
        {
            left--;
            if (left <= 0) { timer.Stop(); Close(); }
            else label.Text = left.ToString();
        };
        timer.Start();
    }

    protected override bool ShowWithoutActivation { get { return true; } }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= 0x08000000 | 0x00000080 | 0x00000008; // NOACTIVATE | TOOLWINDOW | TOPMOST
            return cp;
        }
    }
}

static class Program
{
    [DllImport("user32.dll")] static extern bool SetProcessDpiAwarenessContext(IntPtr value);
    [DllImport("user32.dll")] static extern bool SetProcessDPIAware();

    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            // Bez tego przy skalowaniu (125%, 150%...) Windows podaje zanizone rozmiary
            // i zrzut bylby obciety albo rozmyty.
            bool ok = false;
            try { ok = SetProcessDpiAwarenessContext(new IntPtr(-4)); } catch (EntryPointNotFoundException) { }
            if (!ok) SetProcessDPIAware();

            string folder = null;
            bool open = false, clipboard = false;
            int delay = 10;
            for (int i = 0; i < args.Length; i++)
            {
                string a = args[i].TrimStart('-', '/').ToLowerInvariant();
                if (a == "folder" && i + 1 < args.Length) folder = args[++i];
                else if (a == "open") open = true;
                else if (a == "clipboard") clipboard = true;
                else if (a == "delay" && i + 1 < args.Length) int.TryParse(args[++i], out delay);
            }
            if (string.IsNullOrEmpty(folder))
                folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
            Directory.CreateDirectory(folder);

            if (delay > 0)
            {
                Application.Run(new CountdownForm(delay));
                // Chwila, zeby okienko odliczania na pewno zniknelo z ekranu przed zrzutem.
                System.Threading.Thread.Sleep(300);
            }

            // Prostokat obejmujacy wszystkie monitory (wspolrzedne moga byc ujemne,
            // np. gdy monitor stoi na lewo lub powyzej glownego).
            Rectangle all = Rectangle.Empty;
            foreach (Screen s in Screen.AllScreens)
                all = all.IsEmpty ? s.Bounds : Rectangle.Union(all, s.Bounds);

            string file = Path.Combine(folder, "Screenshot_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");
            using (Bitmap bmp = new Bitmap(all.Width, all.Height, PixelFormat.Format32bppArgb))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    // Puste miejsca (gdy monitory maja rozne rozmiary) zostaja czarne.
                    g.Clear(Color.Black);
                    foreach (Screen s in Screen.AllScreens)
                    {
                        Rectangle b = s.Bounds;
                        g.CopyFromScreen(b.Left, b.Top, b.Left - all.Left, b.Top - all.Top, b.Size);
                    }
                }
                bmp.Save(file, ImageFormat.Png);
                if (clipboard) Clipboard.SetImage(bmp);
            }

            if (open) Process.Start(file);
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Screenshot - blad", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }
}

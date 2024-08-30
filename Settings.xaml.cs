using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;

namespace Scoreboard
{
    /// <summary>
    /// Settings.xaml の相互作用ロジック
    /// </summary>
    public partial class Settings : Window
    {
        public int stateClose = 0;
        public string leftTeamName { get; set; }
        public int leftTeamNameSzFont { get; set; }
        public string rightTeamName { get; set; }
        public int rightTeamNameSzFont { get; set; }
        public int periodSets { get; set; }
        public int periodTime { get; set; }
        public int periodInterval { get; set; }
        public string wav_path { get; set; }

        public Settings()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                textLeftTeam.Text = leftTeamName;
                listLeftFont.SelectedIndex = (leftTeamNameSzFont - 62) / 2;
                textRightTeam.Text = rightTeamName;
                listRightFont.SelectedIndex = (rightTeamNameSzFont - 62) / 2;
                textPeriodSets.Text = periodSets.ToString();
                textPeriodTime.Text = periodTime.ToString();
                textPeriodInterval.Text = periodInterval.ToString();

                textWaveFile.Text = wav_path;
            };
        }

        private void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            int[] sz = { 62, 64, 66, 68, 70, 72, 74, 76, 78, 80 };
            
            leftTeamName = textLeftTeam.Text;
            leftTeamNameSzFont = sz[listLeftFont.SelectedIndex];
            rightTeamName = textRightTeam.Text;
            rightTeamNameSzFont = sz[listRightFont.SelectedIndex];
            try
            {
                int n = Int32.Parse(textPeriodSets.Text.ToString());
                if (1 <= n && n <= 10)
                {
                    periodSets = n;
                }
            }
            catch(Exception ex) { }
            try
            {
                int n = Int32.Parse(textPeriodTime.Text.ToString());
                if (1 <= n && n <= 20)
                {
                    periodTime = n;
                }
            }
            catch (Exception ex) { }
            try
            {
                int n = Int32.Parse(textPeriodInterval.Text.ToString());
                if (1 <= n && n <= 20)
                {
                    periodInterval = n;
                }
            }
            catch (Exception ex) { }

            wav_path = null;
            if (textWaveFile.Text.Length > 0)
            {
                wav_path = textWaveFile.Text;
                if (!File.Exists(wav_path))
                {
                    wav_path = null;
                }
            }

            stateClose = 1;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            stateClose = 0;
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            string k = e.Key.ToString();
            if (k == "Q" || k == "E")
            {
                Button btn = (Button)FindName("buttonCancel");
                var provider = new ButtonAutomationPeer(btn) as IInvokeProvider;
                provider.Invoke();
            }

        }

        private void WaveFile_Click(object sender, RoutedEventArgs e)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".wav"; // Default file extension
            dlg.Filter = "Text documents (.wav)|*.wav"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                textWaveFile.Text = dlg.FileName;
            }
        }
    }
}

using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Interop;

namespace Scoreboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int VK_F4 = 0x73;
        private DownTimer? downTimer = null;

        private int countPeriod = 0;
        private int scoreLeft = 0;  // period countPeriod の leftチームのスコア
        private int scoreRight = 0; // period countPeriod の rightチームのスコア

        private Match match;
        WaveOut? waveOut = null;
        WaveFileReader? wav_reader = null;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_OnLoaded;
        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));

            Debug.Print("1");
            match = new Match();
            Debug.Print("1.1");

            downTimer = new DownTimer(match.periodSets, match.periodTime, match.periodInterval);
            downTimer.StateChange += ChangeState;

            prnPeriod(0);

            MMDevice device;
            MMDeviceEnumerator DevEnum = new();
            device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            int val = (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            labelVolume.Content = val.ToString();
            sliderSystemVolume.Value = val;

            if (match.wav_path == null)
            {
                buttonAkiofutatabi.IsEnabled = false;
            }
            else
            {
                wav_reader = new WaveFileReader(match.wav_path);
                waveOut = new WaveOut();
                waveOut.Init(wav_reader);
                waveOut.Play();
                waveOut.Pause();
            }

#if !DEBUG
            this.WindowStyle = WindowStyle.None;
            this.WindowState = System.Windows.WindowState.Maximized;
#endif
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if ( msg == WM_SYSKEYDOWN  && wParam.ToInt32() == VK_F4 )
            {
                handled = true;
            }

            return IntPtr.Zero;
        }
        protected virtual void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
#if !DEBUG
            if (MessageBoxResult.Yes != MessageBox.Show("このプログラムを終了。いいですか？", "確認",
                MessageBoxButton.YesNo, MessageBoxImage.Information, MessageBoxResult.No) )
            {
                e.Cancel = true;
                return;
            }
#endif
        }

        private void prnPeriod(int mode)
        {
            if (mode == 0)
            {
                countPeriod = 1;
                lblPeriod.Content = "1";

                scoreLeft = 0;
                scoreRight = 0;

                buttonReset.IsEnabled = false;

                buttonSettings.IsEnabled = true;
            }
            string tm = match.periodTime.ToString() + ":00";
            labelWatch.Content = tm;

            leftTeam.Content = match.left.name;
            leftTeam.FontSize = match.left.sizeFont;
            rightTeam.Content = match.right.name;
            rightTeam.FontSize = match.right.sizeFont;

            if (match.wav_path == null)
            {
                buttonAkiofutatabi.IsEnabled = false;
            }

            for (int i = 1; i <= 5; i++)
            {
                string objnL = "labelLPeriod" + i.ToString();
                string objnR = "labelRPeriod" + i.ToString();
                Label objL = (Label)FindName(objnL);
                string txt = i <= match.periodSets ? "P" + i.ToString() : "";
                objL.Content = txt;
                Label objR = (Label)FindName(objnR);
                objR.Content = txt;

                objnL = "labelLPeriod" + i.ToString() + "score";
                objL = (Label)FindName(objnL);
                objnR = "labelRPeriod" + i.ToString() + "score";
                objR = (Label)FindName(objnR);
                if (i <= match.periodSets)
                {
                    if (mode == 0)
                    {
                        objL.Content = "";
                        objR.Content = "";
                    }
                }
                else
                {
                    objL.Content = "";
                    objR.Content = "";
                }
            }

            labelScoreLeft.Content = scoreLeft.ToString();
            labelScoreRight.Content = scoreRight.ToString();
        }

        private void sliderSystemVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int val = (int)sliderSystemVolume.Value;
            labelVolume.Content = val.ToString();

            VolumeController vc = new VolumeController();
            vc.SetVolume(val);
        }

        private void buttonClose_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Stop();
                    Task.Delay(100).Wait();
                }
            }

            Close();
        }
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            string k = e.Key.ToString();
            if (k == "Q" || k == "E")
            {
                Button btn = (Button)FindName("buttonClose");
                var provider = new ButtonAutomationPeer(btn) as IInvokeProvider;
                provider.Invoke();
            }
        }

        private void buttonAkiofutatabi_Click(object sender, RoutedEventArgs e)
        {
            if (waveOut != null)
            {
                if (waveOut.PlaybackState == PlaybackState.Stopped)
                {
                    wav_reader.Seek(0, SeekOrigin.Begin);
                    waveOut.Play();
                }
                if (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    waveOut.Pause();
                }
                else if (waveOut.PlaybackState == PlaybackState.Paused)
                {
                    waveOut.Play();
                }
            }
        }

        private void buttonStatus_Click(object sender, RoutedEventArgs e)
        {
            downTimer.buttonPush();

            buttonSettings.IsEnabled = downTimer.state == StateGame.StateTAIKI ? true : false;
        }

        private void buttonReset_Click(object sender, RoutedEventArgs e)
        {
            prnPeriod(0);
            downTimer.reset();

            buttonSettings.IsEnabled = downTimer.state == StateGame.StateTAIKI ? true : false;
        }

        private void ChangeState(object? sender, StateChangeEventArgs args)
        {
            this.Dispatcher.Invoke(() =>
            {
                buttonStatus.Content = args.buttonCaption;
                labelState.Content = args.stateCaption;
                string tm = (args.time / 60).ToString() + ":" + (args.time % 60).ToString("D2");
                labelWatch.Content = tm;

                buttonReset.IsEnabled = args.state == StateGame.StateTEISI ? true : false;
            });
        }

        private void buttonSetting_Click(object sender, RoutedEventArgs e)
        {
            var settings = new Settings();

            settings.Owner = this;
            settings.leftTeamName = match.left.name;
            settings.leftTeamNameSzFont = match.left.sizeFont;
            settings.rightTeamName = match.right.name;
            settings.rightTeamNameSzFont = match.right.sizeFont;
            settings.periodSets = match.periodSets;
            settings.periodTime = match.periodTime;
            settings.periodInterval = match.periodInterval;
            settings.wav_path = match.wav_path;
            settings.ShowDialog();
            if (settings.stateClose == 0)
            {
                return;
            }
            match.left.name = settings.leftTeamName;
            match.left.sizeFont = settings.leftTeamNameSzFont;
            match.right.name = settings.rightTeamName;
            match.right.sizeFont = settings.rightTeamNameSzFont;
            match.periodSets = settings.periodSets;
            match.periodTime = settings.periodTime;
            match.periodInterval = settings.periodInterval;
            match.wav_path = settings.wav_path;

            prnPeriod(1);

            downTimer.UpdateProps(match.periodSets, match.periodTime, match.periodInterval);
            match.update();
        }

        private void lblPeriod_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            countPeriod++;
            if (countPeriod > match.periodSets)
            {
                countPeriod = 1;
            }
            else
            {
                scoreLeft = 0;
                scoreRight = 0;
            }
            lblPeriod.Content = countPeriod.ToString();
            labelScoreLeft.Content = scoreLeft.ToString();
            labelScoreRight.Content = scoreRight.ToString();
        }

        private void lblPeriod_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            countPeriod--;
            if (countPeriod < 1)
            {
                countPeriod = match.periodSets;
            }
            else
            {
                scoreLeft = 0;
                scoreRight = 0;
            }
            lblPeriod.Content = countPeriod.ToString();
            labelScoreLeft.Content = scoreLeft.ToString();
            labelScoreRight.Content = scoreRight.ToString();
        }

        private void labelScoreLeft_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            scoreLeft++;
            prnSideScore("labelLPeriod", labelScoreLeft, scoreLeft);
        }
        private void labelScoreLeft_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            scoreLeft--;
            if (scoreLeft < 0)
            {
                scoreLeft = 0;
            }
            prnSideScore("labelLPeriod", labelScoreLeft, scoreLeft);
        }

        private void labelScoreRight_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            scoreRight++;
            prnSideScore("labelRPeriod", labelScoreRight, scoreRight);
        }
        private void prnSideScore(string lblscore, Label o_label, int score)
        {
            o_label.Content = score.ToString();
            string o = lblscore + countPeriod.ToString() + "score";
            Label lbl = (Label)FindName(o);
            lbl.Content = score.ToString();
        }
        private void labelScoreRight_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            scoreRight--;
            if (scoreRight < 0)
            {
                scoreRight = 0;
            }
            prnSideScore("labelRPeriod", labelScoreRight, scoreRight);
        }

        private void labelWatch_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (downTimer.state == StateGame.StateTEISI)
            {
                downTimer.adjustTime(1);
            }
        }

        private void labelWatch_PreviewMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (downTimer.state == StateGame.StateTEISI)
            {
                downTimer.adjustTime(-1);
            }
        }
    }

    class VolumeController
    {
        public void SetVolume(int value)
        {
            //音量を変更
            MMDevice device;
            MMDeviceEnumerator DevEnum = new MMDeviceEnumerator();
            device = DevEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = ((float)value / 100.0f);
        }
    }
}
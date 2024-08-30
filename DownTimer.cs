using System.Windows.Threading;

namespace Scoreboard
{
    public enum StateGame : int { StateTAIKI = 0, StateKEIJI, StateTEISI, StateGAMEOVER, StateRESET }

    internal class DownTimer
    {
        private string[] StateState = ["待機", "計時", "停止", "ゲームオーバー"];
        private string[] StateButtonName = ["開始", "ストップ", "再開", "待機"];

        public StateGame state;
        private int periodSets;
        private int periodTime;
        private int periodInterval;

        private DispatcherTimer? timer = null;
        private int downtm = 0;

        public DownTimer(int periodSets_, int periodTime_, int periodInterval_)
        {
            UpdateProps(periodSets_, periodTime_, periodInterval_);
        }

        public void UpdateProps(int periodSets_, int periodTime_, int periodInterval_)
        {
            periodSets = periodSets_;
            periodTime = periodTime_;
            periodInterval = periodInterval_;
            downtm = 60 * periodTime;

            state = StateGame.StateTAIKI;

            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += Timer_Tick;
        }
        public void adjustTime(int sec)
        {
            downtm += sec;
            if (downtm < 0)
            {
                downtm = 60 * periodTime;
            }
            else
            {
                if (downtm >= 60 * periodTime)
                {
                    downtm = 0;
                }
            }
            EventHandler<StateChangeEventArgs> handler = StateChange;
            if (handler != null)
            {
                StateChangeEventArgs args = new StateChangeEventArgs(state, StateState[(int)state], StateButtonName[(int)state], downtm);
                handler(this, args);
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            downtm--;
            if (downtm == 0)
            {
                state = StateGame.StateGAMEOVER;
                timer.Stop();
            }

            EventHandler<StateChangeEventArgs> handler = StateChange;
            if (handler != null)
            {
                StateChangeEventArgs args = new StateChangeEventArgs(state, StateState[(int)state], StateButtonName[(int)state], downtm);
                handler(this, args);
            }
        }

        public event EventHandler<StateChangeEventArgs> StateChange;

        public void reset()
        {
            state = StateGame.StateGAMEOVER;
            buttonPush();
        }
        public void buttonPush()
        {
            switch (state)
            {
                case StateGame.StateTAIKI:
                    state = StateGame.StateKEIJI;

                    downtm = 60 * periodTime;
                    timer.Start();
                    break;

                case StateGame.StateKEIJI:
                    state = StateGame.StateTEISI;

                    timer.Stop();
                    break;

                case StateGame.StateTEISI:
                    state = StateGame.StateKEIJI;

                    timer.Start();
                    break;

                case StateGame.StateGAMEOVER:
                    state = StateGame.StateTAIKI;

                    downtm = 60 * periodTime;
                    timer.Stop();
                    break;
            }
            EventHandler<StateChangeEventArgs> handler = StateChange;
            if (handler != null)
            {
                StateChangeEventArgs args = new StateChangeEventArgs(state, StateState[(int)state], StateButtonName[(int)state], downtm);
                handler(this, args);
            }
        }
    }

    public class StateChangeEventArgs : EventArgs
    {
        public StateGame state { get; set; }
        public string stateCaption { get; set; }
        public string buttonCaption { get; set; }
        public int time { get; set; }
        public StateChangeEventArgs(StateGame state, string stateCaption, string buttonCaption, int time)
        {
            this.state = state;
            this.stateCaption = stateCaption;
            this.buttonCaption = buttonCaption;
            this.time = time;
        }
    }


}

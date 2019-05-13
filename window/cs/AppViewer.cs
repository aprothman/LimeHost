using System;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace LimeWrapper
{
    public partial class AppViewer : UserControl
    {
        // unlike the DesignMode property, this works before the object is constructed
        private bool IsInDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public AppViewer()
        {
            InitializeComponent();

            if (!IsInDesignMode) {
                SetupLimeHost();
            }

            _ticks = new StringBuilder();
            _lastTick = DateTime.Now;
        }

        private void SetupLimeHost()
        {
            this._limeHost = new LimeWrapper.LimeHost();

            this._limeHost.BackColor = System.Drawing.Color.Gray;
            this._limeHost.Location = new System.Drawing.Point(0, 0);
            this._limeHost.Name = "limeHost";
            this._limeHost.Size = this.Size;
            this._limeHost.TabIndex = 0;
            this._limeHost.EngineInitialized += (o, e) => { _engineInitialized = true; };

            this.SizeChanged += (o, e) => _limeHost.Size = this.Size;
            this.Controls.Add(this._limeHost);
        }

        private const int WM_TIMER = 0x0113;
        private const uint TIMER_ID = 99;
        private const int PERIOD = 28;

        [DllImport("User32.dll", EntryPoint = "SetTimer", CallingConvention = CallingConvention.StdCall)]
        private static extern UIntPtr SetTimer(IntPtr hWnd, uint nIDEvent, uint uElapse, IntPtr lpTimerFunction);
        [DllImport("User32.dll", EntryPoint = "KillTimer", CallingConvention = CallingConvention.StdCall)]
        private static extern int KillTimer(IntPtr hWnd, uint nIDEvent);

        private LimeWrapper.LimeHost _limeHost;

        private bool _disposing = false;

        private bool _timerInitialized = false;
        private bool _engineInitialized = false;

        private StringBuilder _ticks;
        private DateTime _lastTick;
        private TimeSpan _tickPeriod = TimeSpan.FromMilliseconds(15.0);
        private int _targetTimerPeriod = PERIOD;

        public bool TimerTicking => _timerInitialized;

        public int TickRate => (int)(1 / _tickPeriod.TotalSeconds);

        public int TargetTickRate
        {
            get => 1000 / _targetTimerPeriod;
            set => _targetTimerPeriod = 1000 / value;
        }

        public void StartTicking()
        {
            var ret = SetTimer(this.Handle, TIMER_ID, (uint)_targetTimerPeriod, IntPtr.Zero);
            _timerInitialized = UIntPtr.Zero != ret ? true : false;
        }

        public void StopTicking()
        {
            if (_timerInitialized) {
                KillTimer(this.Handle, TIMER_ID);
                _timerInitialized = false;
            }
        }

        public void TimerTick(DateTime tickTime)
        {
            _tickPeriod = tickTime - _lastTick;
            _lastTick = tickTime;

            if (_engineInitialized) {
                _limeHost.EngineUpdate(10);
                return;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!IsInDesignMode) {
                StartTicking();
            }

            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            StopTicking();

            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg) {
                case WM_TIMER:
                    if (TIMER_ID == m.WParam.ToInt32()) {
                        if (!_disposing) {
                            TimerTick(DateTime.Now);
                        }
                    }
                    break;
                default:
                    break;
            }

            base.WndProc(ref m);
        }
    }
}

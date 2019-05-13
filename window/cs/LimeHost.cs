using System;
using System.Windows.Forms;
using System.ComponentModel;

namespace LimeWrapper
{
    internal class LimeHost : Control
    {
        // unlike the DesignMode property, this works before the object is constructed
        private bool IsInDesignMode => LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        public LimeHost()
        {
            if (!IsInDesignMode) {
                SetStyle(ControlStyles.UserPaint
                       | ControlStyles.AllPaintingInWmPaint
                       | ControlStyles.Opaque, true);
            }
        }

        public bool EngineUpdate(int numEvents)
        {
            return DllWrapper.Update(numEvents);
        }

        protected const int CS_OWNDC    = 0x0020;
        protected const int CS_VREDRAW  = 0x0001;
        protected const int CS_HREDRAW  = 0x0002;
        protected const int CS_PARENTDC = 0x0080;
        protected const int CS_DBLCLKS  = 0x0008;

        protected override CreateParams CreateParams
        {
            get {
                // important class styles:
                // CS_OWNDC | CS_DBLCLKS | CS_HREDRAW | CS_VREDRAW;
                // ~CS_PARENTDC
                var createParams = base.CreateParams;

                if (String.IsNullOrEmpty(createParams.ClassName)) {
                    createParams.ClassName = DllWrapper.GetWindowClass();
                }

                return createParams;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            GlWrapper.SetupPixelFormat(Handle);

            DllWrapper.InitHaxe();
            DllWrapper.InitLocalWindow(Handle);
            //DllWrapper.InitWindow();

            base.OnHandleCreated(e);

            EngineInitialized?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler EngineInitialized;

        #region Boilerplate

        public class OnIsInputKeyEventArgs : EventArgs
        {
            public Keys KeyData;
            public bool InputKey;
        }
        public event EventHandler<OnIsInputKeyEventArgs> OnIsInputKey;

        protected override bool IsInputKey(Keys keyData)
        {
            // support PreviewKeyDown
            var e = new OnIsInputKeyEventArgs() {
                KeyData = keyData,
                InputKey = base.IsInputKey(keyData)
            };

            // give a client the ability to change InputKey
            OnIsInputKey?.Invoke(this, e);

            return e.InputKey;
        }

        #endregion
    }
}

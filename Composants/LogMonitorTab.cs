using System;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    class LogMonitorTab : TabPage, IDisposable
    {
        public LogMonitorTab(LogMonitorControl logControl)
        {
            _logControl = logControl;
            _logControl.LogFileChanged += LogFileChangedHandler;
            this.Enter += LogMonitorTab_GotFocus;
            _logControl.ScrollToEnd();
        }

        private void LogFileChangedHandler(object sender, EventArgs args)
        {
            if (Parent is TabControl tabControl)
            {
                if (tabControl.InvokeRequired) { tabControl.Invoke(new MethodInvoker(delegate { if (tabControl.SelectedTab != this && !Text.StartsWith("*")) { Text = "*" + Text; } })); }
                else { if (tabControl.SelectedTab != this && !Text.StartsWith("*")) { Text = "*" + Text; } }
            }
        }

        private void LogMonitorTab_GotFocus(object sender, EventArgs e)
        {
            Text = Text.Substring(1);
            _logControl.ScrollToEnd();
        }

        override protected void Dispose(bool calledDirectly)
        {
            if (calledDirectly)
            {
                if (null != _logControl)
                {
                    _logControl.LogFileChanged -= LogFileChangedHandler;
                    _logControl.Dispose();
                }
            }
            base.Dispose(calledDirectly);
        }

        private readonly LogMonitorControl _logControl;
    }
}

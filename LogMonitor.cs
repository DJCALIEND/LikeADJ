using System.IO;
using System.Windows.Forms;

namespace MusicBeePlugin
{
    public partial class LogMonitor : Form
    {
        public LogMonitor(string LogFile)
        {
            InitializeComponent();
            MonitorNewFile(LogFile);
        }

        private void MonitorNewFile(string fileName)
        {
            LogMonitorControl logControl = new LogMonitorControl(fileName);
            LogMonitorTab newTab = new LogMonitorTab(logControl);

            newTab.Controls.Add(logControl);
            newTab.Text = "_" + Path.GetFileName(fileName);
            newTab.Name = "_" + Path.GetFileName(fileName);
            logControl.Dock = System.Windows.Forms.DockStyle.Fill;
            _tabControl.TabPages.Add(newTab);
        }
    }
}
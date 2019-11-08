using LogMonitor.UserControls;
using System.IO;
using System.Windows.Forms;

namespace LogMonitor
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
            newTab.Location = new System.Drawing.Point(4, 22);
            newTab.Padding = new System.Windows.Forms.Padding(3);
            newTab.Size = new System.Drawing.Size(664, 537);
            newTab.TabIndex = 0;
            newTab.UseVisualStyleBackColor = true;

            string title = Path.GetFileName(fileName);
            newTab.Text = title;
            newTab.Name = title;

            logControl.Dock = System.Windows.Forms.DockStyle.Fill;
            logControl.Location = new System.Drawing.Point(3, 3);
            logControl.Name = "_logMonitorControl";
            logControl.Size = new System.Drawing.Size(658, 531);
            logControl.TabIndex = 0;

            _tabControl.TabPages.Add(newTab);
            _tabControl.SelectedIndex = _tabControl.TabPages.Count - 1;
        }
    }
}
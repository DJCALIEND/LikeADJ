using System;
using System.IO;
using System.Windows.Forms;

namespace LogMonitor.UserControls
{
    public partial class LogMonitorControl : UserControl
    {
        public LogMonitorControl()
        {
            InitializeComponent();
        }

        public LogMonitorControl(string fileName)
        {
            InitializeComponent();

            FileName = fileName;
        }

        //public EventHandler<EventArgs> BrowseForLogFile;

        //protected void OnBrowseForLogFile()
        //{
        //    EventHandler<EventArgs> handler = BrowseForLogFile;
        //    if (null != handler)
        //    {
        //        handler(this, EventArgs.Empty);
        //    }
        //}

        public EventHandler<EventArgs> LogFileChanged;

        protected void OnLogFileChanged()
        {
            EventHandler<EventArgs> handler = LogFileChanged;
            if (null != handler)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;

                        ClearMonitoringMethod();

                        PrepareMonitoringMethod();
                        ReloadFile();                
                }
            }
        }

        private void ClearMonitoringMethod()
        {
            ClearTimer();
            ClearWatcher();
        }

        private void ClearTimer()
        {
            if (null != _timer)
            {
                _timer.Tick -= timer_Tick;
                _timer.Dispose();
                _timer = null;
            }
        }

        private void ClearWatcher()
        {
            if (null != _watcher)
            {
                FileSystemEventHandler handler = new FileSystemEventHandler(watcher_Changed);
                _watcher.Changed -= handler;
                _watcher.Created -= handler;
                _watcher.Deleted -= handler;
                _watcher.Renamed -= watcher_Renamed;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        public void CopyToClipboard()
        {
            if (0 < _textBoxContents.SelectionLength) _textBoxContents.Copy();
            else Clipboard.SetText(_textBoxContents.Text);
        }

        private void PrepareMonitoringMethod()
        {

                ClearTimer();

                if (null != _fileName && null == _watcher)
                {
                    string path = Path.GetDirectoryName(_fileName);
                    string baseName = Path.GetFileName(_fileName);

                    _watcher = new System.IO.FileSystemWatcher(path.Length == 0 ? "." : path, baseName);
                    FileSystemEventHandler handler = new FileSystemEventHandler(watcher_Changed);
                    _watcher.Changed += handler;
                    _watcher.Created += handler;
                    _watcher.Deleted += handler;
                    _watcher.Renamed += watcher_Renamed;
                    _watcher.EnableRaisingEvents = true;
                }
        }

        private void ReloadFile()
        {
            if (_reloadingFile) return; 
            _reloadingFile = true;
            try
            {
                if (null == _fileName)
                {
                    _textBoxContents.Text = "";
                }
                else
                {
                    string newFileLines = "";
                    _lastModifiedTime = File.GetLastWriteTime(_fileName); 
                    long newLength = 0;  
                    bool fileExists = File.Exists(_fileName);
                    bool needToClear = !fileExists;
                    if (fileExists)
                    {
                        int count = 0;
                        bool success = false;
                        while (count < 5 && !success)
                        {
                            try
                            {
                                using (FileStream stream = File.Open(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    newLength = stream.Length;
                                    if (newLength >= _lastFileSize)
                                    {
                                        stream.Position = _lastFileSize;  
                                    }
                                    else
                                    {
                                        needToClear = true;
                                    }
                                    using (StreamReader reader = new StreamReader(stream))
                                    {
                                        newFileLines = reader.ReadToEnd();
                                    }
                                }
                                success = true;
                            }
                            catch (IOException)
                            {
                                System.Threading.Thread.Sleep(50); 
                            }
                            ++count;
                        }
                    }

                    _lastFileSize = newLength;
                    string fileSizeString = _lastFileSize.ToString();
                    string modifiedTimeStr = _lastModifiedTime.ToShortDateString() + " " + _lastModifiedTime.ToLongTimeString();
                    if (!fileExists)
                    {
                        fileSizeString = "<Not Found>";
                        modifiedTimeStr = "";
                    }
                    if (0 != newFileLines.Length)
                    {
                        int lastCr = newFileLines.LastIndexOf('\n');
                        if (-1 != lastCr && 0 < lastCr)
                        {
                            if (newFileLines[lastCr - 1] != '\r')
                            {
                                newFileLines = newFileLines.Replace("\n", "\r\n");
                            }
                        }
                    }
                    if (needToClear)
                    {
                        if (_textBoxContents.InvokeRequired)
                        {
                            _textBoxContents.Invoke(new MethodInvoker(delegate { _textBoxContents.Clear(); }));
                        }
                        else
                        {
                            _textBoxContents.Clear();
                        }
                    }
                    if (_textBoxContents.InvokeRequired)
                    {
                        if (0 != newFileLines.Length)
                        {
                            _textBoxContents.Invoke(new DoUpdateTextDelegate(DoUpdateText), new object[] { newFileLines });
                        }
                    }
                    else
                    {
                        if (0 != newFileLines.Length)
                        {
                            DoUpdateText(newFileLines);
                        }
                    }
                    ScrollToEnd();
                }
            }
            finally
            {
                _reloadingFile = false;
            }
        }

        private delegate void DoUpdateTextDelegate(string newLogLines);

        private void DoUpdateText(string newLogLines)
        {
            int selStart = _textBoxContents.SelectionStart;
            int selLength = _textBoxContents.SelectionLength;
            int textTrimSize = 40000;

            if (_textBoxContents.Text.Length + newLogLines.Length > 65535)
            {
                if (newLogLines.Length >= textTrimSize)
                {
                    _textBoxContents.Text = "";
                    newLogLines = newLogLines.Substring(newLogLines.Length - textTrimSize);
                }
                else
                {
                    _textBoxContents.Text = _textBoxContents.Text.Substring(textTrimSize - newLogLines.Length);
                }
            }

            _textBoxContents.AppendText(newLogLines);

            if (-1 != selStart)
            {
                _textBoxContents.SelectionStart = selStart;
                _textBoxContents.SelectionLength = selLength;

            }
        }

        public void ScrollToEnd()
        {
            if (_textBoxContents.InvokeRequired)
            {
                _textBoxContents.Invoke(new MethodInvoker(DoScrollToEnd));
            }
            else
            {
                DoScrollToEnd();
            }
        }

        private void DoScrollToEnd()
        {
            int selStart = _textBoxContents.SelectionStart;
            int selLength = _textBoxContents.SelectionLength;
            _textBoxContents.SelectionStart = _textBoxContents.Text.Length;
            _textBoxContents.SelectionLength = 0;
            _textBoxContents.ScrollToCaret();
            if (-1 != selStart)
            {
                _textBoxContents.SelectionStart = selStart;
                _textBoxContents.SelectionLength = selLength;
            }
        }

        private void UpdateBasedOnFileTime()
        {
            if (null != _fileName)
            {
                DateTime newLastModifiedTime = File.GetLastWriteTime(_fileName);
                if ((newLastModifiedTime.Year < 1620 && newLastModifiedTime != _lastModifiedTime)
                    || newLastModifiedTime > _lastModifiedTime)
                {
                    OnLogFileChanged();
                    ReloadFile();
                }
            }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            UpdateBasedOnFileTime();
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            OnLogFileChanged();
            ReloadFile();
        }

        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            ReloadFile();
        }

        private string _fileName;
        private FileSystemWatcher _watcher;
        private Timer _timer;
        private DateTime _lastModifiedTime;
        private long _lastFileSize;
        private bool _intervalChanged;
        private bool _reloadingFile;
    }
}

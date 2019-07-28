using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MusicBeePlugin
{
    public class LogFile : TraceListener
    {
        private readonly FileSystemWatcher watcher = null;
        private string _logPath;
        private readonly Object fileLock = new Object();
        private static Stack StackLogToWrite;

        public string LogPath
        {
            set
            {
                _logPath = value;
                string fileName = Path.GetFileName(_logPath);
                string directoryLogPath = Path.GetDirectoryName(_logPath);
                if (directoryLogPath != string.Empty) if (!Directory.Exists(directoryLogPath)) Directory.CreateDirectory(directoryLogPath);
            }
            get { return _logPath; }
        }
        public int MaxLogInWait { set; get; }
        public long MaxLogSize { set; get; }
        public bool ShowFatalErrorInMessageBox { set; get; }
        public bool WriteDateInfo { set; get; }
        public bool IsErrorDetected { private set; get; } = false;
        public Exception LastException { private set; get; } = null;
        public string LastErrorMsg { private set; get; } = null;

        public LogFile(string logPath)
        {
            LogPath = logPath;
            MaxLogSize = 2000000;
            WriteDateInfo = true;
            StackLogToWrite = new Stack();
            MaxLogInWait = 50;

            watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(this.LogPath),
                Filter = Path.GetFileName(this.LogPath),
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName
            };
            watcher.Changed += new FileSystemEventHandler(Watcher_Changed);
            watcher.Created += new FileSystemEventHandler(Watcher_Changed);
            watcher.Deleted += new FileSystemEventHandler(Watcher_Changed);
            watcher.Renamed += new RenamedEventHandler(Watcher_Changed);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (message == string.Empty) message = "";
            if (eventType == TraceEventType.Error) RaiseExceptionDetectedEvent(message, null);

            message = eventType.ToString() + " : " + message + "\r\n";
            WriteLine(message);
        }

        public event EventHandler ErrorDetectedEvent;

        protected virtual void RaiseExceptionDetectedEvent(string messageCourt, Exception ex)
        {
            LastException = ex;
            LastErrorMsg = messageCourt;
            IsErrorDetected = true;

            ErrorDetectedEvent?.Invoke(this, new EventArgs());
        }

        public override void WriteLine(string message)
        {
            Write(message);
        }

        public override void Write(string message)
        {
            message = DateTime.Now.ToString() + " " + message;
            WriteInFic(message);
        }

        public override void WriteLine(object o)
        {
            base.WriteLine(o);
        }

        private void WriteInFic(string message)
        {
            bool runThread = false;

            lock (StackLogToWrite)
            {
                if (StackLogToWrite.Count == 0) runThread = true;
                StackLogToWrite.Push(message);
            }

            if (runThread)
            {
                Thread WriteThread = new Thread(WriteInFicThreadStart);
                WriteThread.Start();
            }
        }

        private void WriteInFicThreadStart()
        {
            try
            {
                lock (fileLock)
                {
                    long tailleFic = 0;
                    string oldFilePath = LogPath + ".old";
                    bool bRecopieNedd = false;
                    if (File.Exists(LogPath)) bRecopieNedd = true;

                    FileStream fsw = null;

                    try { fsw = new FileStream(LogPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read); }
                    catch
                    {
                        if (watcher.EnableRaisingEvents == false) watcher.EnableRaisingEvents = true;
                        return;
                    }

                    if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
                    if (bRecopieNedd) File.Copy(this.LogPath, oldFilePath);

                    StreamWriter sw = new StreamWriter(fsw);
                    lock (StackLogToWrite)
                    {
                        while (StackLogToWrite.Count > 0)
                        {
                            string message = (string)StackLogToWrite.Pop();
                            sw.Write(message);
                            tailleFic += message.Length;
                        }
                    }

                    if (bRecopieNedd)
                    {
                        using (StreamReader sr = new StreamReader(oldFilePath))
                        {
                            String line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                tailleFic += line.Length;

                                if (tailleFic < MaxLogSize || MaxLogSize == 0) sw.WriteLine(line);
                                else break;
                            }
                        }
                        File.Delete(oldFilePath);
                    }
                    sw.Close();
                    fsw.Close();

                    lock (StackLogToWrite)
                    {
                        if (StackLogToWrite.Count > 0)
                        {
                            if (StackLogToWrite.Count > MaxLogInWait && MaxLogInWait != 0) { StackLogToWrite.Clear(); }
                            Thread WriteThread = new Thread(WriteInFicThreadStart);
                            WriteThread.Start();
                        }
                    }
                }
            }
            catch { }
        }

        private void Watcher_Changed(object source, FileSystemEventArgs e)
        {
            if (watcher.EnableRaisingEvents == true) watcher.EnableRaisingEvents = false;
            WriteInFicThreadStart();
        }
    }
}

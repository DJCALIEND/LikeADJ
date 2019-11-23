using System.IO;

namespace MusicBeePlugin
{
    public class SimpleLogger
    {
        private readonly string logFilename;
        private readonly object fileLock = new object();

        public SimpleLogger(string logfile)
        {
            logFilename = logfile;
            if (System.IO.File.Exists(logFilename)) File.Delete(logFilename);

            if (!System.IO.File.Exists(logFilename)) { WriteLine("", false); }
        }

        public void Debug(string text)
        {
            WriteFormattedLog(LogLevel.DEBUG, text);
        }

        public void Error(string text)
        {
            WriteFormattedLog(LogLevel.ERROR, text);
        }

        public void Fatal(string text)
        {
            WriteFormattedLog(LogLevel.FATAL, text);
        }

        public void Info(string text)
        {
            WriteFormattedLog(LogLevel.INFO, text);
        }

        public void Trace(string text)
        {
            WriteFormattedLog(LogLevel.TRACE, text);
        }

        public void Warning(string text)
        {
            WriteFormattedLog(LogLevel.WARNING, text);
        }

        private void WriteLine(string text, bool append = true)
        {
            if (string.IsNullOrEmpty(text)) { return; }
            lock (fileLock)
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilename, append, System.Text.Encoding.UTF8)) { writer.WriteLine(text); }
            }
        }

        private void WriteFormattedLog(LogLevel level, string text)
        {
            string pretext;

            switch (level)
            {
                case LogLevel.TRACE:
                    pretext = System.DateTime.Now.ToString() + " - [TRACE] - ";
                    break;

                case LogLevel.INFO:
                    pretext = System.DateTime.Now.ToString() + " - [INFO] - ";
                    break;

                case LogLevel.DEBUG:
                    pretext = System.DateTime.Now.ToString() + " - [DEBUG] - ";
                    break;

                case LogLevel.WARNING:
                    pretext = System.DateTime.Now.ToString() + " - [WARNING] - ";
                    break;

                case LogLevel.ERROR:
                    pretext = System.DateTime.Now.ToString() + " - [ERROR] - ";
                    break;

                case LogLevel.FATAL:
                    pretext = System.DateTime.Now.ToString() + " - [FATAL] - ";
                    break;

                default:
                    pretext = "";
                    break;
            }

            WriteLine(pretext + text);
        }

        [System.Flags]
        private enum LogLevel
        {
            TRACE,
            INFO,
            DEBUG,
            WARNING,
            ERROR,
            FATAL
        }
    }
}

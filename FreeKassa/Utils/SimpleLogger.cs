/*
MIT License
Copyright (c) 2016 Heiswayi Nrird
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


using System;
using System.Threading;

namespace FreeKassa.Utils
{
    public class SimpleLogger
    {
        private readonly Action<string> _logWritingCallback;
        private const string FILE_EXT = ".log";
        private readonly object _locker = new object();
        private readonly string datetimeFormat;
        private readonly string logFilename;

        /// <summary>
        /// Initiate an instance of SimpleLogger class constructor.
        /// If log file does not exist, it will be created automatically.
        /// </summary>
        public SimpleLogger(Action<string> logWritingCallback)
        {
            _logWritingCallback = logWritingCallback;
            datetimeFormat = "dd-MM-yyyy HH:mm:ss.fff";
            logFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + FILE_EXT;

            // Log file header line
            string logHeader = logFilename + " is created.";
            if (!System.IO.File.Exists(logFilename))
            {
                WriteLine(System.DateTime.Now.ToString(datetimeFormat) + " " + logHeader);
            }
        }
        
        public SimpleLogger()
        {
            datetimeFormat = "dd-MM-yyyy HH:mm:ss.fff";
            logFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + FILE_EXT;

            // Log file header line
            string logHeader = logFilename + " is created.";
            if (!System.IO.File.Exists(logFilename))
            {
                WriteLine(System.DateTime.Now.ToString(datetimeFormat) + " " + logHeader);
            }
        }

        /// <summary>
        /// Log a DEBUG message
        /// </summary>
        /// <param name="text">Message</param>
        public void Debug(string text)
        {
            WriteFormattedLog(LogLevel.DEBUG, text);
        }

        /// <summary>
        /// Log an ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public void Error(string text)
        {
            WriteFormattedLog(LogLevel.ERROR, text);
        }

        /// <summary>
        /// Log a FATAL ERROR message
        /// </summary>
        /// <param name="text">Message</param>
        public void Fatal(string text)
        {
            WriteFormattedLog(LogLevel.FATAL, text);
        }

        /// <summary>
        /// Log an INFO message
        /// </summary>
        /// <param name="text">Message</param>
        public void Info(string text)
        {
            WriteFormattedLog(LogLevel.INFO, text);
        }

        /// <summary>
        /// Log a TRACE message
        /// </summary>
        /// <param name="text">Message</param>
        public void Trace(string text)
        {
            WriteFormattedLog(LogLevel.TRACE, text);
        }

        /// <summary>
        /// Log a WARNING message
        /// </summary>
        /// <param name="text">Message</param>
        public void Warning(string text)
        {
            WriteFormattedLog(LogLevel.WARNING, text);
        }

        private void WriteLine(string text, bool append = false)
        {
            try
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilename, append, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine(text);
                }
            }
            catch
            {
                throw;
            }
        }

        private void WriteFormattedLog(LogLevel level, string text)
        {
            lock (_locker)
            {
                string pretext;
                var threadLabel = Thread.CurrentThread.Name;
                switch (level)
                {
                    case LogLevel.TRACE:
                        pretext = System.DateTime.Now.ToString(datetimeFormat) + " [" + threadLabel + "]" + " [TRACE]   ";
                        break;
                    case LogLevel.INFO:
                        pretext = System.DateTime.Now.ToString(datetimeFormat) + " [" + threadLabel + "]" + " [INFO]    ";
                        break;
                    case LogLevel.DEBUG:
                        pretext = System.DateTime.Now.ToString(datetimeFormat) + " [" + threadLabel + "]" + " [DEBUG]   ";
                        break;
                    case LogLevel.WARNING:
                        pretext = System.DateTime.Now.ToString(datetimeFormat) + " [" + threadLabel + "]" + " [WARNING] ";
                        break;
                    case LogLevel.ERROR:
                        pretext = System.DateTime.Now.ToString(datetimeFormat) + " [" + threadLabel + "]" + " [ERROR]   ";
                        break;
                    case LogLevel.FATAL:
                        pretext = System.DateTime.Now.ToString(datetimeFormat) + " [" + threadLabel + "]" + " [FATAL]   ";
                        break;
                    default:
                        pretext = "";
                        break;
                }

                WriteLine(pretext + text, true);
                // _logWritingCallback(pretext + text);
            }
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
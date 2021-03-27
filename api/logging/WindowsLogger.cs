using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CSharp.NFC
{
    public class WindowsLogger : INFCLogger
    {
        private List<string> _logLines = new List<string>();
        private string _folderName = "NFCTicketValidatorLogs";
        private string _logFileName = "NFCTicketValidatorLog.txt";
        private StorageFolder _folder;
        private StorageFile _logFile;

        // I will manage big log files later
        public WindowsLogger()
        {
            EnsureFile();
        }

        private async void EnsureFile()
        {
            try
            {
                try
                {
                    _folder = await ApplicationData.Current.LocalFolder.GetFolderAsync(_folderName);
                }
                catch (FileNotFoundException)
                {
                    _folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(_folderName);
                }
                try
                {
                    _logFile = await _folder.GetFileAsync(_logFileName);
                }
                catch (FileNotFoundException)
                {
                    _logFile = await _folder.CreateFileAsync(_logFileName);
                }
            }            
            catch (Exception)
            {
                return;
            }
        }

        public void AddToLog(string message)
        {
            _logLines.Add($"[{DateTime.Now.ToString("G")}] Information: {message}");
        }

        public async void ManageException(Exception ex)
        {
            if(_logFile != null)
            {
                await FileIO.AppendTextAsync(_logFile, $"[{DateTime.Now.ToString("G")}] Exception: {ex.Message}{Environment.NewLine}");
            }
        }

        public async void CommitLogWrite()
        {
            try
            {
                await FileIO.AppendLinesAsync(_logFile, _logLines);

            }
            catch (Exception ex)
            {

            }
        }
    }
}

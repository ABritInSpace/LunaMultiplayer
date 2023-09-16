using JPCC.Settings.Structures;
using JPCC.Logging;
using System.Reflection;
using System.IO;
using Server.System;
using System.Security.Permissions;

namespace JPCC.Handler
{
    public class BackupSavesHandler
    {
        private string saveFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "..//..//Universe//";
        private string backupFilePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "..//..//Backups//";

        public BackupSavesHandler() {}
        public string[] GetBackupList()
        {
            return Directory.GetDirectories(backupFilePath).Select(d => new DirectoryInfo(d).Name).Reverse().ToArray();
        }

        public string MakeBackup()
        {
            if (!Directory.Exists(backupFilePath)){
                Directory.CreateDirectory(backupFilePath);
            }
            string rn = DateTime.Now.ToString("yyyy-MM-dd@H");
            string toCreate = backupFilePath+@"/"+rn;
            if (!Directory.Exists(toCreate))
            {
                Directory.CreateDirectory(toCreate);
                JPCCLog.Debug("Creating backup...");
                CloneDirectory(saveFilePath, toCreate);
                JPCCLog.Debug("Backup created!");
                return rn;
            }
            JPCCLog.Debug("Backup already exists!");
            return null;
        }
        private static void CloneDirectory(string root, string dest)
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                var newDirectory = Path.Combine(dest, Path.GetFileName(directory));
                Directory.CreateDirectory(newDirectory);
                CloneDirectory(directory, newDirectory);
            }

            foreach (var file in Directory.GetFiles(root))
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)));
            }
        }
        public bool RestoreBackup(string backup)
        {
            if(Directory.Exists(backupFilePath+backup))
            {
                JPCCLog.Debug("Restoring from backup...");
                Directory.Delete(saveFilePath, true);
                Directory.CreateDirectory(saveFilePath);
                CloneDirectory(backupFilePath+backup, saveFilePath);
                JPCCLog.Debug("Backup restore complete!");
                return true;
            }
            JPCCLog.Debug("Backup restore failed - backup no longer exists");
            return false;
        }
    }
}
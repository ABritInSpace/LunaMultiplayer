using LmpCommon.Xml;
using JPCC.Logging;
using Server.System;
using System.Reflection;

namespace JPCC.Settings.Base
{
    public abstract class SettingsBase<T> : ISettings
        where T : class, new()
    {
        protected abstract string Filename { get; }

        private string ConfigDirectory = Assembly.GetExecutingAssembly().Location + "//..//JPCC";

        protected string SettingsPath => Path.Combine(ConfigDirectory, Filename);
        public static T SettingsStore { get; private set; } = new T();

        protected SettingsBase()
        {
            if (!FileHandler.FolderExists(ConfigDirectory))
            {
                JPCCLog.Normal("Creating config folder...");
                try{FileHandler.FolderCreate(ConfigDirectory);}
                catch (Exception e){JPCCLog.Error(e.Message);}
            }
        }

        public virtual void Load()
        {
            if (!File.Exists(SettingsPath))
                LunaXmlSerializer.WriteToXmlFile(Activator.CreateInstance(typeof(T)), Path.Combine(ConfigDirectory, Filename));

            try
            {
                SettingsStore = LunaXmlSerializer.ReadXmlFromPath(typeof(T), SettingsPath) as T;
                Save(); //We call the save to add the new settings into the file
            }
            catch (Exception)
            {
                JPCCLog.Fatal($"Error while trying to read {SettingsPath}. Default settings will be used. Please remove the file so a new one can be generated");
            }
        }

        public void Save()
        {
            LunaXmlSerializer.WriteToXmlFile(SettingsStore, SettingsPath);
        }
    }
}

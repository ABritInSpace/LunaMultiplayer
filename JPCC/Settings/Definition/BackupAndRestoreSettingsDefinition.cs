using LmpCommon.Xml;

namespace JPCC.Settings.Definition
{
    [Serializable]
    public class BackupAndRestoreSettingsDefinition
    {
        [XmlComment(Value = "Items and folders to delete when a world reset is called upon.")]
        public string ItemsToReset { get; set; } =
            "Subspace.txt,\n" +
            "StartTime.txt,\n" +
            "Vessels,\n" +
            "Scenarios,\n" +
            "Kerbals,\n" +
            "Groups";
        [XmlComment(Value = "The interval in hours between each backup - 0 to disable.")]
        public double AutoBackupInterval { get; set; } = 0;
        [XmlComment(Value = "Whether to clear all but some backups every week (00:00 Monday UTC).")]
        public bool ClearBackups { get; set; } = false;
        [XmlComment(Value = "If ClearBackups is enabled, how many of the newest should we keep?")]
        public int KeepBackups { get; set; } = 5;
    }
}

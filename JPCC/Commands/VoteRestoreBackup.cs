using Server.Client;
using JPCC.Commands.SubHandler;
using JPCC.Handler;
using JPCC.Models;
using JPCC.Logging;
using JPCC.BaseStore;
using System.Reflection;

namespace JPCC.Commands
{
    // Vote restore backup chat command
    public class VoteRestoreBackup
    {
        private static MessageDispatcherHandler _messageDispatcherHandler;
        private static VotingTracker _votingTracker;
        private static RunVoteSubHandler _runVoteSubHandler;

        public VoteRestoreBackup(MessageDispatcherHandler messageDispatcherHandler, VotingTracker votingTracker, RunVoteSubHandler runVoteSubHandler)
        {
            _messageDispatcherHandler = messageDispatcherHandler;
            _votingTracker = votingTracker;
            _runVoteSubHandler= runVoteSubHandler;
        }

        public void VoteRestoreBackupHandler(string[] command, ClientStructure client)
        {
            JPCCLog.Debug($"Vote Restore Backup Command Handler activated for player {client.PlayerName}");
            bool isIndex = true;
            try{
                foreach (char c in command[1]){
                    if (!char.IsDigit(c)){
                        isIndex = false;
                    }
                }
            }catch{isIndex = false;}

            // Do we already have a vote running? If not, proceed and start a new one
            if (!_votingTracker.IsVoteRunning && _votingTracker.CanStartNewVote && command.Count() >= 2) 
            {
                if (isIndex)
                {
                    BackupSavesHandler _backupSavesHandler = new BackupSavesHandler();
                    string dir = _backupSavesHandler.GetBackupList()[int.Parse(command[1])-1];
                    command[1] = dir;
                    _votingTracker.VoteType = "restorebackup";
                    // Use vote subhandler to run vote
                    _runVoteSubHandler.StartVoteHandler(command, client);
                }
                else if (Directory.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "..\\..\\Backups\\" + command[1]))
                {
                    _votingTracker.VoteType = "restorebackup";
                    // Use vote subhandler to run vote
                    _runVoteSubHandler.StartVoteHandler(command, client);
                }
                else
                {
                    _messageDispatcherHandler.DispatchMessageToSingleClient($"Backup {command[1]} does not exist!", client);
                }
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToSingleClient("No backup parameter given or vote already running.", client);
            }
        }
    }
}

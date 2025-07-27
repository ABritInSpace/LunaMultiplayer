using Server.Client;
using JPCC.Commands.SubHandler;
using JPCC.Handler;
using JPCC.Models;
using JPCC.Logging;
using JPCC.BaseStore;
using System.Reflection;
using Server.Command;

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
                    _runVoteSubHandler.StartVoteHandler(command, client, SuccessAction, Validate);
                }
                else if (Directory.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "..\\..\\Backups\\" + command[1]))
                {
                    _votingTracker.VoteType = "restorebackup";
                    // Use vote subhandler to run vote
                    _runVoteSubHandler.StartVoteHandler(command, client, SuccessAction, Validate);
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
        private bool Validate(string[] command, ClientStructure client)
        {
            _messageDispatcherHandler.DispatchMessageToAllClients(
                $"Player {client.PlayerName} has initiated a vote on " +
                $"restoring a backup!{Environment.NewLine}Please use the commands " +
                $"/yes or /no to cast your vote!"
            );
            JPCCLog.Normal($"{client.PlayerName} has started a vote on making a backup!");
            return true;
        }

        private async void SuccessAction(string[] command, ClientStructure client)
        {
            await Task.Delay(0001);
            
            _messageDispatcherHandler.DispatchMessageToAllClients($"Vote has succeeded! Enough players voted yes. Backup will be restored.");
            JPCCLog.Normal($"Vote has succeeded! Enough players voted yes. Backup will be restored.");
            await Task.Delay(4000);

            _messageDispatcherHandler.DispatchMessageToAllClients($"Server will reboot in 5 seconds...");
            JPCCLog.Normal($"Server will reboot in 5 seconds...");

            await Task.Delay(5000);

            RunVoteSubHandler.baseKeeper.RestoreWorld = true;
            RunVoteSubHandler.baseKeeper.Backup = command[1];
            CommandHandler.Commands["restartserver"].Func(null);
        }
    }
}

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
    public class VoteMakeBackup
    {
        private static MessageDispatcherHandler _messageDispatcherHandler;
        private static VotingTracker _votingTracker;
        private static RunVoteSubHandler _runVoteSubHandler;

        public VoteMakeBackup(MessageDispatcherHandler messageDispatcherHandler, VotingTracker votingTracker, RunVoteSubHandler runVoteSubHandler)
        {
            _messageDispatcherHandler = messageDispatcherHandler;
            _votingTracker = votingTracker;
            _runVoteSubHandler= runVoteSubHandler;
        }

        public void VoteMakeBackupHandler(string[] command, ClientStructure client)
        {
            JPCCLog.Debug($"Vote Make Backup Command Handler activated for player {client.PlayerName}");

            // Do we already have a vote running? If not, proceed and start a new one
            if (!_votingTracker.IsVoteRunning && _votingTracker.CanStartNewVote) 
            {
                _votingTracker.VoteType = "makebackup";
            }
            
            // Use vote subhandler to run vote
            _runVoteSubHandler.StartVoteHandler(command, client, SuccessAction, Validate);
        }
        
        private bool Validate(string[] command, ClientStructure client)
        {
            _messageDispatcherHandler.DispatchMessageToAllClients(
                $"Player {client.PlayerName} has initiated a vote on " +
                $"making a backup!{Environment.NewLine}Please use the commands " +
                $"/yes or /no to cast your vote!"
            );
            JPCCLog.Normal($"{client.PlayerName} has started a vote on making a backup!");
            return true;
        }

        private async void SuccessAction(string[] command, ClientStructure client)
        {
            _messageDispatcherHandler.DispatchMessageToAllClients($"Vote has succeeded! Enough players voted yes. World will be backed up.");
            JPCCLog.Normal($"Vote has succeeded! Enough players voted yes. World will be backed up.");
            await Task.Delay(4000);

            BackupSavesHandler backupSavesHandler = new BackupSavesHandler();
            string result = null;
            try{result = backupSavesHandler.MakeBackup();}
            catch{JPCCLog.Debug("Exception");};
            if (result != null)
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Backup made successfully! {result}");
                JPCCLog.Normal($"Backup made successfully! {result}");
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToAllClients("Backup failed!");
                JPCCLog.Error("Backup failed!");
            }
        }
    }
}

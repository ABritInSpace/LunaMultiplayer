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
            _runVoteSubHandler.StartVoteHandler(command, client);
        }
    }
}

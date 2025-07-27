using System.Reflection;
using Server.Client;
using JPCC.Commands.SubHandler;
using JPCC.Handler;
using JPCC.Models;
using JPCC.Logging;
using Server.Command;

namespace JPCC.Commands
{
    // Vote reset world chat command
    public class VoteResetWorldChatCommand
    {
        private static MessageDispatcherHandler _messageDispatcherHandler;
        private static VotingTracker _votingTracker;
        private static RunVoteSubHandler _runVoteSubHandler;

        public VoteResetWorldChatCommand(MessageDispatcherHandler messageDispatcherHandler, VotingTracker votingTracker, RunVoteSubHandler runVoteSubHandler)
        {
            _messageDispatcherHandler = messageDispatcherHandler;
            _votingTracker = votingTracker;
            _runVoteSubHandler= runVoteSubHandler;
        }

        public void VoteResetWorldCommandHandler(string[] command, ClientStructure client)
        {
            JPCCLog.Debug($"Vote Reset World Command Handler activated for player {client.PlayerName}");

            // Do we already have a vote running? If not, proceed and start a new one
            if (!_votingTracker.IsVoteRunning && _votingTracker.CanStartNewVote) 
            {
                _votingTracker.VoteType = "resetworld";
            }

            // Use vote subhandler to run vote
            _runVoteSubHandler.StartVoteHandler(command, client, SuccessAction, Validate);
        }
        
        private bool Validate(string[] command, ClientStructure client)
        {
            _messageDispatcherHandler.DispatchMessageToAllClients(
                $"Player {client.PlayerName} has initiated a vote on " +
                $"resetting the world!{Environment.NewLine}Please use the commands " +
                $"/yes or /no to cast your vote!"
            );
            JPCCLog.Normal($"{client.PlayerName} has started a vote on resetting the world!");
            return true;
        }

        private async void SuccessAction(string[] command, ClientStructure client)
        {
            _messageDispatcherHandler.DispatchMessageToAllClients($"Vote has succeeded! Enough players voted yes. World will be reset.");
            JPCCLog.Normal($"Vote has succeeded! Enough players voted yes. World will be reset.");
            await Task.Delay(4000);

            _messageDispatcherHandler.DispatchMessageToAllClients($"Server will reboot in 5 seconds...");
            JPCCLog.Normal($"Server will reboot in 5 seconds...");

            await Task.Delay(5000);

            // Set reset state to true, then reboot
            RunVoteSubHandler.baseKeeper.ResetWorld = true;
            CommandHandler.Commands["restartserver"].Func(null);
        }
    }
}

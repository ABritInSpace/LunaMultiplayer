using Server.Client;
using Server.Command;
using JPCC.Handler;
using JPCC.Models;
using JPCC.Logging;
using JPCC.BaseStore;
using System.Reflection;

namespace JPCC.Commands.SubHandler
{
    public class RunVoteSubHandler
    {
        public static BaseKeeper baseKeeper;

        private static MessageDispatcherHandler _messageDispatcherHandler;
        private static VotingTracker _votingTracker;

        public RunVoteSubHandler(BaseKeeper baseKeeper, MessageDispatcherHandler messageDispatcherHandler, VotingTracker votingTracker) 
        {
            RunVoteSubHandler.baseKeeper = baseKeeper;

            _messageDispatcherHandler = messageDispatcherHandler;
            _votingTracker = votingTracker;
        }

        public async void StartVoteHandler(string[] command, ClientStructure client, Action<string[], ClientStructure> successAction, Func<string[], ClientStructure,  bool> validate)
        {
            JPCCLog.Debug($"Start Vote Sub Handler activated for player {client.PlayerName}");

            // Do we have a vote running already? If not, proceed
            if (!_votingTracker.IsVoteRunning && _votingTracker.CanStartNewVote)
            {
                // Set states
                _votingTracker.IsVoteRunning = true;
                _votingTracker.CanStartNewVote = false;
                _votingTracker.PlayersWhoVoted.Clear();
                _votingTracker.VotedYesCount = 0;
                _votingTracker.VotedNoCount = 0;
                ClientStructure? target = null;

                if (validate(command, client))
                { 
                    await VoteTimerAsync(command, client, successAction);
                    return;
                }
                
                // Reset states on vote cancelled
                _votingTracker.IsVoteRunning = false;
                _votingTracker.CanStartNewVote = true;
                _votingTracker.PlayersWhoVoted.Clear();
                _votingTracker.VotedYesCount = 0;
                _votingTracker.VotedNoCount = 0;
            }
            _messageDispatcherHandler.DispatchMessageToSingleClient("Vote is currently running, can not start a new one!", client);
        }

        // Counter, used for all vote types
        private async Task VoteTimerAsync(string[] command, ClientStructure client, Action<string[], ClientStructure> successAction)
        {
            ClientStructure? exclTarget = null;
            try{exclTarget = ClientRetriever.GetClientByName(command[1]);}
            catch{}

            await Task.Delay(5000);

            _messageDispatcherHandler.DispatchMessageExcludeClient("30 seconds left to vote!", exclTarget);
            JPCCLog.Debug($"Vote has 30 seconds left!");

            await Task.Delay(10000);

            _messageDispatcherHandler.DispatchMessageExcludeClient("20 seconds left to vote!", exclTarget);
            JPCCLog.Debug($"Vote has 20 seconds left!");

            await Task.Delay(10000);

            _messageDispatcherHandler.DispatchMessageExcludeClient("10 seconds left to vote!", exclTarget);
            JPCCLog.Debug($"Vote has 10 seconds left!");

            await Task.Delay(10000);
            await VoteResultHandlerAsync(command, client, successAction);
        }

        private async Task VoteResultHandlerAsync(string[] command, ClientStructure client,  Action<string[], ClientStructure> successAction)
        {
            ClientStructure exclTarget = null;
            try{exclTarget = ClientRetriever.GetClientByName(command[1]);}
            catch{}

            await Task.Delay(0100);
            
            // Players will no longer be able to vote
            _votingTracker.IsVoteRunning = false;
            
            // Print vote reults
            _messageDispatcherHandler.DispatchMessageExcludeClient(
                $"Vote has finished! Results:{Environment.NewLine}{_votingTracker.PlayersWhoVoted.Count()} total votes{Environment.NewLine}" +
                $"{_votingTracker.VotedYesCount.ToString()} voted yes{Environment.NewLine}{_votingTracker.VotedNoCount.ToString()} voted no",
                exclTarget
                );
            JPCCLog.Normal(
                $"Vote is over! Results: {_votingTracker.PlayersWhoVoted.Count()} total votes, {_votingTracker.VotedYesCount.ToString()} voted yes, " + 
                $"{_votingTracker.VotedNoCount.ToString()} voted no"
                );
            
            // Use vote specific result handler methods
            await Task.Delay(4000);
            if (_votingTracker.VotedYesCount > _votingTracker.VotedNoCount)
            {
                successAction(command, client);
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Vote failed, not enough players voted yes.");
                JPCCLog.Normal($"Vote has failed! Not enough players voted yes.");
            }

            // Reset the base state for the next vote
            _votingTracker.VoteType = "";
            _votingTracker.PlayersWhoVoted.Clear();
            _votingTracker.VotedYesCount = 0;
            _votingTracker.VotedNoCount = 0;
            _votingTracker.CanStartNewVote = true;
        }
    }
}

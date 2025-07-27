using System.Runtime.CompilerServices;
using Server.Client;
using JPCC.Commands.SubHandler;
using JPCC.Handler;
using JPCC.Models;
using JPCC.Logging;
using Server.Command;

namespace JPCC.Commands
{
    // Vote ban player chat command
    public class VoteBanPlayerChatCommand
    {
        private static MessageDispatcherHandler _messageDispatcherHandler;
        private static VotingTracker _votingTracker;
        private static RunVoteSubHandler _runVoteSubHandler;

        public VoteBanPlayerChatCommand(MessageDispatcherHandler messageDispatcherHandler, VotingTracker votingTracker, RunVoteSubHandler runVoteSubHandler)
        {
            _messageDispatcherHandler = messageDispatcherHandler;
            _votingTracker = votingTracker;
            _runVoteSubHandler = runVoteSubHandler;
        }

        public void VoteBanPlayerCommandHandler(string[] command, ClientStructure client)
        {
            JPCCLog.Debug($"Vote Ban Player Command Handler activated for player {client.PlayerName}");

            // Do we have enough input parameters?
            if (command.Count() >= 2)
            {
                // Get target player
                var player = ClientRetriever.GetClientByName(command[1]);

                // Does the target player exist?
                if (player != null)
                {
                    // If no vote is already running, proceed and start a new one
                    if (!_votingTracker.IsVoteRunning && _votingTracker.CanStartNewVote)
                    {
                        _votingTracker.VoteType = "banplayer";
                    }

                    // Use vote subhandler to run vote
                    _runVoteSubHandler.StartVoteHandler(command, client, SuccessAction, Validate);
                }
                else
                {
                    _messageDispatcherHandler.DispatchMessageToSingleClient("Error, player not found!", client);
                }
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToSingleClient("Error, playername not provided!", client);
            }
        }

        private bool Validate(string[] command, ClientStructure client)
        {
            try
            {
                var target = ClientRetriever.GetClientByName(command[1]);
                _messageDispatcherHandler.DispatchMessageToAllClients(
                    $"Player {client.PlayerName} has initiated a vote on " +
                    $"banning {target.PlayerName}!{Environment.NewLine}Please use the commands " +
                    $"/yes or /no to cast your vote!"
                );
                JPCCLog.Normal($"{client.PlayerName} has started a vote on banning {target.PlayerName}!");
                return true;
            }
            catch
            {
                _messageDispatcherHandler.DispatchMessageToAllClients("A vote failed as target player has left the game.");
                return false;
            }
        }

        private void SuccessAction(string[] command, ClientStructure client)
        {
            // Fetch player details
            var player = ClientRetriever.GetClientByName(command[1]);

            // If player still online, proceed with ban.
            // !!!!!!!!!!!!! Major loophole here as a player can quit before the vote runs out, thus not getting banned. Fix using database storing player ids.
            if (player != null)
            {
                var banMessage = "The server voted to ban you!";
                CommandHandler.Commands["ban"].Func($"{player.PlayerName} {banMessage}");

                _messageDispatcherHandler.DispatchMessageToAllClients($"{command[1]} has been banned!");
                JPCCLog.Normal($"{command[1]} has been banned!");
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Error, {command[1]} could not be banned as they are no longer on the server!");
                JPCCLog.Normal($"Error, {command[1]} could not be banned as they are no longer on the server!");
            }
        }
    }
}

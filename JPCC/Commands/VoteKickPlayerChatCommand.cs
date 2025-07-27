using Server.Client;
using JPCC.Commands.SubHandler;
using JPCC.Handler;
using JPCC.Models;
using JPCC.Logging;
using Server.Command;

namespace JPCC.Commands
{
    // Vote kick player chat command
    public class VoteKickPlayerChatCommand
    {
        private static MessageDispatcherHandler _messageDispatcherHandler;
        private static VotingTracker _votingTracker;
        private static RunVoteSubHandler _runVoteSubHandler;

        public VoteKickPlayerChatCommand(MessageDispatcherHandler messageDispatcherHandler, VotingTracker votingTracker, RunVoteSubHandler runVoteSubHandler)
        {
            _messageDispatcherHandler = messageDispatcherHandler;
            _votingTracker = votingTracker;
            _runVoteSubHandler = runVoteSubHandler;
        }

        public void VoteKickPlayerCommandHandler(string[] command, ClientStructure client)
        {
            JPCCLog.Debug($"Vote Kick Player Command Handler activated for player {client.PlayerName}");

            // Do we have enough input parameters?
            if (command.Count() >= 2)
            {
                // Get target player
                var player = ClientRetriever.GetClientByName(command[1]);

                // Does the target player exist?
                if (player != null)
                {
                    // If no vote is already running proceed and start a new one
                    if (!_votingTracker.IsVoteRunning && _votingTracker.CanStartNewVote)
                    {
                        _votingTracker.VoteType = "kickplayer";
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
                    $"kicking {target.PlayerName}!{Environment.NewLine}Please use the commands " +
                    $"/yes or /no to cast your vote!"
                );
                JPCCLog.Normal($"{client.PlayerName} has started a vote on kicking {target.PlayerName}!");
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

            // If player still online, proceed with kick.
            if (player != null)
            {
                var banMessage = "The server voted to kick you!";
                CommandHandler.Commands["kick"].Func($"{player.PlayerName} {banMessage}");

                _messageDispatcherHandler.DispatchMessageToAllClients($"{command[1]} has been kicked!");
                JPCCLog.Normal($"{command[1]} has been kicked!");
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Error, {command[1]} could not be kicked as they are no longer on the server!");
                JPCCLog.Normal($"Error, {command[1]} could not be kicked as they are no longer on the server!");
            }
        }
    }
}

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
        private static BaseKeeper _baseKeeper;

        private static MessageDispatcherHandler _messageDispatcherHandler;
        private static VotingTracker _votingTracker;

        public RunVoteSubHandler(BaseKeeper baseKeeper, MessageDispatcherHandler messageDispatcherHandler, VotingTracker votingTracker) 
        {
            _baseKeeper = baseKeeper;

            _messageDispatcherHandler = messageDispatcherHandler;
            _votingTracker = votingTracker;
        }

        public void StartVoteHandler(string[] command, ClientStructure client)
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

                // What type of vote do we have?
                if (_votingTracker.VoteType == "resetworld")
                {
                    _messageDispatcherHandler.DispatchMessageToAllClients($"Player {client.PlayerName} has initiated a vote on resetting the world!{Environment.NewLine}Please use the commands /yes or /no to cast your vote!");
                    JPCCLog.Normal($"{client.PlayerName} has started a vote on resetting the world!");

                    VoteTimerAsync(command, client);
                }
                if (_votingTracker.VoteType == "restorebackup" && Directory.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "..//..//Backups//" + command[1]))
                {
                    _messageDispatcherHandler.DispatchMessageToAllClients($"Player {client.PlayerName} has initiated a vote on restoring from backup {command[1]}!{Environment.NewLine}Please use the commands /yes or /no to cast your vote!");
                    JPCCLog.Normal($"{client.PlayerName} has started a vote on restoring the world!");

                    VoteTimerAsync(command, client);
                }
                else if (_votingTracker.VoteType == "restorebackup")
                {
                    _messageDispatcherHandler.DispatchMessageToSingleClient("Failed to restore backup, does not exist!",client);
                    JPCCLog.Normal($"Vote failed, backup does not exist.");
                }
                if (_votingTracker.VoteType == "makebackup")
                {
                    _messageDispatcherHandler.DispatchMessageToAllClients($"Player {client.PlayerName} has initiated a vote on making a world backup!{Environment.NewLine}Please use the commands /yes or /no to cast your vote!");
                    JPCCLog.Normal($"{client.PlayerName} has started a vote on making a backup of the world!");

                    VoteTimerAsync(command, client);
                }
                if (_votingTracker.VoteType == "kickplayer")
                {
                    ClientStructure target = null;
                    try{target = ClientRetriever.GetClientByName(command[1]);}
                    catch{}
                    if (target != null){
                        _messageDispatcherHandler.DispatchMessageExcludeClient($"Player {client.PlayerName} has initiated a vote on kicking {command[1]} from the server!{Environment.NewLine}Please use the commands /yes or /no to cast your vote!", target);
                        JPCCLog.Normal($"{client.PlayerName} has started a vote on kicking {command[1]} from the server!");

                        VoteTimerAsync(command, client);
                    }
                    else{
                        _messageDispatcherHandler.DispatchMessageToAllClients("A vote failed as target player has left the game.");
                    }
                }
                if (_votingTracker.VoteType == "banplayer")
                {
                    ClientStructure target = null;
                    try{target = ClientRetriever.GetClientByName(command[1]);}
                    catch{}
                    if (target != null){
                        _messageDispatcherHandler.DispatchMessageExcludeClient($"Player {client.PlayerName} has initiated a vote on banning {command[1]} from the server!{Environment.NewLine}Please use the commands /yes or /no to cast your vote!", target);
                        JPCCLog.Normal($"{client.PlayerName} has started a vote on banning {command[1]} from the server!");

                        VoteTimerAsync(command, client);
                    }
                    else{
                        _messageDispatcherHandler.DispatchMessageToAllClients("A vote failed as target player has left the game.");
                    }
                }
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToSingleClient("Vote is currently running, can not start a new one!", client);
            }
        }

        // Counter, used for all vote types
        private async Task VoteTimerAsync(string[] command, ClientStructure client)
        {
            ClientStructure exclTarget = null;
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
            VoteResultHandlerAsync(command, client);
        }

        private async Task VoteResultHandlerAsync(string[] command, ClientStructure client)
        {
            ClientStructure exclTarget = null;
            try{exclTarget = ClientRetriever.GetClientByName(command[1]);}
            catch{}

            await Task.Delay(0100);
            
            // Players will no longer be able to vote
            _votingTracker.IsVoteRunning = false;
            
            // Print vote reults
            _messageDispatcherHandler.DispatchMessageExcludeClient($"Vote has finished! Results:{Environment.NewLine}{_votingTracker.PlayersWhoVoted.Count()} total votes{Environment.NewLine}{_votingTracker.VotedYesCount.ToString()} voted yes{Environment.NewLine}{_votingTracker.VotedNoCount.ToString()} voted no", exclTarget);
            JPCCLog.Normal($"Vote is over! Results: {_votingTracker.PlayersWhoVoted.Count()} total votes, {_votingTracker.VotedYesCount.ToString()} voted yes, {_votingTracker.VotedNoCount.ToString()} voted no");
            
            // Use vote specific result handler methods
            await Task.Delay(4000);
            if (_votingTracker.VoteType == "resetworld")
            {
                await HandleResetVoteResults(command, client);
            }
            if (_votingTracker.VoteType == "kickplayer")
            {
                await HandleKickVoteResults(command, client);
            }
            if (_votingTracker.VoteType == "banplayer")
            {
                await HandleBanVoteResults(command, client);
            }
            if (_votingTracker.VoteType == "restorebackup")
            {
                await HandleRestoreVoteResults(command, client);
            }
            if (_votingTracker.VoteType == "makebackup")
            {
                await HandleMakeVoteResults(command, client);
            }

            // Reset the base state for the next vote
            _votingTracker.VoteType = "";
            _votingTracker.PlayersWhoVoted.Clear();
            _votingTracker.VotedYesCount = 0;
            _votingTracker.VotedNoCount = 0;
            _votingTracker.CanStartNewVote = true;
        }

        // Methods for dealing with the results

        // Handle the reset vote results
        private async Task HandleResetVoteResults(string[] command, ClientStructure client) 
        {
            await Task.Delay(0001);
            
            // Do we have enough votes?
            if (_votingTracker.VotedYesCount > _votingTracker.VotedNoCount)
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Vote has succeeded! Enough players voted yes. World will be reset.");
                JPCCLog.Normal($"Vote has succeeded! Enough players voted yes. World will be reset.");
                await Task.Delay(4000);

                _messageDispatcherHandler.DispatchMessageToAllClients($"Server will reboot in 5 seconds...");
                JPCCLog.Normal($"Server will reboot in 5 seconds...");

                await Task.Delay(5000);

                // Old reset logic, no longer used
                //MainServer.ResetWorldAndRestart();

                // Set reset state to true, then reboot
                _baseKeeper.ResetWorld = true;
                CommandHandler.Commands["restartserver"].Func(null);
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Vote has failed! Not enough players voted yes. World will not be reset.");
                JPCCLog.Normal($"Vote has failed! Not enough players voted yes. World will not be reset.");
            }
        }
        private async Task HandleRestoreVoteResults(string[] command, ClientStructure client) 
        {
            await Task.Delay(0001);
            
            // Do we have enough votes?
            if (_votingTracker.VotedYesCount > _votingTracker.VotedNoCount)
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Vote has succeeded! Enough players voted yes. Backup will be restored.");
                JPCCLog.Normal($"Vote has succeeded! Enough players voted yes. Backup will be restored.");
                await Task.Delay(4000);

                _messageDispatcherHandler.DispatchMessageToAllClients($"Server will reboot in 5 seconds...");
                JPCCLog.Normal($"Server will reboot in 5 seconds...");

                await Task.Delay(5000);

                _baseKeeper.RestoreWorld = true;
                _baseKeeper.Backup = command[1];
                CommandHandler.Commands["restartserver"].Func(null);
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Vote has failed! Not enough players voted yes. Backup will not be restored.");
                JPCCLog.Normal($"Vote has failed! Not enough players voted yes. Backup will not be restored.");
            }
        }
        private async Task HandleMakeVoteResults(string[] command, ClientStructure client) 
        {
            await Task.Delay(0001);
            
            // Do we have enough votes?
            if (_votingTracker.VotedYesCount > _votingTracker.VotedNoCount)
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
            else
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Vote has failed! Not enough players voted yes. World will not be backed up.");
                JPCCLog.Normal($"Vote has failed! Not enough players voted yes. World will not be backed up.");
            }
        }

        // Handle kick vote results
        private async Task HandleKickVoteResults(string[] command, ClientStructure client)
        {
            await Task.Delay(0001);

            // Do we have enough votes, and do we have more yes than no votes?
            if ((_votingTracker.VotedYesCount > _votingTracker.VotedNoCount) && _votingTracker.PlayersWhoVoted.Count() >= 1)
            {
                _messageDispatcherHandler.DispatchMessageExcludeClient($"Vote has succeeded! Enough players voted yes. Player {command[1]} will be kicked.", ClientRetriever.GetClientByName(command[1]));
                JPCCLog.Normal($"Vote has succeeded! Enough players voted yes. Player {command[1]} will be kicked.");

                await Task.Delay(2000);

                // Fetch player details
                var player = ClientRetriever.GetClientByName(command[1]);

                // If player still online, proceed with kick
                if (player != null)
                {
                    var kickMessage = "The server voted to kick you out!";
                    CommandHandler.Commands["kick"].Func($"{player.PlayerName} {kickMessage}");

                    _messageDispatcherHandler.DispatchMessageExcludeClient($"{command[1]} has been kicked!", player);
                    JPCCLog.Normal($"{command[1]} has been kicked!");
                }
                else
                {
                    _messageDispatcherHandler.DispatchMessageToAllClients($"Error, {command[1]} could not be kicked as they are no longer on the server!");
                    JPCCLog.Normal($"Error, {command[1]} could not be kicked as they are no longer on the server!");
                }
            }
            else
            {
                _messageDispatcherHandler.DispatchMessageToAllClients($"Vote has failed! Not enough players voted yes. Player {command[1]} will not be kicked.");
                JPCCLog.Normal($"Vote has failed! Not enough players voted yes. Player {command[1]} will not be kicked.");
            }
        }

        // Handle the ban vote reults
        private async Task HandleBanVoteResults(string[] command, ClientStructure client)
        {
            await Task.Delay(0001);

            // Do we have enough votes and more yes than no votes?
            if ((_votingTracker.VotedYesCount > _votingTracker.VotedNoCount) && _votingTracker.PlayersWhoVoted.Count() >= 2)
            {
                _messageDispatcherHandler.DispatchMessageExcludeClient($"Vote has succeeded! Enough players voted yes. Player {command[1]} will be banned.", ClientRetriever.GetClientByName(command[1]));
                JPCCLog.Normal($"Vote has succeeded! Enough players voted yes. Player {command[1]} will be banned.");

                await Task.Delay(2000);

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
            else
            {
                _messageDispatcherHandler.DispatchMessageExcludeClient($"Vote has failed! Not enough players voted yes. Player {command[1]} will not be banned.", ClientRetriever.GetClientByName(command[1]));
                JPCCLog.Normal($"Vote has failed! Not enough players voted yes. Player {command[1]} will not be banned.");
            }
        }
    }
}

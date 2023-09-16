using Server.Client;
using JPCC.Handler;
using JPCC.Models;
using JPCC.Logging;

namespace JPCC.Commands
{
    // Help chat command
    public class BackupListChatCommand
    {
        private static MessageDispatcherHandler _messageDispatcherHandler;
        //private static ChatCommands _chatCommands;
        private static BackupSavesHandler _backupSavesHandler;
        private static List<string> listRecent;

        private static int itemsPerPage = 4;
        private static int totalPages = 0;

        public BackupListChatCommand(MessageDispatcherHandler messageDispatcherHandler, ChatCommands chatCommands) 
        {
            _messageDispatcherHandler = messageDispatcherHandler;
        }

        public void ListCommandHandler(string[] command, ClientStructure client)
        {
            _backupSavesHandler = new BackupSavesHandler();
            listRecent = new List<string>();
            int j = 1;
                foreach (string dir in _backupSavesHandler.GetBackupList()){
                    listRecent.Add(j.ToString() + ". " + dir);
                    j += 1;
                }

            // Get the total help page count by using the total number of entries vs allowed items per page
            totalPages = (int)Math.Ceiling(Convert.ToDouble(listRecent.Count()) / Convert.ToDouble(itemsPerPage));

            JPCCLog.Debug($"Backup List Command Handler activated for player {client.PlayerName}");

            string helpOutput = "";
            int selectedPage = 1;

            try 
            {
                // If the user input enough parameters, check if the requested page is an integer
                if (command.Length > 1)
                {
                    selectedPage = int.Parse(command[1]);
                }

                // Is the input in the page range?
                if (selectedPage >= 1 && selectedPage <= totalPages)
                {
                    string commandsInFocus = "";

                    // Add the entries that should be on the selected page
                    for (int i = (((selectedPage - 1) * itemsPerPage) + 1); i <= (((selectedPage - 1) * itemsPerPage) + itemsPerPage); i++) 
                    {
                        if (i <= listRecent.Count())
                        {
                            commandsInFocus = commandsInFocus + listRecent.ElementAt(i-1) + "\n";
                        }
                    }

                    // Create the final message text the player will see
                    helpOutput =
                        "\n<---Backup Menu--->\n(FORMAT IS YYYY-MM-DD@H)\n" +
                        commandsInFocus +
                        $"<---Page {selectedPage}/{totalPages}--->";

                    _messageDispatcherHandler.DispatchMessageToSingleClient(helpOutput, client);
                }
                else 
                {
                    _messageDispatcherHandler.DispatchMessageToSingleClient("Error, page number out of range!", client);
                }
            }
            catch (Exception ex) 
            {
                _messageDispatcherHandler.DispatchMessageToSingleClient("Error, page must be a number!", client);
            }
        }
    }
}

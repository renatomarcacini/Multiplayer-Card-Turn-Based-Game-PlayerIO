using System;
using System.Collections.Generic;
using PlayerIO.GameLibrary;

[RoomType("Lobby")]
public class LobbyGame: Game<Player>{
    public override void UserJoined(Player player)
    {
        Console.WriteLine("User joined:" + player.ConnectUserId);
    }

    public override void UserLeft(Player player)
    {
        Console.WriteLine("User Left:" + player.ConnectUserId);
    }

    public override void GotMessage(Player player, Message message)
    {
        Console.WriteLine($"Message received from Player: {player.ConnectUserId}, message of type: {message.Type}");
        switch (message.Type)
        {
            case NetworkConstant.FIRST_TIME_LOGIN:
                PlayerIO.BigDB.LoadOrCreate("PlayerObjects", player.ConnectUserId, (DatabaseObject datasetObj) => {
                    datasetObj.Set(NetworkConstant.DATABASE_USERNAME, message.GetString(0));
                    datasetObj.Set(NetworkConstant.DATABASE_CHIPS, 100000);
                    datasetObj.Save();
                });

                player.Leaderboards.Set("chips", null, player.Chips, (LeaderboardEntry LeaderboardEntry) =>
                {
                    Console.WriteLine("Success");
                });

                break;
        }
    }
}
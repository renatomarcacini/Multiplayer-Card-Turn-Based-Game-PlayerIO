
using PlayerIO.GameLibrary;
using System;
using System.Collections.Generic;

[RoomType("GameRoom")]
public class GameRoom: Game<Player>
{
    private List<Player> allPlayers;
    private List<Player> currentRoundPlayers;
    private DeckGenerator deckGenerator;
    private Player currentPlayer;
    private Player currentRoundWinner;
    private Timer turnTimer;

    private const int MINIMUM_CHIPS_AMOUNT = 2500;
    private const int REWARD = 5000;
    public bool isRoundRunning;
    public bool isGameStarted;

    public const int MAX_ROUND = 6;
    public int currentRound = 0;

    public override void GameStarted()
    {
        allPlayers = new List<Player>();
        currentRoundPlayers = new List<Player>();
        PreloadPlayerObjects = true;
    }


    public override bool AllowUserJoin(Player player)
    {
        // Allow only if current players in room is < 3
        if(PlayerCount <= 2 && !isRoundRunning)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void UserJoined(Player player)
    {
        player.Send(Message.Create(NetworkConstant.SPAWN_LOCAL_PLAYER, player.ConnectUserId));

        foreach (Player basePlayer in allPlayers)
        {
            player.Send(Message.Create(NetworkConstant.SPAWN_FOREIGN_PLAYER, basePlayer.ConnectUserId, 
                basePlayer.PlayerObject.GetString(NetworkConstant.DATABASE_USERNAME)));
            basePlayer.Send(Message.Create(NetworkConstant.SPAWN_FOREIGN_PLAYER, player.ConnectUserId,
                player.PlayerObject.GetString(NetworkConstant.DATABASE_USERNAME)));
            
        }
        player.SetNickname(player.PlayerObject.GetString(NetworkConstant.DATABASE_USERNAME));
        allPlayers.Add(player);

        if(allPlayers.Count >= 2 && !isGameStarted)
        {
            ScheduleCallback(StartRound, 3000);
            isGameStarted = true;
        }

        player.CreditChips(player.PlayerObject.GetInt(NetworkConstant.DATABASE_CHIPS));
        player.DeditChips(MINIMUM_CHIPS_AMOUNT);
        BroadcastStatus(player, PlayerState.WAITING);
    }
        

    public override void UserLeft(Player player)
    {
        Broadcast(Message.Create(NetworkConstant.PLAYER_LEFT, player.ConnectUserId));

        player.GetPlayerObject((DatabaseObject datasetObj) => {
            datasetObj.Set(NetworkConstant.DATABASE_CHIPS, player.Chips);
            datasetObj.Save();
        });

        allPlayers.Remove(player);

        if (isRoundRunning && allPlayers.Count > 0)
        {
            currentRoundPlayers.Remove(player);
            CheckWin();
        }

        //Update Leaderboard
        player.Leaderboards.Set("chips", null, player.Chips, (LeaderboardEntry LeaderboardEntry) =>
        {
            Console.WriteLine("Success");
        });
    }

    public void StartRound()
    {
        deckGenerator = new DeckGenerator();
        foreach (Player player in allPlayers)
        {
            Message message = deckGenerator.GetCards();
            player.SetCards(message);
            player.Send(message);
            currentRoundPlayers.Add(player);
            BroadcastStatus(player, PlayerState.PLAYING);
        }

        ChangeTurn();
        isRoundRunning = true;
    }

    public void ChangeTurn()
    {
        if (currentPlayer == null)
            currentPlayer = allPlayers[0];

        if(currentRound >= MAX_ROUND)
        {
            CheckWin();
            return;
        }
        currentRound++;

        Player player = currentPlayer;
        int index = currentRoundPlayers.LastIndexOf(player);
        ++index;

        if(index > currentRoundPlayers.Count - 1)
        {
            player = currentRoundPlayers[0];
        }
        else if(index < 0)
        {
            player = currentRoundPlayers[allPlayers.Count - 1];
        }
        else
        {
            player = currentRoundPlayers[index];
        }

        currentPlayer = player;
        Broadcast(NetworkConstant.CHANGE_TURN, currentPlayer.ConnectUserId);
        turnTimer = ScheduleCallback(OnCycleOver, NetworkConstant.TURN_TIME);
    }

    public void OnCycleOver()
    {
        if (!CheckWin())
        {
            MakePlayerPlayACard();
        }
    }

    public bool CheckWin()
    {
        if(currentRoundPlayers.Count == 1)
        {
            currentRoundWinner = currentRoundPlayers[0];
            RoundOver();
            return true;
        }
        else if(currentRound >= MAX_ROUND)
        {
            
            int highPoint = 0;
            foreach(Player player in currentRoundPlayers)
            {
                if(highPoint <= player.Point)
                {
                    highPoint = player.Point;
                    currentRoundWinner = player;
                }
            }
            RoundOver();
            return true;
        }

        return false;
    }

    public void RoundOver()
    {
        turnTimer.Stop();
        currentRoundWinner.CreditChips(REWARD);
        currentPlayer = currentRoundWinner;
        currentRoundPlayers.Clear();

        currentRoundWinner.Leaderboards.Set("chips", null, currentRoundWinner.Chips, (LeaderboardEntry LeaderboardEntry) =>
        {
            Console.WriteLine("Success");
        });

        ScheduleCallback(()=> { 
            Broadcast(Message.Create(NetworkConstant.ROUND_OVER, currentRoundWinner.ConnectUserId, currentRoundWinner.Nickname, REWARD));
        }, 3000);
        isRoundRunning = false;
 
    }

    public void MakePlayerPlayACard()
    {
        turnTimer.Stop();
        currentPlayer.Send(Message.Create(NetworkConstant.PLAY_RANDOM_CARD, currentPlayer.ConnectUserId));
    }


    /// Requisições enviadas para o servidor pelos jogadores
    public override void GotMessage(Player player, Message message)
    {
        switch (message.Type)
        {
            case NetworkConstant.ACTION_PLAY_CARD:
                ActionPlayCard(player, message);
                break;
        }
    }

    private void ActionPlayCard(Player player, Message message)
    {
        Console.WriteLine($"Player play card: {player.ConnectUserId}, Card: {message.GetString(0)}");
        if(player == currentPlayer && isRoundRunning)
        {
            string cardID = message.GetString(0);
            Broadcast(Message.Create(NetworkConstant.ACTION_PLAY_CARD, player.ConnectUserId, cardID));
            turnTimer.Stop();
            ChangeTurn();
        }
    }

    private void BroadcastStatus(Player player, PlayerState state)
    {
        player.STATE = state;
        Broadcast(Message.Create(NetworkConstant.STATUS_UPDATE, player.ConnectUserId, state.ToString()));
    }

    private void CheckDisconnectAll()
    {
        if (allPlayers.Count > 1)
            return;
        DisconnectAll();
    }


    private void DisconnectAll()
    {
        if(allPlayers.Count > 0)
        {
            foreach (Player player in allPlayers)
            {
                player.Disconnect();
            }
        }
    }

}

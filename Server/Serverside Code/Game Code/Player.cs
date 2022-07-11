using PlayerIO.GameLibrary;
using System.Collections.Generic;
using System;
using System.Linq;
public enum PlayerState
{
    WAITING,
    PLAYING
}

public class Player : BasePlayer
{
    private int _Chips;
    public int Chips { get { return _Chips; } }

    private int _Point;
    public int Point { get { return _Point; } }

    private string _Nickname;
    public string Nickname { get { return _Nickname; } }


    private List<string> Cards = new List<string>();
    public PlayerState STATE { get; set; }


    public void SetCards(Message message)
    {
        Cards.Clear();
        for (uint i = 0; i < message.Count; i++)
        {
            Cards.Add(message.GetString(i));
        }
        CheckPoint();
    }

    public void CheckPoint()
    {
        int score = 0;
        int perPoint = 0;
        for (int i = 0; i < Cards.Count; i++)
        {
            score += Convert.ToInt32(Cards[i].Split(',').Last());
            perPoint = Convert.ToInt32(Cards[i].Split(',').Last());
            Console.WriteLine($"{ConnectUserId} have: { perPoint }");
        }
    
        _Point = score;
        Console.WriteLine($"{ConnectUserId} have: {_Point} points");
    }

    public void CreditChips(int amount)
    {
        _Chips += amount;
        Send(Message.Create(NetworkConstant.TRANSACTION, _Chips));
    }

    public void DeditChips(int amount)
    {
        _Chips -= amount;
        Send(Message.Create(NetworkConstant.TRANSACTION, _Chips));
    }

    public void SetNickname(string nickname)
    {
        _Nickname = nickname;
        Send(Message.Create(NetworkConstant.NICKNAME, _Nickname));
    }
}
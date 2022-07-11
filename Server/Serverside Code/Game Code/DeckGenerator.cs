using PlayerIO.GameLibrary;
using System;
using System.Collections.Generic;
public class DeckGenerator
{
    public string[] suits = new string[] { "S", "H", "D", "C" };
    public string[] cardValues = new string[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14" };

    private List<string> generatedDeck;

    public DeckGenerator()
    {
        GenerateDeck();
    }

    private void GenerateDeck()
    {
        generatedDeck = new List<string>();
        foreach(string suit in suits)
        {
            foreach(string value in cardValues)
            {
                generatedDeck.Add(suit + "," + value);
            }
        }

        Shuffle(generatedDeck);
    }

    private void Shuffle<T>(List<T> list)
    {
        Random random = new Random();
        int n = list.Count;

        while(n > 1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }

    public Message GetCards()
    {
        Message message = Message.Create(NetworkConstant.ROUND_STARTED);

        for (int i = 0; i < 3; i++)
        {
            message.Add(generatedDeck[0]);
            generatedDeck.RemoveAt(0);
        }

        return message;
    }
}


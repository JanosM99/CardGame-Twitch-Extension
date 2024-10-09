using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class ListManager : MonoBehaviour
{   
    [SerializeField]
    public List<Entry> entries = new List<Entry>();                 // Entries List
    [SerializeField]
    public List<Viewer> leaderboard = new List<Viewer>();           // Leaderboard List
    [SerializeField]
    public List<Player> players = new List<Player>();               // Player List

    RoundManager roundmanager;

    void Start()
    {
        roundmanager = GameObject.FindGameObjectWithTag("RoundManager").GetComponent<RoundManager>();
    }

    // Add Player To Entries List
    public void AddNameToList(string name, int leftSideBet, int middleBet, int rightSideBet)
    {
        var item = entries.FirstOrDefault(x => x.name == name);                             // Check If User Already Betted (ONLY 1 BET / ROUND)
        if(item != null || !CheckUserPoints(name, leftSideBet, middleBet, rightSideBet))    // Check If User Has Enough Point To Bet
        {
            Debug.Log("The user already betted or does not have enough points.");
        }
        else
        {
            entries.Add(new Entry(name, leftSideBet, middleBet, rightSideBet, 0, 0, false));
            roundmanager.totalPlayersCount++;
        }
    }

    // Clear Entries List
    public void DeleteList()
    {
        entries.Clear();
    }

    // Add  Viewer To Leaderboard List
    public void AddFromLeaderboardToList(string user, int points)
    {
        leaderboard.Add(new Viewer(user, points));
    }

    // Clear Leaderboard List
    public void DeleteLeaderBoard()
    {
        leaderboard.Clear();
    }

    // Check Points - AddNameToList()
    bool CheckUserPoints(string name, int leftSideBet, int middleBet, int rightSideBet)
    {
        int bet = leftSideBet + middleBet + rightSideBet;
        foreach(var l in leaderboard)
        {
            if(l.name == name && l.points >= bet) 
            {
                return true;
            }
        }
        return false;
    }

    // Clear Players List
    public void DeletePlayerList()
    {
        players.Clear();
    }
}

// Entry Class - entries List
[System.Serializable]
public class Entry
{
    public string name;
    public int leftSideBet;
    public int middleBet;
    public int rightSideBet; 
    public int cardValue;
    public int winValue;
    public bool winner;

    public Entry(string name, int leftSideBet, int middleBet, int rightSideBet, int cardValue, int winValue, bool winner)
    {
        this.name = name;
        this.leftSideBet = leftSideBet;
        this.middleBet = middleBet;
        this.rightSideBet = rightSideBet;
        this.cardValue = cardValue;
        this.winValue = winValue;
        this.winner = winner;
    }
}

// Viewer Class - leaderboard List
[System.Serializable]
public class Viewer
{
    public string name; 
    public int points; 

    public Viewer(string name, int points)
    {
        this.name = name;
        this.points = points;
    }
}

// Players Class - players List
[System.Serializable]
public class Player
{
    public string name;
    public bool hit;

    public Player(string name, bool hit)
    {
        this.name = name;
        this.hit = hit;
    }
}
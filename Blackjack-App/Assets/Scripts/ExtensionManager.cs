using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using WebSocketSharp;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

public class ExtensionManager : MonoBehaviour
{
    private WebSocket _ws;

    ListManager listmanager;
    RoundManager roundmanager;

    void Start()
    {
        listmanager = GameObject.FindGameObjectWithTag("ListManager").GetComponent<ListManager>();
        roundmanager = GameObject.FindGameObjectWithTag("RoundManager").GetComponent<RoundManager>();

        // API
        _ws = new WebSocket("insert your websocket");
        _ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
          
        Debug.Log("[WebSocket Status]: " + _ws.ReadyState);
        _ws.Connect();
        Debug.Log("[WebSocket Status]: " + _ws.ReadyState);
        
        _ws.OnMessage += (sender, e) =>
        {
            var regex = new Regex(@"\s+");

            // Split the string into an array of strings
            var strings = regex.Split($"{e.Data}");
          
            if(roundmanager.isBetEnabled)
            {
                Bet(strings[0],strings[1],strings[2],strings[3]);   // Name, LeftSideBet, MiddleSideBet, RightSideBet
            }
            else if(roundmanager.isDecisionTime)
            {
                Decision(strings[0], strings[1]);   // Name, Hit or Stand
            }
        };
    }

    // Check the API
    private void Update()
    {
        if(_ws == null) 
        {
            return;
        }
    }

    // Manage the incoming bets
    void Bet(string name, string getted_leftSideBet, string getted_middleBet, string getted_rightSideBet)
    {
        int leftSideBet = 0;
        int middleBet = 0;
        int rightSideBet = 0;

        Int32.TryParse(getted_leftSideBet, out leftSideBet);
        Int32.TryParse(getted_middleBet, out middleBet);
        Int32.TryParse(getted_rightSideBet, out rightSideBet);

        listmanager.AddNameToList(name, leftSideBet, middleBet, rightSideBet);
    }

    // Check Decision
    void Decision(string name, string hit)
    {
        foreach(var player in listmanager.entries)
        {
            if(name == player.name)
            {
                if(hit == "hit")
                {
                    listmanager.players.Add(new Player(name, true));

                    roundmanager.hitPlayersCount++;
                    if(roundmanager.totalPlayersCount > 0)
                    {
                        roundmanager.totalPlayersCount--;
                    }
                }
                else
                {
                    listmanager.players.Add(new Player(name, false));

                    roundmanager.standPlayersCount++;
                    if(roundmanager.totalPlayersCount > 0)
                    {
                        roundmanager.totalPlayersCount--;
                    }
                }
            }
        }
    }

    // Send startround msg to the extension
    public void SendRoundStartMessage()
    {
        var msg = new 
        {
            action = "sendmessage",
            message = "startround"
        };
        _ws.Send(JsonConvert.SerializeObject(msg));
    }

    // Send endround msg to the extension
    public void SendRoundEndMessage()
    {
        var msg = new 
        {
            action = "sendmessage",
            message = "endround"
        };
        _ws.Send(JsonConvert.SerializeObject(msg));
    }

    // Send decision msg to the extension
    public void SendDecisionMessage()
    {
        var msg = new 
        {
            action = "sendmessage",
            message = "decision"
        };
        _ws.Send(JsonConvert.SerializeObject(msg));
    }
}

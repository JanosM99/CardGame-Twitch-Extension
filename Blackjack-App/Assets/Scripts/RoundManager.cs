using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : MonoBehaviour
{
    // Scripts
    ListManager listmanager;
    ExtensionManager extensionmanager;
    GameManager gamemanager;
    TwitchConnect twitchconnect;
    SeApi seApi;

    // Timer
    public GameObject objectTimerText;
    public Image countdownCircleTimer;
    public Text timer_Text;    
    public float startTime = 20.0f;
    public float timeRemaining = 20.0f;                                
    public bool isTimerRunning = false;                             
       
    // Winner UI
    public GameObject objectWinnerCount;   
    public Text winnerChat;                                                                 
    public Text winnerCountText;    
    public int winnerCount = 0;  

    // Hit And Stand UI
    public GameObject totalPlayerPanel;
    public GameObject hitandStandPanel;
    public Text totalPlayersStart;
    public int totalPlayersCount;
    public Text totalPlayers;
    public int hitPlayersCount;
    public Text hitPlayers;
    public int standPlayersCount;
    public Text standPlayers; 

    // BetsPanel UI
    public ScrollRect scrollView;
    public GameObject entryPrefab;

    // Game Variables
    public bool isBetEnabled = false; 	
    public bool isDecisionTime = false;    
    public bool helpToRemovePoints;         // SeApi.RemovePointsFromPlayers()  

    // SeApi Botmsg
    string startRoundMsg = "A kör elkezdődött, tegyék meg tétjeiket...woof...woof";
    string betDisabled = "A tétrakás véget ért, sok szerencsét...woof...woof";



    void Start()
    {
        listmanager = GameObject.FindGameObjectWithTag("ListManager").GetComponent<ListManager>();
        extensionmanager = GameObject.FindGameObjectWithTag("ExtensionManager").GetComponent<ExtensionManager>();
        gamemanager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        twitchconnect = GameObject.FindGameObjectWithTag("TwitchConnect").GetComponent<TwitchConnect>();
        seApi = GameObject.FindGameObjectWithTag("SeApi").GetComponent<SeApi>();
    }

    void Update()
    {
        if(isTimerRunning)                                  
        {
            if(timeRemaining > 0.1)                             // Timer Running
            {
                timeRemaining -= Time.deltaTime;
                gamemanager.drawButton.interactable = false;
                gamemanager.drawButton.gameObject.SetActive(false);

                if(isDecisionTime)
                {
                    hitandStandPanel.SetActive(true);
                }
                else
                {
                    totalPlayerPanel.SetActive(true);
                }
            }
            else                                                // Timer Ends
            {
                if(!helpToRemovePoints)                         // Remove Points From Entries
                {
                    seApi.RemovePointsFromPlayers();
                    seApi.SendBotMsg(betDisabled); 
                    helpToRemovePoints = true;
                }

                // Timer
                timeRemaining = 0.0f;
                isTimerRunning = false;
                objectTimerText.SetActive(false);
                hitandStandPanel.SetActive(false);
                totalPlayerPanel.SetActive(false);
                isBetEnabled = false;

                if(isDecisionTime)
                {
                    SetPlayerValue();
                }
                isDecisionTime = false;

                gamemanager.drawButton.interactable = true;
                gamemanager.drawButton.gameObject.SetActive(true);

                totalPlayersCount = 0;
                hitPlayersCount = 0;
                standPlayersCount = 0;
            }
            // Timer UI
            float seconds = Mathf.FloorToInt(timeRemaining % 60);  
            timer_Text.text = seconds.ToString();

            float normalizedValue = Mathf.Clamp(timeRemaining /startTime, 0.0f, 1.0f);
            countdownCircleTimer.fillAmount = normalizedValue;
        }
        winnerCountText.text = winnerCount.ToString();    

        totalPlayersStart.text = totalPlayersCount.ToString();
        totalPlayers.text = totalPlayersCount.ToString();
        hitPlayers.text = hitPlayersCount.ToString();
        standPlayers.text = standPlayersCount.ToString();
    }

    // Start The Round
	public async void StartRound()
    {					
        twitchconnect.ClearLogs();          // Clear The Twitch Chatlogs
        isBetEnabled = true;                // Enable The Chat Commands
        isTimerRunning = true;              // Start The Timer
        objectTimerText.SetActive(true);	// Show The Timer
        seApi.SendBotMsg(startRoundMsg);  	// Chat - startRoundMsg
        helpToRemovePoints = false;		    // roundmanager - seApi.set_points()
		await seApi.GetLeaderboard();		// API - Leaderboard For RemovePointsFromPlayers()
    }

    // Send Decision MSG For Players
    public void SendDecision()
    {
        timeRemaining = 20;
        objectTimerText.SetActive(true);
        isTimerRunning = true; 

        isDecisionTime = true;
        extensionmanager.SendDecisionMessage();

        GetActivePlayers();
    }

    // Stand - Set Value, Hit - Do NOT Set, Nothing - Stand 
    public void SetPlayerValue()
    {
        foreach (var e in listmanager.entries)
        {
            bool nameMatched = false;

            foreach (var p in listmanager.players)
            {
                if (e.name == p.name && e.cardValue == 0)
                {
                    if (!p.hit)
                    {
                        e.cardValue = gamemanager.playerValue;
                    }

                    nameMatched = true;
                    break;
                }
            }
            if (!nameMatched && e.cardValue == 0)
            {
                e.cardValue = gamemanager.playerValue;
            }
        }
        listmanager.DeletePlayerList();
    }

    // Get The Number Of Players Who Hit
    public int CheckHitPlayers()
    {
        int counter = 0;
        foreach(var e in listmanager.entries)
        {
            if(e.cardValue == 0)
            {
                counter++;
            }
        }
        return counter;
    }

    // Set Card Value, Used In GameManager.cs - DrawCard()
    public void SetCardValueForPlayers(int value)
    {
        foreach(var e in listmanager.entries)
        {
            if(e.cardValue == 0)
            {
                e.cardValue = value;
            }
        }
    }

    // Get The Number Of People Currently Playing
    public void GetActivePlayers()
    {
        foreach(var player in listmanager.entries)
        {
            if(player.cardValue == 0)
            {
                totalPlayersCount++;
            }
        }
    }

    // Get The Data From Listmanager - Entry
    public void GetDatasForBetsPanel()
    {
        Transform contentTransform = scrollView.content;

        // Iterate through each child and destroy them
        for (int i = contentTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(contentTransform.GetChild(i).gameObject);
        }

        foreach (Entry entry in listmanager.entries)
        {

            GameObject entryUI = Instantiate(entryPrefab, scrollView.content);

            // Get the Text components within the entry UI element and set their text values
            Text nameText = entryUI.transform.Find("NameText").GetComponent<Text>();
            Text perfectPairsText = entryUI.transform.Find("PerfectPairsText").GetComponent<Text>();
            Text betText = entryUI.transform.Find("BetText").GetComponent<Text>();
            Text twentyThreePlusOneText = entryUI.transform.Find("TwentyThreePlusOneText").GetComponent<Text>();
            Text cardValueText = entryUI.transform.Find("CardValueText").GetComponent<Text>();
            Text winValueText = entryUI.transform.Find("WinValueText").GetComponent<Text>();


            nameText.text = entry.name;
            perfectPairsText.text = entry.leftSideBet.ToString();
            betText.text = entry.middleBet.ToString();
            twentyThreePlusOneText.text = entry.rightSideBet.ToString();
            cardValueText.text = entry.cardValue.ToString();
            winValueText.text = entry.winValue.ToString();        
        }
    }
}

    
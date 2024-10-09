using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Scripts
    RoundManager roundmanager;
    ExtensionManager extensionmanager;
    ListManager listmanager;
    SeApi seApi;

	// Deck 
	public List<Card> deck;
	public GameObject[] cardSlots;
    public Card hiddenCard;
    public Sprite defaultCard;
    public bool[] availableCardSlots;
    public bool hidden;

    public Card[] dealerCards = new Card[2];
    public Card[] playerCards = new Card[2];

    // Values
    public Text playerValueText;
    public Text dealerValueText;
    public GameObject burstImg;
    public int playerValue = 0;
    public int dealerValue = 0;
    public int playerAces = 0;
    public int dealerAces = 0;

    // Buttons
    public Button startRoundButton;
    public Button drawButton;
    public Button shuffleButton;

    // Gameplay Needed Variables
    public bool gameEnd = false;
    public bool playerBj = false;

    // UI
    public Text leftSideBetText;
    public Text rightSideBetText;

    // Dealing Card Sound
    public AudioClip dealingCardClip;
    public AudioSource dealingCardSource;

    // Card Values
    Dictionary<string, int> cardValues = new Dictionary<string, int>()
    {
        {"Two", 2},
        {"Three", 3},
        {"Four", 4},
        {"Five", 5},
        {"Six", 6},
        {"Seven", 7},
        {"Eight", 8},
        {"Nine", 9},
        {"Ten", 10},
        {"J", 10},
        {"Q", 10},
        {"K", 10},
        {"Ace", 11}
    };


    void Start()
    {
        seApi = GameObject.FindGameObjectWithTag("SeApi").GetComponent<SeApi>();
        roundmanager = GameObject.FindGameObjectWithTag("RoundManager").GetComponent<RoundManager>();
        extensionmanager = GameObject.FindGameObjectWithTag("ExtensionManager").GetComponent<ExtensionManager>();
        listmanager = GameObject.FindGameObjectWithTag("ListManager").GetComponent<ListManager>();
    }

    void Update()
    {
        if(!gameEnd || playerBj)
        {
            if(dealerValueText.text == "BJ" || dealerValue >= 17 || playerBj)
            {
                shuffleButton.interactable = true;
                shuffleButton.gameObject.SetActive(true);
                drawButton.interactable = false;
                drawButton.gameObject.SetActive(false);
                
                CheckWinner();
                CheckSideBet();
                gameEnd = true;
                playerBj = false;
            }
        }
    }

	// Draw Card From The Deck And Manage The Game Flow
    public void DrawCard()
    {
        Card randomCard = deck[Random.Range(0, deck.Count)];        // Pick A Random Card From The Deck

        dealingCardSource.Play();                                   // Card Dealing Sound 

        if (availableCardSlots[3])                                  // If The First 4 Slots Are Available
        {                                                          
            for (int i = 0; i < availableCardSlots.Length; i++)
            {
                if (availableCardSlots[i])
                {
                    if (i != 3)                                     // First 3 Card SLOTS
                    {
                        if (cardSlots[i].tag == "PlayerSlot")       // If It Is A PlayerSlot 
                        {
                            PlayerDrawnCardValue(randomCard);       // Add The Value To The Player's Score
                            if(i == 0)                              // Save The Player's First And Second Cards
                            {
                                playerCards[0] = randomCard;        
                            }
                            else
                            {
                                playerCards[1] = randomCard;
                            }
                        }
                        else                                        // If It Is NOT A PlayerSlot 
                        {
                            DealerDrawnCardValue(randomCard);       // Add The Value To The Dealer's Score
                            if(i == 1)
                            {
                                dealerCards[0] = randomCard;        // Save The Dealer's First Card
                            }
                        }
                        cardSlots[i].GetComponent<SpriteRenderer>().sprite = randomCard.GetComponent<SpriteRenderer>().sprite;  // Set The Picked Card To The Slot
                        cardSlots[i].GetComponent<SpriteRenderer>().enabled = true;                                             // Show The Card
                        availableCardSlots[i] = false;                                                                          // Set The Slot To Unavailable
                        if(playerCards[0] != null && playerCards[1] != null)                                                    // Check If Player Has BJ
                        {
                            if((playerCards[0].GetComponent("Ace") && playerCards[1].GetComponent("Ten")) || (playerCards[1].GetComponent("Ace") && playerCards[0].GetComponent("Ten")))
                            {
                                playerValueText.text = "BJ";
                            }
                        }
                    }
                    else                                                                // 4. Card Slot Is Hidden
                    {
                        cardSlots[i].GetComponent<SpriteRenderer>().enabled = true;     // Show The Card (Hidden)
                        hiddenCard = randomCard;                                        // Set The hiddenCard
                        dealerCards[1] = hiddenCard;                                    // Set The Dealer's Second Card
                        hidden = true;                                                  // Bool Variable To True -> We Have A Hidden Card
                        availableCardSlots[i] = false;                                  // Set The Slot To Unavailable

                        if(playerValueText.text == "BJ")                                // Check If Player Has BJ (First Two Cards)
                        {
                            roundmanager.SetCardValueForPlayers(100);                   // If Player Has BJ -> Set The CardValue to 100
                            RevealHiddenCard();                                         // Show Hidden Card
                            playerBj = true;                                            // End Game Because Player Has BJ
                        }
                        else
                        {
                            roundmanager.SendDecision();                                // Send Hit or Stand Option For Players    
                        }
                    }
                    return;
                }
            } 
        }
        else                                                                            // The First Four Slots Are Unavailable
        {
            if (roundmanager.CheckHitPlayers() != 0)                                    // Not All Players Have CardValue
            {
                for (int i = 0; i < availableCardSlots.Length; i++)
                {
                    if (availableCardSlots[i] && cardSlots[i].tag == "PlayerSlot")      // Add New Card To The PlayerSlot
                    {
                        cardSlots[i].GetComponent<SpriteRenderer>().sprite = randomCard.GetComponent<SpriteRenderer>().sprite;
                        cardSlots[i].GetComponent<SpriteRenderer>().enabled = true;
                        availableCardSlots[i] = false;
                        PlayerDrawnCardValue(randomCard);

                        if (playerValue >= 21)                                         // If PlayerValue Is 21 or More
                        {
                            if(playerValue > 21)
                            {
                                burstImg.SetActive(true);
                            }
                            roundmanager.SetCardValueForPlayers(playerValue);          // Set The CardValue
                            if (hidden && hiddenCard != null)                          // If HiddenCard Is Available
                            {
                                RevealHiddenCard();                                    // Check HiddenCard
                            }  
                            else if (dealerValue <= 16)                               // Draw DealerCards
                            {
                                for (int s = 0; s < availableCardSlots.Length; s++)
                                {
                                    if (availableCardSlots[s] && cardSlots[s].tag == "DealerSlot")
                                    {
                                        cardSlots[s].GetComponent<SpriteRenderer>().sprite = randomCard.GetComponent<SpriteRenderer>().sprite;
                                        cardSlots[s].GetComponent<SpriteRenderer>().enabled = true;
                                        availableCardSlots[s] = false;
                                        DealerDrawnCardValue(randomCard);
                                        return;
                                    }
                                }
                            }
                        }
                        else                                                            // If PlayerValue Is Less Then 21
                        {
                            roundmanager.SendDecision();                                // Send Hit or Stand Option For Players 
                        }
                        return;
                    }
                }
            }
            else                                                                        // All Players Have CardValue
            {
                if(hidden && hiddenCard != null)                            
                {
                    RevealHiddenCard();                                  
                }    
                else if(dealerValue <= 16)                                   
                {
                    for (int s = 0; s < availableCardSlots.Length; s++)
                    {
                        if (availableCardSlots[s] && cardSlots[s].tag == "DealerSlot")
                        {
                            cardSlots[s].GetComponent<SpriteRenderer>().sprite = randomCard.GetComponent<SpriteRenderer>().sprite;
                            cardSlots[s].GetComponent<SpriteRenderer>().enabled = true;
                            availableCardSlots[s] = false;
                            DealerDrawnCardValue(randomCard);
                            return;
                        }
                    }
                }
            }
        }
    }

	// Shuffle The Deck
	public void Shuffle()
	{
        listmanager.DeleteList();                               // Clear The Entries List
		listmanager.DeleteLeaderBoard();						// Clear The Leaderboard List
        extensionmanager.SendRoundEndMessage();                 // RoundEnd MSG To Extension

        // Set Game Variables 
        playerValue = 0;
        dealerValue = 0;
        playerAces = 0;
        dealerAces = 0;
        dealerValueText.text = dealerValue.ToString();
        playerValueText.text = playerValue.ToString();

        // Set The CardSlots
		for(int i = 0; i < availableCardSlots.Length; i++)
		{
			availableCardSlots[i] = true; 		
            cardSlots[i].GetComponent<SpriteRenderer>().enabled = false;	
            cardSlots[i].GetComponent<SpriteRenderer>().sprite = defaultCard;		
		}
        hiddenCard = null;
        playerCards = new Card[2];
        dealerCards = new Card[2];

        // Timer
        roundmanager.timeRemaining = 20;
        roundmanager.isTimerRunning = false;
        roundmanager.objectTimerText.SetActive(false);

        // Winner Chat
        roundmanager.winnerChat.text = "";                    
		roundmanager.winnerCount = 0; 							
        roundmanager.objectWinnerCount.SetActive(false); 		

        roundmanager.isBetEnabled = false;
        gameEnd = false;
        burstImg.SetActive(false);

        // UI 
        leftSideBetText.text = "";
        rightSideBetText.text = "";

        StartCoroutine(DelayBeforeNewRound());
	}

    // Check Who Won The Round
    public async void CheckWinner()
    {
        listmanager.DeleteLeaderBoard();                // Delete Leaderboard
        await seApi.GetLeaderboard();                   // Get The Leaderboard With The New Point Balance

        roundmanager.objectWinnerCount.SetActive(true); // Show WinnerChat

        foreach(var e in listmanager.entries)
        {
            if (e.cardValue == 100)                                     
            {
                if (dealerValueText.text == "BJ")                                   
                {
                    e.winner = true; 
                    e.winValue += e.middleBet;
                    Debug.Log("Push - 2 BJ");
                }
                else
                {
                    e.winner = true; 
                    e.winValue += e.middleBet + (e.middleBet * 2);
                    Debug.Log("PLAYER WON PLAYER HAS BJ");
                }
            }
            else if (dealerValueText.text == "BJ")                                   
            {
                Debug.Log("DEALER WON - DEALER HAS BJ");
            }
            else if (e.cardValue > 21)                                             
            {
                Debug.Log("DEALER WON - PlayerValue > 21");
            }
            else if (dealerValue > 21)                                              
            {
                e.winner = true; 
                e.winValue += e.middleBet * 2;
                Debug.Log("PLAYER WON - DealerValue > 21");
            }
            else if (e.cardValue == dealerValue)                                     
            {
                e.winner = true; 
                e.winValue += e.middleBet;
                Debug.Log("Push - PlayerValue = DealerValue");
            }
            else if (e.cardValue > dealerValue)                                     
            {
                e.winner = true; 
                e.winValue += e.middleBet * 2;
                Debug.Log("PLAYER WON - PlayerValue > DealerValue");
            }
            else
            {
                Debug.Log("DEALER WON - DealerValue > PlayerValue");
            }
        }
        seApi.BulkPointsForWinners();
    }

    // Reveal The Hidden Card  
    public void RevealHiddenCard()
    {
        DealerDrawnCardValue(hiddenCard);
        cardSlots[3].GetComponent<SpriteRenderer>().sprite = hiddenCard.GetComponent<SpriteRenderer>().sprite;
        cardSlots[3].GetComponent<SpriteRenderer>().enabled = true;
        hidden = false;

        if(dealerCards[0] != null && dealerCards[1] != null)
        {
            if((dealerCards[0].GetComponent("Ace") && dealerCards[1].GetComponent("Ten")) || (dealerCards[1].GetComponent("Ace") && dealerCards[0].GetComponent("Ten")))
            {
                dealerValueText.text = "BJ";
                Debug.Log("BLACKJACK FOR THE DEALER");
            }
        }        
    }

    // Add Card's Value To Dealer's Score
    public void DealerDrawnCardValue(Card pickedCard)
    {
        Component[] pickedCardComponents = pickedCard.GetComponents(typeof(MonoBehaviour));
        Component valueComponent = pickedCardComponents[1];

        string componentType = valueComponent.GetType().Name;
        if (cardValues.ContainsKey(componentType))
        {
            if (componentType == "Ace")
            {
                dealerAces++;
            }

            if (dealerAces > 0 && dealerValue + cardValues[componentType] > 21)
            {
                // If The Dealer Has Aces And Adding The Card Value Would Exceed 21, Change Ace Value To 1.
                dealerValue -= 10;
                dealerAces--;
            }

            dealerValue += cardValues[componentType];
            dealerValueText.text = dealerValue.ToString();
        }
    }

    // Add Card's Value To Player's Score
    public void PlayerDrawnCardValue(Card pickedCard)
    {
        Component[] pickedCardComponents = pickedCard.GetComponents(typeof(MonoBehaviour));
        Component valueComponent = pickedCardComponents[1];

        string componentType = valueComponent.GetType().Name;
        if (cardValues.ContainsKey(componentType))
        {
            if (componentType == "Ace")
            {
                playerAces++;
            }

            if (playerAces > 0 && playerValue + cardValues[componentType] > 21)
            {
                // If The Player Has Aces And Adding The Card Value Would Exceed 21, Change Ace Value To 1.
                playerValue -= 10;
                playerAces--;
            }

            playerValue += cardValues[componentType];
            playerValueText.text = playerValue.ToString();
        }
    }

    // Check Side Bets
    public void CheckSideBet()
    {
        Component[] componentsForPlayerCards0 = playerCards[0].GetComponents(typeof(MonoBehaviour));    // Player's First Card
        Component[] componentsForPlayerCards1 = playerCards[1].GetComponents(typeof(MonoBehaviour));    // Player's Second Card
        Component[] componentsForDealerCards0 = dealerCards[0].GetComponents(typeof(MonoBehaviour));    // Dealer's First Card

        string playerCard0Color = GetCardColor(componentsForPlayerCards0[0]);   // Get Player's First Card Color
        string playerCard1Color = GetCardColor(componentsForPlayerCards1[0]);   // Get Player's Second Card Color

        bool leftBetSameValue = componentsForPlayerCards0[1].GetType() == componentsForPlayerCards1[1].GetType();   // LEFT BET - Check If Player's First And Second Card's Value Are Same
        bool leftBetSameSuit = componentsForPlayerCards0[0].GetType() == componentsForPlayerCards1[0].GetType();    // LEFT BET - Check If Player's First And Second Card's Suit Are Same
        bool leftBetSameColor = playerCard0Color == playerCard1Color;   // LEFT BET - Check If Player's First And Second Card's Color Are Same

        bool rightBetSameValue = leftBetSameValue && componentsForPlayerCards0[1].GetType() == componentsForDealerCards0[1].GetType();  // RIGHT BET - Check If Player's Cards And Dealer's Card Have Same Value
        bool rightBetSameSuit = leftBetSameSuit && componentsForPlayerCards0[0].GetType() == componentsForDealerCards0[0].GetType();    // RIGHT BET - Check If Player's Cards And Dealer's Card Have Same Suit

        foreach(var e in listmanager.entries)
        {
            // LeftSideBet
            if (leftBetSameValue && leftBetSameSuit)    // Perfect Pair - 25:1
            {
                e.winValue += e.leftSideBet + (e.leftSideBet * 25);
                e.winner = true; 
                Debug.Log("Perfect Pair");
                leftSideBetText.text = "Perfect Pair";
            }
            else if (leftBetSameValue && leftBetSameColor && !leftBetSameSuit)  // Coloured Pair - 12:1
            {
                e.winValue += e.leftSideBet + (e.leftSideBet * 12);
                e.winner = true; 
                Debug.Log("Coloured Pair");
                leftSideBetText.text = "Coloured Pair";
            }
            else if (leftBetSameValue && !leftBetSameColor) // Mixed Pair - 6:1
            {
                e.winValue += e.leftSideBet + (e.leftSideBet * 6);
                e.winner = true;
                Debug.Log("Mixed Pair");
                leftSideBetText.text = "Mixed Pair";
            }
            else
            {
                leftSideBetText.text = "";
            }

            // RightSideBet
            if (rightBetSameValue && rightBetSameSuit)  // Suited Trips - 100:1
            {
                e.winValue += e.rightSideBet + (e.rightSideBet * 100);
                e.winner = true;
                Debug.Log("Suited Trips");
                rightSideBetText.text = "Suited Trips";
            }
            else if (CheckStraightFlush(componentsForPlayerCards0, componentsForPlayerCards1, componentsForDealerCards0) && rightBetSameSuit)   // Straight Flush - 40:1
            {
                e.winValue += e.rightSideBet + (e.rightSideBet * 40);    
                e.winner = true;        
                Debug.Log("Straight Flush");
                rightSideBetText.text = "Straight Flush";
            }
            else if (rightBetSameValue) // Three of a kind - 30:1
            {
                e.winValue += e.rightSideBet + (e.rightSideBet * 30);  
                e.winner = true;
                Debug.Log("Three of a kind");
                rightSideBetText.text = "Three of a kind";
            }
            else if (CheckStraightFlush(componentsForPlayerCards0, componentsForPlayerCards1, componentsForDealerCards0))   // Straight - 10:1
            {
                e.winValue += e.rightSideBet + (e.rightSideBet * 10); 
                e.winner = true;
                Debug.Log("Straight");
                rightSideBetText.text = "Straight";
            }
            else if (rightBetSameSuit)  // Flush - 5:1
            {
                e.winValue += e.rightSideBet + (e.rightSideBet * 5);
                e.winner = true;
                Debug.Log("Flush");
                rightSideBetText.text = "Flush";
            }
            else
            {
                rightSideBetText.text = "";
            }
        }
    }

    public bool CheckStraightFlush(Component[] firstCard, Component[] secondCard, Component[] thirdCard)
    {
        List<int> numbers = new List<int>();    // List For The Card's Values
        List<string> cardNames = new List<string>(); // List For The Card's Names

        Component firstCardValueComp = firstCard[1];    // 3 
        Component secondCardValueComp = secondCard[1];  // 10
        Component thirdCardValueComp = thirdCard[1];  // 9

        string firstCardComponentType = firstCardValueComp.GetType().Name; // Three
        string secondCardComponentType = secondCardValueComp.GetType().Name; // Ten
        string thirdCardComponentType = thirdCardValueComp.GetType().Name; // Nine

        cardNames.Add(firstCardComponentType);  // Add Names To The List
        cardNames.Add(secondCardComponentType);
        cardNames.Add(thirdCardComponentType);

        if (cardValues.ContainsKey(firstCardComponentType) && cardValues.ContainsKey(secondCardComponentType) && cardValues.ContainsKey(thirdCardComponentType)) 
        {
            numbers.Add(cardValues[firstCardComponentType]);    // Add Values To The List
            numbers.Add(cardValues[secondCardComponentType]);
            numbers.Add(cardValues[thirdCardComponentType]);
        }
        numbers.Sort(); // 8 8 8

        // Check 10 and Facecard's Value
        if(cardNames.Contains("Ten") && cardNames.Contains("J") && cardNames.Contains("Q") || 
        cardNames.Contains("J") && cardNames.Contains("Q") && cardNames.Contains("K") ||
        cardNames.Contains("Q") && cardNames.Contains("K") && cardNames.Contains("Ace"))
        {
            return true;
        }

        // Check Ace For Value 1
        if ((numbers.Contains(11) && numbers.Contains(2) && numbers.Contains(3))) 
        {
            return true;
        }

        int difference = 1; // The Difference Between The Values
        
        // Check If The Difference Is The Same For All Numbers
        for (int i = 0; i < numbers.Count - 1; i++) 
        {
            if (numbers[i + 1] - numbers[i] != difference)
            {
                // The Difference Is Not The Same
                return false;
            }
        }
        // The Numbers Form A Numerical Sequence
        return true;
    }

    // Card Color For SideBets
    private string GetCardColor(Component component)
    {
        string cardColor = "";

        switch (component.GetType().Name)
        {
            case "Club":
            case "Spade":
                cardColor = "Black";
                break;
            case "Diamond":
            case "Heart":
                cardColor = "Red";
                break;
        }
        return cardColor;
    }

    // Add Delay
    IEnumerator DelayBeforeNewRound()
    {
        shuffleButton.interactable = false;
        yield return new WaitForSeconds(5);
        shuffleButton.gameObject.SetActive(false);
        startRoundButton.interactable = true;
        startRoundButton.gameObject.SetActive(true);
    }
}

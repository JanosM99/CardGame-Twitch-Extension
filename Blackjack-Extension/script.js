const bet1Button = document.getElementById('bet_1');
const bet5Button = document.getElementById('bet_5');
const bet10Button = document.getElementById('bet_10');
const bet25Button = document.getElementById('bet_25');

const perfectPairsText = document.getElementById('left_side_bet_text');
const betText = document.getElementById('bet_text');
const twentyOneThreeText = document.getElementById('right_side_bet_text');

const betButton = document.getElementById("bet_button");
const hitButton = document.getElementById("hit_button");
const standButton = document.getElementById("stand_button");

let userId;
let jwtToken;
let clientId;
let viewerName;

let selectedBetPlace = null;
let selectedBetPlaceImg = null;
let bets = {
  'leftSideBet': 0,
  'betMiddle': 0,
  'rightSideBet': 0
};
let stand = false;
let clicked = false;


socket.onopen = () => {
  //console.log("Connection opened");
};

// Buttons Are Disabled By Default
betButton.disabled = true;
hitButton.disabled = true;
standButton.disabled = true;
bet1Button.disabled = true;
bet5Button.disabled = true;
bet10Button.disabled = true;
bet25Button.disabled = true;

// Check If The Viewer Is Logged In Or Logged Out
window.Twitch.ext.onAuthorized((auth) => 
{
  if (window.Twitch.ext.viewer.isLinked) 
  {
    // User Is Logged In
    document.body.classList.add('logged_in');
    document.body.classList.remove('logged_out');

    loadProfile();
  } 
  else 
  {
    document.body.classList.remove('logged_in');
    document.body.classList.add('logged_out');
  }
});

// Login
document.getElementById('login_button').addEventListener('click', (e) => {
  e.preventDefault();

  window.Twitch.ext.actions.requestIdShare();
});

// User Is Logged In -> We Get The Viewer userId, jwtToken, clientId and viewerName 
function loadProfile() {
  // userId
  userId = window.Twitch.ext.viewer.id;
  // jwtToken & clientId
  window.Twitch.ext.onAuthorized(function(auth) {
    jwtToken = auth.helixToken;
    clientId = auth.clientId;
  });
  // viewerName
  getViewerName();
}

// Get The Viewer Login Name With userId, jwtToken And clientId
function getViewerName() {
  const url = `https://api.twitch.tv/helix/users?id=${userId}`;

  const headers = {
    "Authorization": 'Extension ' + jwtToken,
    "Client-ID": clientId
  };

  fetch(
    url, 
    {
      headers: headers,
  })
  .then((response) => response.json())
  .then((data) => {
    viewerName = data.data[0].login;
  });
}

// Websocket Listener
socket.onmessage = (event) => {
  if (event.data == "startround") // startround
  {
    betButton.disabled = false;
    setTimeout(function(){
      betButton.disabled = true;
    }, 15000);
  } 
  else if (event.data == "decision")  // decision
  {
    if(!stand)
    {
      hitButton.disabled = false;
      standButton.disabled = false;
      setTimeout(function() {
        hitButton.disabled = true;
        standButton.disabled = true;

        if(!clicked)
        {
          stand = true;
        }
      }, 15000);
    }
  } 
  else if (event.data == "endround") // endround
  {
    resetRound();
    document.body.classList.add('logged_in');
    document.body.classList.remove('hit_or_stand_section');
    stand = false;
  } 
};

// Select leftSideBet - betMiddle - rightSideBet
function selectBetPlace(place) {
  selectedBetPlace = place;

  const elementMap = {
    leftSideBet: "left_side_bet_empty_img",
    betMiddle: "bet_empty_img",
    rightSideBet: "right_side_bet_empty_img",
  };

  const elementId = elementMap[selectedBetPlace];
  selectedBetPlaceImg = document.getElementById(elementId);

  if (selectedBetPlace != null) // Enable Buttons If BetPlace Selected
  {
    bet1Button.disabled = false;
    bet5Button.disabled = false;
    bet10Button.disabled = false;
    bet25Button.disabled = false;
  }
  if (selectedBetPlace == 'leftSideBet' || selectedBetPlace == 'rightSideBet') {  // Disable The bet25Button For leftSideBet & rightSideBet
    bet25Button.disabled = true;
  }
}

// Add Bet Amount To The Selected BetPlace
function placeBet(amount) {
  if (selectedBetPlace) {
    if ((selectedBetPlace == 'leftSideBet' || selectedBetPlace == 'rightSideBet') && (bets[selectedBetPlace] + amount > 10)) {  // Max Bet For leftSideBet & rightSideBet Is 10
      bets[selectedBetPlace] = 10;
    }
    else if((selectedBetPlace == 'betMiddle') && (bets[selectedBetPlace] + amount > 25)) // Max Bet For betMiddle Is 25
    {
      bets[selectedBetPlace] = 25;
    }
    else
    {
      bets[selectedBetPlace] += amount;
    }
  }

  // Update Chip IMGs
  updateBetImages();  
  if (bets[selectedBetPlace] == 25) {
    selectedBetPlaceImg.src = "images/bet_25_img.png";
  } else if (bets[selectedBetPlace] >= 10) {
    selectedBetPlaceImg.src = "images/bet_10_img.png";
  } else if (bets[selectedBetPlace] >= 5) {
    selectedBetPlaceImg.src = "images/bet_5_img.png";
  } else {
    selectedBetPlaceImg.src = "images/bet_1_img.png";
  }

  // Update Bet Values
  perfectPairsText.textContent = `${bets['leftSideBet']}`;
  betText.textContent = `${bets['betMiddle']}`;
  twentyOneThreeText.textContent = `${bets['rightSideBet']}`;
}

// Show/Hide Chip IMGs
function updateBetImages() {
  if (selectedBetPlace) {
    const betClass = selectedBetPlace;
    const betAmount = bets[selectedBetPlace];

    if (betAmount != 0) {
      document.body.classList.add(betClass);
    } 
    else {
      document.body.classList.remove(betClass);
    }
  }
}

// Reset Selected BetPlace
function resetBet()
{
  if(selectedBetPlace)
  {
    bets[selectedBetPlace] = 0;
    updateBetImages();
  }

  // Update Bet Values
  perfectPairsText.textContent = `${bets['leftSideBet']}`;
  betText.textContent = `${bets['betMiddle']}`;
  twentyOneThreeText.textContent = `${bets['rightSideBet']}`;
}

// Reset Round - endround
function resetRound()
{
  document.body.classList.remove('leftSideBet');
  document.body.classList.remove('betMiddle');
  document.body.classList.remove('rightSideBet');

  Object.keys(bets).forEach(key => {
    bets[key] = 0;
  }); 

  // Update Bet Values
  perfectPairsText.textContent = `${bets['leftSideBet']}`;
  betText.textContent = `${bets['betMiddle']}`;
  twentyOneThreeText.textContent = `${bets['rightSideBet']}`;
}

// Bet
betButton.addEventListener("click", () => {
  if (bets['betMiddle'] != 0) 
  {
    socket.send(JSON.stringify({
      action: "sendmessage",
      message: viewerName + ' ' + bets['leftSideBet'] + ' ' + bets['betMiddle'] + ' ' + bets['rightSideBet'] + ' ' 
    }));
    // Push The Viewer To The hit_or_stand_section
    betButton.disabled = true;
    document.body.classList.add('hit_or_stand_section');
    document.body.classList.remove('logged_in');
  }
});

// Hit
hitButton.addEventListener("click", () => {
  socket.send(JSON.stringify({
    action: "sendmessage",
    message: viewerName + ' ' + 'hit'
  }));
  clicked = true;
  hitButton.disabled = true;
  standButton.disabled = true;
});

// Stand
standButton.addEventListener("click", () => {
  socket.send(JSON.stringify({
    action: "sendmessage",
    message: viewerName + ' ' + 'stand'
  }));
  stand = true;
  clicked = true;
  hitButton.disabled = true;
  standButton.disabled = true;
});


// BetPlace Event Listeners
document.getElementById('left_side_bet').addEventListener('click', function() {
  selectBetPlace('leftSideBet');
});

document.getElementById('bet').addEventListener('click', function() {
  selectBetPlace('betMiddle');
});

document.getElementById('right_side_bet').addEventListener('click', function() {
  selectBetPlace('rightSideBet');
});
// Chip Event Listeners
document.getElementById('bet_1').addEventListener('click', function() {
  placeBet(1);
});

document.getElementById('bet_5').addEventListener('click', function() {
  placeBet(5);
});

document.getElementById('bet_10').addEventListener('click', function() {
  placeBet(10);
});

document.getElementById('bet_25').addEventListener('click', function() {
  placeBet(25);
});

document.getElementById('reset_button').addEventListener('click', function() {
  resetBet();
});

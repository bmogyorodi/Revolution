using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

    //base settings
    private string selectedbidType = "";
    public int maxBidNum;


    //Data structures
    private DisplayManager displayManager;
    private DataController data;
    private Settings settings;
    private EnemyController enemyController;
    private MusicManager music;


    //Menu Buttons
    public Button newGame;
    public Button quitGame;


    //Player data containers
    private Player[] players;
    private ComputerPlayer[] computer;
    private readonly int activePlayer = 0;


    //Timer setting for messages and message list
    public float timeformessage;
    private float timeleft = 0;
    public Text messageBoard;
    public Text tutorialBoard;
    public Button messageButton;
    private List<string> messageList = new List<string>();
    private List<string> tutorialList = new List<string>();
    private List<TutorialRound> tutorialRounds;
    private readonly string gameDataFileName="messagedata.json";
    private int tutorialprogress = 0;
    private int turnNumber = 1;
    private bool freezemessage = false;





    //Bid list to be reversed if player changes their mind
    private List<Bid> bidHistory = new List<Bid>();




    //end day Button
    public Button endDay;






    //spy variables
    public GameObject spyPanel;
    public Button spybutton;
    public Text spybuilding;
    private int spyWinner;

    //apothecary variables
    public GameObject apothecaryPanel;
    public Button[] swapButtons;
    public Text[] swapTexts;
    public Button swapConfirm;
    private int apothecaryWinner;





    // Start is called before the first frame update
    void Start()
    {
        // AddMessage("Let's play Revolution!");
        GetDataForGame();
        PlayersReady();
        InitTextandGraphics();
        InitButtons();
        if (settings.GetIsTutorial())
        {
            WelcomeToTutorial();
        }
    }
    private void Update()
    {
        timeleft -= Time.deltaTime;
        UpdateMessage(timeleft);
    }

    private void UpdateMessage(float timeformessage)
    {
        if (timeformessage > 0)
        {

        }
        else
        {


            ChangeMessage();
        }
    }
    private void AddMessage(string message)
    {
        messageList.Add(message);

    }
    private void TutorialMessage(string message)
    {
       
            
        
        if (freezemessage)
        {
            tutorialList.Add(message);
            messageButton.interactable = true;
        }

    }
    private void TutorialNextLine(string line)
    {
        if (freezemessage)
        {
            tutorialList[tutorialList.Count - 1] += "\n" + line;
        }

    }
    private void ShowTutorialMessage()
    {
        if (tutorialList.Count > 0)
        {
            tutorialBoard.text = tutorialList[0];
            messageBoard.text = "";
            tutorialList.RemoveAt(0);
            if (tutorialList.Count == 0)
            {
                messageButton.interactable = false;
            }
        }
    }
    private void ChangeMessage()
    {
        if (messageList.Count > 0)
        {
            messageBoard.text = messageList[0];
            messageList.RemoveAt(0);
            timeleft = timeformessage;
        }
        else
        {
            messageBoard.text = "";
        }
    }

    public void DisplayButtonBids(int characterNum, Player activePlayer)
    {

        displayManager.characterButtons[characterNum].UpdateBidTable(characterNum, activePlayer);
    }


    public void CharacterOnclick(int characterNum)
    {
        if (selectedbidType != "")
        {
            if (players[activePlayer].NumberOfBids() < maxBidNum || players[activePlayer].GetBid(characterNum) != 0)
            {

                if ((!data.characters[characterNum].canbeBlackmailed && selectedbidType == "blackmail") || (!data.characters[characterNum].canbeForced && selectedbidType == "force"))
                { }

                else
                {


                    if (players[activePlayer].PlaceABid(selectedbidType, characterNum))
                    {
                        displayManager.ChangePoolDisplay(players[activePlayer]);
                        DisplayButtonBids(characterNum, players[activePlayer]);
                        bidHistory.Add(new Bid(selectedbidType, characterNum));
                    }

                }
            }
        }
    }







    public void SelectBetType(string betType)
    {
        selectedbidType = betType;
        displayManager.SetBidTypeButton(betType);
        Debug.Log("New betType: " + selectedbidType);
    }


    public void Resolution()
    {

        selectedbidType = "";
        displayManager.ResetBidTypeButtons();
        if (players[0].playerPool.Overall() != 0)
        {

        }
        else
        {
            if (!settings.GetIsTutorial())
            {
                AIBidding();
            }
            else
            {
                TutorialBidding(turnNumber);
            }
            displayManager.FreezeBoard(false);
            // WriteHeadLine();
            for (int i = 0; i < data.characters.Length; i++)
            {
                displayManager.characterButtons[i].DisplayHighestBid(i, players);
                int winnerIndex = ReturnWinnerIndex(players, i);
                ColorBidWinner(winnerIndex, i);
                if (data.characters[i].characterName == "SPY")
                {
                    spyWinner = winnerIndex;

                }
                if (data.characters[i].characterName == "APOTHECARY")
                {
                    apothecaryWinner = winnerIndex;

                }
                if (winnerIndex != -1)
                {
                    RewardBidWinner(players[winnerIndex], data.characters[i]);



                }


            }
            ActivateSpy();
            ActivateSwap(false);
            bidHistory = new List<Bid>();
            displayManager.PanelUpdate(players);

        }
    }



    private void EndGame()
    {
        Debug.Log("Game Over!");
        data.SetBuildingOwners(players);
        // ShowFinalBonus();
        for (int i = 0; i < players.Length; i++)
        {
            players[i].ConvertPoolToPoints();
        }
        data.GivePointsToOwners(players);

        displayManager.ColorOwners(data, players);
        displayManager.PanelUpdate(players);
        DisplayWinner(players);
        endDay.interactable = false;


        //throw new NotImplementedException();
    }

    private void AIBidding()
    {

        for (int i = 0; i < computer.Length; i++)
        {

            computer[i].InitBidselection(data, players);
            computer[i].PriorityController(data, players, enemyController);
           // AddMessage(computer[i].PrintPriorityTable());
            computer[i].ComputerBidding(players);




        }
    }
    private void TutorialBidding(int turnnumber)
    {
        if (turnnumber == 1)
        {
            players[1].PlaceABid("gold", 8);
            players[1].PlaceABid("gold", 8);
            players[1].PlaceABid("blackmail", 5);
            players[1].PlaceABid("force", 7);
            players[1].PlaceABid("gold", 6);

            turnNumber++;

        }
        else
        {
            if (turnnumber == 2)
            {
                players[1].PlaceABid("blackmail", 6);
                players[1].PlaceABid("blackmail", 5);
                players[1].PlaceABid("gold",0);
                players[1].PlaceABid("gold",0);
                players[1].PlaceABid("gold",11);
                players[1].PlaceABid("gold",11);
                players[1].PlaceABid("gold",8);
                players[1].PlaceABid("gold",8);
                players[1].PlaceABid("gold",3);
                players[1].PlaceABid("gold",3);
                /*8 gold, plus 2 blackmail*/
                turnNumber++;
            }
            else
            {
                AIBidding();
            }
        }
    }




    private int ReturnWinnerIndex(Player[] players, int characterIndex)
    {
        int winnerIndex = -1;
        int highestbet = 0;
        for (int j = 0; j < players.Length; j++)
        {
            if (players[j].GetBid(characterIndex) == highestbet)
            {
                winnerIndex = -1;
            }
            else
            {
                if (players[j].GetBid(characterIndex) > highestbet)
                {
                    winnerIndex = j;
                    highestbet = players[j].GetBid(characterIndex);
                }
            }
        }
        return winnerIndex;
    }

    private void RewardBidWinner(Player winner, Character character)
    {
        character.ResolveforPlayer(winner);
        if (character.hasbuildingConnected)
        {
            int buildingindex = data.ReturnBuildingIndex(character.buildingName);
            if (buildingindex != -1)
            {
                data.buildings[buildingindex].PlayerTakeSpace(winner);
            }
            else
            {
                Debug.LogError("Couldn't find building!");
            }
        }
    }
    public void ReverseLastBid(int playerIndex)
    {
        if (bidHistory.Count != 0)
        {
            Bid lastbid = bidHistory[bidHistory.Count - 1];
            lastbid.ReverseBet(players[playerIndex]);
            bidHistory.Remove(lastbid);
            DisplayButtonBids(lastbid.GetCharIndex(), players[playerIndex]);
            displayManager.ChangePoolDisplay(players[playerIndex]);
        }
    }
    private void ColorBidWinner(int playerIndex, int characterIndex)
    {


        if (playerIndex != -1)
        {
            displayManager.characterButtons[characterIndex].characterButton.GetComponentInChildren<Image>().color = players[playerIndex].playerColor;
        }
    }

    public void NextDay()
    {
        if (!(spybutton.interactable || swapConfirm.interactable || displayManager.confirmButton.interactable))
        {
            if (data.IsBoardFilled())
            {
                EndGame();
            }
            else
            {

                displayManager.FreezeBoard(true);
                displayManager.ResetBidding(data, players);

            }
        }
    }
    private void ActivateSwap(bool activeSpy)
    {
        if (apothecaryWinner != -1 && (spyWinner == 0) == activeSpy && spyPanel.activeSelf == false)
        {
            if (apothecaryWinner == 0)
            {
                apothecaryPanel.SetActive(true);
                swapButtons[0].interactable = true;
                swapButtons[1].interactable = true;
                swapConfirm.interactable = true;
            }
            else
            {
                AddMessage(computer[apothecaryWinner - 1].ComputerApothecary(data, players));
            }
        }

    }
    private void ActivateSpy()
    {
        if (spyWinner != -1)
        {
            if (spyWinner == 0)
            {
                spyPanel.SetActive(true);
                spybutton.interactable = true;
            }
            else
            {
                AddMessage(computer[spyWinner - 1].ComputerSpy(data, players));

            }
        }
    }
    private void InitComputer(Player[] players, DataController data)
    {
        int counter = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].ShowisHuman())
            {
                counter++;
            }
        }
        computer = new ComputerPlayer[counter];
        counter = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].ShowisHuman())
            {
                computer[counter] = new ComputerPlayer();
                computer[counter].SetPlayerIndex(i);
                computer[counter].InitSpecialBuildingIndex(data);
                counter++;
            }
        }
    }
    private void DisplayWinner(Player[] players)
    {
        int maxpoint = 0;
        int leadIndex = -1;
        int count = 0;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetPoints() > maxpoint)
            {
                maxpoint = players[i].GetPoints();
                leadIndex = i;
            }
        }
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].GetPoints() == maxpoint)
            {
                count++;
            }
        }
        if (count == 1)
        {
            AddMessage("Player " + (leadIndex + 1) + " wins!");
        }
    }
    private void NewGame()
    {

        Destroy(FindObjectOfType<Settings>());

        SceneManager.LoadScene("Main menu");

    }
    private void PlayersReady()
    {
        players = new Player[settings.GetPlayerNum()];
        displayManager.GetPlayerColor(settings);
        displayManager.InitPlayers(players);
        players[0].SetisHuman(true);
        InitComputer(players, data);
        displayManager.ChangePoolDisplay(players[0]);
    }
    private void GetDataForGame()
    {
        settings = FindObjectOfType<Settings>();
        enemyController = settings.GetDifficulty();
        data = FindObjectOfType<DataController>();
        music = FindObjectOfType<MusicManager>();

        displayManager = FindObjectOfType<DisplayManager>();
        if (settings.GetIsTutorial())
        {
            LoadGameData();
        }
    }
    private void InitButtons()
    {
        for (int i = 0; i < data.buildings.Length; i++)
        {
            for (int j = 0; j < data.buildings[i].occupancy.Length; j++)
            {

                data.SlotButtonSetting(i, j, spybutton, spybuilding);
                data.SwapSlotSetting(i, j, swapButtons, swapTexts, data);
            }
        }
        spybutton.onClick.AddListener(() => data.ExecuteSpy(spybutton, spybuilding, players[0], spyPanel));
        swapConfirm.onClick.AddListener(() => data.ExecuteApothecary(swapButtons, swapTexts, displayManager, swapConfirm, spybutton, apothecaryPanel));
        spybutton.onClick.AddListener(() => ActivateSwap(true));
        newGame.onClick.AddListener(() => NewGame());
        displayManager.playerGold.onClick.AddListener(() => SelectBetType("gold"));
        displayManager.playerBlackMail.onClick.AddListener(() => SelectBetType("blackmail"));
        displayManager.playerForce.onClick.AddListener(() => SelectBetType("force"));

        displayManager.confirmButton.onClick.AddListener(() => Resolution());


        endDay.onClick.AddListener(() => NextDay());

    }
    private void InitTextandGraphics()
    {
        displayManager.CreateBuildingPanel(data);


        displayManager.InitCharacterButton(data.characters, displayManager.characterButtons);
        displayManager.InitBidTables(data);
    }
    private void WelcomeToTutorial()
    {
        displayManager.FreezeBoard(false);
        displayManager.FreezeCharacters(false);
        data.DisableBuildingButtons(false);
        messageButton.gameObject.SetActive(true);
        messageButton.onClick.AddListener(() => ShowTutorialMessage());
        freezemessage = true;
        
        ProcessNextMessageTier();
        ShowTutorialMessage();

        messageButton.onClick.AddListener(() => TutorialContinue(0));




    }
    private void TutorialContinue(int i)
    {
        if (tutorialprogress == i || i == 0)
        {
            if (tutorialprogress == 0 && tutorialList.Count == 0)
            {
                
                SetTutorialStep(displayManager.playerForce, 1);







            }
            else
            {


                if (tutorialprogress == 1)
                {
                    
                    SetTutorialStep(displayManager.characterButtons[7].characterButton, 2);
                }
                else
                {
                    if (tutorialprogress == 2)
                    {
                        displayManager.characterButtons[7].characterButton.interactable = false;
                        

                        SetTutorialStep(displayManager.playerGold, 3);

                    }
                    else
                    {
                        if (tutorialprogress == 3)
                        {
                           

                            SetTutorialStep(displayManager.characterButtons[11].characterButton, 4);
                        }
                        else
                        {
                            if (tutorialprogress == 4 && players[activePlayer].GetBid(11) == 3)
                            {
                                

                                SetTutorialStep(displayManager.reverseButton, 5);
                            }
                            else
                            {
                                if (tutorialprogress == 5)
                                {
                                    

                                    SetTutorialStep(displayManager.characterButtons[8].characterButton, 6);
                                }
                                else
                                {
                                    if (tutorialprogress == 6)
                                    {
                                        

                                        SetTutorialStep(displayManager.playerBlackMail, 7);
                                    }
                                    else
                                    {
                                        if (tutorialprogress == 7)
                                        {
                                            

                                            SetTutorialStep(displayManager.characterButtons[0].characterButton, 8);
                                        }
                                        else
                                        {
                                            if (tutorialprogress == 8 && players[activePlayer].GetBid(0)==10)
                                            {
                                                
                                                SetTutorialStep(displayManager.confirmButton, 9);
                                            }
                                            else
                                            {
                                                if (tutorialprogress == 9)
                                                {
                                                    displayManager.FreezeCharacters(false);
                                                    endDay.interactable = false;
                                                    
                                                    SetTutorialStep(endDay, 10);
                                                }
                                                else
                                                {
                                                    if (tutorialprogress == 10)
                                                    {
                                                        
                                                        SetTutorialStep(new Button[] { displayManager.characterButtons[3].characterButton, displayManager.characterButtons[6].characterButton }, 11);

                                                    }
                                                    else
                                                    {
                                                        if (tutorialprogress == 11 && players[activePlayer].GetBid(3) == 100 && players[activePlayer].GetBid(6) == 100)
                                                        {
                                                            
                                                            SetTutorialStep(new Button[] { displayManager.characterButtons[2].characterButton, displayManager.characterButtons[9].characterButton }, 12);


                                                        }
                                                        else
                                                        {
                                                            if (tutorialprogress == 12 && players[activePlayer].GetBid(2) == 1 && players[activePlayer].GetBid(9) == 1)
                                                            {
                                                                
                                                                SetTutorialStep(displayManager.characterButtons[4].characterButton,13);
                                                            }
                                                            else
                                                            {
                                                                if(tutorialprogress==13 && players[activePlayer].GetBid(4) == 1)
                                                                {
                                                                   
                                                                    SetTutorialStep(displayManager.confirmButton,14);
                                                                }
                                                                else
                                                                {
                                                                    if (tutorialprogress == 14)
                                                                    {

                                                                        
                                                                        SetTutorialStep(data.buildings[0].occupancy[1],15);
                                                                    }
                                                                    else
                                                                    {
                                                                        if (tutorialprogress == 15)
                                                                        {

                                                                            
                                                                            SetTutorialStep(spybutton, 16);
                                                                        }
                                                                        else
                                                                        {
                                                                            if (tutorialprogress == 16)
                                                                            {
                                                                                SetTutorialStep(endDay, 17);
                                                                            }
                                                                            else
                                                                            {
                                                                                if (tutorialprogress == 17)
                                                                                {

                                                                                }
                                                                            }
                                                                           
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }


                            }
                        }
                    }
                }
            }
        }
    }



    private void SetTutorialStep(Button button, int i)
    {
        if (freezemessage)
        {
            music.audiosource.Play();
            ProcessNextMessageTier();
            ShowTutorialMessage();
            freezemessage = false;
        }

        

        if (tutorialList.Count == 0)
        {
            tutorialprogress++;
            freezemessage = true;
            SetTutorialProgressButton(button, i);

        }
    }
    private void SetTutorialStep(Button[] buttons, int j)
    {
        
        if (freezemessage)
        {
            music.audiosource.Play();
            ProcessNextMessageTier();
            ShowTutorialMessage();
            freezemessage = false;


        }
        

        if (tutorialList.Count == 0)
        {
            tutorialprogress++;
            freezemessage = true;
            for (int i = 0; i < buttons.Length; i++)
            {
                SetTutorialProgressButton(buttons[i], j);
            }
        }
    }
    private void SetTutorialProgressButton(Button button, int i)
    {
        button.interactable = true;
        button.onClick.AddListener(() => TutorialContinue(i));
    }
    private void LoadGameData()
    {
        // Path.Combine combines strings into a file path
        // Application.StreamingAssets points to Assets/StreamingAssets in the Editor, and the StreamingAssets folder in a build
        string filePath = Path.Combine(Application.streamingAssetsPath, gameDataFileName);

        if (File.Exists(filePath))
        {
            // Read the json from the file into a string
            string dataAsJson = File.ReadAllText(filePath);
            // Pass the json to JsonUtility, and tell it to create a GameData object from it
            TutorialData loadedData = JsonUtility.FromJson<TutorialData>(dataAsJson);

            // Retrieve the allRoundData property of loadedData
            tutorialRounds = loadedData.tutorialRounds;
        }
        else
        {
            Debug.LogError("Cannot load game data!");
        }
    }
    private void ProcessMessage(TutorialMessage message)
    {
         TutorialMessage(message.lines[0]);
        for(int i=1; i<message.lines.Length; i++)
        {
            TutorialNextLine(message.lines[i]);
        }
    }
    private void ProcessMessageTier(TutorialRound round)
    {
        for(int i=0; i<round.messages.Length; i++)
        {
            ProcessMessage(round.messages[i]);
        }
    }
    private void ProcessNextMessageTier()
    {
        if (tutorialRounds.Count > 0)
        {
            ProcessMessageTier(tutorialRounds[0]);
            tutorialRounds.RemoveAt(0);
        }
    }
}











using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour {

    private enum RationsInteractionState { IDLE, TAKE, CONSUMESTORE, OVER}
    private RationsInteractionState rationsInteractionState = RationsInteractionState.IDLE;

    [FoldoutGroup("Game Manager")]
    public int lengthOfFamine = 10; // Years
    [FoldoutGroup("Game Manager")]
    public int daysInYear = 3; // There are x number of days in a year
    [FoldoutGroup("Game Manager")]
    public int rationReplinishPercentMin = 30;
    [FoldoutGroup("Game Manager")]
    public int rationsReplinishPercentMax = 90;

    [FoldoutGroup("Time Of Year")]
    public int day = 1;
    [FoldoutGroup("Time Of Year")]
    public int year = 1;

    [FoldoutGroup("External Classes")]
    public Government government;
    [FoldoutGroup("External Classes")]
    [DisableInEditorMode]
    public List<Family> families = new List<Family>();

    [FoldoutGroup("Player Specific")]
    public Family playerAssignedFamily;

    [FoldoutGroup("UI - Family Title")]
    public Text familyTitleLabel;

    [FoldoutGroup("UI - Family Info")]
    public Text currentRationsLabel;
    [FoldoutGroup("UI - Family Info")]
    public Text familyHealthLabel;
    [FoldoutGroup("UI - Family Info")]
    public Text familyMembersLabel;
    [FoldoutGroup("UI - Family Info")]
    public Text neededRationsLabel;
    [FoldoutGroup("UI - Family Info")]
    public Text rationsPerMemberLabel;
    [FoldoutGroup("UI - Family Info")]
    public Text stolenRationLabel;
    [FoldoutGroup("UI - Family Info")]
    public Text storedRationsLabel;

    [FoldoutGroup("UI - Rations Interaction")]
    public InputField takeInputField;
    [FoldoutGroup("UI - Rations Interaction")]
    public InputField consumeInputField;
    [FoldoutGroup("UI - Rations Interaction")]
    public InputField storeInputField;
    [FoldoutGroup("UI - Rations Interaction")]
    public InputField consumeFromStoreInputField;
    [FoldoutGroup("UI - Rations Interaction")]
    public Button takeButton;
    [FoldoutGroup("UI - Rations Interaction")]
    public Button consumeStoreButton;
    [FoldoutGroup("UI - Rations Interaction")]
    public Button consumeFromStoreButton;

    [FoldoutGroup("UI - Government Information")]
    public Text deadFamiliesLabel;
    [FoldoutGroup("UI - Government Information")]
    public Text familiesAtStartLabel;
    [FoldoutGroup("UI - Government Information")]
    public Text starvingFamiliesLabel;
    [FoldoutGroup("UI - Government Information")]
    public Text stolenRationsTotalLabel;

    [FoldoutGroup("UI - Year Info")]
    public Text yearLabel;
    [FoldoutGroup("UI - Year Info")]
    public Text dayLabel;
    [FoldoutGroup("UI - Year Info")]
    public Text rationLengthLabel;

    [FoldoutGroup("UI - Notification")]
    public GameObject notificationPrefab;
    [FoldoutGroup("UI - Notification")]
    public GameObject notificationParent;


    private System.Random random = new System.Random();

	// Use this for initialization
	void Start () {
        // Rations interaction state is idle
        rationsInteractionState = RationsInteractionState.TAKE;
        // Create Government
        CreateGovernment(); // Does nothing yet - no randomization
        // Create families
        CreateFamilies(government.numberOfFamilies);
        // Assign player family
        AssignPlayerFamily();
        // First UI update
        UpdateUI();
        // Subscribe buttons to events
        SubscribeButtonsToEvents();
        // Disable Consume/store UI
        consumeInputField.interactable = false;
        storeInputField.interactable = false;
        consumeStoreButton.interactable = false;
        // Calculate number of rations
        int _rations = 0;
        foreach(Family f in families){
            _rations += f.numberOfFamilyMembers;
        }
        _rations += 225;
        government.availableRations = _rations;
    }
	
	// Update is called once per frame
	void Update () {
        UpdateUI();
	}

    private void CreateGovernment()
    {

    }

    private void CreateFamilies(int _numberOfFamilies)
    {
        for (int i = 0; i < government.numberOfFamilies; i++)
        {
            // Family gameobjects
            GameObject _newFamily = new GameObject();
            _newFamily.transform.parent = GameObject.Find("_Families").GetComponent<Transform>();
            // Add family info
            Family _familyInfo =_newFamily.AddComponent<Family>();
            _familyInfo.familyName = CreateRandomString();
            // Add number of family members
            _familyInfo.numberOfFamilyMembers = random.Next(2,7);
            if(_familyInfo.numberOfFamilyMembers % 2 != 0)
            {
                _familyInfo.numberOfFamilyMembers -= 1;
            }
            // Set needed rations per day
            if(_familyInfo.numberOfFamilyMembers >= government.allowedRationsPerFamily)
            {
                _familyInfo.neededRations = government.allowedRationsPerFamily;
            }
            else
            {
                _familyInfo.neededRations = _familyInfo.numberOfFamilyMembers;
            }
            // Add fmaily to list
            families.Add(_familyInfo);
            _newFamily.transform.name = "Family_" + _familyInfo.familyName;
        }
    }

    private void AssignPlayerFamily()
    {
        playerAssignedFamily = families[random.Next(families.Count)];
    }

    private string CreateRandomString()
    {
        string _availableCharacters = "abcdefghijklmnopqrstuvwxyz1234567890";
        char[] _randomName = new char[8];
        for(int i = 0; i < _randomName.Length; i++)
        {
            _randomName[i] = _availableCharacters[random.Next(_availableCharacters.Length)];
        }
        return new string(_randomName);
    }

    private void SubscribeButtonsToEvents()
    {
        takeButton.onClick.AddListener(PlayerTakeRations);
        consumeStoreButton.onClick.AddListener(PlayerConsumeStoreRations);
        consumeFromStoreButton.onClick.AddListener(AddFromStore);
    }

    private void AddFromStore()
    {
        if(consumeFromStoreInputField.text != string.Empty)
        {
            int _addFromStore = Convert.ToInt32(consumeFromStoreInputField.text);
            if(_addFromStore > 0 && _addFromStore <= playerAssignedFamily.storedRations)
            {
                playerAssignedFamily.currentRations += _addFromStore;
                playerAssignedFamily.storedRations -= _addFromStore;
            }
            else
            {
                InstantiateNotification("You do not have that much in your store!");
            }
            // Set consume from store input to empty
            consumeFromStoreInputField.text = string.Empty;
        }
        else
        {
            InstantiateNotification("If you want to consume from store, you need to add a number you want to take!");
        }
    }

    private void PlayerTakeRations()
    {
        if(playerAssignedFamily.overallFamilyHealth <= 0){
            if(playerAssignedFamily.yearOfDeath != 0){
                playerAssignedFamily.yearOfDeath = year;
            }
            InstantiateNotification("You are dead. Restart the game.");
        }
        else{
            int _availableRations = government.availableRations;
            if(takeInputField.text != string.Empty || takeInputField.text != "")
            {
                int _take = Convert.ToInt32(takeInputField.text);
                if(_take > _availableRations)
                {

                    playerAssignedFamily.currentRations = 0;
                    playerAssignedFamily.consumeRations = 0;
                    playerAssignedFamily.storedRations = 0;
                    playerAssignedFamily.CalculateRationsPerMember();
                    if(playerAssignedFamily.overallFamilyHealth < 0.5f){
                        government.numberOfFamilies++;
                    }
                    InstantiateNotification(playerAssignedFamily.familyName + "You have tried to take everything! You will be punished with no rations!");
                }
                else
                {
                    if(_take > government.allowedRationsPerFamily || _take > playerAssignedFamily.numberOfFamilyMembers)
                    {
                        int _stolenRations = 0; 
                        if(_take > government.allowedRationsPerFamily)
                        {
                            _stolenRations = _take - government.allowedRationsPerFamily;
                            InstantiateNotification("Take bigger than government allowed");
                        } 
                        else if(_take > playerAssignedFamily.numberOfFamilyMembers)
                        {
                            _stolenRations = _take - playerAssignedFamily.numberOfFamilyMembers;
                            //InstantiateNotification("Take bigger than number of members");
                        }
                        playerAssignedFamily.rationsStoleOverTime = _stolenRations;
                        government.stolenRations += _stolenRations;
                        InstantiateNotification("You've stolen " + _stolenRations + " ration(s)!");
                    }
                    playerAssignedFamily.currentRations = _take;
                    playerAssignedFamily.totalRationsThisYear += _take;
                    playerAssignedFamily.totalRationsAllTime += _take;
                    // Decress from government
                    government.availableRations -= _take;
                    // Set take input to empty
                    takeInputField.text = string.Empty;
                    // Change ration interaction state
                    rationsInteractionState = RationsInteractionState.CONSUMESTORE;
                }
            }
            else
            {
                InstantiateNotification("You need to take your rations!");
            }
        }
    }

    private void PlayerConsumeStoreRations()
    {
        int _currentRations = playerAssignedFamily.currentRations;
        if(consumeInputField.text == string.Empty && storeInputField.text == string.Empty)
        {
            InstantiateNotification("You need to enter your 'Consume' and 'Store' rations!");
        }
        else if((consumeInputField.text != string.Empty && storeInputField.text == string.Empty) || (consumeInputField.text == string.Empty && storeInputField.text != string.Empty))
        {
            InstantiateNotification("You need to enter a number for both consume and store!");
        }
        else
        {
            int _consumeAmount = Convert.ToInt32(consumeInputField.text);
            int _storeAmount = Convert.ToInt32(storeInputField.text);
            int _totalAmount = _consumeAmount + _storeAmount;
            if((_totalAmount > _currentRations) || (_totalAmount < _currentRations))
            {
                InstantiateNotification("Your numbers don't add up to you current take!");
            }
            else
            {
                playerAssignedFamily.currentRations = 0;
                playerAssignedFamily.consumeRations = _consumeAmount;
                playerAssignedFamily.storedRations += _storeAmount;
                // Calculate rations per member
                playerAssignedFamily.CalculateRationsPerMember();
                // TODO: Write consume method in playerAssignedFamily
                // Set to null strings
                consumeInputField.text = string.Empty;
                storeInputField.text = string.Empty;
                // Change ration interaction state
                rationsInteractionState = RationsInteractionState.IDLE;
                // Tell computer to choose for rest of families
                ComputerTakeConsumeStoreRations();
            }
        }
    }

    private void ComputerTakeConsumeStoreRations()
    {
        // Dead families
        int _deadFamilies = 0;
        // Starving Families
        int _starvingFamilies = 0;
        // Get each family in families
        foreach(Family _family in families){
            if(!_family.dead && _family){
                if(_family != playerAssignedFamily){
                    // Choose take
                    int _take = 0;
                    if(_family.numberOfFamilyMembers <= government.allowedRationsPerFamily){
                        _take = _family.numberOfFamilyMembers;
                    }
                    else if(_family.numberOfFamilyMembers > government.allowedRationsPerFamily){
                        _take = government.allowedRationsPerFamily;
                    }
                    // Get take and stolen rations from government
                        if(government.stolenRations > 4){
                            _take += random.Next(0, _family.numberOfFamilyMembers - 2);
                        }
                    // See if take is allowed
                    if(_take < government.availableRations){
                        _family.currentRations = _take;
                        // Decide what to consume and what to store
                        if(_family.currentRations == _family.numberOfFamilyMembers || _family.currentRations < _family.numberOfFamilyMembers){
                            // Consume rations
                            _family.consumeRations = _family.currentRations;
                            _family.CalculateRationsPerMember();
                        }
                        else if(_family.currentRations > _family.numberOfFamilyMembers){
                            _family.consumeRations = _family.currentRations - _family.numberOfFamilyMembers;
                            _family.storedRations = _family.currentRations - _family.consumeRations;
                            _family.CalculateRationsPerMember();
                        }
                        government.availableRations -= _take;
                    } 
                    else if(_take / 2 < government.availableRations){
                        _family.currentRations = _take;
                        // Decide what to consume and what to store
                        if(_family.currentRations == _family.numberOfFamilyMembers || _family.currentRations < _family.numberOfFamilyMembers){
                            // Consume rations
                            _family.consumeRations = _family.currentRations;
                            _family.CalculateRationsPerMember();
                        }
                        else if(_family.currentRations > _family.numberOfFamilyMembers){
                            _family.consumeRations = _family.currentRations - _family.numberOfFamilyMembers;
                            _family.storedRations = _family.currentRations - _family.consumeRations;
                            _family.CalculateRationsPerMember();
                        }
                        government.availableRations -= _take;
                    }
                    else if(_take > government.availableRations){
                        _family.currentRations = 0;
                        _family.consumeRations = 0;
                        _family.storedRations = 0;
                        _family.CalculateRationsPerMember();
                        if(_family.overallFamilyHealth < 0.5f){
                            _starvingFamilies++;
                        }
                        InstantiateNotification(_family.familyName + " has tried to take everything! They will be punished with no rations!");
                    }
                } 
            }
            else{
                _deadFamilies++;
            }
        }
        // Tell government Starving families
        government.numberOfStarvingFamilies = _starvingFamilies;
        // Tell government there is a dead family
        government.numberOfDeadFamilies = _deadFamilies;
        InstantiateNotification("The day is over. Please choose your next days rations...");
        // End computer turn and set time of year and replinish rations if needed
        SetTimeOfGame();
        rationsInteractionState = RationsInteractionState.TAKE;
    }

    private void SetTimeOfGame (){
        int _day = this.day;
        int _year = this.year;
        
        if(_day < this.daysInYear){
            day++;
        }
        else if(_day == daysInYear && _year < this.lengthOfFamine){
            year++;
            day = 1;
            // Tell player how much theyve taken
            InstantiateNotification("You have taken " + playerAssignedFamily.totalRationsThisYear + " this year and " + playerAssignedFamily.totalRationsAllTime + " since the start of the famine!");
            // Set player assigned rations for year to 0
            playerAssignedFamily.totalRationsThisYear = 0;
            // Replinish government
            float _replinish = government.startAvailableRations * ((float)random.Next(rationReplinishPercentMin, rationsReplinishPercentMax) / 100);
            government.availableRations += Convert.ToInt32(_replinish);
            InstantiateNotification("Government was replinished with " + Convert.ToInt32(_replinish) + " rations!");
        }
        else if (_day == daysInYear && _year >= this.lengthOfFamine){
            rationsInteractionState = RationsInteractionState.OVER;
            if(playerAssignedFamily.yearOfDeath > 0){
                InstantiateNotification("The famine is over. You died in year " + playerAssignedFamily.yearOfDeath);
            }
            else{
                InstantiateNotification("You survived the famine along with " + (government.numberOfFamilies - government.numberOfDeadFamilies) + " other families!");
            }
        }
    }

    private void UpdateUI()
    {
        switch (rationsInteractionState)
        {
            case RationsInteractionState.IDLE:
                // Set take UI interactable false
                takeInputField.interactable = false;
                takeButton.interactable = false;
                // Set consume/store interactable true
                consumeInputField.interactable = false;
                storeInputField.interactable = false;
                consumeStoreButton.interactable = false;
                consumeFromStoreInputField.interactable = false;
                consumeFromStoreButton.interactable = false;
                break;
            case RationsInteractionState.TAKE:
                // Set take UI interactable false
                takeInputField.interactable = true;
                takeButton.interactable = true;
                // Set consume/store interactable true
                consumeInputField.interactable = false;
                storeInputField.interactable = false;
                consumeStoreButton.interactable = false;
                consumeFromStoreInputField.interactable = false;
                consumeFromStoreButton.interactable = false;
                break;
            case RationsInteractionState.CONSUMESTORE:
                // Set take UI interactable false
                takeInputField.interactable = false;
                takeButton.interactable = false;
                // Set consume/store interactable true
                consumeInputField.interactable = true;
                storeInputField.interactable = true;
                consumeStoreButton.interactable = true;
                if(playerAssignedFamily.storedRations > 0)
                {
                    consumeFromStoreInputField.interactable = true;
                    consumeFromStoreButton.interactable = true;
                }
                else
                {
                    consumeFromStoreInputField.interactable = false;
                    consumeFromStoreButton.interactable = false;
                }
                break;
            case RationsInteractionState.OVER:
                // Set take UI interactable false
                takeInputField.interactable = false;
                takeButton.interactable = false;
                // Set consume/store interactable true
                consumeInputField.interactable = true;
                storeInputField.interactable = true;
                consumeStoreButton.interactable = true;
                consumeFromStoreInputField.interactable = false;
                consumeFromStoreButton.interactable = false;
            break;
        }
        // Family title
        familyTitleLabel.text = "Family: " + playerAssignedFamily.familyName;
        // Family info
        currentRationsLabel.text = "Current Rations: " + playerAssignedFamily.currentRations.ToString();
        familyHealthLabel.text = "Family Health: " + playerAssignedFamily.overallFamilyHealth.ToString();
        familyMembersLabel.text = "Family Members: " + playerAssignedFamily.numberOfFamilyMembers.ToString();
        neededRationsLabel.text = "Needed Rations: " + "(Legal Max)" + playerAssignedFamily.neededRations.ToString();
        rationsPerMemberLabel.text = "Rations Per Member: " + playerAssignedFamily.rationsPerMember.ToString();
        stolenRationLabel.text = "Stolen Rations: " + playerAssignedFamily.rationsStoleOverTime.ToString();
        storedRationsLabel.text = "Stored Rations: " + playerAssignedFamily.storedRations.ToString();

        // Government info
        deadFamiliesLabel.text = "Dead Families: " + government.numberOfDeadFamilies.ToString();
        familiesAtStartLabel.text = "Families At Start: " + government.numberOfFamilies.ToString();
        starvingFamiliesLabel.text = "Starving Families: " + government.numberOfStarvingFamilies.ToString();
        stolenRationsTotalLabel.text = "Reported Stolen Rations: " + government.stolenRations.ToString();
        yearLabel.text = "Year: " + year.ToString();
        dayLabel.text = "Day: " + day.ToString();
    }

    public void InstantiateNotification(string _text){
        GameObject go = Instantiate(notificationPrefab,notificationParent.transform);
        go.GetComponent<Notification>().SetText(_text);
    }
}

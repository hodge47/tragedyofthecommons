using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour {

    private enum RationsInteractionState { IDLE, TAKE, CONSUMESTORE}
    private RationsInteractionState rationsInteractionState = RationsInteractionState.IDLE;

    [FoldoutGroup("Game Manager")]
    public int lengthOfFamine = 10; // Years
    [FoldoutGroup("Game Manager")]
    public int daysInYear = 3; // There are x number of days in a year

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
                Debug.LogError("You do not have that much in your store!");
            }
            // Set consume from store input to empty
            consumeFromStoreInputField.text = string.Empty;
        }
        else
        {
            Debug.LogError("If you want to consume from store, you need to add a number you want to take!");
        }
    }

    private void PlayerTakeRations()
    {
        int _availableRations = government.availableRations;
        if(takeInputField.text != string.Empty || takeInputField.text != "")
        {
            int _take = Convert.ToInt32(takeInputField.text);
            if(_take > _availableRations)
            {
                takeInputField.text = string.Empty;
                Debug.LogError("You cannot take more than the supply!");
            }
            else
            {
                if(_take > government.allowedRationsPerFamily || _take > playerAssignedFamily.numberOfFamilyMembers)
                {
                    int _stolenRations = 0; 
                    if(_take > government.allowedRationsPerFamily)
                    {
                        _stolenRations = _take - government.allowedRationsPerFamily;
                    } else if(_take > playerAssignedFamily.numberOfFamilyMembers)
                    {
                        _stolenRations = _take - playerAssignedFamily.numberOfFamilyMembers;
                    }
                    playerAssignedFamily.rationsStoleOverTime = _stolenRations;
                    government.stolenRations += _stolenRations;
                    Debug.Log("You've stolen " + _stolenRations + " ration(s)!");
                }
                playerAssignedFamily.currentRations = _take;
                // Set take input to empty
                takeInputField.text = string.Empty;
                // Change ration interaction state
                rationsInteractionState = RationsInteractionState.CONSUMESTORE;
            }
        }
        else
        {
            Debug.LogError("You need to take your rations!");
        }
    }

    private void PlayerConsumeStoreRations()
    {
        int _currentRations = playerAssignedFamily.currentRations;
        if(consumeInputField.text == string.Empty && storeInputField.text == string.Empty)
        {
            Debug.LogError("You need to enter your 'Consume' and 'Store' rations!");
        }
        else if((consumeInputField.text != string.Empty && storeInputField.text == string.Empty) || (consumeInputField.text == string.Empty && storeInputField.text != string.Empty))
        {
            Debug.LogError("You need to enter a number for both consume and store!");
        }
        else
        {
            int _consumeAmount = Convert.ToInt32(consumeInputField.text);
            int _storeAmount = Convert.ToInt32(storeInputField.text);
            int _totalAmount = _consumeAmount + _storeAmount;
            if((_totalAmount > _currentRations) || (_totalAmount < _currentRations))
            {
                Debug.LogError("Your numbers don't add up to you current take!");
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
                ComputerTakeRations();
            }
        }
    }

    private void ComputerTakeRations()
    {
        Debug.Log("Computer has chosen...");
        rationsInteractionState = RationsInteractionState.TAKE;
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
    }
}

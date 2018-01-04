using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using UnityEngine.UI;

public class SceneManager_Game : Photon.MonoBehaviour
{
    private RecordingManager _recordingManager;
    private RandomizeWords _randomizeManager;

    private GameObject _board1;
    private GameObject _board2;
    private GameObject _rope1;
    private GameObject _rope2;
    private GameObject[] _avatars;
    

    private string[] _wordList;
    private string[] _distanceList;   
    private int _wordID;
    private int _distanceID;   

    public bool HasStarted = false;  
    public float CountDownTime = 7f;   // Using count down 3min, 180s
    private float currentCountDown;

    // UI section
    private Text _Timer;
    [HideInInspector]
    public GameObject[] InGameRating;
    [HideInInspector]
    public GameObject[] OnScreenRating;
    private string[] _ratingList;

    void Start () {
        _recordingManager = GetComponent<RecordingManager>();

        _Timer = GameObject.Find("CountDownTimer").GetComponent<Text>();
        _Timer.gameObject.SetActive(false);

        _board1 = GameObject.FindGameObjectWithTag("Board1");
        _board2 = GameObject.FindGameObjectWithTag("Board2");

        _rope1 = GameObject.FindGameObjectWithTag("Rope1");
        _rope2 = GameObject.FindGameObjectWithTag("Rope2");

        _avatars = GameObject.FindGameObjectsWithTag("Avatar");
        

        _randomizeManager = GameObject.Find("RandomizeManager").GetComponent<RandomizeWords>();

        OnScreenRating = GameObject.FindGameObjectsWithTag("RatingOnScreen");
        InGameRating = GameObject.FindGameObjectsWithTag("RatingInGame");
        _ratingList = _randomizeManager.RatingList;

        OnScreenRating[0].transform.parent.parent.gameObject.SetActive(false);
        InGameRating[0].transform.parent.parent.gameObject.SetActive(false);

        // Only runs on master client
        // if (PhotonNetwork.isMasterClient)
        {
	        if (_randomizeManager != null)
	        {
                // Get word list and distance list
                _wordList = new string[_randomizeManager.WordList.Length];
                _distanceList = new string[_randomizeManager.DistanceList.Length];

	            _randomizeManager.WordList.CopyTo(_wordList, 0);
	            _randomizeManager.DistanceList.CopyTo(_distanceList, 0);
                
                // Randomize the listf
                _randomizeManager.RandomizeTexts(_wordList);
                _randomizeManager.RandomizeTexts(_distanceList);
	        }          

	        _wordID = 0;
            _distanceID = 0;

            AssignDistancesOnRopes();
	    }

        currentCountDown = CountDownTime;

    }

    void Update()
    {
        // Use "Space" key to control for next word, players take turn to guess word
        // Use "R" key to start next group of words after change rope distance        

        if (HasStarted)
        {
            currentCountDown = currentCountDown - Time.deltaTime;
            _Timer.gameObject.SetActive(true);
            _Timer.text = currentCountDown.ToString("0.00");

            //if (PhotonNetwork.isMasterClient)
            if (currentCountDown <= 0)
            {
                StopGame();
                AssignDistancesOnRopes();
                ShowRatingUI();
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_wordID < _wordList.Length)
                {
                    AssignWordsOnBoards();
                    _wordID++;
                }
                else
                {
                    Debug.LogWarning("Run out of word list! Resetting wordID to 0.");
                    _wordID = 0;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {               
                StartGame();
            }
            else if(_avatars!=null)
            {
                if (_avatars.Length != 2)
                {
                    Debug.Log("Waiting for more avatars to join");
                    // TODO: change avatar to HMD avatar
                    _avatars = GameObject.FindGameObjectsWithTag("Avatar");
                }
                else
                {
                    foreach (GameObject avatar in _avatars)
                    {
                        if (avatar.transform.position.x > 0)
                        {
                            var distToRope = (avatar.transform.position.x - _rope2.transform.position.x).ToString("0.00");
                            _board1.GetComponent<Text>().text = "Please adjust your position to the red rope. Your distance to the rope is: " + distToRope + "m";
                        }
                        else
                        {
                            var distToRope = (avatar.transform.position.x - _rope1.transform.position.x).ToString("0.00");
                            _board2.GetComponent<Text>().text = "Please adjust your position to the red rope. Your distance to the rope is: " + distToRope + "m";
                        }
                    }
                }
            }               
        }
    }

    // Call on start experiment game "20 Questions"
    public void StartGame()
    {
        // TODO: uncomment photonview line and recording manager line
        //photonView.RPC("RPC_StartGame", PhotonTargets.All);
        RPC_StartGame();
       // _recordingManager.StartRecording();

        Debug.Log("20 Questions game Start!");
    }

    public void StopGame()
    {
        // TODO: uncomment this line
        //photonView.RPC("RPC_StopGame", PhotonTargets.All);
        RPC_StopGame();

        //_recordingManager.StopRecording();
        Debug.Log("20 Questions game Stop!");
    }

    [PunRPC]
    void RPC_StartGame()
    {
        FindObjectOfType<SceneManager_Game>().HasStarted = true;
        FindObjectOfType<SceneManager_Game>().currentCountDown = FindObjectOfType<SceneManager_Game>().CountDownTime;
        GameObject.Find("StartButton").GetComponent<Button>().interactable = false;
    }

    [PunRPC]
    void RPC_StopGame()
    {
        FindObjectOfType<SceneManager_Game>().HasStarted = false;
        GameObject.Find("StartButton").GetComponent<Button>().interactable = true;
    }

    // Call on space key press during "20 Questions"
    public void AssignWordsOnBoards()
    {
        RPC_AssignWordsOnBoards(_wordID, _wordList[_wordID]);
        //photonView.RPC("RPC_AssignWordsOnBoards",PhotonTargets.All,_wordID,_wordList[_wordID]);
    }

    public void AssignDistancesOnRopes()
    { 
        
        if (_distanceID < _distanceList.Length)
        {
            float distance = float.Parse(_distanceList[_distanceID]) / 2f;
            RPC_AssignDistancesOnRopes(distance);
            //photonView.RPC("RPC_AssignDistancesOnRopes", PhotonTargets.All);
            _distanceID++;
        }else
        {
            Debug.LogWarning("Distance run out of list. Please start next session!");
        }       
    }

    [PunRPC]
    void RPC_AssignWordsOnBoards(int wordID, string word)
    {
        Debug.Log("wordID:" + wordID);
        if (wordID%2 == 0)
        {
            _board1.GetComponent<Text>().text = word;
            _board2.GetComponent<Text>().text = "It's your turn to guess.";
        }
        else
        {         
            _board1.GetComponent<Text>().text = "It's your turn to guess.";
            _board2.GetComponent<Text>().text = word;
        }
    }

    [PunRPC]
    void RPC_AssignDistancesOnRopes(float dist)
    {
        Debug.Log("Distance between players is: " + 2*dist);
      
        _rope1.transform.position = new Vector3(-dist, 1f, 0);
        _rope2.transform.position = new Vector3(dist, 1f, 0);      
    }

    // TODO: add rpc, since the event is triggered by masterclient, only showed on avatar screen
    void ShowRatingUI()
    {
        _randomizeManager.RandomizeTexts(_ratingList);
        for(int i=0; i<3; i++)
        {
            InGameRating[i].GetComponent<Text>().text = _ratingList[i];
        }

        foreach (GameObject rating in OnScreenRating) {
            rating.GetComponent<SyncRatingOnScreen>().SyncOption();
        }

        OnScreenRating[0].transform.parent.parent.gameObject.SetActive(true);
        InGameRating[0].transform.parent.parent.gameObject.SetActive(true);
    }

    

}

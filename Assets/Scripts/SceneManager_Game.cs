using System;
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
    private GameObject[] _avatarHeads;
    private GameObject[] _inGameRatingUI;
    private GameObject _screenUI;

    private Dictionary<string, string> _categoryWordDict;
    private string[] _wordList;
    private string[] _distanceList;
    private int _wordID;
    private int _distanceID;

    public float CurrentDistance;
    public bool HasStarted = false;  
    public float CountDownTime = 7f;   // Using count down 3min, 180s
    private float currentCountDown;

    public KeyCode ChangeWordKey = KeyCode.Space;
    public KeyCode ReStartKey = KeyCode.R;
    public KeyCode ChangeRatingQuestionKey = KeyCode.Q;

    // UI section
    private Text _Timer;

    void Start () {
        _recordingManager = GameManager.Instance.gameObject.GetComponent<RecordingManager>();

        _Timer = GameObject.Find("CountDownTimer").GetComponent<Text>();
        _Timer.gameObject.SetActive(false);

        _board1 = GameObject.FindGameObjectWithTag("Board1");
        _board2 = GameObject.FindGameObjectWithTag("Board2");

        _rope1 = GameObject.FindGameObjectWithTag("Rope1");
        _rope2 = GameObject.FindGameObjectWithTag("Rope2");

        if (GameManager.Instance.UsingVR)
        {
            _avatarHeads = GameObject.FindGameObjectsWithTag("HMDHead");
        }
        else
        {
            _avatarHeads = GameObject.FindGameObjectsWithTag("OptitrackHead");
        }
        
        _randomizeManager = GetComponent<RandomizeWords>();

        _inGameRatingUI = new GameObject[2];
        _inGameRatingUI[0] = GameObject.Find("RatingListInGame1");
        _inGameRatingUI[1] = GameObject.Find("RatingListInGame2");

        _inGameRatingUI[0].SetActive(false);
        _inGameRatingUI[1].SetActive(false);

        // Only runs on master client
        if (PhotonNetwork.isMasterClient)
        {
	        if (_randomizeManager != null)
	        {
                // Get word list and distance list
                _wordList = new string[_randomizeManager.CategoryWordDict.Count];
                _distanceList = new string[_randomizeManager.DistanceList.Length];
                _categoryWordDict = new Dictionary<string, string>();

                _randomizeManager.WordList.CopyTo(_wordList,0);
	            _randomizeManager.DistanceList.CopyTo(_distanceList, 0);
                
                // Randomize the listf
                _randomizeManager.RandomizeTexts(_wordList);
                _randomizeManager.RandomizeTexts(_distanceList);

	            _categoryWordDict = _randomizeManager.CategoryWordDict;

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
        // Use "alpha 1" key to switch to next time    

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
                // TODO: change to photonview.rpc call
                RPC_ShowRatingUI(_inGameRatingUI,0);
            }
            else if (Input.GetKeyDown(ChangeWordKey))
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
            if (Input.GetKeyDown(ReStartKey))
            {       
                // restart game        
                StartGame();
            } else if (Input.GetKeyDown(ChangeRatingQuestionKey))
            {
                //TODO: change to photonview.rpc call
                RPC_ShowRatingUI(_inGameRatingUI,1);
                //photonView.RPC("RPC_ShowRatingUI",PhotonTargets.All,_inGameRatingUI,1);
            }
            else if (_avatarHeads != null)
            {
                if (_avatarHeads.Length != 2)
                {
                    Debug.Log("Waiting for more avatars to join");
                    // TODO: change avatar to HMD avatar
                    _avatarHeads = GameObject.FindGameObjectsWithTag("Avatar");
                }
                else
                {
                    foreach (GameObject avatar in _avatarHeads)
                    {
                        if (avatar.transform.position.x > 0)
                        {
                            var distToRope = (avatar.transform.position.x - _rope2.transform.position.x).ToString("0.00");
                            _board1.GetComponent<Text>().text =
                                "Please adjust your position to the red rope. Your distance to the rope is: " +
                                distToRope + "m";
                        }
                        else
                        {
                            var distToRope = (avatar.transform.position.x - _rope1.transform.position.x).ToString("0.00");
                            _board2.GetComponent<Text>().text =
                                "Please adjust your position to the red rope. Your distance to the rope is: " +
                                distToRope + "m";
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
        RPC_StartGame(_inGameRatingUI);
        _recordingManager.StartRecording();

        Debug.Log("20 Questions game Start!");
    }

    public void StopGame()
    {
        // TODO: uncomment this line
        //photonView.RPC("RPC_StopGame", PhotonTargets.All);
        RPC_StopGame();

        _recordingManager.StopRecording();
        Debug.Log("20 Questions game Stop!");
    }

    [PunRPC]
    void RPC_StartGame(GameObject[] ratingUI)
    {
        FindObjectOfType<SceneManager_Game>().HasStarted = true;
        FindObjectOfType<SceneManager_Game>().currentCountDown = FindObjectOfType<SceneManager_Game>().CountDownTime;
        GameObject.Find("StartButton").GetComponent<Button>().interactable = false;

        ratingUI[0].SetActive(false);
        ratingUI[1].SetActive(false);
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
            float distance = float.Parse(_distanceList[_distanceID]);
            CurrentDistance = distance;
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
            _board1.GetComponent<Text>().text = _categoryWordDict[word] + ":" + word;
            _board2.GetComponent<Text>().text = "It's your turn to guess.";
        }
        else
        {         
            _board1.GetComponent<Text>().text = "It's your turn to guess.";
            _board2.GetComponent<Text>().text = _categoryWordDict[word] + ":" + word;
        }
    }

    [PunRPC]
    void RPC_AssignDistancesOnRopes(float dist)
    {
        Debug.Log("Distance between players is: " + dist);
      
        _rope1.transform.position = new Vector3(-dist/2f, 1f, 0);
        _rope2.transform.position = new Vector3(dist/2f, 1f, 0);      
    }

    [PunRPC]
    void RPC_ShowRatingUI(GameObject[] ratingUI, int index)
    {
        ratingUI[index].gameObject.SetActive(true);
        ratingUI[1-index].gameObject.SetActive(false);
    }

}

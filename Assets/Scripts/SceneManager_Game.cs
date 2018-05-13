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
    private List<GameObject> _avatarHeads;
    private GameObject[] _avatars;
    private GameObject[] _inGameRatingUI;
    private GameObject _screenUI;

    private Dictionary<string, string> _categoryWordDict;
    private string[] _wordList;
    private string[] _distanceList;
    private int _wordID;
    private int _distanceID;

    public Text DistanceText;
    public float CurrentDistance;
    public bool HasStarted = false;  
    public float CountDownTime = 7f;   // Using count down 3min, 180s
    private float currentCountDown;

    public KeyCode ChangeWordKey = KeyCode.Space;
    public KeyCode ReStartKey = KeyCode.R;
    public KeyCode ChangeRatingQuestionKey = KeyCode.Q;
    public KeyCode LookForAvatarsKey = KeyCode.L;
    public KeyCode ToggleInstructionKey = KeyCode.T;

    private bool _hasSetAvatars;
    public bool Has2Avatars;

    // UI section
    private Text _Timer;
    private WorldTimer _worldTimer;

    void Start () {
        _worldTimer = GameManager.Instance.gameObject.GetComponent<WorldTimer>();
        _recordingManager = GameManager.Instance.gameObject.GetComponent<RecordingManager>();
        _avatarHeads = new List<GameObject>();
        _avatars = new GameObject[2];

        _Timer = GameObject.Find("CountDownTimer").GetComponent<Text>();
        _Timer.gameObject.SetActive(false);

        _board1 = GameObject.FindGameObjectWithTag("Board1");
        _board2 = GameObject.FindGameObjectWithTag("Board2");

        _rope1 = GameObject.FindGameObjectWithTag("Rope1");
        _rope2 = GameObject.FindGameObjectWithTag("Rope2");

        //StartCoroutine(LookingForAvatars());
        
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

        //currentCountDown = CountDownTime;

    }

    void Update()
    {
        if(Input.GetKeyDown(ToggleInstructionKey))
            ToggleInstructionBoards();

        // Use "Space" key to control for next word, players take turn to guess word
        // Use "R" key to start next group of words after change rope distance    

        if (HasStarted)
        {
            currentCountDown = (float)_worldTimer.ElapsedTimeSinceStart.TotalSeconds;
            _Timer.gameObject.SetActive(true);
            _Timer.text = currentCountDown.ToString("0.000");

            if (PhotonNetwork.isMasterClient) {
                if (currentCountDown >= CountDownTime)
                {
                    StopGame();
                    AssignDistancesOnRopes();
                    photonView.RPC("RPC_ShowRatingUI", PhotonTargets.All, 0);
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
        }
        else
        {
            if (Input.GetKeyDown(ReStartKey))
            {       
                // restart game        
                StartGame();
            } else if (Input.GetKeyDown(ChangeRatingQuestionKey))
            {
                photonView.RPC("RPC_ShowRatingUI",PhotonTargets.All,1);
            }
            else
            {
                if (Has2Avatars && !_hasSetAvatars) {
                    _avatars = GameObject.FindGameObjectsWithTag("Avatar");
                    _avatarHeads = new List<GameObject> { null, null };
                    if (GameManager.Instance.UsingVR) {
                        _avatarHeads[0] = _avatars[0].GetComponentInChildren<SetHeadPos>().gameObject;
                        _avatarHeads[1] = _avatars[1].GetComponentInChildren<SetHeadPos>().gameObject;                  
                    } else {
                        _avatarHeads[0] = _avatars[0].GetComponent<PlayerManager>().OptitrackHead.gameObject;
                        _avatarHeads[1] = _avatars[1].GetComponent<PlayerManager>().OptitrackHead.gameObject;
                    }
                        
                    _hasSetAvatars = true;
                }

                if (_avatarHeads.Count!=2)
                    return;

                var dist1 = _avatarHeads[0].transform.position.x - _rope1.transform.position.x;
                var dist2 = _avatarHeads[1].transform.position.x - _rope1.transform.position.x;

                if (dist1 < dist2)
                {
                    var distToRope1Str = dist1.ToString("0.00");
                    _board2.GetComponent<Text>().text =
                        "Please adjust your position to the red rope. Your distance to the rope is: " +
                        distToRope1Str + "m";

                    var distToRope2Str = (_avatarHeads[1].transform.position.x - _rope2.transform.position.x).ToString("0.00");
                    _board1.GetComponent<Text>().text =
                        "Please adjust your position to the red rope. Your distance to the rope is: " +
                        distToRope2Str + "m";
                }
                else {
                    var distToRope1Str = dist2.ToString("0.00");
                    _board2.GetComponent<Text>().text =
                        "Please adjust your position to the red rope. Your distance to the rope is: " +
                        distToRope1Str + "m";

                    var distToRope2Str = (_avatarHeads[0].transform.position.x - _rope2.transform.position.x).ToString("0.00");
                    _board1.GetComponent<Text>().text =
                        "Please adjust your position to the red rope. Your distance to the rope is: " +
                        distToRope2Str + "m";
                }

                    
                
            }
        }

        if (Input.GetKeyDown(LookForAvatarsKey))
        {
            if(_avatarHeads!=null)
                _avatarHeads.Clear();

            StartCoroutine(LookingForAvatars());
        }
            
    }

    // Call on start experiment game "20 Questions"
    public void StartGame()
    {
        photonView.RPC("RPC_StartGame", PhotonTargets.All);
        photonView.RPC("RPC_StartRecording",PhotonTargets.All);

        
    }

    public void StopGame()
    {
        photonView.RPC("RPC_StopGame", PhotonTargets.All);
        photonView.RPC("RPC_StopRecording", PhotonTargets.All);

       
    }

    [PunRPC]
    void RPC_StartGame()
    {
        FindObjectOfType<SceneManager_Game>().HasStarted = true;
        //FindObjectOfType<SceneManager_Game>().currentCountDown = FindObjectOfType<SceneManager_Game>().CountDownTime;
        GameObject.Find("StartButton").GetComponent<Button>().interactable = false;

        _inGameRatingUI[0].SetActive(false);
        _inGameRatingUI[1].SetActive(false);

        _worldTimer.StartTimer();

        Debug.Log("20 Questions game Start!");
    }

    [PunRPC]
    void RPC_StopGame()
    {
        FindObjectOfType<SceneManager_Game>().HasStarted = false;
        GameObject.Find("StartButton").GetComponent<Button>().interactable = true;
        _worldTimer.StopTimer();
        _worldTimer.ResetTimer();

        Debug.Log("20 Questions game Stop!");
    }

    [PunRPC]
    void RPC_StartRecording()
    {
        Debug.Log("Send start recording RPC.");
        var recordingManager = FindObjectOfType<RecordingManager>();
        recordingManager.StartRecording();
    }

    [PunRPC]
    void RPC_StopRecording()
    {
        Debug.Log("Send stop recording RPC.");
        var recordingManager = FindObjectOfType<RecordingManager>();
        recordingManager.StopRecording();
    }

    public void ToggleInstructionBoards()
    {
        photonView.RPC("RPC_ToggleInstructionBoard",PhotonTargets.All);
    }

    [PunRPC]
    void RPC_ToggleInstructionBoard()
    {
        var boardObj1 = _board1.transform.parent.parent.gameObject;
        var boardObj2 = _board2.transform.parent.parent.gameObject;
        boardObj1.SetActive(!boardObj1.activeSelf);
        boardObj2.SetActive(boardObj1.activeSelf);
    }

    // Call on space key press during "20 Questions"
    public void AssignWordsOnBoards()
    {
        //RPC_AssignWordsOnBoards(_wordID, _wordList[_wordID]);
        photonView.RPC("RPC_AssignWordsOnBoards",PhotonTargets.All,_wordID,_wordList[_wordID],_categoryWordDict[_wordList[_wordID]]);
    }

    public void AssignDistancesOnRopes()
    {       
        if (_distanceID < _distanceList.Length)
        {
            float distance = float.Parse(_distanceList[_distanceID]);
            CurrentDistance = distance;
            DistanceText.text = "Current distance is: " + distance;
            //RPC_AssignDistancesOnRopes(distance);
            photonView.RPC("RPC_AssignDistancesOnRopes", PhotonTargets.AllBuffered,distance);
            _distanceID++;
        }else
        {
            Debug.LogWarning("Distance run out of list. Please start next session!");
            DistanceText.text = "Distance run out of list. Please start next session!";
        }       
    }

    [PunRPC]
    void RPC_AssignWordsOnBoards(int wordID, string word,string category)
    {
        Debug.Log("wordID:" + wordID);
        if (wordID%2 == 0)
        {
            _board1.GetComponent<Text>().text = category + ":" + word;
            _board2.GetComponent<Text>().text = "It's your turn to guess.";
        }
        else
        {         
            _board1.GetComponent<Text>().text = "It's your turn to guess.";
            _board2.GetComponent<Text>().text = category + ":" + word;
        }
    }

    [PunRPC]
    void RPC_AssignDistancesOnRopes(float dist)
    {
        Debug.Log("Distance between players is: " + dist);
      
        _rope1.transform.position = new Vector3(-dist/2f, 1f, 0);
        _rope2.transform.position = new Vector3(dist/2f, 1f, 0);

        var distUI = FindObjectOfType<SceneManager_Game>().DistanceText;
        distUI.text = "Current distance is: " + dist;
        FindObjectOfType<SceneManager_Game>().CurrentDistance = dist;
    }

    [PunRPC]
    void RPC_ShowRatingUI(int index)
    {
        _inGameRatingUI[index].gameObject.SetActive(true);
        _inGameRatingUI[1-index].gameObject.SetActive(false);
    }

    public IEnumerator LookingForAvatars() {
        while (_avatarHeads.Count != 2)
        {
            _hasSetAvatars = false;
            //_avatarHeads.Clear();
            if (GameManager.Instance.UsingVR)
            {
                var avatars = GameObject.FindGameObjectsWithTag("HMDHead");
                if (avatars.Length != 2) {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                foreach (GameObject a in avatars) {
                    _avatarHeads.Add(a);
                }

                _hasSetAvatars = true;
            }
            else
            {
                var avatars = GameObject.FindGameObjectsWithTag("OptitrackHead");
                if (avatars.Length != 2) {
                    yield return new WaitForEndOfFrame();
                    continue;
                }

                foreach (GameObject a in avatars)
                {
                    _avatarHeads.Add(a);
                }
                _hasSetAvatars = true;
            }
            yield return new WaitForEndOfFrame();
        }
    }


    /// <summary>
    /// For recording when boards switch status, not being used...
    /// </summary>
    void Log20QuesionGame()
    {
        if (PhotonNetwork.isMasterClient)
        {
            var recordingManager = _recordingManager = GameManager.Instance.gameObject.GetComponent<RecordingManager>();
            var worldTimer = GameManager.Instance.gameObject.GetComponent<WorldTimer>();
            GameObject board = null;

            if (_board1.GetComponent<Text>().text == "It's your turn to guess.")
            {
                board = _board1;
            }
            if (_board2.GetComponent<Text>().text == "It's your turn to guess.")
            {
                board = _board2;
            }

            if(board==null)
                return;

            var dist1 = (board.transform.position - _avatarHeads[0].transform.position).magnitude;
            var dist2 = (board.transform.position - _avatarHeads[1].transform.position).magnitude;
            if (dist1 > dist2)
            {
                //recordingManager.sw_otherLogForEyes.WriteLine(worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + _avatars[0].name + ",is guessing");
            }
            else
            {
                //recordingManager.sw_otherLogForEyes.WriteLine(worldTimer.ElapsedTimeSinceStart.TotalSeconds + "," + _avatars[1].name + ",is guessing");
            }
        }
    }
}

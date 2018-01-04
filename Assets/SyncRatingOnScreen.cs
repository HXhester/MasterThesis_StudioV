using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SyncRatingOnScreen : MonoBehaviour
{
    public Text TextToSyncWith;

    public void SyncOption()
    {
        GetComponent<Text>().text = TextToSyncWith.text;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class RandomizeWords : MonoBehaviour
{
    [HideInInspector] public Dictionary<string, string> CategoryWordDict;
    [HideInInspector] public string[] WordList;
    [HideInInspector] public string[] DistanceList;
    [HideInInspector] public string[] RatingList;

    // Use this for initialization
    void Awake()
    {
        // TODO: change wordlist
        // TODO: try out build files
        CategoryWordDict = new Dictionary<string, string>();
       
        if (Environment.CommandLine.Contains("Experiment"))
        {
            using (StreamReader sr = new StreamReader(Application.streamingAssetsPath + "wordlist.txt"))
            {
                string line;
                string[] values;
                string[] words;
                while ((line = sr.ReadLine()) != null)
                {
                    values = line.Split(':');
                    words = values[1].Split(',');
                    foreach (string s in words)
                    {
                        CategoryWordDict.Add(s,values[0]);
                    }
                }
            }           
            GetComponent<SceneManager_Game>().CountDownTime = 180f;
        }
        else
        {
            GetComponent<SceneManager_Game>().CountDownTime = 15f;
            CategoryWordDict.Add("Cat","Animals");
            CategoryWordDict.Add("Tiger","Animals");
            CategoryWordDict.Add("Stawberry","Fruits");
        }

        WordList = new string[CategoryWordDict.Count];
        WordList = CategoryWordDict.Keys.ToArray();

        DistanceList = new string[] {"0.4", "0.6", "1", "1.4", "1.8"};
        RatingList = new string[]
        {
            "This is an intimate distance for me. I keep this distance with close people, like parents, spouse etc.",
            "This is a personal distance for me.I keep this distance with my friends and relatives.",
            "This is a social distance for me.I keep this distance with acquaintances or unfamiliar people."
        };
    }

    public void RandomizeTexts(String[] words)
    {
        // Knuth shuffle algorithm :: courtesy of Wikipedia :)
        for (int t = 0; t < words.Length; t++)
        {
            string tmp = words[t];
            int r = UnityEngine.Random.Range(t, words.Length);
            words[t] = words[r];
            words[r] = tmp;
        }
    }
}

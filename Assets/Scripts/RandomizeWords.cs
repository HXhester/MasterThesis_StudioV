using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeWords : MonoBehaviour
{
    [HideInInspector]
    public string[] WordList;
    [HideInInspector]
    public string[] DistanceList;
    [HideInInspector]
    public string[] RatingList;

	// Use this for initialization
	void Awake () {
        // TODO: change wordlist
		WordList = new string[]{ "Cat",  "Dog",  "Car"};
        DistanceList = new string[] { "0.6", "1", "1.4", "2","2.8" };
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

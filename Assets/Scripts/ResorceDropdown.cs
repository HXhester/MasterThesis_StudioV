using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResorceDropdown : MonoBehaviour {

	Dropdown dropdown;
	public string pathFromAssets = "/Resources/Avatars";
	public string alternativePath = "/Resources/";
	public bool isScene = false;


	public string[] unfinishedScenes = new string[0];

	// Use this for initialization
	void Awake () {
		dropdown = GetComponent<Dropdown> ();
		string[] files;
		if (isScene) {
			files = Directory.GetFiles (Application.dataPath + pathFromAssets, "*.unity");
			unfinishedScenes = Directory.GetFiles (Application.dataPath + alternativePath, "*.unity");
		} else {
			files = Directory.GetFiles (Application.dataPath + pathFromAssets, "*.prefab");
		}

		string fileName;
		if (!isScene) {
			foreach (string s in files) {
				fileName = s.Replace (Application.dataPath + "/Resources/", string.Empty).ToString ();
				fileName = fileName.Replace ("." + "prefab", string.Empty).ToString ();
				dropdown.options.Add(new Dropdown.OptionData(){text = fileName});
				dropdown.RefreshShownValue ();
			}

		} else {
			foreach (string s in files) {
				fileName = s.Replace (Application.dataPath + pathFromAssets + "\\", string.Empty).ToString ();
				fileName = fileName.Replace ("." + "unity", string.Empty).ToString ();
				dropdown.options.Add(new Dropdown.OptionData(){text = fileName});
				dropdown.RefreshShownValue ();
			}
			foreach (string s in unfinishedScenes) {
				Debug.Log (s);
				fileName = s.Replace (Application.dataPath + alternativePath + "\\", string.Empty).ToString ();
				fileName = fileName.Replace (".unity", string.Empty).ToString ();
				//				dropdown.options.Add(new Dropdown.OptionData(){text = fileName});

				var texture = new Texture2D(1,1); // creating texture with 1 pixel
				texture.SetPixel(1, 1, Color.red); // setting to this pixel some color
				texture.Apply(); //applying texture. necessarily
				var item = new Dropdown.OptionData(fileName, Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0))); // creating dropdown item and converting texture to sprite
				dropdown.options.Add(item); // adding this item to dropdown options

				dropdown.RefreshShownValue ();
			}
		}

	}
}

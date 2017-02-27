using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndMessage : MonoBehaviour {
    bool showing = false;
    // Use this for initialization
    void Awake () {
        GetComponentInChildren<Button>().interactable = false;
    }
	
	// Update is called once per frame
	void Update () {
		if (showing && GetComponent<CanvasGroup>().alpha < 1)
        {
            GetComponent<CanvasGroup>().alpha += 0.01f;
        }
	}
    public void ShowMessage(string text)
    {
        showing = true;
        GetComponentInChildren<Text>().text = text;
        GetComponentInChildren<Button>().interactable = true;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadLevel : MonoBehaviour {
	public void OnClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.GetComponentInChildren<Text>().text);
    }
}

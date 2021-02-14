using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CombatantRenderer : MonoBehaviour {

	public Text infoText;

	public void setText(string text) {
		infoText.text = text;
	}

    // Start is called before the first frame update
    void Start() {
		
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

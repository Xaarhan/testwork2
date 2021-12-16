using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementsSpawner : MonoBehaviour {

	// Use this for initialization
	void Start () {
        add_btn.onClick.AddListener(onAddBtn);
        remove_btn.onClick.AddListener(onRemoveBtn);
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    private void onAddBtn() {
        RectTransform element = Instantiate( def_element, content );
        element.gameObject.SetActive(true);
    }


    private void onRemoveBtn() {
        if ( content.childCount > 0 ) {
             Destroy(content.GetChild(content.childCount - 1).gameObject);
        }
        
    }

    

    public Button add_btn;
    public Button remove_btn;
    public RectTransform content;
    public RectTransform def_element;


}

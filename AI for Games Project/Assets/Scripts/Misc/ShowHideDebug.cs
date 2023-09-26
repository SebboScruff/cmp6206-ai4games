using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowHideDebug : MonoBehaviour
{
    public bool showDebugThings;
    public KeyCode toggleKey;
    
    public TextMeshProUGUI tmp;
    
    // Start is called before the first frame update
    void Start()
    {
        tmp = this.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(toggleKey)){
            showDebugThings = !showDebugThings;
        }
        tmp.enabled = showDebugThings;
    }
}

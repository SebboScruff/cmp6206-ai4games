using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualGridNode : MonoBehaviour
{
    MeshRenderer r;
    bool isVisible = false;
    // Start is called before the first frame update
    void Start()
    {
        r = gameObject.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F5)){
            isVisible = !isVisible;
        }
        r.enabled = isVisible;
    }
}

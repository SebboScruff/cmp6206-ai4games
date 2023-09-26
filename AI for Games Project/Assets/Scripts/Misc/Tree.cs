using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public bool isChoppedDown = true;
    [SerializeField] GameObject grown, stump;

    [SerializeField] private float baseRegrowTime = 5f;
    private float timeRemaining;
    // Start is called before the first frame update
    void Start()
    {
        isChoppedDown = false;
        timeRemaining = baseRegrowTime;
    }

    // Update is called once per frame
    void Update()
    {
        stump.SetActive(isChoppedDown);
        grown.SetActive(!isChoppedDown);

        if(Input.GetKeyDown(KeyCode.C)){
            isChoppedDown = !isChoppedDown;
        }

        if(isChoppedDown){
            timeRemaining -= Time.deltaTime;
            if(timeRemaining <= 0){
                isChoppedDown = false;
            }
        }
        if(!isChoppedDown && timeRemaining != baseRegrowTime){
            timeRemaining = baseRegrowTime;
        }
    }

    void GetChoppedDown()
    {
        // give the lumberjack some wood
        isChoppedDown = true;
    }
}

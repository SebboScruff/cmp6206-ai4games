using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraTargetControls : MonoBehaviour
{
    [SerializeField] CinemachineFreeLook controller;
    [SerializeField] GameObject player, lumberjack;
    bool aerialView = false;
    [SerializeField] Camera aerialCam;
    public bool isTrackingPlayer {get; private set;}
    // Start is called before the first frame update
    void Start()
    {
        isTrackingPlayer = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F9)){
            aerialView = !aerialView;
        }
        if(Input.GetKeyDown(KeyCode.F10)){
            isTrackingPlayer = !isTrackingPlayer;
        }
        
        aerialCam.enabled = aerialView;

        if(isTrackingPlayer){
            controller.Follow = player.transform;
            controller.LookAt = player.transform;
        }
        else{
            controller.Follow = lumberjack.transform;
            controller.LookAt = lumberjack.transform;
        }
    }
}

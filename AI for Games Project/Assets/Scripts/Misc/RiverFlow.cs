using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Super basic, bare-bones texture offset to make the water texture flow slowly
public class RiverFlow : MonoBehaviour
{
    [SerializeField] float flowSpeed = -0.05f;
    public Material riverMat;

    // Update is called once per frame
    void Update()
    {
        riverMat.mainTextureOffset += new Vector2(flowSpeed, flowSpeed) * Time.deltaTime;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public sealed class World
{
    private static readonly World instance = new World(); // only have one instance at a time
    private static WorldStates world;

    static World()
    {
        world = new WorldStates();
    }
    private World()
    {

    }
    public static World Instance
    {
        get{return instance;}
    }
    public WorldStates GetWorld()
    {
        return world;
    }
}

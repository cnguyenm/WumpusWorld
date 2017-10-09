using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    // list of direction
    public Transform leftPos;
    public Transform rightPos;
    public Transform upPos;
    public Transform downPos;
    public GameObject floor;

    public enum Type
    {
        Safe,
        Pit,
        Gold,
        Wumpus
    }

    public enum Direction
    {
        Left,
        Right, 
        Up, 
        Down
    }

    // manage the connection of room
    public bool leftConnect     { get; set; }
    public bool rightConnect    { get; set; }
    public bool upConnect       { get; set; }
    public bool downConnect     { get; set; }

    public int row { get; set; }    // current row of this room
    public int col { get; set; }    // current collumn of this room
}

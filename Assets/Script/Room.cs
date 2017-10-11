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
    public GameObject roomObject;

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

    public GameManager gameManager { get; set; }    // game manager

    // manage the connection of room
    public bool leftConnect     { get; set; }   // left
    public bool rightConnect    { get; set; }   // right
    public bool upConnect       { get; set; }   // up
    public bool downConnect     { get; set; }   // down

    public int row { get; set; }    // current row of this room
    public int col { get; set; }    // current collumn of this room

    /// <summary>
    /// Allow player to see this room
    /// </summary>
    public bool display {
        get
        {
            return roomObject.activeSelf;
        }
        set {
            roomObject.SetActive(value);
        }
    }


    /// <summary>
    /// If player enter the collision of new room
    /// set that room fires to be on
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        // just checking
        if (other.tag != "Player")
            return;

        // if room enter is cur room
        if (gameManager.curRoom == this)
        {
            return;
        }

        // if room enter is new room
        gameManager.SwitchRoom(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour {

    // list of direction
    public Transform leftPos;
    public Transform rightPos;
    public Transform upPos;
    public Transform downPos;

    public enum type
    {
        Safe,
        Pit,
        Gold,
        Wumpus
    }
}

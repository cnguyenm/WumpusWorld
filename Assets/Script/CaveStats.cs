using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CaveStats {

    public int boardSize; // boardSize: 4x4
    public float pitProb; // pit Probability

    public float roomSize; // real world room Size = scale(Floor) = 10
    public float hallLength; // real hall length = 10
}

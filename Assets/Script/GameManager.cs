using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
   
    public CameraController cameraController; // camera

    public Room curRoom { get; set; }   // current Room that player is in

    private CaveGenerator caveGenerator; // generate cave
    private PlayerController player;

    // Use this for initialization
    void Start () {

        // generate cave
        caveGenerator = GetComponent<CaveGenerator>();
        caveGenerator.GenerateCave();

        // generate player
        player = caveGenerator.GeneratePlayer();
        cameraController.player = player.gameObject; // set camera to follow player

        string[] percepts = caveGenerator.GetCaveInfluence(curRoom.row, curRoom.col);
        Debug.Log(percepts);
    }

  
	
	// Update is called once per frame
	void Update () {
		
	}


    /// <summary>
    /// Switch current active room
    /// Inform player of receive sequence
    /// </summary>
    /// <param name="newRoom"></param>
    public void SwitchRoom(Room newRoom)
    {
        // switch on/off
        newRoom.display = true;
        curRoom.display = false;

        // set new room
        curRoom = newRoom;

        // inform player
        string[] percepts = caveGenerator.GetCaveInfluence(curRoom.row, curRoom.col);
        Debug.Log(percepts);
    }
    

   

}

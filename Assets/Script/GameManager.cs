using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

    public Room roomPrefab;
    public GameObject hallPrefab;
    public GameObject fencePrefab;

    public int roomLength = 10;  // use this for now
    public int hallLength = 10;

    public CaveStats caveStats; // stats used to generate cave

    private int[,] cave;

	// Use this for initialization
	void Start () {

        //GenerateCaveDatabase();
        //PrintCave();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void GenerateCavePhysical()
    {
        Vector3 position = new Vector3(0, 0, 0);
        Room room = Instantiate<Room>(roomPrefab, position, Quaternion.identity);
        /*
        //because that is how the hallway is designed
        Instantiate(hallPrefab, 
            room.rightPos.position + room.rightPos.right * (hallLength/2), 
            room.rightPos.rotation);

        //Instantiate(hallPrefab, 
        //    room.leftPos.position + room.leftPos.right * (hallLength/2), 
        //    room.leftPos.rotation);

        
        // fence
        // if upPos, leave it be
        // kind of hard coding, but yeah it works
        Instantiate(fencePrefab,
             room.upPos.position + new Vector3(0, 0.4f, 0),
             Quaternion.Euler(90, 0, 0), 
             room.transform);

        // if down, rotate a bit
        Instantiate(fencePrefab,
             room.leftPos.position + new Vector3(0, 0.4f, 0),
             Quaternion.Euler(90, 90, 0),
             room.transform);
        */


    }

    /// <summary>
    /// Generate Cave data
    /// put Pit, Gold, Wumpus inside
    /// </summary>
    private void GenerateCaveDatabase()
    {
        // init var, C# feature
        cave = new int[caveStats.size, caveStats.size];
        float random;
        int x, z;

        // generate cave with pit
        for (int row = 0; row < caveStats.size; row++)
        {
            for (int col = 0; col < caveStats.size; col++)
            {
                // leave 1st room for player
                if (row == 0 && col == 0)
                {
                    cave[row, col] = (int)Room.type.Safe;
                    continue;
                }
                    

                // put Pit in
                random = Random.Range(0, 1f);
                if (random <= caveStats.pitProb)
                {
                    cave[row, col] = (int) Room.type.Pit;
                }
                else
                {
                    cave[row, col] = (int)Room.type.Safe;
                }
            }
        }


        // put wumpus in
        x = Random.Range(1, caveStats.size); //leave 1 room for player
        z = Random.Range(1, caveStats.size);
        cave[x, z] = (int)Room.type.Wumpus;

        // put gold in
        x = Random.Range(1, caveStats.size); //leave 1 room for player
        z = Random.Range(1, caveStats.size);
        cave[x, z] = (int)Room.type.Gold;
    }

    /// <summary>
    /// Used to debug
    /// Print out the cave dataset
    /// </summary>
    private void PrintCave()
    {
        string message = "";

        for (int row = caveStats.size - 1; row >= 0; row--)
        {
            for (int col = 0; col < caveStats.size; col++)
            {
                Room.type type = (Room.type)cave[row, col];
                message += "[" + type.ToString() + "]";
            }
            print(message);
            message = "";
        }
    }

}

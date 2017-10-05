using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Used to generate environment: such as room, hallway, wumpus, gold
/// </summary>
public class CaveGenerator : MonoBehaviour {

    // prefab used
    public Room roomPrefab;
    public GameObject hallPrefab;
    public GameObject fenceHorPrefab; // horizontal fence
    public GameObject fenceVerPrefab; // vertical fence
    public GameObject pitPrefab;
    public Wumpus wumpusPrefab;
    

    // stats
    public CaveStats caveStats;

    // cave dataset
    public int[,] cave { get; private set; }

    // environment: clear everything easy
    private GameObject environment;

    private enum FenceType
    {
        Horizontal,
        Vertical
    }

    // Use this for initialization
    void Start () {
		
	}

#region Cave Generation


    public void GenerateCave()
    {
        GenerateCaveDatabase();
        GenerateCavePhysicsWorld();
    }

    private void GenerateCavePhysicsWorld()
    {
  
        // start position
        Vector3 startPos = new Vector3(0, 0, 0);

        // create empty obj "Environment"
        environment = new GameObject("Environment");
        environment.transform.position = new Vector3(0, 0, 0);


        // test
        Room room = MakeRoom(startPos);
        MakeFence(room, room.leftPos.position, FenceType.Vertical);
        MakeFence(room, room.upPos.position, FenceType.Horizontal);

        MakeHallWay(room.rightPos);
        MakeHallWay(room.downPos);

        MakePit(room);
      
    }

    /// <summary>
    /// Create an instance of roomPrefab
    /// room.parent = environment
    /// </summary>
    /// <param name="position">position of room</param>
    /// <returns>reference to room</returns>
    private Room MakeRoom(Vector3 position)
    {
        // check null
        if (position == null)
            return null;

        // instantiate 
        Room room = Instantiate<Room>(roomPrefab,
            position,
            Quaternion.identity,
            environment.transform); // set parent

        return room;
    }

    /// <summary>
    /// Create an instance of Wumpus
    /// wumpus.parent = room
    /// </summary>
    /// <param name="room">Room of wumpus</param>
    /// <returns>ref to wumpus</returns>
    private Wumpus MakeWumpus(Room room)
    {
        if (room == null)
            return null;

        Wumpus wumpus = Instantiate<Wumpus>(wumpusPrefab,
            room.transform.position + new Vector3(0, 1, 0),
            Quaternion.Euler(0, 180, 0),
            room.transform);

        return wumpus;
    }

    /// <summary>
    /// Create an instance of Fence
    /// </summary>
    /// <param name="room">room where it belong</param>
    /// <param name="position">position of fence</param>
    /// <param name="fenceType">type: may be hor | ver</param>
    private void MakeFence(Room room, Vector3 position, FenceType fenceType)
    {
        // check null
        if (position == null )
            return;

        // get prefab
        GameObject fenceUse = null;
        switch(fenceType)
        {
            case FenceType.Horizontal:
                fenceUse = fenceHorPrefab;
                break;

            case FenceType.Vertical:
                fenceUse = fenceVerPrefab;
                break;    
        }

        // error
        if (fenceUse == null)
        {
            Debug.Log("Make fence: no fence Prefab");
            return;
        }

        // instantiate
        Instantiate(fenceUse,
            position,
            Quaternion.identity,
            room.transform);
    }

    /// <summary>
    /// Create an instance of hallPrefab
    /// </summary>
    /// <param name="roomPos">Position of room, ex: room.leftPos</param>
    private void MakeHallWay(Transform roomPos)
    {
        Instantiate(hallPrefab,
            roomPos.position + roomPos.right * (caveStats.hallLength / 2),
            roomPos.rotation,
            environment.transform);
    }

    /// <summary>
    /// Create an instance of pit
    /// </summary>
    /// <param name="room">room of pit</param>
    private void MakePit(Room room)
    {
        // check null
        if (room == null)
            return;

        // delete old floor
        GameObject oldFloor = room.transform.Find("Floor").gameObject;
        Destroy(oldFloor);

        // put Pit in
        Instantiate(pitPrefab,
            room.transform.position,
            Quaternion.identity,
            room.transform);
    }


    /// <summary>
    /// Generate Cave data
    /// put Pit, Gold, Wumpus inside
    /// </summary>
    private void GenerateCaveDatabase()
    {
        // init var, C# feature
        cave = new int[caveStats.boardSize, caveStats.boardSize];
        float random;
        int x, z;

        // generate cave with pit
        for (int row = 0; row < caveStats.boardSize; row++)
        {
            for (int col = 0; col < caveStats.boardSize; col++)
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
                    cave[row, col] = (int)Room.type.Pit;
                }
                else
                {
                    cave[row, col] = (int)Room.type.Safe;
                }
            }
        }


        // put wumpus in
        x = Random.Range(1, caveStats.boardSize); //leave 1 room for player
        z = Random.Range(1, caveStats.boardSize);
        cave[x, z] = (int)Room.type.Wumpus;

        // put gold in
        x = Random.Range(1, caveStats.boardSize); //leave 1 room for player
        z = Random.Range(1, caveStats.boardSize);
        cave[x, z] = (int)Room.type.Gold;
    }



    /// <summary>
    /// Used to debug
    /// Print out the cave dataset
    /// </summary>
    private void PrintCave()
    {
        string message = "";

        for (int row = caveStats.boardSize - 1; row >= 0; row--)
        {
            for (int col = 0; col < caveStats.boardSize; col++)
            {
                Room.type type = (Room.type)cave[row, col];
                message += "[" + type.ToString() + "]";
            }
            print(message);
            message = "";
        }
    }

#endregion



}

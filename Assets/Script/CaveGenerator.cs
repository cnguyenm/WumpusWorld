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
    public GameObject goldPrefab;
    public Wumpus wumpusPrefab;
    public PlayerController playerPrefab;   // player
    

    // stats
    public CaveStats caveStats;

    // cave dataset
    public int[,] cave { get; private set; }

    // environment: clear everything easy
    private GameObject environment;
    private Room[,] rooms;
    private GameManager gameManager;

    private enum FenceType
    {
        Horizontal,
        Vertical
    }


    private void Start()
    {
        gameManager = GetComponent<GameManager>();
    }


    #region Cave Generation


    public void GenerateCave()
    {
        GenerateCaveDatabase();
        GenerateCavePhysicsWorld();
        PrintCave();
        
    }

    /// <summary>
    /// default: Put player in Safe room, [0, 0]
    /// </summary>
    public PlayerController GeneratePlayer()
    {       
        return GeneratePlayer(0, 0);
    }

    /// <summary>
    /// Put player in rooms[row, col]
    /// </summary>
    /// <param name="row">input row</param>
    /// <param name="col">input col</param>
    private PlayerController GeneratePlayer(int row, int col)
    {
        // set room
        gameManager.curRoom = rooms[row, col];

        // init player
        PlayerController player =  Instantiate<PlayerController>(
            playerPrefab,
            rooms[0, 0].transform.position,
            Quaternion.identity);

        return player;
    }

    /// <summary>
    /// from dataset from GenerateCaveDatabase()
    /// create physical world of cave, hallway, gold, wumpus
    /// </summary>
    private void GenerateCavePhysicsWorld()
    {
  
        // variables
        Vector3 pos;
        float distance = caveStats.roomSize + caveStats.hallLength;
        rooms = new Room[caveStats.boardSize, caveStats.boardSize];

        // create empty obj "Environment"
        environment = new GameObject("Environment");
        environment.transform.position = new Vector3(0, 0, 0);

        // generate cave
        // remember, in WumpusWorld, the cave board is
        // [1,0], [1,1]
        // [0,0], [1,0]
        Room room;

        for (int row = 0; row < caveStats.boardSize; row++)
        {
            for (int col = 0; col < caveStats.boardSize; col++)
            {
                // get pos
                pos = new Vector3(col * distance, 0, row * distance);

                // create room
                room = MakeRoom(pos);
                room.row = row;
                room.col = col;

                // put stuff in room
                FillRoomContent(room, row, col);

                // add room to array
                rooms[row, col] = room;
            }
        }


        // generate connection
        for (int row = 0; row < caveStats.boardSize; row++)
        {
            for (int col = 0; col < caveStats.boardSize; col++)
            {
                FillRoomConnection(rooms[row, col]);
            }
        }
    }

    /// <summary>
    /// Put Pit, Gold, Wumpus in the current room
    /// </summary>
    /// <param name="room">current room</param>
    /// <param name="row">room row</param>
    /// <param name="col">room col</param>
    private void FillRoomContent(Room room, int row, int col)
    {
        // get the type
        Room.Type type = (Room.Type)cave[row, col];

        // put stuff in
        switch (type)
        {
            // pit
            case Room.Type.Pit:
                MakePit(room);
                break;

            // gold
            case Room.Type.Gold:
                MakeGold(room);
                break;

            // wumpus
            case Room.Type.Wumpus:
                MakeWumpus(room);
                break;

            // safe, leave it there
        }
    }

    /// <summary>
    /// Create hallway or fence in all direction of room
    /// </summary>
    /// <param name="room">input</param>
    private void FillRoomConnection(Room room)
    {
        // check null
        if (room == null)
        {
            Debug.Log("Room null or invalid location");
            return;
        }

        int row = room.row;
        int col = room.col;

        // left
        if (!room.leftConnect)
        {
            MakeHallWayOrFence(room, new int[] { row, col - 1}, Room.Direction.Left);
        }

        if (!room.rightConnect)
        {
            MakeHallWayOrFence(room, new int[] { row, col + 1 }, Room.Direction.Right);
        }

        if (!room.upConnect)
        {
            MakeHallWayOrFence(room, new int[] { row + 1, col }, Room.Direction.Up);
        }

        if (!room.downConnect)
        {
            MakeHallWayOrFence(room, new int[] { row - 1, col}, Room.Direction.Down);
        }
    }

    /// <summary>
    /// Create fence of hallway in particular direction of room
    /// </summary>
    /// <param name="room">input</param>
    /// <param name="nextPos">next location of room on Board, ex: [2,3]</param>
    /// <param name="direction">direction from cur room to next room</param>
    private void MakeHallWayOrFence(Room room, int[] nextPos, Room.Direction direction)
    {
        // check
        if (room == null || nextPos == null)
            return;

        // get variable to use
        FenceType fenceType = FenceType.Horizontal; // default, avoid errors
        Transform roomPos = null;

        if (direction == Room.Direction.Left)
        {
            roomPos = room.leftPos;
            fenceType = FenceType.Vertical;
            room.leftConnect = true;
        }
        if (direction == Room.Direction.Right)
        {
            roomPos = room.rightPos;
            fenceType = FenceType.Vertical;
            room.rightConnect = true;
        }
        if (direction == Room.Direction.Up)
        {
            roomPos = room.upPos;
            fenceType = FenceType.Horizontal;
            room.upConnect = true;
        }
        if (direction == Room.Direction.Down)
        {
            roomPos = room.downPos;
            fenceType = FenceType.Horizontal;
            room.downConnect = true;
        }



        // if next location is valid
        if (isValidLocation(nextPos[0], nextPos[1]))
        {
            if (direction == Room.Direction.Left)
            {            
                MakeHallWay(room.leftPos);            
                rooms[nextPos[0], nextPos[1]].rightConnect = true;
            }
            if (direction == Room.Direction.Right)
            {
                MakeHallWay(room.rightPos);
                rooms[nextPos[0], nextPos[1]].leftConnect = true;
            }
            if (direction == Room.Direction.Up)
            {
                MakeHallWay(room.upPos);
                rooms[nextPos[0], nextPos[1]].downConnect = true;
            }
            if (direction == Room.Direction.Down)
            {
                MakeHallWay(room.downPos);
                rooms[nextPos[0], nextPos[1]].upConnect = true;
            }

        }

        // if next position is invalid
        else
        {
            MakeFence(room, roomPos.position, fenceType);
        }

       

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
    /// Create an instance of gold prefab
    /// </summary>
    /// <param name="room">Room to put gold</param>
    private void MakeGold(Room room)
    {
        if (room == null)
            return;

        Instantiate(goldPrefab,
            room.transform.position,
            Quaternion.identity,
            room.transform);
    }

    /// <summary>
    /// Create an instance of Fence
    /// </summary>
    /// <param name="room">room where it belong</param>
    /// <param name="position">position of fence. ex: room.leftPos.pos</param>
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
        GameObject oldFloor = room.floor;
        if (oldFloor == null)
        {
            Debug.Log("MakePit: floor null");
            return;
        }

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
                    cave[row, col] = (int)Room.Type.Safe;
                    continue;
                }


                // put Pit in
                random = Random.Range(0, 1f);
                if (random <= caveStats.pitProb)
                {
                    cave[row, col] = (int)Room.Type.Pit;
                }
                else
                {
                    cave[row, col] = (int)Room.Type.Safe;
                }
            }
        }


        // put wumpus in
        x = Random.Range(1, caveStats.boardSize); //leave 1 room for player
        z = Random.Range(1, caveStats.boardSize);
        cave[x, z] = (int)Room.Type.Wumpus;

        // put gold in
        x = Random.Range(1, caveStats.boardSize); //leave 1 room for player
        z = Random.Range(1, caveStats.boardSize);
        cave[x, z] = (int)Room.Type.Gold;
    }

    /// <summary>
    /// Check if certain cell is a valid location on caveBoard
    /// ex: [-1, 2] is invalid
    /// </summary>
    /// <param name="row">row</param>
    /// <param name="col">col</param>
    /// <returns>True if valid</returns>
    private bool isValidLocation(int row, int col)
    {
        return (row >= 0 && row < caveStats.boardSize
            && col >= 0 && col < caveStats.boardSize);
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
                Room.Type type = (Room.Type)cave[row, col];
                message += "[" + type.ToString() + "]";
            }
            print(message);
            message = "";
        }
    }

#endregion



}

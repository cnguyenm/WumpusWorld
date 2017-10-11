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

    
    public int[,] cave { get; private set; } // array of cave, or room as dataset

    // environment: clear everything easy
    private GameObject environment;
    private Room[,] rooms;  // array of real-world room
    private GameManager gameManager;

    private enum FenceType
    {
        Horizontal,
        Vertical
    }


    private void Awake()
    {
        gameManager = GetComponent<GameManager>();
    }


    #region Cave Generation


    /// <summary>
    /// Generate cave dataset + realworld
    /// Should be the first function to call
    /// </summary>
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
        if (gameManager == null)
        {
            Debug.Log("Game manager null");
            return null;
        }
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
                room.gameManager = gameManager;

                // put stuff in room
                FillRoomContent(room, row, col);

                // add room to array
                rooms[row, col] = room;

                // not allow player to see
                // prevent cheating
                // set every room except [0,0] to false
                if (row != 0 || col != 0)
                {
                    room.display = false;
                }
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
            room.roomObject.transform);

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
            room.roomObject.transform);
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
            room.roomObject.transform);
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
            room.roomObject.transform);
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

    #region PlayerInteract


    /// <summary>
    /// Get simple cave influence
    /// Have not implemented Bump, Scream yet
    /// </summary>
    /// <param name="row">row where player is</param>
    /// <param name="col">col where player is</param>
    /// <returns>array of sequence percepts</returns>
    public string[] GetCaveInfluence(int row, int col)
    {
        // check valid
        if (!isValidLocation(row, col))
            return null;

        // construct result
        string[] result = new string[5];

        // check 4 direction
        GetRoomInfluence(row    , col - 1, result);
        GetRoomInfluence(row    , col + 1, result);
        GetRoomInfluence(row + 1, col    , result);
        GetRoomInfluence(row - 1, col - 1, result);

        // return
        return result;
    }

    /// <summary>
    /// Get influence of a particular room
    /// </summary>
    /// <param name="row">row</param>
    /// <param name="col">col</param>
    /// <returns>string like: null, "Stench", "Glitter", "Breeze"</returns>
    private void GetRoomInfluence(int row, int col, string[] result)
    {
        // check valid
        if (!isValidLocation(row, col))
            return;

        // get room
        Room.Type roomType = (Room.Type) cave[row, col];
        int index;

        // safe
        if (roomType == Room.Type.Safe)
        {
            return;
        }
            
        // pit
        if (roomType == Room.Type.Pit)
        {
            index = (int)PlayerController.Percept.Breeze;
            result[index] = "Breeze";
        }

        // gold
        if (roomType == Room.Type.Gold)
        {
            index = (int)PlayerController.Percept.Glitter;
            result[index] = "Glitter";
        }

        // wumpus
        if (roomType == Room.Type.Wumpus)
        {
            index = (int)PlayerController.Percept.Stench;
            result[index] = "Stench";
        }
        
        // bump, scream will be implemented in other functions

    }

    #endregion

}

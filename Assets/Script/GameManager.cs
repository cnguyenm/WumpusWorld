using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {


    private CaveGenerator caveGenerator; // generate cave

	// Use this for initialization
	void Start () {
 
        caveGenerator = GetComponent<CaveGenerator>();
        caveGenerator.GenerateCave();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    

   

}

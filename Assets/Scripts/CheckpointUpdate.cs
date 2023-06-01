using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointUpdate : MonoBehaviour
{
    public int whichCheckpoint;
    PlayerController player;
    Vector3 checkPointPos;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        GameObject[] allCheckPoints = GameObject.FindGameObjectsWithTag("Checkpoint");
        checkPointPos = allCheckPoints[whichCheckpoint].transform.GetChild(0).position;
        
    }

    void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            player.SetCheckpoint(checkPointPos);
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}

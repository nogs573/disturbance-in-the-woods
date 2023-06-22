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
        if (whichCheckpoint == 0)
            checkPointPos = GameObject.FindWithTag("Checkpoint-0").transform.GetChild(0).position; 
        else if (whichCheckpoint == 1)
            checkPointPos = GameObject.FindWithTag("Checkpoint-1").transform.GetChild(0).position; 
        else if (whichCheckpoint == 2)
            checkPointPos = GameObject.FindWithTag("Checkpoint-2").transform.GetChild(0).position; 
               
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

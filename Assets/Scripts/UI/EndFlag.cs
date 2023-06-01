using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndFlag : MonoBehaviour
{  
    GameObject endText;

    private void Start()
    {
        endText = transform.GetChild(0).GetChild(0).gameObject;
        endText.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            endText.SetActive(true);
            Debug.Log("Finished game!");
        }
    }
}

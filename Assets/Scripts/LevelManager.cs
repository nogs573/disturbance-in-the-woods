using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    //private string LEVEL_NAME;
    private float DIMENSION_DIF;
    // Start is called before the first frame update
    void Awake()
    {
        //LEVEL_NAME = "Forest";
        GameObject[] ground = GameObject.FindGameObjectsWithTag("Ground");
        DIMENSION_DIF = ground[0].transform.position.y - ground[1].transform.position.y;
    }

    public float getDimDiff() { return DIMENSION_DIF; }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class mouseprower : MonoBehaviour
{

    void Update()
    {
        GetComponent<Image>().enabled = Input.GetKey(KeyCode.M) && Input.GetKey(KeyCode.P);    
    }
}

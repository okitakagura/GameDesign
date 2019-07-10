using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Opendoor : MonoBehaviour
{
    //c.GetComponent<Login>().enabled = false;
    // Start is called before the first frame update

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerStay(Collider other)
    {
        GameObject c = GameObject.Find("Main Camera");
        c.GetComponent<Login>().enabled = true;
        Debug.Log("stay");


        //SceneManager.LoadScene("win");
    }
    private void OnTriggerExit(Collider other)
    {
        GameObject c = GameObject.Find("Main Camera");
        c.GetComponent<Login>().enabled = false;
        //SceneManager.LoadScene("prototype");
    }
}
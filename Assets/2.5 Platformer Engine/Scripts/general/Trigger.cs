using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer
{
    public class Trigger : MonoBehaviour
    {
        //private Door m_Door;
        private bool enterCollide = false;
        //public CharacterMotor
        // Start is called before the first frame update
        void Start()
        {
            transform.position = new Vector3(-22.16f, 6.03f, -118.19f);
        }


        void OnTriggerEnter(Collider collider)
        {
            Debug.Log("enter");
            Debug.Log(CharacterMotor.monster_num);
            enterCollide = true;
        }

        void OnTriggerExit(Collider collider)
        {
            Debug.Log("exit");
            enterCollide = false;
        }
        // Update is called once per frame
        void Update()
        {
            if (enterCollide)
            {
                if  (CharacterMotor.monster_num == 3)
                {
                    SceneManager.LoadScene("Prototype3");
                    Debug.Log("next");
                }
             
                //{
                //    if (m_Door.GetIsOpen())
                //    {
                //        m_Door.CloseDoorMethod();

                //    }
                //    else
                //    {
                //        m_Door.OpenDoorMethod();
                      
                //    }
                //}
            }

        }
    }
}

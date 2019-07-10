using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Music2: MonoBehaviour
{
    // Start is called before the first frame update
    public AudioClip shootSound;
    private AudioSource m_AudioSource;
    private int play = 0;
    // Start is called before the first frame update
    void Start()
    {
        m_AudioSource = GetComponent<AudioSource>();    //获取音频源组件
        m_AudioSource.clip = shootSound;

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        play = 1;
        if (play == 1)
        {
            m_AudioSource.Play();    //播放音效
            Debug.Log("enter72");
            //play = 1;
        }

        //SceneManager.LoadScene("prototype");
    }
    private void OnTriggerExit(Collider other)
    {
        m_AudioSource.Stop();
        Debug.Log("exit72");
        play = 0;
    }
}
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{

    private string Password;//密码
    private string info;

    void Start()
    {
        //初始化
        Password = "";
        info = "";
        GameObject c = GameObject.Find("Main Camera");
        c.GetComponent<Login>().enabled = false;
    }

    void OnGUI()
    {
        //用户名
        GUI.Label(new Rect(300, 20, 500, 20), "请输入密码");
        Password = GUI.PasswordField(new Rect(280, 50, 100, 20), Password, '*');//'*'为密码遮罩
        //信息
        GUI.Label(new Rect(300, 100, 500, 20), info);
        //登录按钮
        if (GUI.Button(new Rect(280, 80, 100, 20), "确定"))
        {
            if (Password == "007")
            {
                SceneManager.LoadScene("Prototype");
            }
            else
            {
                Password = "";
                info = "密码错误";
            }
        }

    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GateKeeper1 : MonoBehaviour
{
    //定义NPC对话数据
    private string[] mData = { "The road next to me is false." };
    //当前对话索引
    private int index = 0;
    //用于显示对话的GUI Text
    public Text mText;
    //对话标示贴图
    public Texture mTalkIcon;
    //是否显示对话标示贴图
    private bool isTalk = false;
    Vector3 screenPosition;//将物体从世界坐标转换为屏幕坐标
    Vector3 mousePositionOnScreen;//获取到点击屏幕的屏幕坐标
    Vector3 mousePositionInWorld;//将点击屏幕的屏幕坐标转换为世界坐标

    void Update()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (Vector3.Distance(transform.position, player.transform.position) < 2)
        {
            //进入对话状态
            // isTalk = true;
            //允许绘制

            //获取鼠标在相机中（世界中）的位置，转换为屏幕坐标；
            screenPosition = Camera.main.WorldToScreenPoint(transform.position);
            //获取鼠标在场景中坐标
            mousePositionOnScreen = Input.mousePosition;
            //让场景中的Z=鼠标坐标的Z
            mousePositionOnScreen.z = screenPosition.z;
            //将相机中的坐标转化为世界坐标
            mousePositionInWorld = Camera.main.ScreenToWorldPoint(mousePositionOnScreen);
            if (Vector3.Distance(transform.position, mousePositionInWorld) < 1)
            {
                isTalk = true;
                if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetMouseButtonDown(0))
                {
                    //绘制指定索引的对话文本
                    if (index < mData.Length)
                    {
                        mText.text = "GateKeeper:" + mData[index];
                        index = index + 1;
                    }
                    else
                    {
                        index = 0;
                        mText.text = "GateKeeper:" + mData[index];
                    }
                }
            }
            else
            {
                isTalk = false;
            }
            //Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            //RaycastHit mHi;
            ////判断是否击中了NPC
            //if (Physics.Raycast(mRay, out mHi))
            //{
            //    //如果击中了NPC
            //    if (mHi.collider.gameObject.tag == "NPC")
            //    {
            //        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetMouseButtonDown(0))
            //        {
            //            //绘制指定索引的对话文本
            //            if (index < mData.Length)
            //            {
            //                mText.text = "Fox:" + mData[index];
            //                index = index + 1;
            //            }
            //            else
            //            {
            //                index = 0;
            //                mText.text = ":" + mData[index];
            //            }
            //        }
            //    }
            //}


            //if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetMouseButtonDown(0))
            //{
            //    //绘制指定索引的对话文本
            //    if (index < mData.Length)
            //    {
            //        mText.text = "Fox:" + mData[index];
            //        index = index + 1;
            //    }
            //    else
            //    {
            //        index = 0;
            //        mText.text = ":" + mData[index];
            //    }
            //}
        }
        else
        {
            isTalk = false;
            mText.text = " ";
        }

    }

    void OnGUI()
    {
        if (isTalk)
        {
            //禁用系统鼠标指针
            Cursor.visible = false;
            Rect mRect = new Rect(Input.mousePosition.x - mTalkIcon.width,
                   Screen.height - Input.mousePosition.y - mTalkIcon.height,
                   mTalkIcon.width, mTalkIcon.height);
            //绘制自定义鼠标指针
            GUI.DrawTexture(mRect, mTalkIcon);
        }
        else
        {
            //禁用系统鼠标指针
            Cursor.visible = true;
        }

    }
}
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tips : MonoBehaviour
{

    //定义NPC对话数据
    private string[] mData = { "Music is the only the language of the universe."};
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

    private Camera camera;
    private string name = "Tips";
    private float npcHeight;
    void Start()
    {
        camera = Camera.main;
        //得到模型原始高度
        float size_y = GetComponent<Collider>().bounds.size.y;
        //得到模型缩放比例
        float scal_y = transform.localScale.y;
        //NPC模型高度
        npcHeight = (size_y * scal_y);
    }
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
                        mText.text = mData[index];
                        index = index + 1;
                    }
                    else
                    {
                        index = 0;
                        mText.text = mData[index];
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

        //得到NPC头顶在3D世界中的坐标
        //默认NPC坐标点在脚底下，所以这里加上npcHeight它模型的高度即可
        Vector3 worldPosition = new Vector3(transform.position.x, transform.position.y + npcHeight, transform.position.z);
        //根据NPC头顶的3D坐标换算成它在2D屏幕中的坐标
        Vector2 position = camera.WorldToScreenPoint(worldPosition);
        //得到真实NPC头顶的2D坐标
        position = new Vector2(position.x, Screen.height - position.y);
        //计算NPC名称的宽高
        Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(name));
        //设置显示颜色为黄色
        GUI.color = Color.yellow;
        //绘制NPC名称
        GUI.Label(new Rect(position.x - (nameSize.x / 2), position.y - nameSize.y, nameSize.x, nameSize.y), name);
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
            //启用系统鼠标指针
            Cursor.visible = true;
        }

    }
}
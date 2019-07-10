using UnityEngine;
using UnityEngine.UI;//注意添加RawImage命名空间


public class FadeInOut : MonoBehaviour
{

    public float fadeSpeed = 3f;
    public bool sceneStarting = true;
    public  RawImage rawImage;
    //void Awake()
    //{
    //    rawImage = GetComponent<RawImage>();
    //}

    void Start()
    {
    }

    void Update()
    {
        if (sceneStarting)
            StartScene();
    }

    private void FadeToClear()
    {
        rawImage.color = Color.Lerp(rawImage.color, Color.clear, fadeSpeed * Time.deltaTime);
    }

    private void FadeToBlack()
    {
        rawImage.color = Color.Lerp(rawImage.color, Color.black, fadeSpeed * Time.deltaTime);
    }

    void StartScene()
    {
        FadeToClear();
        if (rawImage.color.a < 0.05f)
        {
            rawImage.color = Color.clear;
            rawImage.enabled = false;
            sceneStarting = false;
        }
    }

    //void EndScene()
    //{
    //    rawImage.enabled = true;
    //    FadeToBlack();
    //    if (rawImage.color.a > 0.95f)
    //    {
    //        SceneManager.LoadScene(0);
    //    }
    //}

    void OnDestroy()
    {

    }
}
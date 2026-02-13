using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SplashScreen : MonoBehaviour
{
    [Header("时间设置")]
    public float fadeInDuration = 1.2f;    // 黑到透明的时间
    public float displayDuration = 2.0f;   // 完全显示时间
    public float fadeOutDuration = 1.2f;   // 透明到黑的时间

    [Header("场景设置")]
    public string mainMenuSceneName = "IntroMenu";

    [Header("UI组件")]
    public Image fadeImage; // 可手动拖拽，如果为空则自动创建
    public TextMeshProUGUI GameName;
    public TextMeshProUGUI AboutGame;
    public TextMeshProUGUI GovTip;
    public Image WarningText;
    void Start()
    {
        // 如果fadeImage没有手动指定，尝试获取或创建
        if (fadeImage == null)
        {
            // 先尝试获取现有的Image组件
            fadeImage = GetComponent<Image>();

            // 如果没有，则创建一个
            if (fadeImage == null)
            {
                // 确保有Canvas
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    GameObject canvasObj = new GameObject("Canvas");
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<GraphicRaycaster>();
                }

                // 创建全屏黑色面板
                GameObject panelObj = new GameObject("FadePanel");
                panelObj.transform.SetParent(canvas.transform);
                fadeImage = panelObj.AddComponent<Image>();

                // 设置全屏
                RectTransform rect = fadeImage.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }
        }

        // 开始启动序列
        StartCoroutine(SplashSequence());
    }

    IEnumerator SplashSequence()
    {
        // 初始设置为黑色
        fadeImage.color = Color.black;
        fadeImage.enabled = true;

        Debug.Log("开始：黑屏");
        GameName.gameObject.SetActive(true);
        AboutGame.gameObject.SetActive(true);
        WarningText.gameObject.SetActive(false);
        GovTip.gameObject.SetActive(false);
        // 1. 从黑到透明（淡入，显示splash图片） - 1.3秒
        yield return StartCoroutine(FadeImage(Color.black, Color.clear, fadeInDuration));

        Debug.Log("淡入完成，开始等待");

        // 2. 保持完全显示 - 2秒
        yield return new WaitForSeconds(displayDuration);

        Debug.Log("等待完成，开始淡出");
        // 3. 从透明到黑（淡出） - 1.3秒
        yield return StartCoroutine(FadeImage(Color.clear, Color.black, fadeOutDuration));
#if !UNITY_EDITOR
        AboutGame.gameObject.SetActive(false);
        GameName.gameObject.SetActive(false);
        WarningText.gameObject.SetActive(true);
        yield return StartCoroutine(FadeImage(Color.black, Color.clear, fadeInDuration));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeImage(Color.clear, Color.black, fadeOutDuration));
        GovTip.gameObject.SetActive(true);
        WarningText.gameObject.SetActive(false);
        yield return StartCoroutine(FadeImage(Color.black, Color.clear, fadeInDuration));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FadeImage(Color.clear, Color.black, fadeOutDuration));
        Debug.Log("淡出完成，加载主菜单");
#endif
        if (AccountManagement.Instance.IsLogin)
        {
            Debug.Log("已登录，跳转到主界面");
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.Log("未登录，跳转到登录界面");
            SceneManager.LoadScene("Login");
        }
        // 4. 加载主菜单场景


    }

    IEnumerator FadeImage(Color startColor, Color endColor, float duration)
    {
        if (duration <= 0)
        {
            fadeImage.color = endColor;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        fadeImage.color = endColor; // 确保最终颜色正确
    }
}
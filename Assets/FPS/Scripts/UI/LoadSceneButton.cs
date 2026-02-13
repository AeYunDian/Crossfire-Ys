using System.Security.Cryptography;
using UnityEngine;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Unity.FPS.UI
{
    public class LoadSceneButton : MonoBehaviour
    {
        // 问题1修复：正确声明并初始化场景名数组
        private static readonly string[] SceneNames = { "MainScene", "SecondaryScene", "thirdScene" };

        // 问题2&3修复：将变量名改为能清晰反映其用途的 `currentSceneIndex`
        // 初始化为0，表示从第一个场景开始
        private static int currentSceneIndex = 0;
        void Start()
        {
            SyncCurrentSceneIndex();
        }
        async void Awake()
        {
            int ressa = await AccountManagement.Instance.GetBagData();
            if (ressa != -1)
            {
                currentSceneIndex = ressa;
            } else if (ressa == -1) { currentSceneIndex = 0; }
        }
        // 新增：同步方法
        void SyncCurrentSceneIndex()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            for (int i = 0; i < SceneNames.Length; i++)
            {
                if (SceneNames[i] == currentSceneName)
                {
                    currentSceneIndex = i;
                    return;
                }
            }
            // 如果当前场景不在数组中，给予警告
            Debug.LogWarning($"当前场景 '{currentSceneName}' 未在SceneNames列表中，状态未同步。");
        }
        public void LoadTargetScene()
        {
            // 加载当前索引指向的场景
            if (currentSceneIndex >= 0 && currentSceneIndex < SceneNames.Length)
            {
                SceneManager.LoadScene(SceneNames[currentSceneIndex]);
            }
            else
            {
                Debug.LogError("场景索引超出范围: " + currentSceneIndex);
            }
        }

        public async void LoadNextScene()
        {
            // 增加索引以指向下一个场景
            currentSceneIndex++;

            // 关键修复：检查索引是否已经超过数组最后一个位置
            if (currentSceneIndex > SceneNames.Length)
            {
                Debug.LogWarning("已是最后一个场景，将循环回第一个场景。" + currentSceneIndex);
                currentSceneIndex = 0; // 循环回到开头
                // 或者你也可以选择不增加，停在最后一个场景：
                // currentSceneIndex = SceneNames.Length - 1;
                // return;
            }
            int result = await AccountManagement.Instance.PushBagData(currentSceneIndex);
            switch (result)
            {
                case AccountManagement.Code_OK:
                    Debug.Log("推送currentSceneIndex 为" + currentSceneIndex + "成功");
                    break;
                case AccountManagement.Code_Unknown:
                    
                    Debug.LogError($"服务器错误 无法解析的状态码");
                    break;
                case AccountManagement.Code_InternalServerError:
                    Debug.LogError($"服务器无法处理推送的请求");
                    break;
                case AccountManagement.Code_InvalidInfo:
                    Debug.LogError($"推送currentSceneIndex有误");
                    break;
                case AccountManagement.Code_BadRequest:
                    Debug.LogError($"坏请求");
                    break;
                case AccountManagement.Code_Unauthorized:
                    Debug.LogError($"帐户登录失效");
                    break;
                default:
                    Debug.LogError($"switch 触发default");
                    break;
            }
            // 加载新场景
            LoadTargetScene();
        }

        public void Restart()
        {
            // 重新加载当前场景（注意：这里的当前是 `currentSceneIndex` 指向的场景）
            LoadTargetScene();
        }
        public void LoadScenes(string WillLoadScene)
        {
            // 先尝试更新索引
            int targetIndex = System.Array.IndexOf(SceneNames, WillLoadScene);
            if (targetIndex != -1)
            {
                currentSceneIndex = targetIndex;
            }
            else
            {
                Debug.LogWarning($"跳转目标 '{WillLoadScene}' 未在SceneNames列表中，currentSceneIndex未更新。");
            }
            // 再加载场景
            SceneManager.LoadScene(WillLoadScene);
        }
    }
}
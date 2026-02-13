using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
//using UnityEditor.PackageManager;
using UnityEngine;


public class GameApiClient : MonoBehaviour
{
    public TMP_Text AboutText;
    public static GameApiClient Instance { get; private set; }
    private  HttpClient Client  = new HttpClient();
    private  string Domain = "https://api.io.hb.cn/api/crossfire";
    private  HttpResponseMessage response;
    void Awake()
    {
        AboutText.text = $"Version: "+
            Application.version + 
            "\r\nBuild Date: 2026 - 02 - 13\r\n" +
            "Engine: Unity 2022.3.62f3\r\n" +
            "Developer: Yund\r\n" +
            "首次不删档测试，不代表最终品质";

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameApiClent创建成功！");
        }
        else
        {
            Destroy(gameObject);
            Debug.Log("已存在GameApiClent，销毁重复的");
        }
    }

    public  async Task<HttpResponseMessage>  PostNetData(string path, string json)
    {

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        Client.Timeout = TimeSpan.FromSeconds(5);
        response = await Client.PostAsync(Domain + path, content);

        return response;
    }
}

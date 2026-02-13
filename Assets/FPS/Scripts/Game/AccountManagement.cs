
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class AccountManagement : MonoBehaviour
{

    // ----------- 第一部分：单例核心 -----------
    // 这就像公司的"总经理办公室" - 只有一个！
    public static AccountManagement Instance { get; private set; }


    // ----------- 第二部分：你的登录状态 -----------
    private static bool isLogin = false;  // 私有变量，别人不能直接改

    private static string token;
    private static string uuid;
    private static string accountEmail ;
    private static string accountName;
    private static string accountPassword;

    public bool IsLogin
    {
        get { return isLogin; } 
        private set { isLogin = value; } 
    }

    public  string UserName
    {
        get { return accountName; }
        private set { accountName = value; }  
    }
    public  string UserEmail
    {
        get { return accountEmail; }
        private set { accountEmail = value; } 
    }
    public  string UserPassword
    {
        get { return accountPassword; }  
        private set { accountPassword = value; } 
    }
    private string Path_LoginAccount = "/v1/account/login";
    //private string Path_LogoutAccount = "/v1/account/logout";
    private string Path_CreateAccount = "/v1/account/create";
    private string Path_BagPush = "/v1/bag/push";
    private string Path_BagGet = "/v1/bag/get";

    public const int Code_OK = 0;
    public const int Code_Unknown = 1;
    public const int Code_BadRequest = 2;
    public const int Code_InvalidInfo = 3;
    public const int Code_AccountBanned = 4;
    public const int Code_Unauthorized = 5;
    public const int Code_Forbidden = 6;
    public const int Code_InternalServerError = 7;
    public const int Code_DataExists = 8;
    public const int Code_DataExists_EMAIL = 9;
    public const int Code_DataExists_UUID = 10;
    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;  
            DontDestroyOnLoad(gameObject); 
            Debug.Log("AccountManager创建成功！");
        }
        else
        {
            Destroy(gameObject); 
            Debug.Log("已存在AccountManager，销毁重复的");
        }
    }
    public class LoginSuccessResponse
    {
        public string status { get; set; }
        public string token { get; set; }
        public string uuid { get; set; }
        public string name { get; set; }
    }

    public class ErrorResponse
    {
        public string error { get; set; }

    }
    public class ErrorResponse2
    {
        public string error { get; set; }
        public string code { get; set; }
    }
    public class BagData
    {
        public string status { get; set; }
        public string level { get; set; }
    }
    public async Task<int> LoginAccount(string email, string password)
    {

        var data = new { email, password };
        string json = JsonConvert.SerializeObject(data);
        HttpResponseMessage response = await GameApiClient.Instance.PostNetData(Path_LoginAccount, json);
        string responseBody = await response.Content.ReadAsStringAsync();
       // var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                var success = JsonConvert.DeserializeObject<LoginSuccessResponse>(responseBody);
                token = success.token;
                uuid = success.uuid;
                accountName = success.name;
                accountEmail = email;
                accountPassword = password;
                isLogin = true;
                PlayerPrefs.SetString("email", email);
                PlayerPrefs.SetString("password", password);
                return Code_OK;
            case HttpStatusCode.BadRequest:
            case HttpStatusCode.Unauthorized:
                var error = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
                Debug.Log($"登录失败: {error.error}");
                return error.error.Contains("password") ? Code_InvalidInfo : Code_BadRequest;
            case HttpStatusCode.Forbidden:
                Debug.Log($"账号被封禁");
                return Code_AccountBanned;
            case HttpStatusCode.InternalServerError:
                return Code_InternalServerError;
            default:
                return Code_Unknown;
               
        }
        
    }

    public int LogoutAccount()
    {

        return 0;
    }
    public async Task<int> CreateAccount(string name, string email, string password)
    {
        var data = new { name, email, password };
        string json = JsonConvert.SerializeObject(data);
        HttpResponseMessage response = await GameApiClient.Instance.PostNetData(Path_CreateAccount, json);
        string responseBody = await response.Content.ReadAsStringAsync();
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return Code_OK;
            case HttpStatusCode.BadRequest:
                var error = JsonConvert.DeserializeObject<ErrorResponse>(responseBody);
                Debug.Log($"创建账号失败: {error.error}");
                return error.error.Contains("required") ? Code_InvalidInfo : Code_BadRequest;
            case HttpStatusCode.InternalServerError:
                var error500 = JsonConvert.DeserializeObject<ErrorResponse2>(responseBody);
                Debug.Log($"创建账号失败: {error500.code}");
                if (error500.code.Contains("UUID_CONFLICT"))
                {
                    return Code_DataExists_UUID;
                }
                return Code_DataExists;
            case HttpStatusCode.Conflict:
                var error409 = JsonConvert.DeserializeObject<ErrorResponse2>(responseBody);
                Debug.Log($"创建账号失败: {error409.code}");
                if(error409.code.Contains("EMAIL_EXISTS"))
                {
                    return Code_DataExists_EMAIL;
                }
                return Code_DataExists;
            default:
                return Code_Unknown;

        }
    }

    public async Task<int> GetBagData()
    {
        var data = new { uuid, token };
        string json = JsonConvert.SerializeObject(data);
        HttpResponseMessage response = await GameApiClient.Instance.PostNetData(Path_BagGet, json);
        string responseBody = await response.Content.ReadAsStringAsync();
        Debug.Log(response.StatusCode);
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                var bagData = JsonConvert.DeserializeObject<BagData>(responseBody);
                return int.Parse(bagData.level.ToLower());
            //case HttpStatusCode.InternalServerError:
            //    return Code_InternalServerError;
            //case HttpStatusCode.Conflict:
            //    var error409 = JsonConvert.DeserializeObject<ErrorResponse2>(responseBody);
            //    Debug.Log($"创建账号失败: {error409.code}");
            //    if (error409.code.Contains("EMAIL_EXISTS"))
            //    {
            //        return Code_DataExists_EMAIL;
            //    }
            //    return Code_DataExists;
            default:
                return -1;

        }
    }
    public async Task<int> PushBagData(int level) {

        var data = new { uuid, token, level };
        string json = JsonConvert.SerializeObject(data);
        HttpResponseMessage response = await GameApiClient.Instance.PostNetData(Path_BagPush, json);
        string responseBody = await response.Content.ReadAsStringAsync();
        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                return Code_OK;

            case HttpStatusCode.BadRequest:
                return Code_InvalidInfo;

            case HttpStatusCode.InternalServerError:
                return Code_InternalServerError;
            case HttpStatusCode.NotFound:
                Debug.LogWarning("登录状态失效");
                isLogin = false;
                return Code_Unauthorized;
            default:
                return Code_Unknown;

        }
    }

}

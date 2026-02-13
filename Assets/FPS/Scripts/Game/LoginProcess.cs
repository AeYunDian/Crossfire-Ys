using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using TMPro;
public class LoginProcess : MonoBehaviour
{
    [Header("UI组件")]
    public Button 登录账户按钮;
    public Button 创建账户按钮;
    public Button 切换到注册的按钮;
    public Button 切换到登录的按钮;
    public Button 关闭信息框的按钮;
    public Button 打开注册邮箱的按钮;
    public TMP_InputField 登录的邮箱;
    public TMP_InputField 登录的密码;
    public TMP_InputField 注册的名称;
    public TMP_InputField 注册的邮箱;
    public TMP_InputField 注册的密码;
    public TMP_InputField 注册的验证密码;
    public GameObject 登录界面;
    public GameObject 注册界面;
    public GameObject 信息框;
    public TMP_Text 提示的标题;
    public TMP_Text 提示的内容;

    private void Awake()
    {
        检查所有UI引用();
        if (PlayerPrefs.HasKey("email") && PlayerPrefs.HasKey("password"))
        {
            登录的邮箱 .text = PlayerPrefs.GetString("email");
            登录的密码.text = PlayerPrefs.GetString("password");
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        登录账户按钮.onClick.AddListener(OnLoginButtonClick);
        创建账户按钮.onClick.AddListener(OnCreateAccountButtonClick);
        切换到注册的按钮.onClick.AddListener(SwitchToCreate);
        切换到登录的按钮.onClick.AddListener(SwitchToLogin);
        关闭信息框的按钮.onClick.AddListener(HideMessage);
        打开注册邮箱的按钮.onClick.AddListener(OpenYsPost);
        登录界面.SetActive(true);
        注册界面.SetActive(false);
        信息框.SetActive(false);
    }

    void OpenYsPost()
    {
        Application.OpenURL("https://mail.io.hb.cn/login");
    }

    private void 检查所有UI引用()
    {
        // 获取当前脚本的所有实例字段
        FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            // 只检查 Unity 引擎相关的对象类型
            if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
            {
                UnityEngine.Object obj = field.GetValue(this) as UnityEngine.Object;
                if (obj == null)
                {
                    Debug.LogError($"字段 [{field.Name}] 未在Inspector中赋值！", this);
                    enabled = false;
                    return;
                }
            }
        }
    }
    void SwitchToLogin()
    {
        注册界面.SetActive(false);
        登录界面.SetActive(true);
        Debug.Log("切换到登录");
    }

    void SwitchToCreate()
    {
        注册界面.SetActive(true);
        登录界面.SetActive(false);
        Debug.Log("切换到注册");
    }

    async void OnLoginButtonClick()
    {

        if (String.IsNullOrEmpty(登录的邮箱.text) || String.IsNullOrEmpty(登录的密码.text))
        {
            ShowMessage("提示", "请填写完整信息");
            Debug.LogError($"请填写完整信息");
            return;
        }
        登录账户按钮.interactable = false;
        try
        {
            int result = await AccountManagement.Instance.LoginAccount(登录的邮箱.text, 登录的密码.text);
            switch (result)
            {
                case AccountManagement.Code_OK:
                    Debug.Log("登录成功");
                    ShowMessage("登录成功", AccountManagement.Instance.UserName + " 欢迎进入游戏\r\n游戏加载中..." , false);
                    await Task.Delay(1500);
                    if (await AccountManagement.Instance.GetBagData() == -1)
                    {
                        ShowMessage("提示", "无法从服务器获取上一次游玩进度，继续游玩将从第一关开始\r\n您可以退出游戏，然后再次尝试", false);
                    }
                    await Task.Delay(2000);
                    SceneManager.LoadScene("IntroMenu");
                    break;
                case AccountManagement.Code_Unknown:
                    ShowMessage("提示", "服务器返回了一个客户端无法解析的信息\r\n您可以尝试更新客户端，然后重试：\r\nUnresolvable status code");
                    Debug.LogError($"服务器错误 无法解析的状态码");
                    break;
                case AccountManagement.Code_InternalServerError:
                    ShowMessage("提示", "服务器无法处理此次请求：\r\nStatus code 500");
                    Debug.LogError($"服务器无法处理请求");
                    break;
                case AccountManagement.Code_InvalidInfo:
                    ShowMessage("提示", "邮箱或密码错误");
                    Debug.LogError($"邮箱或密码错误");
                    break;
                case AccountManagement.Code_BadRequest:
                    ShowMessage("提示", "客户端请求有误，可能是服务端更新所致，您可尝试更新客户端");
                    Debug.LogError($"坏请求");
                    break;
                case AccountManagement.Code_AccountBanned:
                    ShowMessage("帐户被封禁", "您的帐户已被封禁，如有疑问可以发送邮件至\r\n<color=#569CD6>crossfire@io.hb.cn</color>");
                    Debug.LogError($"帐户被封禁");
                    break;
                default:
                    ShowMessage("提示", "解析请求失败");
                    Debug.LogError($"switch 触发default");
                    break;
            }
        }
        catch (Exception e)
        {
            ShowMessage("无法连接至服务器", $"一个来自于网络或服务器的错误，使我们无法与服务器通讯: \r\n{e.Message}");
            Debug.LogError($"网络或服务器错误: {e.Message}");
        }
        finally
        {
            登录账户按钮.interactable = true;
        }

    }

    // 创建账户按钮点击事件处理方法
    async void OnCreateAccountButtonClick()
    {
        
        if (String.IsNullOrEmpty(注册的邮箱.text))
        {
            ShowMessage("提示", "邮箱不能为空");
            Debug.LogError($"邮箱不能为空");
            return;
        }
        if (String.IsNullOrEmpty(注册的名称.text))
        {
            ShowMessage("提示", "用户名不能为空");
            Debug.LogError($"用户名不能为空");
            return;
        }
        if (String.IsNullOrEmpty(注册的验证密码.text) || String.IsNullOrEmpty(注册的密码.text))
        {
            ShowMessage("提示", "密码不能为空");
            Debug.LogError($"密码不能为空");
            return;
        }
        创建账户按钮.interactable = false;
        try
        {
            int result = await AccountManagement.Instance.CreateAccount(注册的名称.text, 注册的邮箱.text, 注册的密码.text);
            switch (result)
            {
                case AccountManagement.Code_OK:
                    Debug.Log("注册成功");
                    ShowMessage("注册成功", "您现在可以使用您刚注册的账户登陆了！");
                    SwitchToLogin();
                    break;
                case AccountManagement.Code_InvalidInfo:
                    ShowMessage("提示", "请填写完整您的信息");
                    Debug.LogError($"请填写完整您的信息");
                    break;
                case AccountManagement.Code_Unknown:
                    ShowMessage("提示", "服务器返回了一个客户端无法解析的信息\r\n您可以尝试更新客户端，然后重试：\r\nUnresolvable status code");
                    Debug.LogError($"服务器错误 无法解析的状态码");
                    break;
                case AccountManagement.Code_BadRequest:
                    ShowMessage("提示", "客户端请求有误，可能是服务端更新所致，您可尝试更新客户端");
                    Debug.LogError($"坏请求");
                    break;
                case AccountManagement.Code_DataExists_UUID:
                    ShowMessage("提示", "您的用户唯一标识码重复，因为服务器并发请求较多，请重试");
                    Debug.LogError($"用户唯一标识码重复");
                    break;
                case AccountManagement.Code_DataExists:
                    ShowMessage("提示", "您的邮箱或UUID重复，请尝试更换一个邮箱，\r\n然后重试");
                    Debug.LogError($"未知的信息重复");
                    break;
                case AccountManagement.Code_DataExists_EMAIL:
                    ShowMessage("提示", "该邮箱已被注册，请尝试更换一个邮箱，\r\n然后重试");
                    Debug.LogError($"未知的信息重复");
                    break;
                default:
                    ShowMessage("提示", "解析请求失败");
                    Debug.LogError($"switch 触发default");
                    break;
            }
        }
        catch (Exception e)
        {
            ShowMessage("无法连接至服务器", $"一个来自于网络或服务器的错误，使我们无法与服务器通讯: \r\n{e.Message}");
            Debug.LogError($"网络或服务器错误: {e.Message}");
        }
        finally
        {
            创建账户按钮.interactable = true;
        }

    }

    // 如果需要，可以在OnDestroy中移除监听器
    void OnDestroy()
    {
        登录账户按钮.onClick.RemoveListener(OnLoginButtonClick);
        创建账户按钮.onClick.RemoveListener(OnCreateAccountButtonClick);
        切换到注册的按钮.onClick.RemoveListener(SwitchToCreate);
        切换到登录的按钮.onClick.RemoveListener(SwitchToLogin);
        关闭信息框的按钮.onClick.RemoveListener(HideMessage);
        打开注册邮箱的按钮.onClick.RemoveListener(OpenYsPost);
    }

    void ShowMessage(string title, string text, bool ShowCloseBtn = true)
    {
        //HideMessage();
        关闭信息框的按钮.gameObject.SetActive(ShowCloseBtn);
        提示的标题.text = title;
        提示的内容.text = text;
        信息框.SetActive(true);
    }
    void HideMessage()
    {
        信息框.SetActive(false);
    }
}

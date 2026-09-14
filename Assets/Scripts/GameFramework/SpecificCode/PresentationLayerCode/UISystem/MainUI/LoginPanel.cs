using UnityEngine;
using UnityEngine.UI;

//*****************************************
//功能说明：登录面板（联网）
// 登录成功 → 拉 MySQL 角色列表 → 再进选角
//*****************************************
public class LoginPanel : BasePanel
{
    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button regBtn;
    private Button exitBtn;

    private INetSystem netSystem;
    private bool waitingLogin;

    public override void OnInit()
    {
        base.OnInit();
        idInput = transform.Find("IF_UserID").GetComponent<InputField>();
        pwInput = transform.Find("IF_PassWord").GetComponent<InputField>();
        loginBtn = transform.Find("Btn_Login").GetComponent<Button>();
        regBtn = transform.Find("Bottom/Btn_Group/Btn_Regist").GetComponent<Button>();
        exitBtn = transform.Find("Bottom/Btn_Group/Btn_Exit").GetComponent<Button>();

        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegistClick);
        exitBtn.onClick.AddListener(OnExitGame);

        netSystem = this.GetSystem<INetSystem>();
    }

    public override void OnShow(params object[] objs)
    {
        base.OnShow(objs);
        idInput.text = "";
        pwInput.text = "";
        waitingLogin = false;
        this.RegistEvent<LoginResultEvent>(OnLoginResult);
        this.RegistEvent<CharacterListResultEvent>(OnCharacterListResult);
    }

    public override void OnClose()
    {
        this.UnRegistEvent<LoginResultEvent>(OnLoginResult);
        this.UnRegistEvent<CharacterListResultEvent>(OnCharacterListResult);
        waitingLogin = false;
        base.OnClose();
    }

    private void OnRegistClick()
    {
        uiSystem.OpenPanel<RegisterPanel>();
    }

    private void OnLoginClick()
    {
        if (waitingLogin)
        {
            return;
        }

        string user = idInput.text != null ? idInput.text.Trim() : "";
        string pass = pwInput.text ?? "";
        if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
        {
            uiSystem.OpenPanel<TipPanel>("请输入账号和密码");
            return;
        }

        if (netSystem == null)
        {
            uiSystem.OpenPanel<TipPanel>("网络系统未注册");
            return;
        }

        waitingLogin = true;
        netSystem.SendLogin(user, pass);
    }

    private void OnLoginResult(object data)
    {
        LoginResMsg res = data as LoginResMsg;
        if (res == null)
        {
            waitingLogin = false;
            uiSystem.OpenPanel<TipPanel>("登录回包异常");
            return;
        }

        if (!res.ok)
        {
            waitingLogin = false;
            uiSystem.OpenPanel<TipPanel>(res.message);
            return;
        }

        Debug.Log("[LoginPanel] 登录成功，请求角色列表 userId=" + res.userId);
        netSystem.SendCharacterList();
    }

    private void OnCharacterListResult(object data)
    {
        waitingLogin = false;
        CharacterListResMsg res = data as CharacterListResMsg;
        if (res == null)
        {
            uiSystem.OpenPanel<TipPanel>("角色列表回包异常");
            return;
        }

        if (!res.ok)
        {
            uiSystem.OpenPanel<TipPanel>(res.message);
            return;
        }

        uiSystem.ClearPlayerDataList();
        if (res.characters != null)
        {
            for (int i = 0; i < res.characters.Length; i++)
            {
                PlayerData pd = CharacterDataConverter.ToPlayerData(res.characters[i]);
                if (pd != null)
                {
                    uiSystem.AddPlayerData(pd);
                }
            }
        }

        OnClose();
        uiSystem.OpenPanel<ChoicePanel>();
    }

    private void OnExitGame()
    {
        Application.Quit();
    }
}

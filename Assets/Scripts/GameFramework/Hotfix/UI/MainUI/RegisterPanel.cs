using UnityEngine.UI;

//*****************************************
//功能说明：注册面板（联网）
//***************************************** 
public class RegisterPanel : BasePanel
{
	private InputField idInput;
	private InputField pwInput;
	private InputField reInput;
	private Button registBtn;
	private Button cancelBtn;

	private INetSystem netSystem;
	private bool waitingRegister;

	public override void OnInit()
	{
		idInput = DeepFindTransform.DeepFindChild(transform, "IF_UserID").GetComponent<InputField>();
		pwInput = DeepFindTransform.DeepFindChild(transform, "IF_Password").GetComponent<InputField>();
		reInput = DeepFindTransform.DeepFindChild(transform, "IF_ReInputPassword").GetComponent<InputField>();
		registBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Register").GetComponent<Button>();
		cancelBtn = DeepFindTransform.DeepFindChild(transform, "Btn_Cancel").GetComponent<Button>();
		registBtn.onClick.AddListener(OnRegistClick);
		cancelBtn.onClick.AddListener(() => { OnClose(); });

		netSystem = this.GetSystem<INetSystem>();
		base.OnInit();
	}

	public override void OnShow(params object[] objs)
	{
		base.OnShow(objs);
		waitingRegister = false;
		this.RegistEvent<RegisterResultEvent>(OnRegisterResult);
	}

	public override void OnClose()
	{
		this.UnRegistEvent<RegisterResultEvent>(OnRegisterResult);
		waitingRegister = false;
		base.OnClose();
	}

	private void OnRegistClick()
	{
		if (waitingRegister)
		{
			return;
		}

		if (idInput.text == "" || pwInput.text == "")
		{
			uiSystem.OpenPanel<TipPanel>("用户名和密码不能为空");
			return;
		}

		if (reInput.text != pwInput.text)
		{
			uiSystem.OpenPanel<TipPanel>("两次输入密码不同");
			return;
		}

		if (netSystem == null)
		{
			uiSystem.OpenPanel<TipPanel>("网络系统未注册");
			return;
		}

		waitingRegister = true;
		netSystem.SendRegister(idInput.text.Trim(), pwInput.text);
	}

	private void OnRegisterResult(object data)
	{
		waitingRegister = false;
		RegisterResMsg res = data as RegisterResMsg;
		if (res == null)
		{
			uiSystem.OpenPanel<TipPanel>("注册回包异常");
			return;
		}

		uiSystem.OpenPanel<TipPanel>(res.message);
		if (res.ok)
		{
			idInput.text = "";
			pwInput.text = "";
			reInput.text = "";
			OnClose();
		}
	}
}

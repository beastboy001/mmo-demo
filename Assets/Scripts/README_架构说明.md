# MMO-demo 架构说明（已按魔兽课可复用部分整理）

## 目录结构（对齐 WowProject）

```
Assets/Scripts/
  GameFramework/
    ReusableCode/                 ← 从魔兽课原样搬过来（可复用内核）
      Architecture/               IOC / Command / Event / Rules / Singleton
      Layers/                     IController / ISystem / IModel / IUtility
        Utility/Editor/           创建 Event/Command 脚本的编辑器窗口
    SpecificCode/                 ← 你自己写玩法与系统（目前是空架子）
      GameStarter.cs              入口（已有）
      UnderLyingCode/
        Architectrure/
          WowArchitercture.cs     柜子 Init（已有，暂不注册系统）
      PresentationLayerCode/
        UISystem/                 预留：UISystem、BasePanel…
        NetSystem/                预留：INetSystem、NetSystem…
        SceneSystem/
        CharacterSystem/
```

## 已搬过来的（ReusableCode）

- `Architecture`：`Architecture<T>`、`IOCContainer`、`StartArchitecture`、`ICommand`、事件系统、权限接口与扩展方法
- `Layers`：`IController` / `ISystem` / `IModel` / `IUtility`
- `Editor`：右键创建 Event / Command 脚本模板

唯一改动：`EventExternsion.UnRegistEvent` 原课误调 `RegistEvent`，已改成真正注销。

## 刻意没搬的（SpecificCode / 玩法）

- `NetSystem`、登录注册面板、协议 PT、选角、角色 FSM、资源加载等  
这些属于具体玩法，按计划你自己写。目录已预留。

## 怎么验证

1. 回到 Unity，等脚本编译完成  
2. 在 `SampleScene` 建空物体，挂上 `GameStarter`  
3. Play，Console 应出现：`[WowArchitercture] Init` 和 `[GameStarter] 架构已启动`

## 你接下来自己写系统时

在 `WowArchitercture.Init()` 里注册，例如：

```csharp
this.RegistSystem<IUISystem>(new UISystem());
this.RegistSystem<INetSystem>(new NetSystem());
```

对照魔兽课路径：  
`WowProject/Assets/Scripts/GameFramework/SpecificCode/`

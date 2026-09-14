using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//*****************************************
//功能说明：玩家数据信息
//***************************************** 
/// <summary>
/// 玩家数据对象
/// </summary>
[Serializable]
public class PlayerData
{
	/// <summary>角色显示名</summary>
	public string id;

	/// <summary>数据库 characters.id，未入库为 0</summary>
	public int characterId;

	public float x;
	public float y;
	public float z;
	public float ex;
	public float ey;
	public float ez;
	public int coin;
	public int hp;
	public int mp;
	public int level;
	public ROLE role;
	public GENDER gender;
	public int faceTypeId;
	public int hairStyleId;
	public int hairColorId;
	public int skinId;
	public int eyeColorId;
	public int faceEmotionId;

	/// <summary>打开创建角色界面时，把字段恢复成默认值（不 new 新对象）</summary>
	public void ResetToCreateDefaults()
	{
		id = "";
		characterId = 0;
		x = 0;
		y = 0;
		z = 0;
		ex = 0;
		ey = 0;
		ez = 0;
		coin = 0;
		hp = 100;
		mp = 50;
		level = 1;
		gender = GENDER.FEMALE;
		role = ROLE.WARRIOR;
		faceTypeId = 1;
		hairStyleId = 1;
		hairColorId = 9;
		skinId = 1;
		eyeColorId = 1;
		faceEmotionId = 1;
	}

	/// <summary>把另一个 PlayerData 的内容拷贝过来（选角列表、服务器下发时用）</summary>
	public void CopyFrom(PlayerData other)
	{
		if (other == null)
		{
			return;
		}

		id = other.id;
		characterId = other.characterId;
		x = other.x;
		y = other.y;
		z = other.z;
		ex = other.ex;
		ey = other.ey;
		ez = other.ez;
		coin = other.coin;
		hp = other.hp;
		mp = other.mp;
		level = other.level;
		role = other.role;
		gender = other.gender;
		faceTypeId = other.faceTypeId;
		hairStyleId = other.hairStyleId;
		hairColorId = other.hairColorId;
		skinId = other.skinId;
		eyeColorId = other.eyeColorId;
		faceEmotionId = other.faceEmotionId;
	}
}

/// <summary>
/// 玩家数据列表类，作为容器方便JsonUtility解析
/// </summary>
[Serializable]
public class PlayerDatasList
{
	public int choiceID;//当前选择人物的ID
	public List<PlayerData> playerDatas;
}

/// <summary>
/// 职业（与选角图标一致）
/// </summary>
public enum ROLE
{
	WARRIOR,    // 战士
	ALCHEMIST,  // 炼金师
	MARKSMAN,   // 射手
	MAGE,       // 法师
	ROGUE,      // 潜行者
	PALADIN,    // 圣骑士
	ASSASSIN,   // 刺客
	ENGINEER,   // 工程师
}

public enum GENDER
{
	MALE,
	FEMALE
}

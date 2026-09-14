using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：资源加载系统
//***************************************** 
public class GameResSystem : MonoBehaviour
{
    private static Dictionary<string, Object> resDict = new Dictionary<string, Object>();

    public static T GetRes<T>(string resPath) where T : Object
    {
        if (resDict.ContainsKey(resPath))
        {
            return resDict[resPath] as T;
        }
        else
        {
            Object res= Resources.Load(resPath);
            resDict.Add(resPath,res);
            return res as T;
        }
    }
}

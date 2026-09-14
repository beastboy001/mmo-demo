using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//功能说明：深度查找Transform组件
//***************************************** 
public class DeepFindTransform
{
    public static Transform DeepFindChild(Transform root,string childName)    //静态方法：可以直接调用，不需要创建实例
    {
        Transform result = null;
        result = root.Find(childName);  
        if (!result)  //如果在root的的子集中没有找到childName，就再遍历root的子集的子集
        {
            foreach (Transform item in root)   //递归调用
            {
                result = DeepFindChild(item,childName);   //此处的根节点就是item了
                if (result!=null)
                {
                    return result;
                }
            }
        }
        return result;
    }
}

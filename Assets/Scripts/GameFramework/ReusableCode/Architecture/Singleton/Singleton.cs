using System.Reflection;
using System;
//*****************************************
//功能说明：单例模板（实例化单例对象）
//***************************************** 
public class Singleton<T> where T : class, ISingleton    //泛型约束类Singleton<T> 本身不需要实现 ISingleton，所以不需要实现接口的类必须提供接口中所有成员的具体实现
{
    private static T mInstance;
    public static T Instance
    {
        get 
        {
            if (mInstance==null)
            {
                var ctors= typeof(T).GetConstructors(BindingFlags.Instance|BindingFlags.NonPublic);    //BindingFlags 是一个枚举类型，用于指定在反射搜索中要包含哪些成员。
                var ctor= Array.Find(ctors,c=>c.GetParameters().Length==0);           //这里的nonpublic是指使构造函数私有化，不会被外界new出来
                if (ctor==null)              //使用 Array.Find 方法在 ctors 数组中查找无参构造函数
                {
                    throw new Exception("没有找到非公共构造函数"+typeof(T));
                }
                mInstance= ctor.Invoke(null) as T;
            }
            return mInstance;
        }
    }
}

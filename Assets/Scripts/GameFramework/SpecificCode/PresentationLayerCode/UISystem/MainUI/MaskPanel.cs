using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//*****************************************
//功能说明：遮罩面板
//***************************************** 
public class MaskPanel : BasePanel
{
    private Image maskImg;

    public override void OnInit()
    {
        maskImg = GetComponent<Image>();
        base.OnInit();
    }

}

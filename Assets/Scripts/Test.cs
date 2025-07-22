using System.Collections;
using System.Collections.Generic;
using System.Text;
using ExcelEx;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        ConfigMgr.Init();
        var data = ConfigMgr.GetAll<RoleConfig>();
        var sb = new StringBuilder();
        foreach (var item in data)
        {
            sb.AppendLine($"Id:{item.id}");
            sb.AppendLine($"Name:{item.name}");
            sb.AppendLine($"Hp:{item.hp}");
            foreach (var pos in item.pos)
            {
                sb.AppendLine($"Pos:{pos}");
            }
            foreach (var target in item.target)
            {
                sb.AppendLine($"Target:{target}");
            }
            foreach (var duiyou in item.duiyou)
            {
                sb.AppendLine($"Duiyou:{duiyou}");
            }
            sb.AppendLine("--------------------------------");
        }
        text.text = $"{ConfigMgr.Get<RoleConfig>("1002").name} \n\n{sb.ToString()}";
    }

}

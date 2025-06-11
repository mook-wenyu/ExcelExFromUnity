using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    public Text text;
    // Start is called before the first frame update
    void Start()
    {
        CSVMgr.Init();
        var data = CSVMgr.GetAll<RoleConfig>();
        var sb = new StringBuilder();
        foreach (var item in data)
        {
            sb.AppendLine($"Id:{item.id} Name:{item.name} Hp:{item.hp} Pos:{item.pos.x},{item.pos.y},{item.pos.z} Target:{item.target.x},{item.target.y}");
        }
        text.text = $"{CSVMgr.Get<RoleConfig>("1002").name} \n\n{sb.ToString()}";
    }

}

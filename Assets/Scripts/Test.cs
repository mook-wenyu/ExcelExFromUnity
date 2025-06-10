using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        CSVMgr.Init();
        var data = CSVMgr.GetAll<RoleConfig>();
        var sb = new StringBuilder();
        foreach (var item in data)
        {
            sb.AppendLine(item.id);
            sb.AppendLine(item.name);
            sb.AppendLine(item.hp.ToString());
            sb.AppendLine(item.pos.x.ToString());
            sb.AppendLine(item.pos.y.ToString());
            sb.AppendLine(item.pos.z.ToString());
            sb.AppendLine(item.target.x.ToString());
            sb.AppendLine(item.target.y.ToString());
        }
        Debug.Log(sb.ToString());
        Debug.Log(CSVMgr.Get<RoleConfig>("1002").name);
    }

}

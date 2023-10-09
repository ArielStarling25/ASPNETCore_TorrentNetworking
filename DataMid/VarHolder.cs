using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMid
{
    public class VarHolder
    {
        public int? intValue {  get; set; }
        public string strValue { get; set; }

        public bool isInt()
        {
            return intValue != null;
        } 
    }
}

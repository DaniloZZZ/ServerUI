using System;
using System.Collections.Generic;
using System.Text;

namespace ServerUI.Data
{
    public sealed partial class Stack
    {
        int len;
        Type dtype;
        object[] arr { get; set; }
        public Stack(int l)
        {
            len = l;
            arr = new object[l];
        }
        public void Push(object val)
        {
            if (dtype == null) { dtype = val.GetType(); }

            for (int i = len-1; i > 0; i--)
            {
                arr[i] = arr[i - 1];
            }
            arr[0] = val;
            
        }
        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < len; i++)
            {
                if (arr[i]!=null)
                {
                    s += arr[i].ToString();
                    s += " \n";
                }
            }
            return s;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using hpMvc.Models;

namespace hpMvc.Infrastructure
{
    public static class ListToCsvUtils
    {
        public static MemoryStream ListToCsvStream<T>(List<T> list)
        {
            if (list == null || list.Count == 0) return null;

            //get type from 0th member
            Type t = list[0].GetType();
            string newLine = Environment.NewLine;


            using (var stream = new MemoryStream())
            {
                var sw = new StreamWriter(stream);

                //make a new instance of the class name we figured out to get its props
                object o = Activator.CreateInstance(t);
                //gets all properties
                PropertyInfo[] props = o.GetType().GetProperties();

                //foreach of the properties in class above, write out properties
                //this is the header row
                foreach (PropertyInfo pi in props)
                {
                    var val = AttributeHelpers.GetDisplayName(t, pi.Name, typeof(DisplayNameAttribute), "DisplayName");
                    if (val != null)
                        sw.Write(val + ",");
                    else
                        sw.Write(pi.Name + ",");
                }
                sw.Write(newLine);

                //this acts as datarow
                foreach (T item in list)
                {
                    //this acts as datacolumn
                    foreach (PropertyInfo pi in props)
                    {
                        //this is the row+col intersection (the value)
                        string whatToWrite =
                            Convert.ToString(item.GetType()
                                                 .GetProperty(pi.Name)
                                                 .GetValue(item, null))
                                .Replace(',', ' ') + ',';

                        sw.Write(whatToWrite);

                    }
                    sw.Write(newLine);
                }
                sw.Flush();

                return stream;
            }
        }

    }

}
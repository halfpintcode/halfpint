using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace hpMvc.Helpers
{
    public static class ListHelpers { 
        public static void Repeater<T>(this HtmlHelper html, IEnumerable<T> items, Action<T> render, Action<T> renderAlt) 
        { 
            if (items == null)
                return; 
            
            int i = 0; 
            items.ToList().ForEach(item => { 
                if (i++ % 2 == 0)
                    render(item); 
                else                    
                    renderAlt(item); 
            }); 
        } 
        
        public static void Repeater<T>(this HtmlHelper html, Action<T> render, Action<T> renderAlt) 
        { 
            var items = html.ViewContext.ViewData as IEnumerable<T>; 
            html.Repeater(items, render, renderAlt); 
        } 
        
        public static void Repeater<T>(this HtmlHelper html, string viewDataKey, Action<T> render, Action<T> renderAlt) 
        { 
            var items = html.ViewContext.ViewData as IEnumerable<T>; 
            var viewData = html.ViewContext.ViewData as IDictionary<string, object>; 
            
            if (viewData != null) 
            { 
                items = viewData[viewDataKey] as IEnumerable<T>; 
            } 
            else 
            { 
                items = new ViewDataDictionary(viewData)[viewDataKey] as IEnumerable<T>; 
            } 
            
            html.Repeater(items, render, renderAlt); 
        } 
        
        public static void Repeater<T>(this HtmlHelper html, IEnumerable<T> items, string className, string classNameAlt, Action<T, string> render) 
        { 
            if (items == null)                
                return; 
            
            int i = 0; 
            foreach (var item in items) 
            { 
                render(item, (i++ % 2 == 0) ? className : classNameAlt); 
            } 
        } 
    }
}
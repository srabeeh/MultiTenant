using System;
using System.Collections.Generic;
using System.Web;
using WebApp.Models;

namespace WebApp
{
    public class TCache<T>
    {
         private static readonly object Locker = new object();
        public static T Get(string cacheName, int cacheTimeOutSeconds, Func<T> func)
        {
            var obj = HttpContext.Current.Cache.Get(cacheName);

            if (obj != null)
            {
                return (T) obj;
            }

            lock (Locker)
            {
                obj = HttpContext.Current.Cache.Get(cacheName);

                if (obj == null)
                {
                    obj = func();
                    HttpContext.Current.Cache.Insert(cacheName, obj, null, DateTime.Now.Add(new TimeSpan(0,0, cacheTimeOutSeconds)), TimeSpan.Zero);
                }

                return (T) obj;
            }
        }

   
    }
}
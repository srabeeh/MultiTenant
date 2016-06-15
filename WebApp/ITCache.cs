using System;

namespace WebApp
{
    public interface ITCache<T>
    {
        T Get(string cacheKeyName, int cacheTimeOutSeconds, Func<T> func);
    }
}
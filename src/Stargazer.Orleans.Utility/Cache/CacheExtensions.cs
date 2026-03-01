using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Stargazer.Orleans.Utility.Cache
{
    public static class CacheExtensions
    {
        /// <summary>
        /// 使用给定的键缓存值。
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static async Task SetValueAsync(this IDistributedCache cache, string key, string value, TimeSpan expiry)
        {
            byte[] encodedValue = Encoding.UTF8.GetBytes(value);
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(expiry);
            await cache.SetAsync(key, encodedValue, options);
        }

        /// <summary>
        /// 使用给定的键缓存值。
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static void SetValue(this IDistributedCache cache, string key, string value, TimeSpan expiry)
        {
            byte[] encodedValue = Encoding.UTF8.GetBytes(value);
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(expiry);
            cache.Set(key, encodedValue, options);
        }

        /// <summary>
        /// 使用给定的键缓存值。
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static async Task SetValueAsync<T>(this IDistributedCache cache, string key, T value, TimeSpan expiry) where T : class, new()
        {
            string valueStr = JsonConvert.SerializeObject(value);
            byte[] encodedValue = Encoding.UTF8.GetBytes(valueStr);
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(expiry);
            await cache.SetAsync(key, encodedValue, options);
        }


        /// <summary>
        /// 使用给定的键缓存值。
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static void SetValue<T>(this IDistributedCache cache, string key, T value, TimeSpan expiry) where T : class, new()
        {
            string valueStr = JsonConvert.SerializeObject(value);
            byte[] encodedValue = Encoding.UTF8.GetBytes(valueStr);
            var options = new DistributedCacheEntryOptions().SetSlidingExpiration(expiry);
            cache.Set(key, encodedValue, options);
        }

        /// <summary>
        /// 获取指定key的值
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        public static async Task<T?> GetValueAsync<T>(this IDistributedCache cache, string key) where T : class, new()
        {
            var value = await cache.GetAsync(key);

            if (value == null)
            {
                return null;
            }

            string result = Encoding.UTF8.GetString(value);
            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// 获取指定key的值
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <typeparam name="T"></typeparam>
        public static T? GetValue<T>(this IDistributedCache cache, string key) where T : class, new()
        {
            var value = cache.Get(key);

            if (value == null)
            {
                return default(T);
            }

            string result = Encoding.UTF8.GetString(value);
            return JsonConvert.DeserializeObject<T>(result);
        }

        /// <summary>
        /// 获取指定key的值
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<string?> GetValueAsync(this IDistributedCache cache, string key)
        {
            var value = await cache.GetAsync(key);

            if (value == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(value);
        }

        /// <summary>
        /// 获取指定key的值
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string? GetValue(this IDistributedCache cache, string key)
        {
            var value = cache.Get(key);

            if (value == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(value);
        }

        /// <summary>
        /// 移除指定键值的数据
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task RemoveValueAsync(this IDistributedCache cache, string key)
        {
            await cache.RemoveAsync(key);
        }

        /// <summary>
        /// 移除指定键值的数据
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static void RemoveValue(this IDistributedCache cache, string key)
        {
            cache.Remove(key);
        }
    }
}

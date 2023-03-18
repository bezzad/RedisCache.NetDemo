﻿namespace RedisCacheDemo.Cache
{
    public interface ICacheService
    {
        /// <summary>
        /// Get data using key and if it's not exist create new data and cache it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="acquire">data generator async method</param>
        /// <param name="expireAfterSeconds">Seconds of expiration after now</param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string key, Func<Task<T>> acquire, int expireAfterSeconds);

        /// <summary>
        /// Get data using key and if it's not exist create new data and cache it
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="acquire">data generator method</param>
        /// <param name="expireAfterSeconds">Seconds of expiration after now</param>
        /// <returns></returns>
        T Get<T>(string key, Func<T> acquire, int expireAfterSeconds);

        /// <summary>
        /// Get data using key and if it's not exist get default value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);

        /// <summary>
        /// Gets the item associated with this key if present.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">An object identifying the requested entry.</param>
        /// <param name="value">The located value or null.</param>
        /// <returns>True if the key was found.</returns>
        bool TryGetValue<T>(string key, out T value);

        /// <summary> 
        /// Set data with Value and Expiration Time of Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationTime"></param>
        /// <returns></returns>
        bool SetData<T>(string key, T value, DateTimeOffset expirationTime);

        /// <summary> 
        /// Set data as async with Value and Expiration Time of Key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expirationTime"></param>
        Task<bool> SetDataAsync<T>(string key, T value, DateTimeOffset expirationTime);

        /// <summary>
        /// Remove Data 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Remove(string key);

        /// <summary>
        /// Clear all data
        /// </summary>
        void Clear();

        /// <summary>
        /// Clear all data
        /// </summary>
        Task ClearAsync();
    }
}

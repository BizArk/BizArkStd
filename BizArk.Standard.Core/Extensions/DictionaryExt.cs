﻿using System.Collections.Generic;

namespace BizArk.Standard.Core.Extensions.DictionaryExt
{

    /// <summary>
    /// Provides extension methods to make it easier to work with dictionaries.
    /// </summary>
    public static class DictionaryExt
    {

        #region Convert

        /// <summary>
        /// Gets the specified value from the dictionary. Converts to the given type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key">The key to the dictionary.</param>
        /// <param name="dflt">Returned if the key is not found or if the dictionary is null.</param>
        /// <returns></returns>
        public static T TryGetValue<T>(this IDictionary<string, object> dict, string key, T dflt = default(T))
        {
            return TryGetValue<string, object, T>(dict, key, dflt);
        }

        /// <summary>
        /// Gets the specified value from the dictionary. Converts to the given type.
        /// </summary>
        /// <typeparam name="TKey">Key type for the dictionary.</typeparam>
        /// <typeparam name="TVal">Value type for the dictionary.</typeparam>
        /// <typeparam name="TRet">Type to convert the value to.</typeparam>
        /// <param name="dict"></param>
        /// <param name="key">The key to the dictionary.</param>
        /// <param name="dflt">Returned if the key is not found or if the dictionary is null.</param>
        /// <returns></returns>
        public static TRet TryGetValue<TKey, TVal, TRet>(this IDictionary<TKey, TVal> dict, TKey key, TRet dflt = default(TRet))
        {
            if (dict == null) return dflt;
            if (!dict.ContainsKey(key)) return dflt;

            var val = dict[key];
            return ConvertEx.To<TRet>(val);
        }

        #endregion

    }
}

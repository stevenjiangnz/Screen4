using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Screen.Utils
{
    public static class ObjectHelper
    {
        /// <summary>
        /// Clone object by serialization
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T CloneBySerialize<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // Serialize the object to a JSON string and then deserialize it back to a clone
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        /// <summary>
        /// Object to JSON string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <param name="isIndented"></param>
        /// <returns></returns>
        public static string ToJsonString<T>(this T input, bool isIndented = false)
        {
            if (input != null)
            {
                if (isIndented)
                {
                    return JsonConvert.SerializeObject(input, Newtonsoft.Json.Formatting.Indented);
                }

                return JsonConvert.SerializeObject(input);
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// JSON string to object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static T FromJsonString<T>(string inputString)
        {
            return JsonConvert.DeserializeObject<T>(inputString);
        }

        /// <summary>
        /// Get the property value from an object
        /// </summary>
        /// <param name="src"></param>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        /// <summary>
        /// Timeout the task based on the configuration
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="task"></param>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int milliseconds)
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {
                var completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromMilliseconds(milliseconds), timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }

                throw new TimeoutException("The operation has timed out.");
            }
        }

        /// <summary>
        /// Timeout the task based on the configuration
        /// </summary>
        /// <param name="task"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task TimeoutAfter(this Task task, int timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            {
                await task;
            }
            else
            {
                throw new TimeoutException("The operation has timed out.");
            }
        }

        /// <summary>
        /// Check if the list contains the substring
        /// </summary>
        /// <param name="stringList"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public static bool IsContainSubString(this List<string> stringList, string substring)
        {
            bool IsContain = false;

            if (stringList != null && stringList.Count > 0)
            {
                foreach (var itemString in stringList)
                {
                    if (itemString.Contains(substring))
                    {
                        IsContain = true;
                        break;
                    }
                }
            }

            return IsContain;
        }
    }
}

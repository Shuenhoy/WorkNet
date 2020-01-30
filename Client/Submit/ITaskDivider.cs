using WorkNet.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace WorkNet.Client.Submit
{
    public interface ITaskDivider
    {
        IEnumerable<TaskGroup> Divide(RawTaskGroup group);
    }
    public static class DistinctByExtension
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
     (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
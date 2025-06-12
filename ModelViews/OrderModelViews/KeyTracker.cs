﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelViews.OrderModelViews
{
    public static class KeyTracker
    {
        public static HashSet<string> OrderKeys { get; } = new HashSet<string>();

        public static void AddKey(string key)
        {
            lock (OrderKeys)
            {
                OrderKeys.Add(key);
            }
        }

        public static void RemoveKey(string key)
        {
            lock (OrderKeys)
            {
                OrderKeys.Remove(key);
            }
        }

        public static List<string> GetKeysByPrefix(string prefix)
        {
            lock (OrderKeys)
            {
                return OrderKeys.Where(k => k.StartsWith(prefix)).ToList();
            }
        }
    }
}

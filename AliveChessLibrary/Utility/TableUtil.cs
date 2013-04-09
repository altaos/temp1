﻿#if !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;

namespace AliveChessLibrary.Utility
{
    public static class TableUtil
    {
        public static void DoAction<T>(this Table<T> source, Func<T, bool> selector,
           Action<T> action) where T : class
        {
            IEnumerable<T> target = source.Where(selector);
            foreach (var item in target)
                action(item);
        }
    }
}
#endif

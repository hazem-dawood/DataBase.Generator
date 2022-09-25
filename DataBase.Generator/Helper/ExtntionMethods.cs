using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataBase.Generator
{
    public static class ExtntionMethods
    {

        public static void DeepCopy(this DirectoryInfo directory, string destinationDir)
        {
            foreach (string dir in Directory.GetDirectories(directory.FullName, "*", SearchOption.AllDirectories))
            {
                string dirToCreate = dir.Replace(directory.FullName, destinationDir);
                Directory.CreateDirectory(dirToCreate);
            }

            foreach (string newPath in Directory.GetFiles(directory.FullName, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(directory.FullName, destinationDir), true);
            }
        }
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
    (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        public static List<T> DataReaderMapToList<T>(this IDataReader dr)
        {
            List<T> list = new List<T>();
            T obj = default;

            var allColumns = Enumerable.Range(0, dr.FieldCount).Select(x => dr.GetName(x) + "").ToList();

            var props = typeof(T).GetProperties().Where(x => x.CanWrite && allColumns.Any(c => c == x.Name)).ToList();

            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                foreach (PropertyInfo prop in props)
                {
                    try
                    {
                        var col = dr[prop.Name];

                        if (!Equals(col, DBNull.Value))
                        {
                            prop.SetValue(obj, GetConvertedValue(col, prop), null);
                        }
                    }
                    catch (Exception ex)
                    {
                        _ = ex.Message;
                    }
                }

                list.Add(obj);
            }
            return list;
        }

        private static object GetConvertedValue(this object col, PropertyInfo pro)
        {
            if (pro.PropertyType == col.GetType())
            {
                return col;
            }

            var ty = Nullable.GetUnderlyingType(pro.PropertyType);
            if (ty == null) ty = pro.PropertyType;

            if (ty == typeof(bool))
            {
                if (bool.TryParse(col.ToString(), out var v))
                    col = v;
                else if (col.ToString() == "0" || col.ToString() == "1")
                {
                    col = col.ToString() == "1";
                }
            }
            else if (ty == typeof(string))
            {
                col = col.ToString();
            }
            else if (ty.IsEnum)
            {

            }
            else if (ty == typeof(long))
            {
                col = col.ToString().AsLong();
            }
            else if (ty == typeof(int))
            {
                col = col.ToString().AsInt();
            }
            else if (ty == typeof(double))
            {
                col = col.ToString().AsDouble();
            }
            else if (ty == typeof(float))
            {
                col = col.ToString().AsFloat();
            }
            else if (ty == typeof(DateTime))
            {
                if (!DateTime.TryParse(col + "", new CultureInfo("en"), DateTimeStyles.None, out DateTime date))
                {
                    if (!(col + "").GetDateTimeFromString("MM/dd/yyyy", out date))
                    {
                        if (!(col + "").GetDateTimeFromString("MM-dd-yyyy", out date))
                        {
                            if (!(col + "").GetDateTimeFromString("MM-dd-yyyy", out date))
                            {
                                if (!(col + "").GetDateTimeFromString("dd-MM-yyyy", out date))
                                {
                                    return col;
                                }
                            }
                        }
                    }
                }
                col = date;
            }
            return col;
        }

        public static bool GetDateTimeFromString(this string s, string format, out DateTime date)
        {
            return DateTime.TryParseExact(s, format, new CultureInfo("de-DE"), DateTimeStyles.None, out date);
        }

        /// <summary>
        /// Check If This Field Is Empty Or Null or "" or white space
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsEmpty(this string s) => s == null || string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s);


        /// <summary>
        /// Check If This Field Is Empty Or Null or "" or white space
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsEmpty(this object s) => s == null || string.IsNullOrEmpty(s.ToString()) || string.IsNullOrWhiteSpace(s.ToString());

        /// <summary>
        /// convert string to long if it is a valid number else return <see cref="default"/>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static long AsLong(this string s)
        {
            if (!s.IsEmpty() && long.TryParse(s, out long l))
            {
                return l;
            }
            return default;
        }

        public static int AsInt(this string s)
        {
            if (!s.IsEmpty() && int.TryParse(s, out var l))
            {
                return l;
            }
            return default;
        }

        public static double AsDouble(this string s)
        {
            if (!s.IsEmpty() && double.TryParse(s, out var l))
            {
                return l;
            }
            return default;
        }

        public static float AsFloat(this string s)
        {
            if (!s.IsEmpty() && float.TryParse(s, out var l))
            {
                return l;
            }
            return default;
        }

    }
}

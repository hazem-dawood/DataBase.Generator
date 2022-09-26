using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using DataBase.Generator.Models;

namespace DataBase.Generator
{
    public static class ExtntionMethods
    {
       
        public static string GetNavBar(this string s)
        {
            s = s.Replace(Form1.Script, @" <script src='lib/admin-lte/plugins/jQuery/jquery-2.2.3.min.js'></script>
    <script src='lib/admin-lte/plugins/jQueryUI/jquery-ui.min.js'></script>
    <script src='lib/admin-lte/bootstrap/js/bootstrap.min.js'></script>
    <script src='lib/datatables.net/jquery.dataTables.min.js'></script>
    <script src='lib/datatables.net-bs/js/dataTables.bootstrap.min.js'></script>
    <script src='lib/datatables.net-buttons/dataTables.buttons.min.js'></script>
    <script src='lib/datatables.net-buttons-bs/js/buttons.bootstrap.min.js'></script>
    <script src='lib/datatables.net-buttons/buttons.html5.min.js'></script>
    <script src='lib/datatables.net-buttons/buttons.print.min.js'></script>
    <script src='lib/datatables.net-buttons/buttons.colVis.min.js'></script>
    <script src='lib/js-xlsx/xlsx.full.min.js'></script>
    <script src='lib/pdfmake/pdfmake.min.js'></script>
    <script src='lib/pdfmake/vfs_fonts.js'></script>
    <script src='lib/admin-lte/plugins/slimScroll/jquery.slimscroll.min.js'></script>
    <script src='lib/admin-lte/plugins/fastclick/fastclick.js'></script>
    <script src='lib/salvattore/salvattore.min.js'></script>
    <script src='lib/anchor-js/anchor.min.js'></script>
    <script src='lib/codemirror/codemirror.js'></script>
    <script src='lib/codemirror/sql.js'></script>
    <script src='lib/admin-lte/dist/js/app.min.js'></script><script src='main.js''></script>");
            s = s.Replace(Form1.HeadTag, $@"<head>
    <meta charset='utf-8'>
    <meta http-equiv='content-type' content='text/html;charset=utf-8' />
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <title>Database</title>

    <meta content='width=device-width, initial-scale=1, maximum-scale=1, user-scalable=no' name='viewport'>
    <link rel='icon' type='image/png' sizes='16x16' href='favicon.png'>
    <link rel='stylesheet' href='lib/admin-lte/bootstrap/css/bootstrap.min.css'>
    <link rel='stylesheet' href='lib/font-awesome/css/font-awesome.min.css'>
    <link rel='stylesheet' href='lib/ionicons/css/ionicons.min.css'>
    <link rel='stylesheet' href='lib/datatables.net-bs/css/dataTables.bootstrap.min.css'>
    <link rel='stylesheet' href='lib/datatables.net-buttons-bs/css/buttons.bootstrap.min.css'>
    <link rel='stylesheet' href='lib/codemirror/codemirror.css'>
    <link href='fonts/indieflower/indie-flower.css' rel='stylesheet' type='text/css'>
    <link href='fonts/source-sans-pro/source-sans-pro.css' rel='stylesheet' type='text/css'>
    <link rel='stylesheet' href='lib/admin-lte/dist/css/AdminLTE.min.css'>
    <link rel='stylesheet' href='lib/salvattore/salvattore.css'>
    <link rel='stylesheet' href='lib/admin-lte/dist/css/skins/_all-skins.min.css'>
    <link rel='stylesheet' href='database.css'>
    <!--[if lt IE 9]>
    <script src='lib/html5shiv/html5shiv.min.js'></script>
    <script src='lib/respond/respond.min.js'></script>
    <![endif]-->
</head>");
            s = s.Replace(Form1.navBar, $@" <header class='main-header'>
            <nav class='navbar navbar-static-top'>
                <div class='container'>
                    <div class='navbar-header'>
                        <a href='index.html' class='navbar-brand'><b>Jeddah</b> Database</a>
                        <button type='button' class='navbar-toggle collapsed' data-toggle='collapse' data-target='#navbar-collapse'>
                            <i class='fa fa-bars'></i>
                        </button>
                    </div>
                    <div class='collapse navbar-collapse pull-left' id='navbar-collapse'>
                        <ul class='nav navbar-nav'>
                            <li><a href='index.html'>Tables <span class='sr-only'>(current)</span></a></li>
                            <li><a href='columns.html' title='All of the columns in the schema'>Columns</a></li>
                            <li><a href='constraints.html' title='Useful for diagnosing error messages that just give constraint name or number'>Constraints</a></li>
                            <li><a href='relationships.html' title='Diagram of table relationships'>Relationships</a></li>
                            <li><a href='orphans.html' title='View of tables with neither parents nor children'>Views</a></li>
                            <li><a href='anomalies.html' title='Things that might not be quite right'>Stored Procedures</a></li>
                            <li><a href='routines.html' title='Procedures and functions'>Schemas</a></li>
                        </ul>
                    </div>
                </div>
            </nav>
        </header>");
            return s.Replace(Form1.footer, $@" <footer class='main-footer'>
            <div>
                <strong>Generated by <a href='#' class='logo-text'><i class='fa fa-database'></i> Matrix</a></strong>
            </div>
        </footer>");
        }
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

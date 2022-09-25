using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DataBase.Generator
{
    public partial class Form1 : Form
    {
        private const string ResourceTemp = "Resources1";
        private readonly string query;
        private readonly string currentDirectory;

        private string connectionString = "Persist Security Info=true;Server=108.170.14.114\\SQLJMDDS;Database=jmdssnew;user id=sa;password=matrix@#200;MultipleActiveResultSets=true;";

        private const string TablesCount = "@TablesCount";
        private const string Indexes = "@Indexes";
        private const string Relations = "@Relations";
        private const string ViewsCount = "@ViewsCount";
        private const string ColumnsCount = "@ColumnsCount";
        private const string ConstraintsCount = "@ConstraintsCount";
        private const string SchemasCount = "@SchemasCount";
        private const string StoredProceduresCount = "@StoredProceduresCount";


        private const string TablesAndViewsTrs = "@TablesAndViewsTrs";

        public Form1()
        {
            currentDirectory = Directory.GetCurrentDirectory().Replace(@"bin\Debug", "");
            query = File.ReadAllText(currentDirectory + "Resources\\" + "sql.sql");
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var data = ExecuteStoredProcedure<ListAlldata>(query);
            if (data.Count > 0)
            {
                CopyResToRes2();

                var allTablesWithCoulmns = data.Where(x => x.Type == "Table").ToList();
                var allTriggers = data.Where(x => x.Type == "Trigger").ToList();
                var allStoredProcedure = data.Where(x => x.Type == "Stored Procedure").ToList();
                var allViewsWithCoulmns = data.Where(x => x.Type == "View").ToList();
                var allConstrain = data.Where(x => x.Type == "Constrain").ToList();
                var allIndexs = data.Where(x => x.Type == "Index").ToList();

                var allTables = allTablesWithCoulmns.DistinctBy(x => x.TableName).ToList();
                var allViews = allViewsWithCoulmns.DistinctBy(x => x.TableName).ToList();

                var index = File.ReadAllText(currentDirectory + ResourceTemp + "\\index.html");
                index = BuildIndex(index, allTablesWithCoulmns, allViewsWithCoulmns,
                    data, allTables, allViews,
                    allStoredProcedure, allIndexs,
                    allConstrain);

            }

        }

        private string BuildIndex(string index, List<ListAlldata> allTablesWithCoulmns,
            List<ListAlldata> allViewsWithCoulmns, List<ListAlldata> data,
            List<ListAlldata> allTables, List<ListAlldata> allViews, List<ListAlldata> allStoredProcedure,
             List<ListAlldata> allIndexs, List<ListAlldata> allConstrain)
        {
            index = index.Replace(TablesCount, allTables.Count() + "");
            index = index.Replace(ViewsCount, allViews.Count() + "");
            index = index.Replace(ColumnsCount, allTablesWithCoulmns.Count + "");
            index = index.Replace(ConstraintsCount, allTablesWithCoulmns.Count + "");
            index = index.Replace(SchemasCount, data.Select(c => c.SchemaName)
                .DistinctBy(x => x).Count() + "");
            index = index.Replace(StoredProceduresCount, allStoredProcedure.Count + "");
            var trs = "";
            foreach (var item in allTablesWithCoulmns.GroupBy(x =>
            new { x.TableName, x.SchemaName, x.Type }).
                Concat(allViewsWithCoulmns.GroupBy(x =>
            new { x.TableName, x.SchemaName, x.Type })))
            {
                trs += $@"<tr class='tbl even' valign='top'>
                                    <td class='detail'>{item.Key.SchemaName}</td>
                                    <td class='detail'><a href='tables/{item.Key.TableName}.html'>{item.Key.TableName}</a></td>
                                    <td class='detail' align='right'>{item.Count()}</td>
                                    <td class='detail' align='right'>{item.Key.Type}</td>
                                    <td class='comment detail' style='display: table-cell;'></td>
                                </tr>";
                BuildTablePages(item, allIndexs, allConstrain) ;

            }
            index = index.Replace(TablesAndViewsTrs, trs);
            File.WriteAllText(currentDirectory + ResourceTemp + "\\index.html", index);
            return index;
        }

        private void BuildTablePages(IGrouping<dynamic, ListAlldata> item, List<ListAlldata> allIndexs, 
            List<ListAlldata> allConstrain)
        {
            var tempFile = File.ReadAllText(currentDirectory + "Resources\\tables\\index.html");
            var trs = "";
            foreach (var column in item)
            {
                var isNull = column.IsNullable ? "Null" : "Not Null";
                trs += $@"<tr><td class='primaryKey' title='Primary Key'>
                                        <span id=''>{column.ColumnName}</span>
                                    </td>
                                    <td>{column.DataType}</td>
                                    <td>{isNull}</td>
                                    <td></td>
                                </tr>";
            }
            tempFile = tempFile.Replace(TablesAndViewsTrs, trs);
            tempFile = tempFile.Replace(ColumnsCount, item.Count() + "");

            var indx = "";
            foreach (var indexx in allIndexs.Where(x => x.TableName == item.Key.TableName))
            {
                indx += $@"<tr><td class='primaryKey' title='Primary Key'>
                                       {indexx.ColumnName}
                                    </td>
                                    <td>{indexx.DataType}</td>
                                </tr>";
            }
            tempFile = tempFile.Replace(Indexes, indx);

            indx = "";
            foreach (var indexx in allConstrain.Where(x => x.TableName == item.Key.TableName))
            {
                indx += $@"<tr><td class='primaryKey' title='Primary Key'>
                                        <i class='icon ion-key iconkey'></i> {indexx.ColumnName}
                                    </td>
                                    <td>{indexx.DataType}</td>
                                </tr>";
            }
            tempFile = tempFile.Replace(Relations, indx);

            File.WriteAllText(currentDirectory + $"{ResourceTemp}\\tables\\{item.Key.TableName}.html",tempFile);
        }

        private void CopyResToRes2()
        {
            var dir = new DirectoryInfo(currentDirectory + "Resources");
            var createDir = Directory.CreateDirectory(currentDirectory + ResourceTemp);
            dir.DeepCopy(createDir.FullName);
        }

        public List<U> ExecuteStoredProcedure<U>(string query, CommandType commandType = CommandType.Text)
        {
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                {
                    cmd.CommandTimeout = 60 * 20;
                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();

                    cmd.CommandType = commandType;
                    var reader = cmd.ExecuteReader();

                    return reader.DataReaderMapToList<U>();
                }
            }
        }
    }
    public class ListAlldata
    {
        public bool IsNullable { get; set; }

        public string SchemaName { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public string DataType { get; set; }

        public int IsUpdate { get; set; }

        public int IsDelete { get; set; }

        public int IsInsert { get; set; }

        public int IsAfter { get; set; }

        public int IsInsteadOf { get; set; }

        public int Disabled { get; set; }

        public string Type { get; set; }


    }
    public static class Extentions
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

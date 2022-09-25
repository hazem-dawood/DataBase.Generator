using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using DataBase.Generator.Models;

namespace DataBase.Generator
{
    public partial class Form1 : Form
    {
        internal const string navBar = "@NavBar";
        internal const string footer = "@Footer";
        internal const string Schemas = "@Schemas";

        private const string ResourceTemp = "Resources1";
        private readonly string query;
        private readonly string currentDirectory;

        private readonly string connectionString = "Persist Security Info=true;Server=108.170.14.114\\SQLJMDDS;Database=jmdssnew;user id=sa;password=matrix@#200;MultipleActiveResultSets=true;";

        private const string TablesCount = "@TablesCount";
        private const string Indexes = "@Indexes";
        private const string Relations = "@Relations";
        private const string ViewsCount = "@ViewsCount";
        private const string ColumnsCount = "@ColumnsCount";
        private const string ConstraintsCount = "@ConstraintsCount";
        private const string SchemasCount = "@SchemasCount";
        private const string StoredProceduresCount = "@StoredProceduresCount";


        private const string TablesAndViewsTrs = "@TablesAndViewsTrs";
        private const string DataBaseDetails = "@DataBaseDetails";

        public Form1()
        {
            currentDirectory = Directory.GetCurrentDirectory().Replace(@"bin\Debug", "");
            query = File.ReadAllText(currentDirectory + "Resources\\" + "sql.sql");
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var loadServerDetails = new ServerDetails();
            try
            {
                loadServerDetails = ExecuteStoredProcedure<ServerDetails>
                (@"SELECT [Edition]= cast(SERVERPROPERTY('Edition') as nvarchar(max)),[Product_Version]= cast(SERVERPROPERTY('ProductVersion') as nvarchar(max)),
                    [Collation]= cast(SERVERPROPERTY('Collation') as nvarchar(max)),[CLR_Version] = cast(SERVERPROPERTY('BuildClrVersion') as nvarchar(max))")
                .FirstOrDefault();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                return;
            }

            var data = ExecuteStoredProcedure<ListAlldata>(query);
            if (data.Count > 0)
            {
                CopyResToRes2();
                BuildHtml(loadServerDetails, data);
            }
        }

        private void BuildHtml(ServerDetails loadServerDetails, List<ListAlldata> data)
        {
            var allTablesWithCoulmns = data.Where(x => x.Type == "Table").ToList();
            var allTriggers = data.Where(x => x.Type == "Trigger").ToList();
            var allStoredProcedure = data.Where(x => x.Type == "Stored Procedure").ToList();
            var allViewsWithCoulmns = data.Where(x => x.Type == "View").ToList();
            var allConstrain = data.Where(x => x.Type == "Constrain").ToList();
            var allIndexs = data.Where(x => x.Type == "Index").ToList();

            var allTables = allTablesWithCoulmns.DistinctBy(x => x.TableName).ToList();
            var allViews = allViewsWithCoulmns.DistinctBy(x => x.TableName).ToList();

            var index = File.ReadAllText(currentDirectory + ResourceTemp + "\\index.html")
                .GetNavBar();

            index = BuildIndex(data, allTablesWithCoulmns, allStoredProcedure,
                allTables, allViews, index);

            var trs = "";
            IEnumerable<IGrouping<GroupBuListData, ListAlldata>> source1 =
                LoadAllTablesAndViews(allTablesWithCoulmns, allViewsWithCoulmns);

            foreach (var item in source1)
            {
                trs += $@"<tr class='tbl even' valign='top'>
                                    <td class='detail'>{item.Key.SchemaName}</td>
                                    <td class='detail'><a href='tables/{item.Key.TableName}.html'>{item.Key.TableName}</a></td>
                                    <td class='detail' align='right'>{item.Count()}</td>
                                    <td class='detail' align='right'>{item.Key.Type}</td>
                                    <td class='comment detail' style='display: table-cell;'></td>
                                </tr>";
                BuildTablePages(item, allIndexs, allConstrain);

            }
            index = index.Replace(TablesAndViewsTrs, trs);
            index = index.Replace(DataBaseDetails, loadServerDetails.GetDetailsString());
            File.WriteAllText(currentDirectory + ResourceTemp + "\\index.html", index);

            // create columns

            var columns = File.ReadAllText(currentDirectory + ResourceTemp + "\\columns.html").GetNavBar();
            var cls = allTablesWithCoulmns.Select(x => x.GetForAllColumns())
                .Aggregate((a, b) => a + b);

            columns = columns.Replace(ColumnsCount, cls);
            File.WriteAllText(currentDirectory + ResourceTemp + "\\columns.html", columns);

            var schemas = File.ReadAllText(currentDirectory + ResourceTemp + "\\routines.html").GetNavBar();
            var rts = allTables.Select(x => x.SchemaName).Distinct()
                .Aggregate((a, b) => a + b);

            columns = columns.Replace(Schemas, rts);
            File.WriteAllText(currentDirectory + ResourceTemp + "\\routines.html", columns);
        }

        private static IEnumerable<IGrouping<GroupBuListData, ListAlldata>> 
            LoadAllTablesAndViews(List<ListAlldata> allTablesWithCoulmns, List<ListAlldata> allViewsWithCoulmns)
        {
            var source1 = allTablesWithCoulmns.GroupBy(x =>
            new GroupBuListData
            {
                TableName = x.TableName,
                SchemaName = x.SchemaName,
                Type = x.Type
            });
            source1 = source1.Concat(allViewsWithCoulmns.GroupBy(x =>
            new GroupBuListData
            {
                TableName = x.TableName,
                SchemaName = x.SchemaName,
                Type = x.Type
            }));
            return source1;
        }

        private static string BuildIndex(List<ListAlldata> data, List<ListAlldata> allTablesWithCoulmns,
            List<ListAlldata> allStoredProcedure, List<ListAlldata> allTables, List<ListAlldata> allViews, string index)
        {
            index = index.Replace(TablesCount, allTables.Count() + "");
            index = index.Replace(ViewsCount, allViews.Count() + "");
            index = index.Replace(ColumnsCount, allTablesWithCoulmns.Count + "");
            index = index.Replace(ConstraintsCount, allTablesWithCoulmns.Count + "");
            index = index.Replace(SchemasCount, data.Select(c => c.SchemaName)
                .DistinctBy(x => x).Count() + "");
            index = index.Replace(StoredProceduresCount, allStoredProcedure.Count + "");
            return index;
        }

        private void BuildTablePages(IGrouping<GroupBuListData, ListAlldata> item, List<ListAlldata> allIndexs,
            List<ListAlldata> allConstrain)
        {
            var tempFile = File.ReadAllText(currentDirectory + "Resources\\tables\\index.html").GetNavBar();
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

            File.WriteAllText(currentDirectory + $"{ResourceTemp}\\tables\\{item.Key.TableName}.html", tempFile);
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
}

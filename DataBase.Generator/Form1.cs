﻿using System;
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
        internal const string Script = "@Script";
        internal const string HeadTag = "@Head";
        internal const string StoredProcedures = "@StoredProcedures";

        private const string ResourceTemp = "Resources1";
        private readonly string query;
        private readonly string currentDirectory;

        private string connectionString = "Persist Security Info=true;Server={};Database=jmdssnew;user id={};password={};MultipleActiveResultSets=true;";

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
            txtServer.Text = "108.170.14.114\\SQLJMDDS";
            txtUser.Text = "sa";
            txtPassword.Text = "matrix@#200";
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (cbDataBase.SelectedItem == null || cbDataBase.SelectedItem.ToString().IsEmpty() == true)
            {
                MessageBox.Show("Please Select a database");
                return;
            }
            connectionString = $@"Persist Security Info=true;Server={txtServer.Text};
Database={cbDataBase.SelectedItem};user id={txtUser.Text};password={txtPassword.Text};MultipleActiveResultSets=true;";
            if (Directory.Exists(currentDirectory + ResourceTemp))
            {
                Directory.Delete(currentDirectory + ResourceTemp, true);
            }

            ServerDetails loadServerDetails;
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

            var allTypes = data.Where(x => x.Type == "Types").ToList();

            var allTables = allTablesWithCoulmns.DistinctBy(x => x.TableName).ToList();
            var allViews = allViewsWithCoulmns.DistinctBy(x => x.TableName).ToList();

            var erd = new BuildTablesToERD().LoadString(allTablesWithCoulmns,
                allConstrain);

            var index = File.ReadAllText(currentDirectory + ResourceTemp + "\\index.html")
                .GetNavBar();

            index = BuildIndex(allTablesWithCoulmns, allStoredProcedure,
                allTables, allViews, index);

            var trs = "";
            var source1 =
                LoadAllTablesAndViews(allTablesWithCoulmns, allViewsWithCoulmns);

            foreach (var item in source1
                .GroupBy(x => new
                {
                    x.TableName,
                    x.SchemaName,
                    x.Type
                }))
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
            BuildTypesPage(allTypes);
            BuildColumnPage(allTablesWithCoulmns);
            BuildSchemas(allTables);
            BuildStoredProcedures(allStoredProcedure);
            BuildViewsPage(allViews);
            BuildConstrainPage(allConstrain);
            BuildRelationShipsPage(allTables);
            BuildSchemaTablesPage(allTablesWithCoulmns);
        }

        private void BuildTypesPage(List<ListAlldata> allTypes)
        {
            var index = File.ReadAllText(currentDirectory + ResourceTemp + "\\types.html")
                .GetNavBar();
            var trs = "";
            foreach (var item in allTypes
                .GroupBy(x => new
                {
                    x.TableName,
                    x.SchemaName,
                    x.Type
                }))
            {

                trs += $@"<tr class='tbl even' valign='top'>
                                    <td class='detail'>{item.Key.SchemaName}</td>
                                    <td class='detail'><a href='tables/{item.Key.TableName}.html'>{item.Key.TableName}</a></td>
                                    <td class='detail' align='right'>{item.Count()}</td>
                                    <td class='comment detail' style='display: table-cell;'></td>
                                </tr>";
                BuildTablePages(item, new List<ListAlldata>(), new List<ListAlldata>());

            }

            index = index.Replace(TablesAndViewsTrs, trs);
            File.WriteAllText(currentDirectory + ResourceTemp + "\\types.html", index);

        }

        private void BuildSchemaTablesPage(List<ListAlldata> allTables)
        {
            var sct = File.ReadAllText(currentDirectory + ResourceTemp + "\\schemaTables.html").GetNavBar();
            sct = sct.Replace("@JsonData", Newtonsoft.Json.JsonConvert.SerializeObject(allTables));

            File.WriteAllText(currentDirectory + ResourceTemp + "\\schemaTables.html", sct);
        }

        private void BuildRelationShipsPage(List<ListAlldata> allTables)
        {
            var columns = File.ReadAllText(currentDirectory + ResourceTemp + "\\relationships.html").GetNavBar();
            File.WriteAllText(currentDirectory + ResourceTemp + "\\relationships.html", columns);
        }

        private void BuildConstrainPage(List<ListAlldata> allConstrain)
        {
            var columns = File.ReadAllText(currentDirectory + ResourceTemp + "\\constraints.html").GetNavBar();
            var cls = allConstrain
                .Select(x => $@" <tr>
                                    <td>{x.SchemaName}</td>
                                    <td><a href='tables/{x.TableName}.html'>{x.TableName}</a></td>
                                    <td>{x.ColumnName}</td>
                                    <td>{x.Defainition}</td>
                                    <td><a href='tables/{x.ReferencedTable}.html'>{x.ReferencedTable}</a></td>
                                    <td>{x.ReferencedColumn}</td>
                                </tr>")
                .Aggregate((a, b) => a + b);
            columns = columns.Replace(ConstraintsCount, cls);
            File.WriteAllText(currentDirectory + ResourceTemp + "\\constraints.html", columns);
        }

        private void BuildColumnPage(List<ListAlldata> allTablesWithCoulmns)
        {
            var columns = File.ReadAllText(currentDirectory + ResourceTemp + "\\columns.html").GetNavBar();
            var cls = allTablesWithCoulmns
                .DistinctBy(x => new
                {
                    x.ColumnName,
                    x.DataType,
                    x.TableName,
                    x.SchemaName
                })
                .Select(x => x.GetForAllColumns())
                .Aggregate((a, b) => a + b);
            columns = columns.Replace(ColumnsCount, cls);
            File.WriteAllText(currentDirectory + ResourceTemp + "\\columns.html", columns);
        }

        private void BuildSchemas(List<ListAlldata> allTables)
        {
            var schemas = File.ReadAllText(currentDirectory + ResourceTemp + "\\routines.html").GetNavBar();
            var rts = allTables.Select(x =>
            $"<tr><td><a href='schemaTables.html?sc={x.SchemaName}'>" + x.SchemaName + "</a></td></tr>").Distinct()
                .Aggregate((a, b) => a + b);
            schemas = schemas.Replace(Schemas, rts);
            File.WriteAllText(currentDirectory + ResourceTemp + "\\routines.html", schemas);
        }

        private void BuildStoredProcedures(List<ListAlldata> allStoredProcedure)
        {
            var sps = File.ReadAllText(currentDirectory + ResourceTemp + "\\anomalies.html").GetNavBar();
            var spsTrs = allStoredProcedure
                .Select(x =>
                {
                    var s =
                    $@"<tr><td>{x.SchemaName}</td><td>{x.TableName}</td><td>
<button class='btn btn-success' onclick='$(this).next().toggle()' type='button'>Show/Hide</button>
<div style='display: none;'>{x.Defainition}</div></td><td>";
                    if (!x.StoredParameters.IsEmpty())
                    {
                        s += @"<table class='table table-bordered'><thead><tr><th>Param Name</th><th>Description</th></tr></thead><tbody>";
                        foreach (var p in x.StoredParameters
                        .Split(new[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            s += $@"<tr><td>{p}</td><td>{GetParameterDescription(p)}</td></tr>";
                        }
                        s += "</tbody></table>";
                    }
                    s += @"</td><td></td></tr>";
                    return s;
                })
                 .Aggregate((a, b) => a + b);
            sps = sps.Replace(StoredProcedures, spsTrs);
            File.WriteAllText(currentDirectory + ResourceTemp + "\\anomalies.html", sps);
        }

        private string GetParameterDescription(string p)
        {
            p = p.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            switch (p.Replace("@", "").Trim().ToLower())
            {
                case "displaylength":
                    return "Show the number of certain items on one page";
                case "displaystart":
                    return "Page number to show the number of items required";
                case "sortcol":
                    return "Sort Data Asc or Desc";
                case "sortdir":
                    return "[Deprecated]";
                case "search":
                    return "search string(nvarchar)";
                case "languageid":
                    return "application is multi language then should use user logged in language";
                case "userid":
                    return "every user has different regions then should show items belongs to user contracts and sub districts";
                case "contracttypeid":
                    return "contract type used to filter data";
                case "contractid":
                    return "contract id used to filter data";
                case "isdeleted":
                    return "show deleted data or not";
                case "datefrom":
                    return "search data from specified date";
                case "dateto":
                    return "search data to specified date";
                case "approvetype":
                    return "search data by approve type";
                case "workertype":
                    return "search data by worker type";
                case "jobtitle":
                    return "search data by job title";
                case "musthavefingerprint":
                    return "is data must have finger print enabled or not";
                case "isabovecontractual":
                    return "is data must have finger print enabled or not";
                case "workerids":
                    return "search data by Worker Ids";
                case "jobtitleids":
                    return "search data by JobTitle Ids";
                case "contractids":
                case "contractsids":
                    return "search data by Contract Ids";
                case "companiesid":
                    return "search data by Companies Ids";
                case "operation":
                    return "get workers by operation (absence , holiday)";
            }

            return "";
        }

        private void BuildViewsPage(List<ListAlldata> allViews)
        {
            if (allViews.Count == 0) return;
            var views = File.ReadAllText(currentDirectory + ResourceTemp + "\\orphans.html").GetNavBar();
            var allViewsData = allViews.GroupBy(x => x.TableName)
                .Select(c => c.First())
                .Select(x => $@"<tr><td>{x.SchemaName}</td><td>{x.TableName}</td><td>{x.Defainition}</td></tr>")
                 .Aggregate((a, b) => a + b);
            views = views.Replace(TablesAndViewsTrs, allViewsData);
            File.WriteAllText(currentDirectory + ResourceTemp + "\\orphans.html", views);
        }

        private static List<ListAlldata> LoadAllTablesAndViews(List<ListAlldata> allTablesWithCoulmns, List<ListAlldata> allViewsWithCoulmns)
        {
            return allTablesWithCoulmns.Concat(allViewsWithCoulmns).ToList();
        }

        private static string BuildIndex(List<ListAlldata> allTablesWithCoulmns,
            List<ListAlldata> allStoredProcedure, List<ListAlldata> allTables, List<ListAlldata> allViews, string index)
        {
            index = index.Replace(TablesCount, allTables.Count() + "");
            index = index.Replace(ViewsCount, allViews.Count() + "");
            index = index.Replace(ColumnsCount, allTablesWithCoulmns.Count + "");
            index = index.Replace(ConstraintsCount, allTablesWithCoulmns.Count + "");
            index = index.Replace(SchemasCount, allTables.Select(x => x.SchemaName).Distinct().Count() + "");
            index = index.Replace(StoredProceduresCount, allStoredProcedure.Count + "");
            return index;
        }

        private void BuildTablePages(IGrouping<dynamic, ListAlldata> item, List<ListAlldata> allIndexs,
            List<ListAlldata> allConstrain)
        {
            var tempFile = File.ReadAllText(currentDirectory + "Resources\\tables\\index.html").GetNavBar();
            var trs = "";
            foreach (var column in item.OrderByDescending(x => x.PrimaryKey))
            {
                var isPrimary = column.PrimaryKey ? "<i class='icon ion-key iconkey' style='padding-left: 5px;'></i>" : "";
                var isNull = column.IsNullable ? "Null" : "Not Null";
                string cment = column.GetColumnDescription();
                trs += $@"<tr><td>
                                 {isPrimary}<span id=''>{column.ColumnName}</span>
                                    </td>
                                    <td>{column.DataType}</td>
                                    <td>{isNull}</td>
                                    <td>{cment}</td>
                                </tr>";
            }
            tempFile = tempFile.Replace(TablesAndViewsTrs, trs);
            tempFile = tempFile.Replace(ColumnsCount, item.Count() + "");

            tempFile = tempFile.Replace("@TableName", item.Key.TableName);

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

        private void cbDataBase_Enter(object sender, EventArgs e)
        {
            if (txtServer.Text.IsEmpty() == false &&
                txtPassword.Text.IsEmpty() == false &&
                txtUser.Text.IsEmpty() == false)
            {
                connectionString = $"Persist Security Info=true;Server={txtServer.Text};user id={txtUser.Text};password={txtPassword.Text};MultipleActiveResultSets=true;"; ;
                var lst = ExecuteStoredProcedure<ListAlldata>
                    ("SELECT [name] DataBaseName FROM master.dbo.sysdatabases");
                cbDataBase.Items.Clear();
                cbDataBase.Items.AddRange(lst.Select(x => x.DataBaseName).ToArray());
            }
        }
    }
}

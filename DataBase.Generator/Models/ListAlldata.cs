﻿

using System;

namespace DataBase.Generator.Models
{
    public class ServerDetails
    {
        public string Edition { get; set; }

        public string Product_Version { get; set; }

        public string Collation { get; set; }

        public string CLR_Version { get; set; }

        public string GetDetailsString()
        {
            return $@"<p>Product_Version : {Product_Version}</p>
            <p>Collation : {Collation}</p><p>CLR_Version : {CLR_Version}</p>";
        }
    }
    public class ListAlldata
    {
        public string DataBaseName { get; set; }

        public bool IsNullable { get; set; }

        public bool PrimaryKey { get; set; }

        public string Defainition { get; set; }

        public string SchemaName { get; set; }

        public string ReferencedSchema { get; set; }
        public string ReferencedTable { get; set; }

        public string ReferencedColumn { get; set; }

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

        public string StoredParameters { get; set; }

        public string Description { get { return this.GetColumnDescription(); } }

        internal string GetForAllColumns()
        {
            var isNull = IsNullable ? "Null" : "Not Null";
            var isPrimary = PrimaryKey ? "<i class='icon ion-key iconkey' style='padding-left: 5px;'></i>" : "";
            string cment = this.GetColumnDescription();

            return $@" <tr><td>{SchemaName}</td>
                                    <td><a href='tables/{TableName}.html'>{TableName}</a></td>
                                    <td>{isPrimary} {ColumnName}</td>
                                    <td>{DataType}</td>
                                    <td>{isNull}</td>
                                    <td>{cment}</td>
                                </tr>";
        }
    }
}

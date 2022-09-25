

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

        internal string GetForAllColumns()
        {
            var isNull = IsNullable ? "Null" : "Not Null";
            return $@" <tr>
                                    <td>{SchemaName}</td>
                                    <td>{TableName}</td>
                                    <td>{ColumnName}</td>
                                    <td>{DataType}</td>
                                    <td>{isNull}</td>
                                    <td></td>
                                </tr>";
        }
    }
}

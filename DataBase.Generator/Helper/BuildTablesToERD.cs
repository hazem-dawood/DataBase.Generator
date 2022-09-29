using DataBase.Generator.Models;

using System.Collections.Generic;
using System.Linq;
using System;
using DataBase.Generator;

namespace DataBase
{
    public class BuildTablesToERD
    {
        public string LoadString(List<ListAlldata> allTables)
        {
            var data =
                allTables.GroupBy(x => new
            {
                x.TableName,
                x.SchemaName
            });
            var s = "";
            foreach (var item in data)
            {
                s += Environment.NewLine + $@"{item.Key.SchemaName.Trim()}_{item.Key.TableName}";
                s += Environment.NewLine + "-" + Environment.NewLine;

                foreach (var column in item.DistinctBy(z=>z.ColumnName))
                {
                    s += $@"{column.ColumnName} " + (column.PrimaryKey ? "PK " : "")
                        + GetCSharpType(column.DataType) + Environment.NewLine;
                }
            }
            return s;
        }

        private string GetCSharpType(string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "varbinary": return "byte[]";
                case "binary": return "byte[]";
                case "varbinary(1)":
                case "binary(1)": return "byte";
                case "image": return "none";
                case "varchar": return "none";
                case "char": return "none";
                case "nvarchar(1), nchar(1)": return "string";
                case "nvarchar": return "string";
                case "nchar": return "string";
                case "text": return "none";
                case "ntext": return "none";
                case "uniqueidentifier": return "guid";
                case "rowversion": return "byte[]";
                case "bit": return "boolean";
                case "tinyint": return "byte";
                case "smallint": return "int16";
                case "int": return "int32";
                case "bigint": return "int64";
                case "smallmoney": return "decimal";
                case "money": return "decimal";
                case "numeric": return "decimal";
                case "decimal": return "decimal";
                case "real":
                    return "single";
                case "float":
                    return "double";
                case "smalldatetime":
                    return "datetime";
                case "datetime":
                case "datetime2":
                    return "datetime";
                case "sql_variant": return "object";
                case "user-defined type(udt)": return "user-defined type";
                case "table": return "none";
                case "cursor": return "none";
                case "timestamp": return "none";
                case "xml": return "none";
            };
            return "string";
        }
    }
    /*
Customer
-
CustomerID PK int
Name string INDEX
Address1 string
Address2 NULL string
Address3 NULL string

Order
-
OrderID PK int
CustomerID int FK >- Customer.CustomerID
TotalAmount money
OrderStatusID int FK >- os.OrderStatusID

OrderLine as ol
----
OrderLineID PK int
OrderID int FK >- Order.OrderID
ProductID int FK >- p.ProductID
Quantity int

# Table documentation comment 1 (try the PDF/RTF export)
Product as p # Table documentation comment 2
------------
ProductID PK int
# Field documentation comment 1
# Field documentation comment 2 
Name varchar(200) UNIQUE # Field documentation comment 3
Price money

OrderStatus as os
----
OrderStatusID PK int
Name UNIQUE string

     */
}

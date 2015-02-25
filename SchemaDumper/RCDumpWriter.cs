//as per https://gist.github.com/rebootd/924680#file-sqlschemadumpwriter
using FluentMigrator.Model;
using FluentMigrator.SchemaDump.SchemaWriters;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SchemaDumper
{
    public class RCDumpWriter : SchemaWriterBase
    {
        public override void WriteToStream(ICollection<TableDefinition> tables, StreamWriter output)
        {
            //int tableCount = tables.Count;
            //int columnCount = tables.Select(t => t.Columns.Count).Sum();
            //int indexCount = tables.Select(t => t.Indexes.Count).Sum();
            //int keyCount = tables.Select(t => t.ForeignKeys.Count).Sum();

            //start writing a migration file
            output.WriteLine("using System;");
            output.WriteLine("using System.Collections.Generic;");
            output.WriteLine("using System.Linq;");
            output.WriteLine("using System.Web;");
            output.WriteLine("using FluentMigrator;");
            output.WriteLine(string.Empty);
            output.WriteLine("namespace MyAppNamespace.Migrations");
            output.WriteLine("{");
            output.WriteLine("\t[Migration(0)]");
            output.WriteLine("\tpublic class BaseMigration : Migration");
            output.WriteLine("\t{");
            output.WriteLine("\t\tpublic override void Up()");
            output.WriteLine("\t\t{");

            foreach (TableDefinition table in tables)
            {
                WriteTable(table, output);
            }

            output.WriteLine("\t\t}"); //end method

            output.WriteLine("\t\tpublic override void Down()");
            output.WriteLine("\t\t{");

            foreach (TableDefinition table in tables)
            {
                WriteDeleteTable(table, output);
            }

            output.WriteLine("\t\t}"); //end method
            output.WriteLine("\t}"); //end class
            output.WriteLine(String.Empty);
            output.WriteLine("}"); //end namespace
        }

        protected void WriteTable(TableDefinition table, StreamWriter output)
        {
            output.WriteLine("\t\t\tCreate.Table(\"" + table.Name + "\")");
            foreach (ColumnDefinition column in table.Columns)
            {
                WriteColumn(column, output, column == table.Columns.Last());
            }
        }

        protected void WriteDeleteTable(TableDefinition table, StreamWriter output)
        {
            //Delete.Table("Bar");
            output.WriteLine("\t\t\tDelete.Table(\"" + table.Name + "\");");
        }

        protected void WriteColumn(ColumnDefinition column, StreamWriter output, bool isLastColumn)
        {
            string columnSyntax = ".WithColumn(\"" + column.Name + "\")";
            switch (column.Type)
            {
                case DbType.Boolean:
                    columnSyntax += ".AsBoolean()";
                    break;
                case DbType.Int16:
                    columnSyntax += ".AsInt16()";
                    break;
                case DbType.Int32:
                    columnSyntax += ".AsInt32()";
                    break;
                default:
                    columnSyntax += ".AsString()";
                    break;
            }
            if (column.IsIdentity)
                columnSyntax += ".Identity()";
            else if (column.IsIndexed)
                columnSyntax += ".Indexed()";

            if (column.IsNullable == false)
                columnSyntax += ".NotNullable()";

            if (isLastColumn) columnSyntax += ";";
            output.WriteLine("\t\t\t\t" + columnSyntax);
        }
    }
}
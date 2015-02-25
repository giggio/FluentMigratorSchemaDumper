using DocoptNet;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Generators.SqlServer;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.Processors.SqlServer;
using FluentMigrator.SchemaDump.SchemaDumpers;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using static System.Console;

namespace SchemaDumper
{
    class Program
    {

        static void Main(string[] args)
        {
            const string usage = @"Fluent Migrator Schema Dumper
  Usage:
    SchemaDumper.exe --connection CONNECTION [--file FILE] [--verbose] [--show] [--open]
    SchemaDumper.exe --version
    SchemaDumper.exe --help

  Options:
    --connection CONNECTION -c CONNECTION    The connection string. Required.
    --file FILE -f FILE                      File to output. Optional. [default: schemaDump.cs]
    --show -s                                Show output. Optional.
    --open -o                                Open file. Optional.
    --verbose                                Verbose. Optional.
    --help -h                                Show this screen.
    --version -v                             Show version.
";

            var arguments = new Docopt().Apply(usage, args, version: Assembly.GetExecutingAssembly().GetName().Version, exit: true);
            var file = arguments["--file"].ToString();
            var verbose = arguments["--verbose"].IsTrue;
            var open = arguments["--open"].IsTrue;
            var show = arguments["--show"].IsTrue;
            if (!Path.IsPathRooted(file)) file = Path.Combine(Environment.CurrentDirectory, file);
            var connectionString = arguments["--connection"].ToString();
            if (verbose) WriteLine($"Saving to {file}.");
            try { var builder = new SqlConnectionStringBuilder(connectionString); }
            catch (ArgumentException)
            {
                WriteLine("Connection string is in incorrect format.");
                return;
            }
            using (var connection = new SqlConnection(connectionString))
            {
                try { connection.Open(); }
                catch (SqlException ex)
                {
                    WriteLine($"Connection couldn't be established:\n{ex.Message}");
                    return;
                }
                var consoleAnnouncer = new ConsoleAnnouncer();
                var dumper = new SqlServerSchemaDumper(new SqlServerProcessor(connection, new SqlServer2000Generator(), consoleAnnouncer, new ProcessorOptions(), new SqlServerDbFactory()), consoleAnnouncer);
                var tables = dumper.ReadDbSchema();
                var writer = new RCDumpWriter();
                writer.WriteToFile(tables, file);
            }
            if (show) WriteLine(File.ReadAllText(file));
            if (open) try { Process.Start(file); } catch { }
            if (verbose) WriteLine("Done.");
        }
    }
}
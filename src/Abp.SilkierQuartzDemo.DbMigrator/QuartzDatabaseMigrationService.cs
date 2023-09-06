using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace Abp.SilkierQuartzDemo.DbMigrator;

public partial class QuartzDatabaseMigrationService : IDataSeedContributor, ITransientDependency
{
    protected string SqlConnectionString { get; }

    protected IConfiguration Configuration { get; }

    private const string QuartzTableSqlFileName = "quartz_table_sqlserver.sql";

    public QuartzDatabaseMigrationService(IConfiguration configuration)
    {
        Configuration = configuration;
        SqlConnectionString = configuration.GetConnectionString("Default")!;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await MigrateAsync();
    }

    public virtual async Task MigrateAsync()
    {
        await CreateTablesAsync();
        await UpdateTablesAsync();
    }

    protected virtual async Task<bool> CheckTablesExistAsync()
    {
        var tableNames = new string[]
        {
            "QRTZ_CALENDARS",
            "QRTZ_LOCKS",
            "QRTZ_SCHEDULER_STATE",
            "QRTZ_JOB_DETAILS",
            "QRTZ_TRIGGERS",
            "QRTZ_SIMPLE_TRIGGERS",
            "QRTZ_CRON_TRIGGERS",
            "QRTZ_BLOB_TRIGGERS",
            "QRTZ_SIMPROP_TRIGGERS",
            "QRTZ_FIRED_TRIGGERS",
            "QRTZ_PAUSED_TRIGGER_GRPS"
        };

        try
        {
            await using var conn = new SqlConnection(SqlConnectionString);
            await conn.OpenAsync();

            foreach (var tableName in tableNames)
            {
                var cmd = new SqlCommand($"SELECT * FROM [{tableName}]", conn);
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    protected virtual async Task CreateTablesAsync()
    {
        var exist = await CheckTablesExistAsync();
        if (exist)
        {
            return;
        }

        await using var conn = new SqlConnection(SqlConnectionString);
        await conn.OpenAsync();
        await using (var transaction = await conn.BeginTransactionAsync())
        {
            var commandText = await GetAssertsFileContentAsync(QuartzTableSqlFileName);
            await ExecuteNonQueryWithGoAsync(conn, commandText, transaction);
            await transaction.CommitAsync();
        }

        await conn.CloseAsync();
    }

    protected virtual async Task UpdateTablesAsync()
    {
        var commandTexts = new List<string>();

        const int maximumLength = 300;

        var columnInfo = await GetColumnInfoAsync("QRTZ_JOB_DETAILS", "JOB_CLASS_NAME");
        if (!string.Equals(columnInfo.DataType, "nvarchar") || columnInfo.CharacterMaximumLength < maximumLength)
        {
            commandTexts.Add($"ALTER TABLE [dbo].[QRTZ_JOB_DETAILS] ALTER COLUMN [JOB_CLASS_NAME] NVARCHAR ({maximumLength}) NOT NULL;");
        }
        if (commandTexts.Count == 0)
        {
            return;
        }

        await using var conn = new SqlConnection(SqlConnectionString);
        await conn.OpenAsync();
        await using (var transaction = await conn.BeginTransactionAsync())
        {
            var commandText = string.Join("\r\n", commandTexts);
            await ExecuteNonQueryWithGoAsync(conn, commandText, transaction);
            await transaction.CommitAsync();
        }

        await conn.CloseAsync();
    }

    protected virtual async Task<ColumnInfo> GetColumnInfoAsync(string tableName, string columnName)
    {
        const string query = """
                             SELECT TOP 1 TABLE_CATALOG,TABLE_SCHEMA,TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH
                             FROM INFORMATION_SCHEMA.COLUMNS
                             where TABLE_CATALOG = @TableCatalog and TABLE_NAME = @TableName and COLUMN_NAME = @ColumnName
                             """;

        var connBuilder = new SqlConnectionStringBuilder(SqlConnectionString);

        await using var conn = new SqlConnection(SqlConnectionString);
        await conn.OpenAsync();

        var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue("TableCatalog", connBuilder.InitialCatalog);
        cmd.Parameters.AddWithValue("TableName", tableName);
        cmd.Parameters.AddWithValue("ColumnName", columnName);
        var dataReader = await cmd.ExecuteReaderAsync();

        await dataReader.ReadAsync();

        if (!dataReader.HasRows)
        {
            return new ColumnInfo();
        }
        return new ColumnInfo
        {
            Catalog = dataReader.GetString(0),
            Schema = dataReader.GetString(1),
            Name = dataReader.GetString(2),
            Table = dataReader.GetString(3),
            DataType = dataReader.GetString(4),
            CharacterMaximumLength = (int?)dataReader.GetValue(5) ?? -1
        };
    }

    protected virtual async Task<int> ExecuteNonQueryWithGoAsync(IDbConnection connection, string commandText, DbTransaction? dbTransaction)
    {
        var hasDbTran = dbTransaction != null;
        var result = 0;
        var sql = RemoveComments(commandText, true, false);
        sql = RemoveEmptyLines(sql);
        var arr = Regex.Split(sql, "GO");

        var conn = (SqlConnection)connection;

        var cmd = new SqlCommand
        {
            Connection = conn
        };

        var tx = (hasDbTran ? dbTransaction : await conn.BeginTransactionAsync())!;
        cmd.Transaction = (SqlTransaction)tx!;
        try
        {
            foreach (var t in arr)
            {
                var strSql = t.Trim();
                if (strSql.Length >= 1)
                {
                    cmd.CommandText = strSql;
                    result = await cmd.ExecuteNonQueryAsync();
                }
            }

            if (!hasDbTran)
            {
                await tx.CommitAsync();
            }
        }
        catch (Exception)
        {
            await tx.RollbackAsync();
            throw;
        }

        return result;
    }

    // http://drizin.io/Removing-comments-from-SQL-scripts/
    // http://web.archive.org/web/*/http://drizin.io/Removing-comments-from-SQL-scripts/
    protected virtual string RemoveComments(string input, bool preservePositions, bool removeLiterals = false)
    {
        //based on http://stackoverflow.com/questions/3524317/regex-to-strip-line-comments-from-c-sharp/3524689#3524689
        var lineComments = @"--(.*?)\r?\n";
        var lineCommentsOnLastLine = @"--(.*?)$"; // because it's possible that there's no \r\n after the last line comment
                                                  // literals ('literals'), bracketedIdentifiers ([object]) and quotedIdentifiers ("object"), they follow the same structure:
                                                  // there's the start character, any consecutive pairs of closing characters are considered part of the literal/identifier, and then comes the closing character
        var literals = @"('(('')|[^'])*')"; // 'John', 'O''malley''s', etc
        var bracketedIdentifiers = @"\[((\]\])|[^\]])* \]"; // [object], [ % object]] ], etc
        var quotedIdentifiers = @"(\""((\""\"")|[^""])*\"")"; // "object", "object[]", etc - when QUOTED_IDENTIFIER is set to ON, they are identifiers, else they are literals
                                                              //var blockComments = @"/\*(.*?)\*/";  //the original code was for C#, but Microsoft SQL allows a nested block comments // //https://msdn.microsoft.com/en-us/library/ms178623.aspx

        //so we should use balancing groups // http://weblogs.asp.net/whaggard/377025
        var nestedBlockComments = """
                                  /\*
                                  (?>
                                  /\*  (?<LEVEL>)      # On opening push level
                                  |
                                  \*/ (?<-LEVEL>)     # On closing pop level
                                  |
                                  (?! /\* | \*/ ) . # Match any char unless the opening and closing strings
                                  )+                         # /* or */ in the lookahead string
                                  (?(LEVEL)(?!))             # If level exists then fail
                                  \*/
                                  """;

        var noComments = Regex.Replace(input,
            nestedBlockComments + "|" + lineComments + "|" + lineCommentsOnLastLine + "|" + literals + "|" + bracketedIdentifiers + "|" + quotedIdentifiers,
            me =>
            {
                if (me.Value.StartsWith("/*") && preservePositions)
                {
                    return EverythingExceptNewLines().Replace(me.Value, " "); // preserve positions and keep line-breaks // return new string(' ', me.Value.Length);
                }
                else if (me.Value.StartsWith("/*") && !preservePositions)
                {
                    return "";
                }
                else if (me.Value.StartsWith("--") && preservePositions)
                {
                    return EverythingExceptNewLines().Replace(me.Value, " "); // preserve positions and keep line-breaks
                }
                else if (me.Value.StartsWith("--") && !preservePositions)
                {
                    return EverythingExceptNewLines().Replace(me.Value, ""); // preserve only line-breaks // Environment.NewLine;
                }
                else if (me.Value.StartsWith("[") || me.Value.StartsWith("\""))
                {
                    return me.Value; // do not remove object identifiers ever
                }
                else
                {
                    switch (removeLiterals)
                    {
                        // Keep the literal strings
                        case false:
                            return me.Value;
                        // remove literals, but preserving positions and line-breaks
                        case true when preservePositions:
                            {
                                var literalWithLineBreaks = EverythingExceptNewLines().Replace(me.Value, " ");
                                return string.Concat("'", literalWithLineBreaks.AsSpan(1, (int)(literalWithLineBreaks.Length - 2)), "'");
                            }
                        // wrap completely all literals
                        case true when !preservePositions:
                            return "''";
                        default:
                            throw new NotImplementedException();
                    }
                }
            },
            RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace);
        return noComments;
    }

    protected virtual string RemoveEmptyLines(string text)
    {
        return Regex.Replace(text, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
    }

    protected virtual async Task<string> GetAssertsFileContentAsync(string filename)
    {
        var asmb = System.Reflection.Assembly.GetExecutingAssembly();
        var strName = asmb.GetName().Name + ".Asserts." + filename;
        using var manifestStream = asmb.GetManifestResourceStream(strName);
        using var reader = new StreamReader(manifestStream);
        return await reader.ReadToEndAsync();
    }

    public class ColumnInfo
    {
        public string Catalog { get; set; } = null!;

        public string Schema { get; set; } = null!;

        public string Table { get; set; } = null!;

        public string Name { get; set; } = null!;

        public string DataType { get; set; } = null!;

        public int CharacterMaximumLength { get; set; }
    }

    [GeneratedRegex("[^\r\n]")]
    private static partial Regex EverythingExceptNewLines();
}

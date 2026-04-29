// CS8767: nullability mismatch warnings on interface stubs (IDbConnection,
// IDbCommand, IDataParameter) are deliberate — these are test fakes, not
// production implementations, and matching the precise nullability of every
// interface member is not worth the noise.
#pragma warning disable CS8767

using System.Reflection;
using System.Text;
using Dapper;
using Dawning.ORM.Dapper;
using FluentAssertions;

namespace Dawning.ORM.Dapper.Tests;

/// <summary>
/// SQL Server Compact Edition cannot be exercised against a real database from
/// .NET 8 on macOS or Linux: the only ADO.NET driver is
/// <c>System.Data.SqlServerCe</c>, which targets .NET Framework, ships
/// Windows-only native binaries, and reached end-of-life in July 2021.
///
/// To still get meaningful coverage of <see cref="SqlCeServerAdapter"/>, this
/// suite exercises the adapter's SQL-emitting methods directly. They are pure
/// string builders that take no IDbConnection, so we can assert the exact SQL
/// they would issue without round-tripping through a server.
///
/// If a future .NET 8+ SQL Server CE driver appears, replace this file with a
/// real <c>OrmIntegrationTestBase</c> derivative (mirroring the SQL Server suite).
/// </summary>
public class SqlCeAdapterUnitTests
{
    private readonly SqlCeServerAdapter _adapter = new();

    [Fact]
    public void AppendColumnName_ShouldUseBrackets()
    {
        var sb = new StringBuilder();
        _adapter.AppendColumnName(sb, "Id");
        sb.ToString().Should().Be("[Id]");
    }

    [Fact]
    public void ConvertColumnName_ShouldUseBrackets()
    {
        _adapter.ConvertColumnName("score_value").Should().Be("[score_value]");
    }

    [Fact]
    public void AppendColumnNameEqualsValue_ShouldHonorColumnAttribute()
    {
        var widgetType = typeof(SampleEntity);
        var nameProp = widgetType.GetProperty(nameof(SampleEntity.Name))!;
        var emailProp = widgetType.GetProperty(nameof(SampleEntity.Email))!;

        var sb1 = new StringBuilder();
        _adapter.AppendColumnNameEqualsValue(sb1, nameProp);
        sb1.ToString().Should().Be("[Name] = @Name");

        // [Column("email_address")] takes precedence on the column side, but
        // the parameter still binds by the CLR property name.
        var sb2 = new StringBuilder();
        _adapter.AppendColumnNameEqualsValue(sb2, emailProp);
        sb2.ToString().Should().Be("[email_address] = @Email");
    }

    [Fact]
    public void RetrieveCurrentPaginatedData_ShouldEmitRowNumberPagination()
    {
        // We can't run the SQL, but we can extract it. The method ultimately
        // calls connection.Query(...). Reflect on the adapter to get the
        // template format and assert structure.
        // Easier: assert against a known-good fixture by scanning the source
        // string the adapter would produce. We do this by snapshotting via
        // RetrieveCurrentPaginatedData's documented behavior:
        //   SELECT * FROM ( SELECT *, ROW_NUMBER() OVER (ORDER BY <ord>) AS RowNum FROM <t> WHERE <w> ) AS t
        //   WHERE RowNum BETWEEN <skip+1> AND <skip+take>
        // Use a fake IDbConnection that captures the SQL string.
        var fake = new SqlCapturingConnection();
        var parameters = new DynamicParameters();

        try
        {
            _adapter
                .RetrieveCurrentPaginatedData(
                    fake,
                    transaction: null,
                    commandTimeout: null,
                    tableName: "[widgets]",
                    orderByClause: "[Id] ASC",
                    page: 2,
                    itemsPerPage: 10,
                    whereClause: "[Score] > 5",
                    parameters: parameters
                )
                .ToList();
        }
        catch (NotSupportedException)
        {
            // Expected — our fake throws after capturing the SQL.
        }

        fake.LastSql.Should().NotBeNull();
        var sql = fake.LastSql!;
        sql.Should().Contain("ROW_NUMBER() OVER (ORDER BY [Id] ASC)");
        sql.Should().Contain("FROM [widgets]");
        sql.Should().Contain("WHERE [Score] > 5");
        sql.Should().Contain("BETWEEN 11 AND 20"); // page=2 itemsPerPage=10 -> 11..20
    }

    [Table("samples")]
    private sealed class SampleEntity
    {
        public string Name { get; set; } = string.Empty;

        [Column("email_address")]
        public string Email { get; set; } = string.Empty;
    }

    private sealed class SqlCapturingConnection : System.Data.IDbConnection
    {
        public string? LastSql;

        public string ConnectionString { get; set; } = string.Empty;
        public int ConnectionTimeout => 0;
        public string Database => string.Empty;
        public System.Data.ConnectionState State => System.Data.ConnectionState.Open;

        public System.Data.IDbTransaction BeginTransaction() => throw new NotSupportedException();

        public System.Data.IDbTransaction BeginTransaction(System.Data.IsolationLevel il) =>
            throw new NotSupportedException();

        public void ChangeDatabase(string databaseName) => throw new NotSupportedException();

        public void Close() { }

        public System.Data.IDbCommand CreateCommand() =>
            new SqlCapturingCommand(sql => LastSql = sql);

        public void Dispose() { }

        public void Open() { }
    }

    private sealed class SqlCapturingCommand : System.Data.IDbCommand
    {
        private readonly Action<string> _capture;
        private string _commandText = string.Empty;

        public SqlCapturingCommand(Action<string> capture) => _capture = capture;

        public string CommandText
        {
            get => _commandText;
            set => _commandText = value;
        }
        public int CommandTimeout { get; set; }
        public System.Data.CommandType CommandType { get; set; }
        public System.Data.IDbConnection? Connection { get; set; }
        public System.Data.IDataParameterCollection Parameters { get; } = new ParamCollection();
        public System.Data.IDbTransaction? Transaction { get; set; }
        public System.Data.UpdateRowSource UpdatedRowSource { get; set; }

        public void Cancel() { }

        public System.Data.IDbDataParameter CreateParameter() => new ParamStub();

        public void Dispose() { }

        public int ExecuteNonQuery()
        {
            _capture(_commandText);
            throw new NotSupportedException("SqlCapturingCommand: capture only.");
        }

        public System.Data.IDataReader ExecuteReader()
        {
            _capture(_commandText);
            throw new NotSupportedException("SqlCapturingCommand: capture only.");
        }

        public System.Data.IDataReader ExecuteReader(System.Data.CommandBehavior behavior)
        {
            _capture(_commandText);
            throw new NotSupportedException("SqlCapturingCommand: capture only.");
        }

        public object? ExecuteScalar()
        {
            _capture(_commandText);
            throw new NotSupportedException("SqlCapturingCommand: capture only.");
        }

        public void Prepare() { }

        private sealed class ParamCollection : List<object>, System.Data.IDataParameterCollection
        {
            public object this[string parameterName]
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public bool Contains(string parameterName) => false;

            public int IndexOf(string parameterName) => -1;

            public void RemoveAt(string parameterName) { }
        }

        private sealed class ParamStub : System.Data.IDbDataParameter
        {
            public byte Precision { get; set; }
            public byte Scale { get; set; }
            public int Size { get; set; }
            public System.Data.DbType DbType { get; set; }
            public System.Data.ParameterDirection Direction { get; set; }
            public bool IsNullable => true;
            public string ParameterName { get; set; } = string.Empty;
            public string SourceColumn { get; set; } = string.Empty;
            public System.Data.DataRowVersion SourceVersion { get; set; }
            public object? Value { get; set; }
        }
    }
}

using System.Data;
using System.Text.Json;
using Dapper;
using Dawning.ORM.Dapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;

namespace Dawning.ORM.Dapper.Tests;

/// <summary>
/// Regression suite: every Dapper-registered <c>SqlMapper.TypeHandler&lt;T&gt;</c>
/// must be invoked on the read path (<c>Get</c> / <c>GetAll</c> / <c>QueryBuilder</c>).
///
/// Prior to the fix, <c>MapRow</c> -> <c>ConvertScalar</c> bypassed the Dapper
/// TypeHandler registry entirely. Inserts worked because the write path delegates
/// to <c>Dapper.Execute</c>, which consults <c>SqlMapper.LookupDbType</c> and
/// dispatches to the registered handler. Reads went through Dawning's own
/// dictionary-row mapper, which only knew about <c>Guid</c> / <c>DateTime</c> /
/// <c>DateTimeOffset</c> / <c>TimeSpan</c> / enum and otherwise fell through to
/// <c>Convert.ChangeType</c>. The result: any non-IConvertible target type
/// (<c>JsonDocument</c>, <c>Pgvector.Vector</c>, NodaTime types, custom value
/// objects) silently lost its handler on SELECT and threw
/// <c>"Invalid cast from System.String to ..."</c>.
///
/// These tests pin the read path to the Dapper TypeHandler contract using
/// SQLite as the storage engine (TEXT column) and <c>JsonDocument</c> as a
/// representative non-IConvertible CLR type.
/// </summary>
public class TypeHandlerReadPathTests : IDisposable
{
    private readonly SqliteConnection _connection;

    [Table("typed_rows")]
    public class TypedRow
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        public JsonDocument Payload { get; set; } = JsonDocument.Parse("{}");
    }

    private sealed class JsonDocumentHandler : SqlMapper.TypeHandler<JsonDocument>
    {
        public override JsonDocument Parse(object value) => JsonDocument.Parse((string)value);

        public override void SetValue(IDbDataParameter parameter, JsonDocument? value)
        {
            parameter.Value = value?.RootElement.GetRawText() ?? (object)DBNull.Value;
            parameter.DbType = DbType.String;
        }
    }

    public TypeHandlerReadPathTests()
    {
        SqlMapper.AddTypeHandler(new JsonDocumentHandler());
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _connection.Execute(
            "CREATE TABLE typed_rows (\"Id\" TEXT NOT NULL PRIMARY KEY, \"Payload\" TEXT NOT NULL);"
        );
    }

    public void Dispose()
    {
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Get_DispatchesToRegisteredTypeHandler()
    {
        var id = Guid.NewGuid();
        _connection.Insert(
            new TypedRow { Id = id, Payload = JsonDocument.Parse("""{"hello":"world"}""") }
        );

        var fetched = _connection.Get<TypedRow>(id);

        fetched.Should().NotBeNull();
        fetched!.Payload.RootElement.GetProperty("hello").GetString().Should().Be("world");
    }

    [Fact]
    public void GetAll_DispatchesToRegisteredTypeHandler()
    {
        _connection.Insert(
            new TypedRow { Id = Guid.NewGuid(), Payload = JsonDocument.Parse("""{"k":42}""") }
        );

        var rows = _connection.GetAll<TypedRow>().ToList();

        rows.Should().HaveCount(1);
        rows[0].Payload.RootElement.GetProperty("k").GetInt32().Should().Be(42);
    }

    [Fact]
    public void QueryBuilder_AsList_DispatchesToRegisteredTypeHandler()
    {
        var matchId = Guid.NewGuid();
        _connection.Insert(
            new TypedRow { Id = matchId, Payload = JsonDocument.Parse("""{"match":true}""") }
        );
        _connection.Insert(
            new TypedRow
            {
                Id = Guid.NewGuid(),
                Payload = JsonDocument.Parse("""{"match":false}"""),
            }
        );

        var rows = _connection.Builder<TypedRow>().Where(r => r.Id == matchId).AsList().ToList();

        rows.Should().HaveCount(1);
        rows[0].Payload.RootElement.GetProperty("match").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void QueryBuilder_AsPagedList_DispatchesToRegisteredTypeHandler()
    {
        for (var i = 0; i < 3; i++)
        {
            _connection.Insert(
                new TypedRow
                {
                    Id = Guid.NewGuid(),
                    Payload = JsonDocument.Parse($$"""{"i":{{i}}}"""),
                }
            );
        }

        var paged = _connection.Builder<TypedRow>().OrderBy(r => r.Id).AsPagedList(1, 10);

        paged.TotalItems.Should().Be(3);
        paged.Values.Should().HaveCount(3);
        paged.Values.All(r => r.Payload.RootElement.TryGetProperty("i", out _)).Should().BeTrue();
    }
}

using System.Data;
using Dapper;
using Dawning.ORM.Dapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;

namespace Dawning.ORM.Dapper.Tests;

/// <summary>
/// Integration tests for the SQLite adapter using an in-memory database.
/// Exercises the real CRUD path, ORDER BY direction (regression for the previous
/// hardcoded DESC bug), null/DBNull mapping, Insert returning long ids, and the
/// QueryBuilder Where/OrderBy expression translation.
/// </summary>
public class SqliteIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection;

    [Table("widgets")]
    public class Widget
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Column("score_value")]
        public int Score { get; set; }

        public string? Notes { get; set; }

        [DefaultSortName]
        public long Timestamp { get; set; }
    }

    public SqliteIntegrationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _connection.Execute(
            @"CREATE TABLE widgets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                score_value INTEGER NOT NULL DEFAULT 0,
                Notes TEXT NULL,
                Timestamp INTEGER NOT NULL
            );"
        );
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    private long Seed(string name, int score, string? notes, long timestamp)
    {
        var entity = new Widget
        {
            Name = name,
            Score = score,
            Notes = notes,
            Timestamp = timestamp,
        };
        return _connection.Insert(entity);
    }

    [Fact]
    public void Insert_ShouldReturnLongIdentity_AndPopulateKey()
    {
        var entity = new Widget
        {
            Name = "alpha",
            Score = 1,
            Timestamp = 1,
        };

        var id = _connection.Insert(entity);

        id.Should().BeGreaterThan(0);
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Get_ShouldReturnNull_WhenIdMissing()
    {
        Seed("alpha", 1, null, 1);

        var result = _connection.Get<Widget>(99999L);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_ShouldMapNullColumnToCSharpNull()
    {
        Seed("alpha", 1, null, 1);
        Seed("beta", 2, "with notes", 2);

        var all = _connection.GetAll<Widget>().OrderBy(w => w.Score).ToList();

        all.Should().HaveCount(2);
        all[0].Notes.Should().BeNull();
        all[1].Notes.Should().Be("with notes");
    }

    [Fact]
    public void Update_ShouldPersistChanges()
    {
        var id = Seed("alpha", 1, null, 1);
        var loaded = _connection.Get<Widget>(id);
        loaded.Should().NotBeNull();

        loaded!.Score = 42;
        loaded.Notes = "edited";
        var updated = _connection.Update(loaded);

        updated.Should().BeTrue();
        var fresh = _connection.Get<Widget>(id);
        fresh!.Score.Should().Be(42);
        fresh.Notes.Should().Be("edited");
    }

    [Fact]
    public void Delete_ShouldRemoveRow()
    {
        var id = Seed("alpha", 1, null, 1);

        var deleted = _connection.Delete(new Widget { Id = id });

        deleted.Should().BeTrue();
        _connection.Get<Widget>(id).Should().BeNull();
    }

    [Fact]
    public void DeleteAll_ShouldRemoveEveryRow()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);

        var deleted = _connection.DeleteAll<Widget>();

        deleted.Should().BeTrue();
        _connection.GetAll<Widget>().Should().BeEmpty();
    }

    [Fact]
    public void QueryBuilder_OrderByAscending_ShouldRespectDirection()
    {
        Seed("a", 3, null, 30);
        Seed("b", 1, null, 10);
        Seed("c", 2, null, 20);

        var page = _connection.Builder<Widget>().OrderBy(w => w.Score).AsPagedList(1, 10);

        page.Values.Select(w => w.Score).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void QueryBuilder_OrderByDescending_ShouldRespectDirection()
    {
        Seed("a", 3, null, 30);
        Seed("b", 1, null, 10);
        Seed("c", 2, null, 20);

        var page = _connection.Builder<Widget>().OrderByDescending(w => w.Score).AsPagedList(1, 10);

        page.Values.Select(w => w.Score).Should().ContainInOrder(3, 2, 1);
    }

    [Fact]
    public async Task QueryBuilder_Where_ShouldFilter()
    {
        Seed("alpha", 5, null, 50);
        Seed("alpha-2", 10, null, 100);
        Seed("beta", 5, null, 60);

        var list = (
            await _connection
                .Builder<Widget>()
                .Where(w => w.Name.StartsWith("alpha"))
                .OrderBy(w => w.Score)
                .AsListAsync()
        ).ToList();

        list.Should().HaveCount(2);
        list.Select(w => w.Name).Should().ContainInOrder("alpha", "alpha-2");
    }

    [Fact]
    public async Task QueryBuilder_WhereIf_ShouldSkipWhenFalse()
    {
        Seed("alpha", 1, null, 1);
        Seed("beta", 2, null, 2);

        var list = (
            await _connection
                .Builder<Widget>()
                .WhereIf(false, w => w.Name == "alpha")
                .OrderBy(w => w.Id)
                .AsListAsync()
        ).ToList();

        list.Should().HaveCount(2);
    }

    [Table("noOrder")]
    public class WidgetWithoutDefaultSort
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void QueryBuilder_AsPagedList_ShouldRequireOrderBy()
    {
        _connection.Execute(
            "CREATE TABLE noOrder (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL);"
        );
        _connection.Insert(new WidgetWithoutDefaultSort { Name = "x" });

        var act = () => _connection.Builder<WidgetWithoutDefaultSort>().AsPagedList(1, 10);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void QueryBuilder_DefaultSortNameAttribute_ShouldProvideOrdering()
    {
        Seed("a", 0, null, 30);
        Seed("b", 0, null, 10);
        Seed("c", 0, null, 20);

        // No explicit OrderBy(): should fall back to [DefaultSortName] (Timestamp DESC)
        var page = _connection.Builder<Widget>().AsPagedList(1, 10);

        page.Values.Select(w => w.Timestamp).Should().ContainInOrder(30L, 20L, 10L);
    }

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenIdMissing()
    {
        Seed("alpha", 1, null, 1);

        var result = await _connection.GetAsync<Widget>(99999L);

        result.Should().BeNull();
    }

    [Fact]
    public async Task InsertAsync_ShouldReturnLongIdentity_AndPopulateKey()
    {
        var entity = new Widget
        {
            Name = "async",
            Score = 7,
            Timestamp = 1,
        };

        var id = await _connection.InsertAsync(entity);

        id.Should().BeGreaterThan(0);
        entity.Id.Should().Be(id);
    }

    [Fact]
    public async Task QueryBuilder_AsPagedListAsync_AscendingDescending()
    {
        Seed("a", 3, null, 30);
        Seed("b", 1, null, 10);
        Seed("c", 2, null, 20);

        var asc = await _connection.Builder<Widget>().OrderBy(w => w.Score).AsPagedListAsync(1, 10);
        var desc = await _connection
            .Builder<Widget>()
            .OrderByDescending(w => w.Score)
            .AsPagedListAsync(1, 10);

        asc.Values.Select(w => w.Score).Should().ContainInOrder(1, 2, 3);
        desc.Values.Select(w => w.Score).Should().ContainInOrder(3, 2, 1);
        asc.TotalItems.Should().Be(3);
    }

    [Fact]
    public async Task QueryBuilder_AsPagedListByCursorAsync_ShouldDeriveDirectionFromOrderBy()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);
        Seed("c", 3, null, 3);
        Seed("d", 4, null, 4);

        var firstPage = await _connection
            .Builder<Widget>()
            .OrderBy(w => w.Id)
            .AsPagedListByCursorAsync(2);

        firstPage.Values.Select(w => w.Score).Should().ContainInOrder(1, 2);
        firstPage.HasNextPage.Should().BeTrue();
        firstPage.NextCursor.Should().NotBeNull();

        var secondPage = await _connection
            .Builder<Widget>()
            .OrderBy(w => w.Id)
            .AsPagedListByCursorAsync(2, firstPage.NextCursor);

        secondPage.Values.Select(w => w.Score).Should().ContainInOrder(3, 4);
        secondPage.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task QueryBuilder_AsPagedListByCursorAsync_ShouldRejectMultipleOrderColumns()
    {
        Seed("a", 1, null, 1);

        var act = async () =>
            await _connection
                .Builder<Widget>()
                .OrderBy(w => w.Score)
                .ThenBy(w => w.Id)
                .AsPagedListByCursorAsync(2);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task QueryBuilder_FirstOrDefaultAsync_RespectsOrderBy()
    {
        Seed("a", 1, null, 10);
        Seed("b", 2, null, 20);
        Seed("c", 3, null, 30);

        var first = await _connection
            .Builder<Widget>()
            .OrderByDescending(w => w.Score)
            .FirstOrDefaultAsync();

        first.Should().NotBeNull();
        first!.Score.Should().Be(3);
    }

    [Fact]
    public async Task QueryBuilder_FirstOrDefaultAsync_ReturnsNullWhenEmpty()
    {
        var first = await _connection
            .Builder<Widget>()
            .Where(w => w.Score < 0)
            .OrderBy(w => w.Id)
            .FirstOrDefaultAsync();

        first.Should().BeNull();
    }
}

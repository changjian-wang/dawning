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

    // ---------------------------------------------------------------------
    // Sync Builder coverage: Where operators, AsList, FirstOrDefault, Count,
    // Any, None, Take, Skip, Select projection, Distinct, multi-column ORDER BY.
    // ---------------------------------------------------------------------

    [Fact]
    public void QueryBuilder_Where_ComparisonOperators()
    {
        Seed("a", 1, null, 1);
        Seed("b", 5, null, 2);
        Seed("c", 9, null, 3);

        var gt = _connection.Builder<Widget>().Where(w => w.Score > 5).AsList().ToList();
        var ge = _connection.Builder<Widget>().Where(w => w.Score >= 5).AsList().ToList();
        var lt = _connection.Builder<Widget>().Where(w => w.Score < 5).AsList().ToList();
        var le = _connection.Builder<Widget>().Where(w => w.Score <= 5).AsList().ToList();
        var ne = _connection.Builder<Widget>().Where(w => w.Score != 5).AsList().ToList();

        gt.Should().HaveCount(1);
        ge.Should().HaveCount(2);
        lt.Should().HaveCount(1);
        le.Should().HaveCount(2);
        ne.Should().HaveCount(2);
    }

    [Fact]
    public void QueryBuilder_Where_AndOr_Compound()
    {
        Seed("alpha", 1, null, 1);
        Seed("alpha", 9, null, 2);
        Seed("beta", 5, null, 3);

        var andList = _connection
            .Builder<Widget>()
            .Where(w => w.Name == "alpha" && w.Score > 5)
            .AsList()
            .ToList();
        var orList = _connection
            .Builder<Widget>()
            .Where(w => w.Name == "beta" || w.Score == 9)
            .OrderBy(w => w.Id)
            .AsList()
            .ToList();

        andList.Should().HaveCount(1);
        andList[0].Score.Should().Be(9);
        orList.Should().HaveCount(2);
    }

    [Fact]
    public void QueryBuilder_Where_NullComparison()
    {
        Seed("with-notes", 1, "hello", 1);
        Seed("no-notes", 2, null, 2);

        var hasNull = _connection.Builder<Widget>().Where(w => w.Notes == null).AsList().ToList();
        var notNull = _connection.Builder<Widget>().Where(w => w.Notes != null).AsList().ToList();

        hasNull.Should().ContainSingle().Which.Name.Should().Be("no-notes");
        notNull.Should().ContainSingle().Which.Name.Should().Be("with-notes");
    }

    [Fact]
    public void QueryBuilder_Where_StringMethods()
    {
        Seed("alpha", 1, null, 1);
        Seed("alphabet", 2, null, 2);
        Seed("beta", 3, null, 3);

        var startsWith = _connection
            .Builder<Widget>()
            .Where(w => w.Name.StartsWith("alpha"))
            .AsList()
            .ToList();
        var endsWith = _connection
            .Builder<Widget>()
            .Where(w => w.Name.EndsWith("bet"))
            .AsList()
            .ToList();
        var contains = _connection
            .Builder<Widget>()
            .Where(w => w.Name.Contains("phab"))
            .AsList()
            .ToList();
        var equalsCall = _connection
            .Builder<Widget>()
            .Where(w => w.Name.Equals("beta"))
            .AsList()
            .ToList();

        startsWith.Should().HaveCount(2);
        endsWith.Should().ContainSingle().Which.Name.Should().Be("alphabet");
        contains.Should().ContainSingle().Which.Name.Should().Be("alphabet");
        equalsCall.Should().ContainSingle().Which.Name.Should().Be("beta");
    }

    [Fact]
    public void QueryBuilder_Where_CollectionContains_TranslatesToIn()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);
        Seed("c", 3, null, 3);

        var allowed = new List<int> { 1, 3 };
        var inList = _connection
            .Builder<Widget>()
            .Where(w => allowed.Contains(w.Score))
            .OrderBy(w => w.Score)
            .AsList()
            .ToList();
        var notInList = _connection
            .Builder<Widget>()
            .Where(w => !allowed.Contains(w.Score))
            .AsList()
            .ToList();

        inList.Select(w => w.Score).Should().ContainInOrder(1, 3);
        notInList.Should().ContainSingle().Which.Score.Should().Be(2);
    }

    [Fact]
    public void QueryBuilder_Where_EmptyCollectionContains_YieldsNoRows()
    {
        Seed("a", 1, null, 1);

        var empty = new List<int>();
        var rows = _connection
            .Builder<Widget>()
            .Where(w => empty.Contains(w.Score))
            .AsList()
            .ToList();

        rows.Should().BeEmpty();
    }

    [Fact]
    public void QueryBuilder_WhereIf_TrueBranch_AppliesPredicate()
    {
        Seed("alpha", 1, null, 1);
        Seed("beta", 2, null, 2);

        var list = _connection
            .Builder<Widget>()
            .WhereIf(true, w => w.Name == "alpha")
            .AsList()
            .ToList();

        list.Should().ContainSingle().Which.Name.Should().Be("alpha");
    }

    [Fact]
    public void QueryBuilder_FirstOrDefault_Sync()
    {
        Seed("a", 5, null, 1);
        Seed("b", 9, null, 2);

        var top = _connection.Builder<Widget>().OrderByDescending(w => w.Score).FirstOrDefault();
        var none = _connection
            .Builder<Widget>()
            .Where(w => w.Score < 0)
            .OrderBy(w => w.Id)
            .FirstOrDefault();

        top!.Score.Should().Be(9);
        none.Should().BeNull();
    }

    [Fact]
    public void QueryBuilder_Count_Any_None()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);

        _connection.Builder<Widget>().Count().Should().Be(2);
        _connection.Builder<Widget>().Any().Should().BeTrue();
        _connection.Builder<Widget>().None().Should().BeFalse();

        _connection.Builder<Widget>().Where(w => w.Score > 100).Any().Should().BeFalse();
        _connection.Builder<Widget>().Where(w => w.Score > 100).None().Should().BeTrue();
        _connection.Builder<Widget>().Where(w => w.Score >= 2).Count().Should().Be(1);
    }

    [Fact]
    public void QueryBuilder_TakeSkip_LimitsRowsCorrectly()
    {
        for (int i = 1; i <= 5; i++)
            Seed($"row-{i}", i, null, i);

        var take2 = _connection.Builder<Widget>().OrderBy(w => w.Score).Take(2).AsList().ToList();
        var skip2 = _connection.Builder<Widget>().OrderBy(w => w.Score).Skip(2).AsList().ToList();
        var skip1Take2 = _connection
            .Builder<Widget>()
            .OrderBy(w => w.Score)
            .Skip(1)
            .Take(2)
            .AsList()
            .ToList();

        take2.Select(w => w.Score).Should().ContainInOrder(1, 2);
        skip2.Select(w => w.Score).Should().ContainInOrder(3, 4, 5);
        skip1Take2.Select(w => w.Score).Should().ContainInOrder(2, 3);
    }

    [Fact]
    public void QueryBuilder_Select_SingleColumnProjection()
    {
        Seed("alpha", 5, "n1", 1);

        // Select only Score: other columns should be default.
        var w = _connection
            .Builder<Widget>()
            .Select(x => x.Score)
            .OrderBy(x => x.Score)
            .FirstOrDefault();

        w.Should().NotBeNull();
        w!.Score.Should().Be(5);
        w.Name.Should().BeEmpty(); // not selected -> default string
        w.Notes.Should().BeNull();
    }

    [Fact]
    public void QueryBuilder_Select_AnonymousProjection()
    {
        Seed("alpha", 5, "n1", 1);

        var w = _connection
            .Builder<Widget>()
            .Select(x => new { x.Id, x.Name })
            .OrderBy(x => x.Id)
            .FirstOrDefault();

        w!.Name.Should().Be("alpha");
        w.Score.Should().Be(0); // not selected
    }

    [Fact]
    public void QueryBuilder_Select_StringColumnProjection()
    {
        Seed("alpha", 5, "n1", 1);

        var w = _connection
            .Builder<Widget>()
            .Select("Name", "score_value")
            .OrderBy(x => x.Id)
            .FirstOrDefault();

        w!.Name.Should().Be("alpha");
        w.Score.Should().Be(5);
    }

    [Fact]
    public void QueryBuilder_Distinct_RemovesDuplicates()
    {
        Seed("alpha", 1, null, 1);
        Seed("alpha", 2, null, 2);
        Seed("beta", 3, null, 3);

        var names = _connection
            .Builder<Widget>()
            .Select(x => x.Name)
            .Distinct()
            .OrderBy(x => x.Name)
            .AsList()
            .Select(x => x.Name)
            .ToList();

        names.Should().ContainInOrder("alpha", "beta");
        names.Should().HaveCount(2);
    }

    [Fact]
    public void QueryBuilder_ThenBy_MultiColumnOrdering()
    {
        Seed("alpha", 5, null, 30);
        Seed("alpha", 1, null, 20);
        Seed("beta", 5, null, 10);

        var listAsc = _connection
            .Builder<Widget>()
            .OrderBy(w => w.Name)
            .ThenBy(w => w.Score)
            .AsList()
            .ToList();
        var listMixed = _connection
            .Builder<Widget>()
            .OrderBy(w => w.Name)
            .ThenByDescending(w => w.Score)
            .AsList()
            .ToList();

        listAsc
            .Select(w => (w.Name, w.Score))
            .Should()
            .ContainInOrder(("alpha", 1), ("alpha", 5), ("beta", 5));
        listMixed
            .Select(w => (w.Name, w.Score))
            .Should()
            .ContainInOrder(("alpha", 5), ("alpha", 1), ("beta", 5));
    }

    // ---------------------------------------------------------------------
    // Insert<T> with list / Update / Delete coverage and async equivalents.
    // ---------------------------------------------------------------------

    [Fact]
    public void Insert_ListOfEntities_ReturnsRowCount()
    {
        var batch = new List<Widget>
        {
            new()
            {
                Name = "x",
                Score = 1,
                Timestamp = 1,
            },
            new()
            {
                Name = "y",
                Score = 2,
                Timestamp = 2,
            },
            new()
            {
                Name = "z",
                Score = 3,
                Timestamp = 3,
            },
        };

        var inserted = _connection.Insert(batch);

        inserted.Should().Be(3);
        _connection.GetAll<Widget>().Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRows()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);

        var all = (await _connection.GetAllAsync<Widget>()).OrderBy(w => w.Score).ToList();

        all.Should().HaveCount(2);
        all.Select(w => w.Score).Should().ContainInOrder(1, 2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var id = Seed("alpha", 1, null, 1);
        var loaded = await _connection.GetAsync<Widget>(id);
        loaded!.Score = 77;

        var ok = await _connection.UpdateAsync(loaded);

        ok.Should().BeTrue();
        (await _connection.GetAsync<Widget>(id))!.Score.Should().Be(77);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRow()
    {
        var id = Seed("alpha", 1, null, 1);

        var ok = await _connection.DeleteAsync(new Widget { Id = id });

        ok.Should().BeTrue();
        (await _connection.GetAsync<Widget>(id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldRemoveEveryRow()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);

        var ok = await _connection.DeleteAllAsync<Widget>();

        ok.Should().BeTrue();
        (await _connection.GetAllAsync<Widget>()).Should().BeEmpty();
    }

    [Fact]
    public async Task QueryBuilder_ToListAsync_MaterializesToList()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);

        var list = await _connection.Builder<Widget>().OrderBy(w => w.Score).ToListAsync();

        list.Should().BeOfType<List<Widget>>();
        list.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryBuilder_AsListAsync_RespectsTakeSkipDistinctSelect()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);
        Seed("c", 3, null, 3);
        Seed("d", 4, null, 4);

        // Take/Skip applied
        var skipTake = (
            await _connection.Builder<Widget>().OrderBy(w => w.Score).Skip(1).Take(2).AsListAsync()
        )
            .Select(w => w.Score)
            .ToList();

        skipTake.Should().ContainInOrder(2, 3);

        // Distinct + Select applied (project Name only, then dedupe — we have 4 distinct names)
        var distinct = (
            await _connection
                .Builder<Widget>()
                .Select(w => w.Name)
                .Distinct()
                .OrderBy(w => w.Name)
                .AsListAsync()
        )
            .Select(w => w.Name)
            .ToList();

        distinct.Should().ContainInOrder("a", "b", "c", "d");
    }

    // ---------------------------------------------------------------------
    // Attribute coverage: [Computed], [Write(false)], [IgnoreUpdate], [ExplicitKey].
    // ---------------------------------------------------------------------

    [Table("gadgets")]
    public class Gadget
    {
        [ExplicitKey]
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // Set on Insert; subsequent UPDATE statements must not touch this column.
        [IgnoreUpdate]
        public string CreatedBy { get; set; } = string.Empty;

        // Database-managed default; never written by INSERT or UPDATE.
        [Write(false)]
        public string ServerTag { get; set; } = string.Empty;

        // Computed at SELECT time; ORM must not write it.
        [Computed]
        public string Display { get; set; } = string.Empty;
    }

    private void EnsureGadgetSchema()
    {
        _connection.Execute(
            @"CREATE TABLE IF NOT EXISTS gadgets (
                Id          TEXT PRIMARY KEY,
                Name        TEXT NOT NULL,
                CreatedBy   TEXT NOT NULL,
                ServerTag   TEXT NOT NULL DEFAULT 'srv-default',
                Display     TEXT NULL
            );"
        );
    }

    [Fact]
    public void ExplicitKey_ClientProvidedGuid_PersistsAndRoundTrips()
    {
        EnsureGadgetSchema();
        var id = Guid.NewGuid();
        var gadget = new Gadget
        {
            Id = id,
            Name = "explicit",
            CreatedBy = "tester",
        };

        var rows = _connection.Insert(gadget);

        rows.Should().Be(1);
        var loaded = _connection.Get<Gadget>(id);
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(id);
        loaded.Name.Should().Be("explicit");
        loaded.ServerTag.Should().Be("srv-default"); // came from DB default
    }

    [Fact]
    public void WriteFalse_ColumnIsExcludedFromInsert()
    {
        EnsureGadgetSchema();
        var gadget = new Gadget
        {
            Id = Guid.NewGuid(),
            Name = "wf",
            CreatedBy = "tester",
            ServerTag = "user-tried-to-set",
        };

        _connection.Insert(gadget);

        var loaded = _connection.Get<Gadget>(gadget.Id);
        loaded!.ServerTag.Should().Be("srv-default");
    }

    [Fact]
    public void IgnoreUpdate_ColumnIsExcludedFromUpdate()
    {
        EnsureGadgetSchema();
        var gadget = new Gadget
        {
            Id = Guid.NewGuid(),
            Name = "iu",
            CreatedBy = "creator",
        };
        _connection.Insert(gadget);

        gadget.Name = "renamed";
        gadget.CreatedBy = "tampered"; // [IgnoreUpdate] should make this a no-op
        var ok = _connection.Update(gadget);

        ok.Should().BeTrue();
        var loaded = _connection.Get<Gadget>(gadget.Id);
        loaded!.Name.Should().Be("renamed");
        loaded.CreatedBy.Should().Be("creator");
    }

    [Fact]
    public void Computed_ColumnIsNeverWrittenButReadable()
    {
        EnsureGadgetSchema();
        var id = Guid.NewGuid();
        var gadget = new Gadget
        {
            Id = id,
            Name = "comp",
            CreatedBy = "x",
            Display = "ignored-by-orm",
        };

        // Insert: ORM must not include Display in INSERT column list.
        _connection.Insert(gadget);

        // Simulate a server-side computed value via raw SQL (no WHERE since the
        // table has only one row). This avoids any binding-format mismatch on Guid.
        _connection.Execute("UPDATE gadgets SET Display = 'computed!'");

        var loaded = _connection.Get<Gadget>(id);
        loaded!.Display.Should().Be("computed!");

        // Update: ORM must not overwrite Display either.
        loaded.Display = "client-tampering";
        loaded.Name = "after-update";
        _connection.Update(loaded);

        var reloaded = _connection.Get<Gadget>(id);
        reloaded!.Display.Should().Be("computed!");
        reloaded.Name.Should().Be("after-update");
    }
}

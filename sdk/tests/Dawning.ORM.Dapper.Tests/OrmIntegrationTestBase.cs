using System.Data;
using Dapper;
using Dawning.ORM.Dapper;
using FluentAssertions;

namespace Dawning.ORM.Dapper.Tests;

/// <summary>
/// Adapter-agnostic ORM integration test surface. The same 48 cases are
/// executed against every concrete database adapter (SQLite, MySQL, Postgres)
/// by deriving from this class and supplying the connection and DDL hooks.
///
/// Lifecycle: xUnit instantiates this class once per test method, so the
/// constructor opens a fresh connection, drops the test tables (if any) and
/// recreates the <see cref="Widget"/> table. Suites that need
/// <see cref="WidgetWithoutDefaultSort"/> or <see cref="Gadget"/> opt in by
/// calling <see cref="EnsureNoOrderSchema"/> or <see cref="EnsureGadgetSchema"/>.
/// </summary>
public abstract class OrmIntegrationTestBase : IDisposable
{
    protected IDbConnection Connection { get; }

    [Table("widgets")]
    public class Widget
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Column("score_value")]
        public int Score { get; set; }

        public string? Notes { get; set; }

        // Mapped to a non-reserved column name so every supported adapter
        // (Firebird in particular treats TIMESTAMP as a reserved keyword)
        // can use bare identifiers in CREATE TABLE statements.
        [Column("ts_ms")]
        [DefaultSortName]
        public long Timestamp { get; set; }
    }

    [Table("noOrder")]
    public class WidgetWithoutDefaultSort
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

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

    protected OrmIntegrationTestBase()
    {
#pragma warning disable CA2214 // Virtual member call in constructor — intentional setup hook.
        Connection = CreateConnection();
        Connection.Open();
        ResetSchema();
#pragma warning restore CA2214
    }

    public void Dispose()
    {
        try
        {
            DropAllTables();
        }
        catch
        {
            // Best-effort cleanup; do not mask the real test failure.
        }
        Connection.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Open and return the adapter-specific connection.</summary>
    protected abstract IDbConnection CreateConnection();

    /// <summary>Drop and recreate the <see cref="Widget"/> table.</summary>
    protected abstract void ResetSchema();

    /// <summary>Drop the <see cref="WidgetWithoutDefaultSort"/> table if present, then create it.</summary>
    protected abstract void EnsureNoOrderSchema();

    /// <summary>Drop the <see cref="Gadget"/> table if present, then create it.</summary>
    protected abstract void EnsureGadgetSchema();

    /// <summary>Best-effort cleanup at end-of-test for shared databases.</summary>
    protected abstract void DropAllTables();

    /// <summary>
    /// Raw SQL that bypasses the ORM to set <see cref="Gadget.Display"/>
    /// (a [Computed] column the ORM refuses to write). Must use whatever
    /// quoting style the underlying database expects for case-preserving
    /// identifiers — defaults to the case-insensitive form which works for
    /// SQLite and MySQL; PostgreSQL overrides this to add double quotes.
    /// </summary>
    protected virtual string SetGadgetDisplaySql => "UPDATE gadgets SET Display = 'computed!'";

    protected long Seed(string name, int score, string? notes, long timestamp)
    {
        var entity = new Widget
        {
            Name = name,
            Score = score,
            Notes = notes,
            Timestamp = timestamp,
        };
        return Connection.Insert(entity);
    }

    // -----------------------------------------------------------------------
    // Sync CRUD
    // -----------------------------------------------------------------------

    [Fact]
    public void Insert_ShouldReturnLongIdentity_AndPopulateKey()
    {
        var entity = new Widget
        {
            Name = "alpha",
            Score = 1,
            Timestamp = 1,
        };

        var id = Connection.Insert(entity);

        id.Should().BeGreaterThan(0);
        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Get_ShouldReturnNull_WhenIdMissing()
    {
        Seed("alpha", 1, null, 1);

        var result = Connection.Get<Widget>(99999L);

        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_ShouldMapNullColumnToCSharpNull()
    {
        Seed("alpha", 1, null, 1);
        Seed("beta", 2, "with notes", 2);

        var all = Connection.GetAll<Widget>().OrderBy(w => w.Score).ToList();

        all.Should().HaveCount(2);
        all[0].Notes.Should().BeNull();
        all[1].Notes.Should().Be("with notes");
    }

    [Fact]
    public void Update_ShouldPersistChanges()
    {
        var id = Seed("alpha", 1, null, 1);
        var loaded = Connection.Get<Widget>(id);
        loaded.Should().NotBeNull();

        loaded!.Score = 42;
        loaded.Notes = "edited";
        var updated = Connection.Update(loaded);

        updated.Should().BeTrue();
        var fresh = Connection.Get<Widget>(id);
        fresh!.Score.Should().Be(42);
        fresh.Notes.Should().Be("edited");
    }

    [Fact]
    public void Delete_ShouldRemoveRow()
    {
        var id = Seed("alpha", 1, null, 1);

        var deleted = Connection.Delete(new Widget { Id = id });

        deleted.Should().BeTrue();
        Connection.Get<Widget>(id).Should().BeNull();
    }

    [Fact]
    public void DeleteAll_ShouldRemoveEveryRow()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);

        var deleted = Connection.DeleteAll<Widget>();

        deleted.Should().BeTrue();
        Connection.GetAll<Widget>().Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // QueryBuilder ORDER BY direction (regression for previous hardcoded DESC)
    // -----------------------------------------------------------------------

    [Fact]
    public void QueryBuilder_OrderByAscending_ShouldRespectDirection()
    {
        Seed("a", 3, null, 30);
        Seed("b", 1, null, 10);
        Seed("c", 2, null, 20);

        var page = Connection.Builder<Widget>().OrderBy(w => w.Score).AsPagedList(1, 10);

        page.Values.Select(w => w.Score).Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void QueryBuilder_OrderByDescending_ShouldRespectDirection()
    {
        Seed("a", 3, null, 30);
        Seed("b", 1, null, 10);
        Seed("c", 2, null, 20);

        var page = Connection.Builder<Widget>().OrderByDescending(w => w.Score).AsPagedList(1, 10);

        page.Values.Select(w => w.Score).Should().ContainInOrder(3, 2, 1);
    }

    [Fact]
    public async Task QueryBuilder_Where_ShouldFilter()
    {
        Seed("alpha", 5, null, 50);
        Seed("alpha-2", 10, null, 100);
        Seed("beta", 5, null, 60);

        var list = (
            await Connection
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
            await Connection
                .Builder<Widget>()
                .WhereIf(false, w => w.Name == "alpha")
                .OrderBy(w => w.Id)
                .AsListAsync()
        ).ToList();

        list.Should().HaveCount(2);
    }

    [Fact]
    public void QueryBuilder_AsPagedList_ShouldRequireOrderBy()
    {
        EnsureNoOrderSchema();
        Connection.Insert(new WidgetWithoutDefaultSort { Name = "x" });

        var act = () => Connection.Builder<WidgetWithoutDefaultSort>().AsPagedList(1, 10);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void QueryBuilder_DefaultSortNameAttribute_ShouldProvideOrdering()
    {
        Seed("a", 0, null, 30);
        Seed("b", 0, null, 10);
        Seed("c", 0, null, 20);

        // No explicit OrderBy(): should fall back to [DefaultSortName] (Timestamp DESC)
        var page = Connection.Builder<Widget>().AsPagedList(1, 10);

        page.Values.Select(w => w.Timestamp).Should().ContainInOrder(30L, 20L, 10L);
    }

    // -----------------------------------------------------------------------
    // Async CRUD basics
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAsync_ShouldReturnNull_WhenIdMissing()
    {
        Seed("alpha", 1, null, 1);

        var result = await Connection.GetAsync<Widget>(99999L);

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

        var id = await Connection.InsertAsync(entity);

        id.Should().BeGreaterThan(0);
        entity.Id.Should().Be(id);
    }

    [Fact]
    public async Task QueryBuilder_AsPagedListAsync_AscendingDescending()
    {
        Seed("a", 3, null, 30);
        Seed("b", 1, null, 10);
        Seed("c", 2, null, 20);

        var asc = await Connection.Builder<Widget>().OrderBy(w => w.Score).AsPagedListAsync(1, 10);
        var desc = await Connection
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

        var firstPage = await Connection
            .Builder<Widget>()
            .OrderBy(w => w.Id)
            .AsPagedListByCursorAsync(2);

        firstPage.Values.Select(w => w.Score).Should().ContainInOrder(1, 2);
        firstPage.HasNextPage.Should().BeTrue();
        firstPage.NextCursor.Should().NotBeNull();

        var secondPage = await Connection
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
            await Connection
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

        var first = await Connection
            .Builder<Widget>()
            .OrderByDescending(w => w.Score)
            .FirstOrDefaultAsync();

        first.Should().NotBeNull();
        first!.Score.Should().Be(3);
    }

    [Fact]
    public async Task QueryBuilder_FirstOrDefaultAsync_ReturnsNullWhenEmpty()
    {
        var first = await Connection
            .Builder<Widget>()
            .Where(w => w.Score < 0)
            .OrderBy(w => w.Id)
            .FirstOrDefaultAsync();

        first.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // Sync Builder coverage
    // -----------------------------------------------------------------------

    [Fact]
    public void QueryBuilder_Where_ComparisonOperators()
    {
        Seed("a", 1, null, 1);
        Seed("b", 5, null, 2);
        Seed("c", 9, null, 3);

        var gt = Connection.Builder<Widget>().Where(w => w.Score > 5).AsList().ToList();
        var ge = Connection.Builder<Widget>().Where(w => w.Score >= 5).AsList().ToList();
        var lt = Connection.Builder<Widget>().Where(w => w.Score < 5).AsList().ToList();
        var le = Connection.Builder<Widget>().Where(w => w.Score <= 5).AsList().ToList();
        var ne = Connection.Builder<Widget>().Where(w => w.Score != 5).AsList().ToList();

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

        var andList = Connection
            .Builder<Widget>()
            .Where(w => w.Name == "alpha" && w.Score > 5)
            .AsList()
            .ToList();
        var orList = Connection
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

        var hasNull = Connection.Builder<Widget>().Where(w => w.Notes == null).AsList().ToList();
        var notNull = Connection.Builder<Widget>().Where(w => w.Notes != null).AsList().ToList();

        hasNull.Should().ContainSingle().Which.Name.Should().Be("no-notes");
        notNull.Should().ContainSingle().Which.Name.Should().Be("with-notes");
    }

    [Fact]
    public void QueryBuilder_Where_StringMethods()
    {
        Seed("alpha", 1, null, 1);
        Seed("alphabet", 2, null, 2);
        Seed("beta", 3, null, 3);

        var startsWith = Connection
            .Builder<Widget>()
            .Where(w => w.Name.StartsWith("alpha"))
            .AsList()
            .ToList();
        var endsWith = Connection
            .Builder<Widget>()
            .Where(w => w.Name.EndsWith("bet"))
            .AsList()
            .ToList();
        var contains = Connection
            .Builder<Widget>()
            .Where(w => w.Name.Contains("phab"))
            .AsList()
            .ToList();
        var equalsCall = Connection
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
        var inList = Connection
            .Builder<Widget>()
            .Where(w => allowed.Contains(w.Score))
            .OrderBy(w => w.Score)
            .AsList()
            .ToList();
        var notInList = Connection
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
        var rows = Connection
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

        var list = Connection
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

        var top = Connection.Builder<Widget>().OrderByDescending(w => w.Score).FirstOrDefault();
        var none = Connection
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

        Connection.Builder<Widget>().Count().Should().Be(2);
        Connection.Builder<Widget>().Any().Should().BeTrue();
        Connection.Builder<Widget>().None().Should().BeFalse();

        Connection.Builder<Widget>().Where(w => w.Score > 100).Any().Should().BeFalse();
        Connection.Builder<Widget>().Where(w => w.Score > 100).None().Should().BeTrue();
        Connection.Builder<Widget>().Where(w => w.Score >= 2).Count().Should().Be(1);
    }

    [Fact]
    public void QueryBuilder_TakeSkip_LimitsRowsCorrectly()
    {
        for (int i = 1; i <= 5; i++)
            Seed($"row-{i}", i, null, i);

        var take2 = Connection.Builder<Widget>().OrderBy(w => w.Score).Take(2).AsList().ToList();
        var skip2 = Connection.Builder<Widget>().OrderBy(w => w.Score).Skip(2).AsList().ToList();
        var skip1Take2 = Connection
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
        var w = Connection
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

        var w = Connection
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

        var w = Connection
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

        var names = Connection
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

        var listAsc = Connection
            .Builder<Widget>()
            .OrderBy(w => w.Name)
            .ThenBy(w => w.Score)
            .AsList()
            .ToList();
        var listMixed = Connection
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

    // -----------------------------------------------------------------------
    // Insert<list> + async CRUD parity
    // -----------------------------------------------------------------------

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

        var inserted = Connection.Insert(batch);

        inserted.Should().Be(3);
        Connection.GetAll<Widget>().Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRows()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);

        var all = (await Connection.GetAllAsync<Widget>()).OrderBy(w => w.Score).ToList();

        all.Should().HaveCount(2);
        all.Select(w => w.Score).Should().ContainInOrder(1, 2);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var id = Seed("alpha", 1, null, 1);
        var loaded = await Connection.GetAsync<Widget>(id);
        loaded!.Score = 77;

        var ok = await Connection.UpdateAsync(loaded);

        ok.Should().BeTrue();
        (await Connection.GetAsync<Widget>(id))!.Score.Should().Be(77);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRow()
    {
        var id = Seed("alpha", 1, null, 1);

        var ok = await Connection.DeleteAsync(new Widget { Id = id });

        ok.Should().BeTrue();
        (await Connection.GetAsync<Widget>(id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAllAsync_ShouldRemoveEveryRow()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);

        var ok = await Connection.DeleteAllAsync<Widget>();

        ok.Should().BeTrue();
        (await Connection.GetAllAsync<Widget>()).Should().BeEmpty();
    }

    [Fact]
    public async Task QueryBuilder_ToListAsync_MaterializesToList()
    {
        Seed("a", 1, null, 1);
        Seed("b", 2, null, 2);

        var list = await Connection.Builder<Widget>().OrderBy(w => w.Score).ToListAsync();

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
            await Connection.Builder<Widget>().OrderBy(w => w.Score).Skip(1).Take(2).AsListAsync()
        )
            .Select(w => w.Score)
            .ToList();

        skipTake.Should().ContainInOrder(2, 3);

        // Distinct + Select applied (project Name only, then dedupe — we have 4 distinct names)
        var distinct = (
            await Connection
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

    // -----------------------------------------------------------------------
    // Attribute coverage: [Computed], [Write(false)], [IgnoreUpdate], [ExplicitKey].
    // -----------------------------------------------------------------------

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

        var rows = Connection.Insert(gadget);

        rows.Should().Be(1);
        var loaded = Connection.Get<Gadget>(id);
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

        Connection.Insert(gadget);

        var loaded = Connection.Get<Gadget>(gadget.Id);
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
        Connection.Insert(gadget);

        gadget.Name = "renamed";
        gadget.CreatedBy = "tampered"; // [IgnoreUpdate] should make this a no-op
        var ok = Connection.Update(gadget);

        ok.Should().BeTrue();
        var loaded = Connection.Get<Gadget>(gadget.Id);
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
        Connection.Insert(gadget);

        // Simulate a server-side computed value via raw SQL (no WHERE since the
        // table has only one row). This avoids any binding-format mismatch on Guid.
        Connection.Execute(SetGadgetDisplaySql);

        var loaded = Connection.Get<Gadget>(id);
        loaded!.Display.Should().Be("computed!");

        // Update: ORM must not overwrite Display either.
        loaded.Display = "client-tampering";
        loaded.Name = "after-update";
        Connection.Update(loaded);

        var reloaded = Connection.Get<Gadget>(id);
        reloaded!.Display.Should().Be("computed!");
        reloaded.Name.Should().Be("after-update");
    }
}

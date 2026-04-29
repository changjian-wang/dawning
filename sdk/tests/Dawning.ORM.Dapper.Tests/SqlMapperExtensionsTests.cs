using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using Dawning.ORM.Dapper;
using FluentAssertions;
using Microsoft.Data.Sqlite;

namespace Dawning.ORM.Dapper.Tests;

public class SqlMapperExtensionsTests
{
    [Table("test_entities")]
    public class TestEntity
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        [Column("email_address")]
        public string Email { get; set; } = string.Empty;
    }

    [Fact]
    public void TableAttribute_ShouldDefineTableName()
    {
        // Arrange
        var type = typeof(TestEntity);

        // Act
        var tableAttr =
            type.GetCustomAttributes(typeof(TableAttribute), true).FirstOrDefault()
            as TableAttribute;

        // Assert
        tableAttr.Should().NotBeNull();
        tableAttr!.Name.Should().Be("test_entities");
    }

    [Fact]
    public void KeyAttribute_ShouldIdentifyPrimaryKey()
    {
        // Arrange
        var type = typeof(TestEntity);
        var idProperty = type.GetProperty("Id");

        // Act
        var keyAttr = idProperty?.GetCustomAttributes(typeof(KeyAttribute), true).FirstOrDefault();

        // Assert
        keyAttr.Should().NotBeNull();
    }

    [Fact]
    public void ColumnAttribute_ShouldDefineColumnName()
    {
        // Arrange
        var type = typeof(TestEntity);
        var emailProperty = type.GetProperty("Email");

        // Act
        var columnAttr =
            emailProperty?.GetCustomAttributes(typeof(ColumnAttribute), true).FirstOrDefault()
            as ColumnAttribute;

        // Assert
        columnAttr.Should().NotBeNull();
        columnAttr!.Name.Should().Be("email_address");
    }

    [Table("default_name")]
    private class MapperRoundTripEntity
    {
        [Key]
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void TableNameMapper_SwapInvalidatesCachedTableName()
    {
        // Regression: GetTableName cached the resolved name keyed only by
        // RuntimeTypeHandle. Assigning SqlMapperExtensions.TableNameMapper
        // at runtime (e.g. for multi-tenant table prefixing or test
        // isolation) silently kept returning the stale name on every
        // entity that had been touched once. The cache is now keyed by
        // (TypeHandle, Mapper-reference) so a swap invalidates the entry.
        //
        // We exercise this by:
        //  1. resolving the table name once with no mapper (falls back to
        //     [Table("default_name")]),
        //  2. swapping in a mapper that returns a different name,
        //  3. resolving again — must reflect the mapper, not the cached
        //     value.
        // Use SQLite as a real connection but exercise only the path that
        // builds SQL (we don't care if the table actually exists).
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var originalMapper = SqlMapperExtensions.TableNameMapper;
        try
        {
            SqlMapperExtensions.TableNameMapper = null;

            // First resolution — populates cache with "default_name".
            // We exercise it via the Insert path; the table doesn't exist
            // so SQLite throws SqliteException, but only AFTER the table
            // name has been resolved and used in the SQL string.
            var firstError = Record.Exception(() =>
                connection.Insert(new MapperRoundTripEntity { Name = "x" })
            );
            firstError.Should().NotBeNull();
            firstError!.Message.Should().Contain("default_name");

            // Swap to a mapper that returns a different name; the cache
            // must NOT continue to return "default_name".
            // Limit the mapper's effect to MapperRoundTripEntity so it does
            // not interfere with adapter integration tests running in
            // parallel against the [Table("widgets")] entity.
            SqlMapperExtensions.TableNameMapper = t =>
                t == typeof(MapperRoundTripEntity)
                    ? "remapped_name"
                    : (t.GetCustomAttribute<TableAttribute>(false)?.Name ?? (t.Name + "s"));

            var secondError = Record.Exception(() =>
                connection.Insert(new MapperRoundTripEntity { Name = "y" })
            );
            secondError.Should().NotBeNull();
            secondError!.Message.Should().Contain("remapped_name");
            secondError.Message.Should().NotContain("default_name");
        }
        finally
        {
            SqlMapperExtensions.TableNameMapper = originalMapper;
        }
    }
}

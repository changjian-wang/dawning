using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Dawning.ORM.Dapper;
using FluentAssertions;

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
}

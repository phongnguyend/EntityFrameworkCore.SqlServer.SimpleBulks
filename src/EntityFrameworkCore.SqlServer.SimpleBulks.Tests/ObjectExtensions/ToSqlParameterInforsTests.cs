using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using System.Data;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.ObjectExtensions;

public class ToSqlParameterInforsTests
{
    [Fact]
    public void ToSqlParameterInfors_NullParameter_ReturnsEmptyList()
    {
        // Arrange
        object? parameters = null;

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithStringProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var parameters = new { Name = "TestValue" };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@Name", result[0].Name);
        Assert.Equal("nvarchar(max)", result[0].Type);
        Assert.Equal("@Name", result[0].Parameter.ParameterName);
        Assert.Equal("TestValue", result[0].Parameter.Value);
        Assert.Equal(SqlDbType.NVarChar, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithIntProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var parameters = new { Count = 42 };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@Count", result[0].Name);
        Assert.Equal("int", result[0].Type);
        Assert.Equal("@Count", result[0].Parameter.ParameterName);
        Assert.Equal(42, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.Int, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithMultipleProperties_ReturnsAllParameters()
    {
        // Arrange
        var parameters = new
        {
            Id = 1,
            Name = "Test",
            IsActive = true
        };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Equal(3, result.Count);

        var idParam = result.First(p => p.Name == "@Id");
        Assert.Equal("int", idParam.Type);
        Assert.Equal("@Id", idParam.Parameter.ParameterName);
        Assert.Equal(1, idParam.Parameter.Value);

        var nameParam = result.First(p => p.Name == "@Name");
        Assert.Equal("nvarchar(max)", nameParam.Type);
        Assert.Equal("@Name", nameParam.Parameter.ParameterName);
        Assert.Equal("Test", nameParam.Parameter.Value);

        var isActiveParam = result.First(p => p.Name == "@IsActive");
        Assert.Equal("bit", isActiveParam.Type);
        Assert.Equal("@IsActive", isActiveParam.Parameter.ParameterName);
        Assert.Equal(true, isActiveParam.Parameter.Value);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithDateTimeProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var testDate = new DateTime(2024, 1, 15, 10, 30, 0);
        var parameters = new { CreatedDate = testDate };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@CreatedDate", result[0].Name);
        Assert.Equal("datetime2", result[0].Type);
        Assert.Equal("@CreatedDate", result[0].Parameter.ParameterName);
        Assert.Equal(testDate, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.DateTime2, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithGuidProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var testGuid = Guid.NewGuid();
        var parameters = new { UniqueId = testGuid };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@UniqueId", result[0].Name);
        Assert.Equal("uniqueidentifier", result[0].Type);
        Assert.Equal("@UniqueId", result[0].Parameter.ParameterName);
        Assert.Equal(testGuid, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.UniqueIdentifier, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithNullableIntWithValue_ReturnsCorrectParameter()
    {
        // Arrange
        int? value = 100;
        var parameters = new { NullableInt = value };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@NullableInt", result[0].Name);
        Assert.Equal("int", result[0].Type);
        Assert.Equal("@NullableInt", result[0].Parameter.ParameterName);
        Assert.Equal(100, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.Int, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithNullableIntWithNull_ReturnsDBNullValue()
    {
        // Arrange
        int? value = null;
        var parameters = new { NullableInt = value };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@NullableInt", result[0].Name);
        Assert.Equal("int", result[0].Type);
        Assert.Equal("@NullableInt", result[0].Parameter.ParameterName);
        Assert.Equal(DBNull.Value, result[0].Parameter.Value);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithNullString_ReturnsDBNullValue()
    {
        // Arrange
        string? value = null;
        var parameters = new { NullString = value };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@NullString", result[0].Name);
        Assert.Equal("@NullString", result[0].Parameter.ParameterName);
        Assert.Equal(DBNull.Value, result[0].Parameter.Value);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithDecimalProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var parameters = new { Amount = 123.45m };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@Amount", result[0].Name);
        Assert.Equal("decimal(38, 20)", result[0].Type);
        Assert.Equal("@Amount", result[0].Parameter.ParameterName);
        Assert.Equal(123.45m, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.Decimal, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithLongProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var parameters = new { BigNumber = 9223372036854775807L };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@BigNumber", result[0].Name);
        Assert.Equal("bigint", result[0].Type);
        Assert.Equal("@BigNumber", result[0].Parameter.ParameterName);
        Assert.Equal(9223372036854775807L, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.BigInt, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithDoubleProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var parameters = new { FloatValue = 3.14159d };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@FloatValue", result[0].Name);
        Assert.Equal("float", result[0].Type);
        Assert.Equal("@FloatValue", result[0].Parameter.ParameterName);
        Assert.Equal(3.14159d, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.Float, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithByteArrayProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        var parameters = new { BinaryData = bytes };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@BinaryData", result[0].Name);
        Assert.Equal("nvarchar(max)", result[0].Type);
        Assert.Equal("@BinaryData", result[0].Parameter.ParameterName);
        Assert.Equal(bytes, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.NVarChar, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithDateTimeOffsetProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var testDate = new DateTimeOffset(2024, 1, 15, 10, 30, 0, TimeSpan.FromHours(5));
        var parameters = new { CreatedAt = testDate };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@CreatedAt", result[0].Name);
        Assert.Equal("datetimeoffset", result[0].Type);
        Assert.Equal("@CreatedAt", result[0].Parameter.ParameterName);
        Assert.Equal(testDate, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.DateTimeOffset, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithEnumProperty_ReturnsIntType()
    {
        // Arrange
        var parameters = new { Status = TestStatus.Active };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@Status", result[0].Name);
        Assert.Equal("int", result[0].Type);
        Assert.Equal("@Status", result[0].Parameter.ParameterName);
        Assert.Equal(SqlDbType.Int, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithNullableEnumWithValue_ReturnsIntType()
    {
        // Arrange
        TestStatus? status = TestStatus.Inactive;
        var parameters = new { Status = status };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@Status", result[0].Name);
        Assert.Equal("int", result[0].Type);
        Assert.Equal("@Status", result[0].Parameter.ParameterName);
        Assert.Equal(SqlDbType.Int, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithNullableEnumWithNull_ReturnsDBNullValue()
    {
        // Arrange
        TestStatus? status = null;
        var parameters = new { Status = status };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@Status", result[0].Name);
        Assert.Equal("int", result[0].Type);
        Assert.Equal("@Status", result[0].Parameter.ParameterName);
        Assert.Equal(DBNull.Value, result[0].Parameter.Value);
    }

    [Fact]
    public void ToSqlParameterInfors_ObjectWithTimeSpanProperty_ReturnsCorrectParameter()
    {
        // Arrange
        var timeSpan = new TimeSpan(10, 30, 0);
        var parameters = new { Duration = timeSpan };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.Single(result);
        Assert.Equal("@Duration", result[0].Name);
        Assert.Equal("nvarchar(max)", result[0].Type);
        Assert.Equal("@Duration", result[0].Parameter.ParameterName);
        Assert.Equal(timeSpan, result[0].Parameter.Value);
        Assert.Equal(SqlDbType.NVarChar, result[0].Parameter.SqlDbType);
    }

    [Fact]
    public void ToSqlParameterInfors_EmptyObject_ReturnsEmptyList()
    {
        // Arrange
        var parameters = new { };

        // Act
        var result = parameters.ToSqlParameterInfors();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private enum TestStatus
    {
        Inactive = 0,
        Active = 1,
        Pending = 2
    }
}

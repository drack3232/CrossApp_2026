
using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class Lab09StorageTests : IDisposable
{
    private readonly string _directory = Path.Combine(
        Path.GetTempPath(),
        "CrossApp-Lab09-" + Guid.NewGuid().ToString("N")
    );

    public Lab09StorageTests()
    {
        Directory.CreateDirectory(_directory);
    }

    [Fact]
    public void FileOrderStore_AddAndReload_PreservesOrder()
    {
        string path = Path.Combine(_directory, "orders.json");

        var order = Order.Create(
            "T-901",
            "Цірі",
            "Каер Морхен"
        );

        order.AddLine("P-901", "Срібний меч", 250.50m, 2);
        order.Confirm();

        var firstStore = new FileOrderStore(path);
        firstStore.Add(order);

        var secondStore = new FileOrderStore(path);
        var loaded = Assert.Single(secondStore.List());

        Assert.Equal("T-901", loaded.Id);
        Assert.Equal("Цірі", loaded.Customer);
        Assert.True(loaded.IsConfirmed);
        Assert.Equal(501m, loaded.Total);
        Assert.Single(loaded.Lines);
        Assert.Equal("Срібний меч", loaded.Lines[0].Name);
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void CsvFormat_RoundTrip_PreservesData()
    {
        string path = Path.Combine(_directory, "orders.csv");

        var order = Order.Create(
            "T-902",
            "Геральт",
            "Львів",
            "Терміново; сьогодні"
        );

        order.AddLine(
            "P-902",
            "Меч \"Вовк\"",
            100m,
            3
        );

        order.Confirm();

        var format = new CsvOrderDataFormat();

        format.Export(path, new[] { order });

        var loaded = Assert.Single(format.Import(path));

        Assert.Equal(order.Comment, loaded.Comment);
        Assert.Equal(300m, loaded.Total);
        Assert.True(loaded.IsConfirmed);
        Assert.Equal(
            "Меч \"Вовк\"",
            loaded.Lines[0].Name
        );
    }

    [Fact]
    public void Validation_RejectsEmptyDataPath()
    {
        var options = new AppOptions();
        options.Storage.DataPath = "";

        Assert.Throws<ArgumentException>(
            () => AppOptionsValidator.Validate(options)
        );
    }

    [Theory]
    [InlineData("orders.txt")]
    [InlineData("orders")]
    public void Validation_RejectsIncorrectFileName(string fileName)
    {
        var options = new AppOptions();
        options.Storage.FileName = fileName;

        Assert.Throws<ArgumentException>(
            () => AppOptionsValidator.Validate(options)
        );
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}

using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;
using EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.Database;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.ConnectionExtensionsTests.ConnectionExtensions;

[Collection("SqlServerCollection")]
public class BulkMatchAsyncTests : BaseTest
{
    private readonly List<Customer> _customers;
    private readonly List<Contact> _contacts;

    public BulkMatchAsyncTests(ITestOutputHelper output, SqlServerFixture fixture) : base(output, fixture, "EFCoreSimpleBulksTests.BulkMatch")
    {
        var tran = _context.Database.BeginTransaction();

        var isoCodes = new string[] { "VN", "US", "GB" };
        var random = new Random(2024);

        _customers = new List<Customer>();

        for (var i = 0; i < 100; i++)
        {
            var customer = new Customer
            {
                FirstName = "FirstName " + i,
                LastName = "LastName " + i,
                Index = i,
                CurrentCountryIsoCode = isoCodes[random.Next(isoCodes.Length)],
                Season = Season.Spring,
                SeasonAsString = Season.Spring
            };

            customer.Contacts = new List<Contact>();

            for (var j = 0; j < 100; j++)
            {
                customer.Contacts.Add(new Contact
                {
                    EmailAddress = $"EmailAddress {i} - {j}",
                    PhoneNumber = $"PhoneNumber {i} - {j}",
                    CountryIsoCode = isoCodes[random.Next(isoCodes.Length)],
                    Season = Season.Spring,
                    SeasonAsString = Season.Spring,
                    Index = j,
                });
            }

            _customers.Add(customer);
        }

        _context.BulkInsert(_customers);

        foreach (var customer in _customers)
        {
            foreach (var contact in customer.Contacts)
            {
                contact.CustomerId = customer.Id;
            }
        }

        _contacts = _customers.SelectMany(x => x.Contacts).ToList();

        _context.BulkInsert(_contacts);

        tran.Commit();
    }

    [Fact]
    public async Task Bulk_Match_GetCustomersByIds_ReturnAllColumns()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var customerIds = customers.Select(x => x.Id).ToList();
        var matchedCustommers = customerIds.Select(x => new Customer { Id = x }).ToList();

        // Act
        var customersFromDb = await connectionContext.BulkMatchAsync(matchedCustommers,
            x => x.Id,
            options: new BulkMatchOptions()
            {
                LogTo = _output.WriteLine
            });

        // Assert
        Assert.Equal(customers.Count, customersFromDb.Count);
        for (var i = 0; i < customers.Count; i++)
        {
            Assert.Equal(customers[i].Id, customersFromDb[i].Id);
            Assert.Equal(customers[i].FirstName, customersFromDb[i].FirstName);
            Assert.Equal(customers[i].LastName, customersFromDb[i].LastName);
            Assert.Equal(customers[i].Index, customersFromDb[i].Index);
            Assert.Equal(customers[i].Season, customersFromDb[i].Season);
            Assert.Equal(customers[i].SeasonAsString, customersFromDb[i].SeasonAsString);
        }
    }

    [Fact]
    public async Task Bulk_Match_GetCustomersByIds_ReturnSelectedColumns()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var customerIds = customers.Select(x => x.Id).ToList();
        var matchedCustommers = customerIds.Select(x => new Customer { Id = x }).ToList();

        // Act
        var customersFromDb = await connectionContext.BulkMatchAsync(matchedCustommers,
            x => x.Id,
            x => new { x.Id, x.FirstName },
            options: new BulkMatchOptions()
            {
                LogTo = _output.WriteLine
            });

        // Assert
        Assert.Equal(customers.Count, customersFromDb.Count);
        for (var i = 0; i < customers.Count; i++)
        {
            Assert.Equal(customers[i].Id, customersFromDb[i].Id);
            Assert.Equal(customers[i].FirstName, customersFromDb[i].FirstName);
            Assert.Null(customersFromDb[i].LastName);
            Assert.Equal(0, customersFromDb[i].Index);
        }
    }

    [Fact]
    public async Task Bulk_Match_GetContactsByCustomerIds_ReturnAllColumns()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var customerIds = customers.Select(x => x.Id).ToList();
        var matchedContacts = customerIds.Select(x => new Contact { CustomerId = x }).ToList();

        // Act
        var contactsFromDb = (await connectionContext.BulkMatchAsync(matchedContacts,
            x => x.CustomerId,
            options: new BulkMatchOptions()
            {
                LogTo = _output.WriteLine
            }))
            .OrderBy(x => x.Id).ToList();

        var contactsInMemory = _contacts.Where(x => customerIds.Contains(x.CustomerId)).OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Equal(contactsInMemory.Count, contactsFromDb.Count);
        for (var i = 0; i < contactsInMemory.Count; i++)
        {
            Assert.Equal(contactsInMemory[i].Id, contactsFromDb[i].Id);
            Assert.Equal(contactsInMemory[i].EmailAddress, contactsFromDb[i].EmailAddress);
            Assert.Equal(contactsInMemory[i].PhoneNumber, contactsFromDb[i].PhoneNumber);
            Assert.Equal(contactsInMemory[i].CountryIsoCode, contactsFromDb[i].CountryIsoCode);
            Assert.Equal(contactsInMemory[i].Index, contactsFromDb[i].Index);
            Assert.Equal(contactsInMemory[i].CustomerId, contactsFromDb[i].CustomerId);
            Assert.Equal(contactsInMemory[i].Season, contactsFromDb[i].Season);
            Assert.Equal(contactsInMemory[i].SeasonAsString, contactsFromDb[i].SeasonAsString);
        }
    }

    [Fact]
    public async Task Bulk_Match_GetContactsByCustomerIds_ReturnSelectedColumns()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var customerIds = customers.Select(x => x.Id).ToList();
        var matchedContacts = customerIds.Select(x => new Contact { CustomerId = x }).ToList();

        // Act
        var contactsFromDb = (await connectionContext.BulkMatchAsync(matchedContacts,
            x => x.CustomerId,
            x => new { x.Id, x.PhoneNumber },
            options: new BulkMatchOptions()
            {
                LogTo = _output.WriteLine
            }))
            .OrderBy(x => x.Id).ToList();

        var contactsInMemory = _contacts.Where(x => customerIds.Contains(x.CustomerId)).OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Equal(contactsInMemory.Count, contactsFromDb.Count);
        for (var i = 0; i < contactsInMemory.Count; i++)
        {
            Assert.Equal(contactsInMemory[i].Id, contactsFromDb[i].Id);
            Assert.Null(contactsFromDb[i].EmailAddress);
            Assert.Equal(contactsInMemory[i].PhoneNumber, contactsFromDb[i].PhoneNumber);
            Assert.Equal(0, contactsFromDb[i].Index);
            Assert.Equal(Guid.Empty, contactsFromDb[i].CustomerId);
        }
    }

    [Fact]
    public async Task Bulk_Match_GetDefaultContactsByCustomerIds_ReturnAllColumns()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var matchedContacts = customers.Select(x => new Contact { CustomerId = x.Id, CountryIsoCode = x.CurrentCountryIsoCode }).ToList();

        // Act
        var contactsFromDb = (await connectionContext.BulkMatchAsync(matchedContacts,
            x => new { x.CustomerId, x.CountryIsoCode },
            options: new BulkMatchOptions()
            {
                LogTo = _output.WriteLine
            }))
            .OrderBy(x => x.Id).ToList();

        var contactsInMemory = _contacts.Where(x => customers.Any(y => y.Id == x.CustomerId && y.CurrentCountryIsoCode == x.CountryIsoCode)).OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Equal(contactsInMemory.Count, contactsFromDb.Count);
        for (var i = 0; i < contactsInMemory.Count; i++)
        {
            Assert.Equal(contactsInMemory[i].Id, contactsFromDb[i].Id);
            Assert.Equal(contactsInMemory[i].EmailAddress, contactsFromDb[i].EmailAddress);
            Assert.Equal(contactsInMemory[i].PhoneNumber, contactsFromDb[i].PhoneNumber);
            Assert.Equal(contactsInMemory[i].CountryIsoCode, contactsFromDb[i].CountryIsoCode);
            Assert.Equal(contactsInMemory[i].Index, contactsFromDb[i].Index);
            Assert.Equal(contactsInMemory[i].CustomerId, contactsFromDb[i].CustomerId);
            Assert.Equal(contactsInMemory[i].Season, contactsFromDb[i].Season);
            Assert.Equal(contactsInMemory[i].SeasonAsString, contactsFromDb[i].SeasonAsString);
        }
    }

    [Fact]
    public async Task Bulk_Match_GetDefaultContactsByCustomerIds_ReturnSelectedColumns()
    {
        var connectionContext = new ConnectionContext(_connection, null);

        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var matchedContacts = customers.Select(x => new Contact { CustomerId = x.Id, CountryIsoCode = x.CurrentCountryIsoCode }).ToList();

        // Act
        var contactsFromDb = (await connectionContext.BulkMatchAsync(matchedContacts,
            x => new { x.CustomerId, x.CountryIsoCode },
            x => new { x.Id, x.PhoneNumber },
            options: new BulkMatchOptions()
            {
                LogTo = _output.WriteLine
            }))
            .OrderBy(x => x.Id).ToList();

        var contactsInMemory = _contacts.Where(x => customers.Any(y => y.Id == x.CustomerId && y.CurrentCountryIsoCode == x.CountryIsoCode)).OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Equal(contactsInMemory.Count, contactsFromDb.Count);

        for (var i = 0; i < contactsInMemory.Count; i++)
        {
            Assert.Equal(contactsInMemory[i].Id, contactsFromDb[i].Id);
            Assert.Null(contactsFromDb[i].EmailAddress);
            Assert.Equal(contactsInMemory[i].PhoneNumber, contactsFromDb[i].PhoneNumber);
            Assert.Equal(0, contactsFromDb[i].Index);
            Assert.Equal(Guid.Empty, contactsFromDb[i].CustomerId);
        }
    }
}

using EntityFrameworkCore.SqlServer.SimpleBulks.BulkInsert;
using EntityFrameworkCore.SqlServer.SimpleBulks.BulkMatch;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions;

public class BulkMatchTests : IDisposable
{
    private readonly ITestOutputHelper _output;

    private TestDbContext _context;

    private readonly List<Customer> _customers;
    private readonly List<Contact> _contacts;

    public BulkMatchTests(ITestOutputHelper output)
    {
        _output = output;

        _context = new TestDbContext($"Server=127.0.0.1;Database=EFCoreSimpleBulksTests.BulkMatch.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
        _context.Database.EnsureCreated();

        var tran = _context.Database.BeginTransaction();

        var isoCodes = new string[] { "VN", "US", "GB" };
        var random = new Random(2024);

        _customers = new List<Customer>();

        for (int i = 0; i < 100; i++)
        {
            var customer = new Customer
            {
                FirstName = "FirstName " + i,
                LastName = "LastName " + i,
                Index = i,
                CurrentCountryIsoCode = isoCodes[random.Next(isoCodes.Length)]
            };

            customer.Contacts = new List<Contact>();

            for (int j = 0; j < 100; j++)
            {
                customer.Contacts.Add(new Contact
                {
                    EmailAddress = $"EmailAddress {i} - {j}",
                    PhoneNumber = $"PhoneNumber {i} - {j}",
                    CountryIsoCode = isoCodes[random.Next(isoCodes.Length)],
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
    public void Bulk_Match_GetCustomersByIds_ReturnAllColumns()
    {
        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var customerIds = customers.Select(x => x.Id).ToList();
        var matchedCustommers = customerIds.Select(x => new Customer { Id = x });

        // Act
        var customersFromDb = _context.BulkMatch(matchedCustommers,
            x => x.Id,
            options =>
            {
                options.LogTo = _output.WriteLine;
            });

        // Assert
        Assert.Equal(customers.Count, customersFromDb.Count);
        for (int i = 0; i < customers.Count; i++)
        {
            Assert.Equal(customers[i].Id, customersFromDb[i].Id);
            Assert.Equal(customers[i].FirstName, customersFromDb[i].FirstName);
            Assert.Equal(customers[i].LastName, customersFromDb[i].LastName);
            Assert.Equal(customers[i].Index, customersFromDb[i].Index);
        }
    }

    [Fact]
    public void Bulk_Match_GetCustomersByIds_ReturnSelectedColumns()
    {
        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var customerIds = customers.Select(x => x.Id).ToList();
        var matchedCustommers = customerIds.Select(x => new Customer { Id = x });

        // Act
        var customersFromDb = _context.BulkMatch(matchedCustommers,
            x => x.Id,
            x => new { x.Id, x.FirstName },
            options =>
            {
                options.LogTo = _output.WriteLine;
            });

        // Assert
        Assert.Equal(customers.Count, customersFromDb.Count);
        for (int i = 0; i < customers.Count; i++)
        {
            Assert.Equal(customers[i].Id, customersFromDb[i].Id);
            Assert.Equal(customers[i].FirstName, customersFromDb[i].FirstName);
            Assert.Null(customersFromDb[i].LastName);
            Assert.Equal(0, customersFromDb[i].Index);
        }
    }

    [Fact]
    public void Bulk_Match_GetContactsByCustomerIds_ReturnAllColumns()
    {
        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var customerIds = customers.Select(x => x.Id).ToList();
        var matchedContacts = customerIds.Select(x => new Contact { CustomerId = x });

        // Act
        var contactsFromDb = _context.BulkMatch(matchedContacts,
            x => x.CustomerId,
            options =>
            {
                options.LogTo = _output.WriteLine;
            })
            .OrderBy(x => x.Id).ToList();

        var contactsInMemory = _contacts.Where(x => customerIds.Contains(x.CustomerId)).OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Equal(contactsInMemory.Count, contactsFromDb.Count);
        for (int i = 0; i < contactsInMemory.Count; i++)
        {
            Assert.Equal(contactsInMemory[i].Id, contactsFromDb[i].Id);
            Assert.Equal(contactsInMemory[i].EmailAddress, contactsFromDb[i].EmailAddress);
            Assert.Equal(contactsInMemory[i].PhoneNumber, contactsFromDb[i].PhoneNumber);
            Assert.Equal(contactsInMemory[i].CountryIsoCode, contactsFromDb[i].CountryIsoCode);
            Assert.Equal(contactsInMemory[i].Index, contactsFromDb[i].Index);
            Assert.Equal(contactsInMemory[i].CustomerId, contactsFromDb[i].CustomerId);
        }
    }

    [Fact]
    public void Bulk_Match_GetContactsByCustomerIds_ReturnSelectedColumns()
    {
        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var customerIds = customers.Select(x => x.Id).ToList();
        var matchedContacts = customerIds.Select(x => new Contact { CustomerId = x });

        // Act
        var contactsFromDb = _context.BulkMatch(matchedContacts,
            x => x.CustomerId,
            x => new { x.Id, x.PhoneNumber },
            options =>
            {
                options.LogTo = _output.WriteLine;
            })
            .OrderBy(x => x.Id).ToList();

        var contactsInMemory = _contacts.Where(x => customerIds.Contains(x.CustomerId)).OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Equal(contactsInMemory.Count, contactsFromDb.Count);
        for (int i = 0; i < contactsInMemory.Count; i++)
        {
            Assert.Equal(contactsInMemory[i].Id, contactsFromDb[i].Id);
            Assert.Null(contactsFromDb[i].EmailAddress);
            Assert.Equal(contactsInMemory[i].PhoneNumber, contactsFromDb[i].PhoneNumber);
            Assert.Equal(0, contactsFromDb[i].Index);
            Assert.Equal(Guid.Empty, contactsFromDb[i].CustomerId);
        }
    }

    [Fact]
    public void Bulk_Match_GetDefaultContactsByCustomerIds_ReturnAllColumns()
    {
        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var matchedContacts = customers.Select(x => new Contact { CustomerId = x.Id, CountryIsoCode = x.CurrentCountryIsoCode });

        // Act
        var contactsFromDb = _context.BulkMatch(matchedContacts,
            x => new { x.CustomerId, x.CountryIsoCode },
            options =>
            {
                options.LogTo = _output.WriteLine;
            })
            .OrderBy(x => x.Id).ToList();

        var contactsInMemory = _contacts.Where(x => customers.Any(y => y.Id == x.CustomerId && y.CurrentCountryIsoCode == x.CountryIsoCode)).OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Equal(contactsInMemory.Count, contactsFromDb.Count);
        for (int i = 0; i < contactsInMemory.Count; i++)
        {
            Assert.Equal(contactsInMemory[i].Id, contactsFromDb[i].Id);
            Assert.Equal(contactsInMemory[i].EmailAddress, contactsFromDb[i].EmailAddress);
            Assert.Equal(contactsInMemory[i].PhoneNumber, contactsFromDb[i].PhoneNumber);
            Assert.Equal(contactsInMemory[i].CountryIsoCode, contactsFromDb[i].CountryIsoCode);
            Assert.Equal(contactsInMemory[i].Index, contactsFromDb[i].Index);
            Assert.Equal(contactsInMemory[i].CustomerId, contactsFromDb[i].CustomerId);
        }
    }

    [Fact]
    public void Bulk_Match_GetDefaultContactsByCustomerIds_ReturnSelectedColumns()
    {
        // Arrange
        var customers = _customers.Where(x => x.Index % 5 == 0).ToList();
        var matchedContacts = customers.Select(x => new Contact { CustomerId = x.Id, CountryIsoCode = x.CurrentCountryIsoCode });

        // Act
        var contactsFromDb = _context.BulkMatch(matchedContacts,
            x => new { x.CustomerId, x.CountryIsoCode },
            x => new { x.Id, x.PhoneNumber },
            options =>
            {
                options.LogTo = _output.WriteLine;
            })
            .OrderBy(x => x.Id).ToList();

        var contactsInMemory = _contacts.Where(x => customers.Any(y => y.Id == x.CustomerId && y.CurrentCountryIsoCode == x.CountryIsoCode)).OrderBy(x => x.Id).ToList();

        // Assert
        Assert.Equal(contactsInMemory.Count, contactsFromDb.Count);

        for (int i = 0; i < contactsInMemory.Count; i++)
        {
            Assert.Equal(contactsInMemory[i].Id, contactsFromDb[i].Id);
            Assert.Null(contactsFromDb[i].EmailAddress);
            Assert.Equal(contactsInMemory[i].PhoneNumber, contactsFromDb[i].PhoneNumber);
            Assert.Equal(0, contactsFromDb[i].Index);
            Assert.Equal(Guid.Empty, contactsFromDb[i].CustomerId);
        }
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }
}

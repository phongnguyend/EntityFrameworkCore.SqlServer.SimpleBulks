using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace EntityFrameworkCore.SqlServer.SimpleBulks.Tests.DbContextExtensions
{
    public class TempTableTests : IDisposable
    {
        private readonly ITestOutputHelper _output;

        private TestDbContext _context;

        private readonly static List<CustomerDto> _customers = new List<CustomerDto>
        {
            new CustomerDto
            {
                IdNumber = "00000001",
                FirstName = "Ti",
                LastName = "Nguyen",
                CurrentCountryIsoCode = "VI"
            },
            new CustomerDto
            {
                IdNumber = "00000002",
                FirstName = "Suu",
                LastName = "Nguyen",
                CurrentCountryIsoCode = "VI"
            },
            new CustomerDto
            {
                IdNumber = "00000003",
                FirstName = "Dan",
                LastName = "Nguyen",
                CurrentCountryIsoCode = "VI"
            }
        };

        private readonly List<ContactDto> _contacts = new List<ContactDto>
        {
            new ContactDto
            {
                CustomerIdNumber = "00000001",
                EmailAddress = "ti@gmail.com",
                PhoneNumber = "+84123456789",
                CountryIsoCode = "VI"
            },
            new ContactDto
            {
                CustomerIdNumber = "00000001",
                EmailAddress = "chuot@gmail.com",
                PhoneNumber = "+84123456790",
                CountryIsoCode = "US"
            },
            new ContactDto
            {
                CustomerIdNumber = "00000002",
                EmailAddress = "suu@gmail.com",
                PhoneNumber = "+84123456791",
                CountryIsoCode = "VI"
            }
        };

        public TempTableTests(ITestOutputHelper output)
        {
            _output = output;

            _context = new TestDbContext($"Server=127.0.0.1;Database=EFCoreSimpleBulksTests.TempTable.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#;Encrypt=False");
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }

        [Fact]
        public void Non_Entity_Type_Single_Table_With_Selected_Columns()
        {
            var customers = new List<CustomerDto>();

            var tableName = _context.CreateTempTable(_customers,
                   x => new
                   {
                       x.IdNumber,
                       x.FirstName,
                       x.LastName,
                       x.CurrentCountryIsoCode
                   },
                   options =>
                   {
                       options.LogTo = _output.WriteLine;
                    });

            string sql = $"select * from {tableName}";

            _context.ExecuteReader(sql, reader =>
            {
                customers.Add(new CustomerDto
                {
                    IdNumber = reader["IdNumber"] as string,
                    FirstName = reader["FirstName"] as string,
                    LastName = reader["LastName"] as string,
                    CurrentCountryIsoCode = reader["CurrentCountryIsoCode"] as string,
                });
            });

            // Assert
            for (int i = 0; i < _customers.Count; i++)
            {
                Assert.Equal(_customers[i].IdNumber, customers[i].IdNumber);
                Assert.Equal(_customers[i].FirstName, customers[i].FirstName);
                Assert.Equal(_customers[i].LastName, customers[i].LastName);
                Assert.Equal(_customers[i].CurrentCountryIsoCode, customers[i].CurrentCountryIsoCode);
            }
        }

        [Fact]
        public void Non_Entity_Type_Multiple_Tables_With_Selected_Columns()
        {
            var result = new List<dynamic>();

            var customerTableName = _context.CreateTempTable(_customers,
                x => new
                {
                    x.IdNumber,
                    x.FirstName,
                    x.LastName,
                    x.CurrentCountryIsoCode
                },
                options =>
                {
                    options.LogTo = _output.WriteLine;
                });

            var contactTableName = _context.CreateTempTable(_contacts,
                            x => new
                            {
                                x.EmailAddress,
                                x.PhoneNumber,
                                x.CustomerIdNumber,
                                x.CountryIsoCode
                            },
                            options =>
                            {
                                options.LogTo = _output.WriteLine;
                            });

            string sql = $"select * from {contactTableName} contact join {customerTableName} customer on contact.CustomerIdNumber = customer.IdNumber";

            _context.ExecuteReader(sql, reader =>
            {
                result.Add(new
                {
                    IdNumber = reader["IdNumber"] as string,
                    FirstName = reader["FirstName"] as string,
                    LastName = reader["LastName"] as string,
                    CurrentCountryIsoCode = reader["CurrentCountryIsoCode"] as string,
                    EmailAddress = reader["EmailAddress"] as string,
                    PhoneNumber = reader["PhoneNumber"] as string
                });
            });

            // Assert
            Assert.Equal(3, result.Count);

            Assert.Equal(result[0].IdNumber, "00000001");
            Assert.Equal(result[0].FirstName, "Ti");
            Assert.Equal(result[0].LastName, "Nguyen");
            Assert.Equal(result[0].CurrentCountryIsoCode, "VI");
            Assert.Equal(result[0].EmailAddress, "ti@gmail.com");
            Assert.Equal(result[0].PhoneNumber, "+84123456789");

            Assert.Equal(result[1].IdNumber, "00000001");
            Assert.Equal(result[1].FirstName, "Ti");
            Assert.Equal(result[1].LastName, "Nguyen");
            Assert.Equal(result[1].CurrentCountryIsoCode, "VI");
            Assert.Equal(result[1].EmailAddress, "chuot@gmail.com");
            Assert.Equal(result[1].PhoneNumber, "+84123456790");

            Assert.Equal(result[2].IdNumber, "00000002");
            Assert.Equal(result[2].FirstName, "Suu");
            Assert.Equal(result[2].LastName, "Nguyen");
            Assert.Equal(result[2].CurrentCountryIsoCode, "VI");
            Assert.Equal(result[2].EmailAddress, "suu@gmail.com");
            Assert.Equal(result[2].PhoneNumber, "+84123456791");
        }

        [Fact]
        public void Non_Entity_Type_Single_Table_With_All_Columns()
        {
            // Arrange
            var customers = new List<CustomerDto>();

            // Act
            var tableName = _context.CreateTempTable(_customers,
                   options =>
                   {
                       options.LogTo = _output.WriteLine;
                   });

            string sql = $"select * from {tableName}";

            _context.ExecuteReader(sql, reader =>
            {
                customers.Add(new CustomerDto
                {
                    IdNumber = reader["IdNumber"] as string,
                    FirstName = reader["FirstName"] as string,
                    LastName = reader["LastName"] as string,
                    CurrentCountryIsoCode = reader["CurrentCountryIsoCode"] as string,
                });
            });

            // Assert
            for (int i = 0; i < _customers.Count; i++)
            {
                Assert.Equal(_customers[i].IdNumber, customers[i].IdNumber);
                Assert.Equal(_customers[i].FirstName, customers[i].FirstName);
                Assert.Equal(_customers[i].LastName, customers[i].LastName);
                Assert.Equal(_customers[i].CurrentCountryIsoCode, customers[i].CurrentCountryIsoCode);
            }
        }

        [Fact]
        public void Non_Entity_Type_Multiple_Tables_With_All_Columns()
        {
            // Arrange
            var result = new List<dynamic>();

            // Act
            var customerTableName = _context.CreateTempTable(_customers,
                   options =>
                   {
                       options.LogTo = _output.WriteLine;
                   });

            var contactTableName = _context.CreateTempTable(_contacts,
                   options =>
                   {
                       options.LogTo = _output.WriteLine;
                   });

            string sql = $"select * from {contactTableName} contact join {customerTableName} customer on contact.CustomerIdNumber = customer.IdNumber";

            _context.ExecuteReader(sql, reader =>
            {
                result.Add(new
                {
                    IdNumber = reader["IdNumber"] as string,
                    FirstName = reader["FirstName"] as string,
                    LastName = reader["LastName"] as string,
                    CurrentCountryIsoCode = reader["CurrentCountryIsoCode"] as string,
                    EmailAddress = reader["EmailAddress"] as string,
                    PhoneNumber = reader["PhoneNumber"] as string
                });
            });

            // Assert
            Assert.Equal(3, result.Count);

            Assert.Equal(result[0].IdNumber, "00000001");
            Assert.Equal(result[0].FirstName, "Ti");
            Assert.Equal(result[0].LastName, "Nguyen");
            Assert.Equal(result[0].CurrentCountryIsoCode, "VI");
            Assert.Equal(result[0].EmailAddress, "ti@gmail.com");
            Assert.Equal(result[0].PhoneNumber, "+84123456789");

            Assert.Equal(result[1].IdNumber, "00000001");
            Assert.Equal(result[1].FirstName, "Ti");
            Assert.Equal(result[1].LastName, "Nguyen");
            Assert.Equal(result[1].CurrentCountryIsoCode, "VI");
            Assert.Equal(result[1].EmailAddress, "chuot@gmail.com");
            Assert.Equal(result[1].PhoneNumber, "+84123456790");

            Assert.Equal(result[2].IdNumber, "00000002");
            Assert.Equal(result[2].FirstName, "Suu");
            Assert.Equal(result[2].LastName, "Nguyen");
            Assert.Equal(result[2].CurrentCountryIsoCode, "VI");
            Assert.Equal(result[2].EmailAddress, "suu@gmail.com");
            Assert.Equal(result[2].PhoneNumber, "+84123456791");
        }

        [Fact]
        public void Entity_Type_Multiple_Tables()
        {
            // Arange
            var customers = new List<Customer>(100);

            for (int i = 0; i < 100; i++)
            {
                var customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    FirstName = "FirstName " + i,
                    LastName = "LastName " + i,
                    Index = i,
                };

                customer.Contacts = new List<Contact>();

                for (int j = 0; j < 5; j++)
                {
                    customer.Contacts.Add(new Contact
                    {
                        CustomerId = customer.Id,
                        EmailAddress = $"EmailAddress {i} - {j}",
                        PhoneNumber = $"PhoneNumber {i} - {j}",
                        Index = j,
                    });
                }

                customers.Add(customer);
            }

            var contacts = customers.SelectMany(x => x.Contacts).ToList();

            // Act
            var customerTableName = _context.CreateTempTable(customers,
                   options =>
                   {
                       options.LogTo = _output.WriteLine;
                   });

            var contactTableName = _context.CreateTempTable(contacts,
                   options =>
                   {
                       options.LogTo = _output.WriteLine;
                   });

            var tempCustomers = _context.Customers.FromSqlRaw($"select * from {customerTableName}").Where(x => x.Index > 10 && x.Index < 20);

            var tempContacts = _context.Contacts.FromSqlRaw($"select * from {contactTableName}");

            var dbQuery = from customer in tempCustomers
                          join contact in tempContacts on customer.Id equals contact.CustomerId
                          orderby customer.Index, contact.Index
                          select new { customer.Id, customer.FirstName, customer.LastName, contact.EmailAddress, contact.PhoneNumber };

            var dataInDb = dbQuery.ToList();

            var inMemoryQuery = from customer in customers
                                join contact in contacts on customer.Id equals contact.CustomerId
                                where customer.Index > 10 && customer.Index < 20
                                orderby customer.Index, contact.Index
                                select new { customer.Id, customer.FirstName, customer.LastName, contact.EmailAddress, contact.PhoneNumber };

            var dataInMemory = inMemoryQuery.ToList();

            // Assert
            Assert.Equal(dataInMemory.Count, dataInDb.Count);

            for (var i = 0; i < dataInMemory.Count; i++)
            {
                Assert.Equal(dataInMemory[i].Id, dataInDb[i].Id);
                Assert.Equal(dataInMemory[i].FirstName, dataInDb[i].FirstName);
                Assert.Equal(dataInMemory[i].LastName, dataInDb[i].LastName);
                Assert.Equal(dataInMemory[i].EmailAddress, dataInDb[i].EmailAddress);
                Assert.Equal(dataInMemory[i].PhoneNumber, dataInDb[i].PhoneNumber);
            }
        }

        [Fact]
        public void Entity_Type_Single_Table_With_Mappings()
        {
            // Arange
            var configurationEntries = new List<ConfigurationEntry>();

            for (int i = 0; i < 100; i++)
            {
                configurationEntries.Add(new ConfigurationEntry
                {
                    Key = $"Key{i}",
                    Value = $"Value{i}",
                    Description = string.Empty,
                    CreatedDateTime = DateTimeOffset.Now,
                });
            }

            // Act
            var tableName = _context.CreateTempTable(configurationEntries,
                   options =>
                   {
                       options.LogTo = _output.WriteLine;
                   });

            var configurationEntriesDb = _context.Set<ConfigurationEntry>().FromSqlRaw($"select * from {tableName}").Select(x => new { x.Id, x.Key, x.Value }).ToList();

            // Assert
            Assert.Equal(configurationEntries.Count, configurationEntriesDb.Count);

            for (var i = 0; i < configurationEntries.Count; i++)
            {
                Assert.Equal(configurationEntries[i].Id, configurationEntriesDb[i].Id);
                Assert.Equal(configurationEntries[i].Key, configurationEntriesDb[i].Key);
                Assert.Equal(configurationEntries[i].Value, configurationEntriesDb[i].Value);
            }
        }

    }

    class CustomerDto
    {
        public string IdNumber { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string CurrentCountryIsoCode { get; set; }

        public override string ToString()
        {
            return $"{IdNumber}\t{FirstName}\t{LastName}";
        }
    }

    class ContactDto
    {
        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string CustomerIdNumber { get; set; }

        public string CountryIsoCode { get; set; }

        public override string ToString()
        {
            return $"{CustomerIdNumber}\t{EmailAddress}\t{PhoneNumber}";
        }
    }
}

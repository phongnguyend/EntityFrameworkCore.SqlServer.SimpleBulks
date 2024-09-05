using EntityFrameworkCore.SqlServer.SimpleBulks.Extensions;
using EntityFrameworkCore.SqlServer.SimpleBulks.TempTable;
using EntityFrameworkCore.SqlServer.SimpleBulks.Tests.Database;
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

            _context = new TestDbContext($"Server=127.0.0.1;Database=EFCoreSimpleBulksTests.TempTable.{Guid.NewGuid()};User Id=sa;Password=sqladmin123!@#");
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }

        [Fact]
        public void Single_Table_With_Selected_Columns()
        {
            var customers = new List<CustomerDto>();

            var tableName = _context.CreateTempTable(_customers,
                   x => new
                   {
                       x.IdNumber,
                       x.FirstName,
                       x.LastName,
                       x.CurrentCountryIsoCode
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
        public void Multiple_Tables_With_Selected_Columns()
        {
            var result = new List<dynamic>();

            var customerTableName = _context.CreateTempTable(_customers,
                 x => new
                 {
                     x.IdNumber,
                     x.FirstName,
                     x.LastName,
                     x.CurrentCountryIsoCode
                 });

            var contactTableName = _context.CreateTempTable(_contacts,
                          x => new
                          {
                              x.EmailAddress,
                              x.PhoneNumber,
                              x.CustomerIdNumber,
                              x.CountryIsoCode
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
        public void Single_Table_With_All_Columns()
        {
            var customers = new List<CustomerDto>();

            var tableName = _context.CreateTempTable(_customers);

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
        public void Multiple_Tables_With_All_Columns()
        {
            var result = new List<dynamic>();

            var customerTableName = _context.CreateTempTable(_customers);
            var contactTableName = _context.CreateTempTable(_contacts);

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

        //[Fact]
        //public void FromSqlRaw()
        //{
        //    //var connection = _context.GetSqlConnection();

        //    var customers = new List<Customer>(100);

        //    for (int i = 0; i < 100; i++)
        //    {
        //        var customer = new Customer
        //        {
        //            Id = Guid.NewGuid(),
        //            FirstName = "FirstName " + i,
        //            LastName = "LastName " + i,
        //            Index = i,
        //        };

        //        customer.Contacts = new List<Contact>();

        //        for (int j = 0; j < 5; j++)
        //        {
        //            customer.Contacts.Add(new Contact
        //            {
        //                CustomerId = customer.Id,
        //                EmailAddress = $"EmailAddress {i} - {j}",
        //                PhoneNumber = $"PhoneNumber {i} - {j}",
        //                IsDefault = j % 2 == 0,
        //                Index = j,
        //            });
        //        }

        //        customers.Add(customer);
        //    }

        //    var contacts = customers.SelectMany(x => x.Contacts).ToList();

        //    var result = new List<dynamic>();

        //    var customerTableName = _context.CreateTempTable(customers);
        //    var contactTableName = _context.CreateTempTable(contacts);

        //    var tempCustomers = _context.Customers.FromSqlRaw($"select * from {customerTableName}").Where(x => x.Index > 10).ToQueryString();

        //    var tempCustomers2 = _context.Customers.FromSqlRaw($"select * from {customerTableName}").Where(x => x.Index > 10).ToList();

        //    // Assert
        //    Assert.Equal(3, result.Count);

        //    Assert.Equal(result[0].IdNumber, "00000001");
        //    Assert.Equal(result[0].FirstName, "Ti");
        //    Assert.Equal(result[0].LastName, "Nguyen");
        //    Assert.Equal(result[0].CurrentCountryIsoCode, "VI");
        //    Assert.Equal(result[0].EmailAddress, "ti@gmail.com");
        //    Assert.Equal(result[0].PhoneNumber, "+84123456789");

        //    Assert.Equal(result[1].IdNumber, "00000001");
        //    Assert.Equal(result[1].FirstName, "Ti");
        //    Assert.Equal(result[1].LastName, "Nguyen");
        //    Assert.Equal(result[1].CurrentCountryIsoCode, "VI");
        //    Assert.Equal(result[1].EmailAddress, "chuot@gmail.com");
        //    Assert.Equal(result[1].PhoneNumber, "+84123456790");

        //    Assert.Equal(result[2].IdNumber, "00000002");
        //    Assert.Equal(result[2].FirstName, "Suu");
        //    Assert.Equal(result[2].LastName, "Nguyen");
        //    Assert.Equal(result[2].CurrentCountryIsoCode, "VI");
        //    Assert.Equal(result[2].EmailAddress, "suu@gmail.com");
        //    Assert.Equal(result[2].PhoneNumber, "+84123456791");
        //}

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

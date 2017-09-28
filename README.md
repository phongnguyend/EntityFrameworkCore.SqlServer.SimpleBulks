 ```c#
    class Program
    {
        static void Main(string[] args)
        {
            //InsertUsingEF();
            //UpdateUsingEF();
            InsertUsingBulkInsert();
            UpdateUsingBulkUpdate();
            Console.WriteLine("Finished!");
            Console.ReadLine();
        }

        private static void InsertUsingEF()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (var dbct = new DemoDbContext())
            {
                var rows = new List<Row>();
                for (int i = 0; i < 500000; i++)
                {
                    rows.Add(new Row
                    {
                        Column1 = i,
                        Column2 = "" + i,
                        Column3 = DateTime.Now
                    });
                }

                dbct.Rows.AddRange(rows);
                dbct.SaveChanges();
            }
            watch.Stop();

            var elapsedTime = watch.Elapsed;
            Console.WriteLine(elapsedTime);
        }

        private static void UpdateUsingEF()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (var dbct = new DemoDbContext())
            {
                var rows = dbct.Rows.ToList();

                foreach (var row in rows)
                {
                    row.Column2 = "abc";
                    row.Column3 = DateTime.Now;
                }

                dbct.SaveChanges();
            }
            watch.Stop();

            var elapsedTime = watch.Elapsed;
            Console.WriteLine(elapsedTime);
        }

        private static void InsertUsingBulkInsert()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (var dbct = new DemoDbContext())
            {
                var rows = new List<Row>();
                for (int i = 0; i < 500000; i++)
                {
                    rows.Add(new Row
                    {
                        Column1 = i,
                        Column2 = "" + i,
                        Column3 = DateTime.Now
                    });
                }

                dbct.BulkInsert(rows, "Rows", "Column1", "Column2", "Column3");
            }
            watch.Stop();

            var elapsedTime = watch.Elapsed;
            Console.WriteLine(elapsedTime);
        }

        private static void UpdateUsingBulkUpdate()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            using (var dbct = new DemoDbContext())
            {
                var rows = dbct.Rows.AsNoTracking().ToList();

                foreach (var row in rows)
                {
                    row.Column2 = "abc";
                    row.Column3 = DateTime.Now;
                }

                dbct.BulkUpdate(rows, "Rows", "Id", "Column3", "Column2");
            }
            watch.Stop();

            var elapsedTime = watch.Elapsed;
            Console.WriteLine(elapsedTime);
        }
    }
 ```

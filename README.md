# SimpleBulkOperations
This is a very simple .netcore library that inserts and updates a large number of records into database using the SqlBulkCopy class.
 
## SimpleBulkOperations supports:
* Bulk insert
* Bulk update

## Overview
This project provide 2 extension methods so that you can use it with your EntityFrameworkCore DbContext class. 

## License
Free to copy, modify and use for whatever you want in your applications.  

## Examples

 ```c#
class Program
{
	static void Main(string[] args)
	{
		//InsertUsingEF();
		//UpdateUsingEF();
		InsertUsingBulkInsert();
		UpdateUsingBulkUpdate();
		DeleteUsingBulkDelete();
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
			//dbct.BulkInsert(rows, "Rows", "Column1", "Column2", "Column3");
			dbct.BulkInsert(rows, "Rows", row => new { row.Column1, row.Column2, row.Column3 });
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

			//dbct.BulkUpdate(rows, "Rows", "Id", "Column3", "Column2");
			dbct.BulkUpdate(rows, "Rows", row => row.Id, row => new { row.Column3, row.Column2 });
		}
		watch.Stop();

		var elapsedTime = watch.Elapsed;
		Console.WriteLine(elapsedTime);
	}

	private static void DeleteUsingBulkDelete()
	{
		Stopwatch watch = new Stopwatch();
		watch.Start();
		using (var dbct = new DemoDbContext())
		{
			var rows = dbct.Rows.AsNoTracking().ToList();
			//dbct.BulkDelete(rows, "Rows", "Id");
			dbct.BulkDelete(rows, "Rows", row => row.Id);
		}
		watch.Stop();

		var elapsedTime = watch.Elapsed;
		Console.WriteLine(elapsedTime);
	}
}
 ```

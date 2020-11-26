
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkAurora; User ID=Sql.BhbkAurora; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context AuroraEntities --startup-project Bhbk.Cli.Aurora --project Bhbk.Lib.Aurora.Data --output-dir Models --use-database-names --schema "svc" --verbose --no-onconfiguring --force

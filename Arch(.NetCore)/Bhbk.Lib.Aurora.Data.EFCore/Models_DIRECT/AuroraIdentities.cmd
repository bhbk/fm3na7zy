﻿
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkAurora; User ID=Sql.BhbkAurora; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context AuroraEntities --startup-project Bhbk.Cli.Aurora --project Bhbk.Lib.Aurora.Data.EFCore --output-dir Models_DIRECT --use-database-names --table "dbo.tbl_Settings" --table "dbo.tbl_SystemKeys" --table "dbo.tbl_UserFiles" --table "dbo.tbl_UserFolders" --table "dbo.tbl_UserPasswords" --table "dbo.tbl_UserPrivateKeys" --table "dbo.tbl_UserPublicKeys" --table "dbo.tbl_Users" --verbose --force

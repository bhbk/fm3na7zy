﻿
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkAurora; User ID=Sql.BhbkAurora; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context AuroraEntities --startup-project Bhbk.Cli.Aurora --project Bhbk.Lib.Aurora.Data --output-dir Models_DIRECT --use-database-names --table "dbo.tbl_SysCredentials" --table "dbo.tbl_SysPrivateKeys" --table "dbo.tbl_SysSettings" --table "dbo.tbl_UserFiles" --table "dbo.tbl_UserFolders" --table "dbo.tbl_UserMounts" --table "dbo.tbl_UserPasswords" --table "dbo.tbl_UserPrivateKeys" --table "dbo.tbl_UserPublicKeys" --table "dbo.tbl_Users" --verbose --force

﻿
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkAurora; User ID=Sql.BhbkAurora; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context AuroraEntities --startup-project Bhbk.Cli.Aurora --project Bhbk.Lib.Aurora.Data --output-dir Models_DIRECT --use-database-names --table "[dbo].tbl_Activity" --table "[dbo].[tbl_Credential]" --table "[dbo].[tbl_Network]" --table "[dbo].[tbl_PrivateKey]" --table "[dbo].[tbl_PublicKey]" --table "[dbo].[tbl_Setting]" --table "[dbo].[tbl_User]" --table "[dbo].[tbl_UserFile]" --table "[dbo].[tbl_UserFolder]" --table "[dbo].[tbl_UserMount]" --verbose --no-onconfiguring --force

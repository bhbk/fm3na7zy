﻿
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkAurora; User ID=Sql.BhbkAurora; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context AuroraEntities --startup-project Bhbk.Cli.Aurora --project Bhbk.Lib.Aurora.Data --output-dir Models --use-database-names --table "svc.uvw_Credential" --table "svc.uvw_Network" --table "svc.uvw_PrivateKey" --table "svc.uvw_PublicKey" --table "svc.uvw_Setting" --table "svc.uvw_User" --table "svc.uvw_UserFile" --table "svc.uvw_UserFolder" --table "svc.uvw_UserMount" --verbose --force

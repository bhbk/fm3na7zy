﻿
CREATE VIEW [svc].[uvw_Usage]

AS

SELECT t1.[UserId]
	  ,t2.[UserName]
      ,t1.[QuotaInBytes]
      ,t1.[QuotaUsedInBytes]
      ,t1.[SessionMax]
      ,t1.[SessionsInUse]
  FROM [dbo].[tbl_Usage] AS t1
			LEFT JOIN [dbo].[tbl_Login] AS t2 ON t1.UserId = t2.UserId
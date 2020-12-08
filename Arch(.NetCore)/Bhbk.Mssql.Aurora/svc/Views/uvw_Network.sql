﻿
CREATE VIEW [svc].[uvw_Network]
AS
SELECT
	Id
	,UserId
	,SequenceId
	,Address
	,Action
	,IsEnabled
	,IsDeletable
	,CreatedUtc
	,LastUpdatedUtc

FROM
	[dbo].[tbl_Network]

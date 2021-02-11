CREATE VIEW [svc].[uvw_Session]
AS
SELECT t1.[Id],
	t1.[UserId],
	t2.[UserName],
	t1.[CallPath],
	t1.[Details],
	t1.[LocalEndPoint],
	t1.[LocalSoftwareIdentifier],
	t1.[RemoteEndPoint],
	t1.[RemoteSoftwareIdentifier],
	t1.[IsActive],
	t1.[CreatedUtc]
FROM [dbo].[tbl_Session] AS t1
LEFT JOIN [dbo].[tbl_Login] AS t2 ON t1.UserId = t2.UserId

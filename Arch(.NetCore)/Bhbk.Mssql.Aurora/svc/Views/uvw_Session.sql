
CREATE VIEW [svc].[uvw_Session]

AS

SELECT t1.[Id]
      ,t1.[IdentityId]
	  ,t2.IdentityAlias
      ,t1.[CallPath]
      ,t1.[Details]
      ,t1.[LocalEndPoint]
	  ,t1.[LocalSoftwareIdentifier]
      ,t1.[RemoteEndPoint]
      ,t1.[RemoteSoftwareIdentifier]
      ,t1.[IsActive]
      ,t1.[CreatedUtc]
  FROM [dbo].[tbl_Session] AS t1
			LEFT JOIN [dbo].[tbl_User] AS t2 ON t1.IdentityId = t2.IdentityId
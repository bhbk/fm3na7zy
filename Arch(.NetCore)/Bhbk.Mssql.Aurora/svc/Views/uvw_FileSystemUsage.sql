
CREATE VIEW [svc].[uvw_FileSystemUsage]

AS

SELECT t1.[FileSystemId]
	  ,t2.[Name] AS FileSystemName
      ,t1.[QuotaInBytes]
      ,t1.[QuotaUsedInBytes]
  FROM [dbo].[tbl_FileSystemUsage] AS t1
			LEFT JOIN [dbo].[tbl_FileSystem] AS t2 ON t1.FileSystemId = t2.Id

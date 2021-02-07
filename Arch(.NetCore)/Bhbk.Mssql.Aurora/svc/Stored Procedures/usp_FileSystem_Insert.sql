
CREATE PROCEDURE [svc].[usp_FileSystem_Insert]
    @FileSystemTypeId int
   ,@Name nvarchar(128)
   ,@Description nvarchar(256)
   ,@UncPath nvarchar(256)
   ,@IsEnabled bit
   ,@IsDeletable bit

AS

SET NOCOUNT ON;

DECLARE @Id uniqueidentifier = NEWID()
DECLARE @CreatedUtc datetimeoffset(7) = GETUTCDATE()

INSERT INTO [dbo].[tbl_FileSystem] 
	(
	 Id
	,FileSystemTypeId
	,Name
	,Description
	,UncPath
	,CreatedUtc
	,IsEnabled
	,IsDeletable
	)
VALUES 
	(
	 @Id
	,@FileSystemTypeId
    ,@Name
    ,@Description
	,@UncPath
    ,@CreatedUtc
    ,@IsEnabled
    ,@IsDeletable
    );

	INSERT INTO [dbo].[tbl_FileSystemUsage]
		(
		 FileSystemId
		,QuotaInBytes
		,QuotaUsedInBytes
		)
	VALUES
		(
		 @Id
		,1073741824
		,0
		);

    /*  Select all entity values to return
        ----------------------------------------------------
       */

SELECT * 
FROM [dbo].[tbl_FileSystem] 
WHERE Id = @Id

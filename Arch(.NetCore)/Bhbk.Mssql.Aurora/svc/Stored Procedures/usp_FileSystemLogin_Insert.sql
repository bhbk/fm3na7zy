
CREATE PROCEDURE [svc].[usp_FileSystemLogin_Insert]
	@FileSystemId uniqueidentifier
   ,@UserId uniqueidentifier
   ,@SmbAuthTypeId int
   ,@AmbassadorId uniqueidentifier
   ,@ChrootPath nvarchar(256)
   ,@IsReadOnly bit

AS

SET NOCOUNT ON;

DECLARE @CreatedUtc datetimeoffset (7) = GETUTCDATE()

INSERT INTO [dbo].[tbl_FileSystemLogin] 
	(FileSystemId
	,UserId
    ,SmbAuthTypeId
    ,AmbassadorId
    ,ChrootPath
    ,CreatedUtc
    ,IsReadOnly
	)
VALUES 
	(@FileSystemId
    ,@UserId
    ,@SmbAuthTypeId
    ,@AmbassadorId
    ,@ChrootPath
    ,@CreatedUtc
    ,@IsReadOnly
    );

    /*  Select all entity values to return
        ----------------------------------------------------
       */

SELECT * 
FROM [dbo].[tbl_FileSystemLogin] 
WHERE FileSystemId = @FileSystemId
    AND UserId = @UserId


CREATE PROCEDURE [svc].[usp_FileSystem_Update]
	@Id uniqueidentifier
   ,@FileSystemTypeId int
   ,@Name nvarchar(128)
   ,@Description nvarchar(256)
   ,@UncPath nvarchar(256)
   ,@IsEnabled bit
   ,@IsDeletable bit

AS

SET NOCOUNT ON;


UPDATE [dbo].[tbl_FileSystem] 
SET FileSystemTypeId = @FileSystemTypeId
   ,Name = @Name
   ,Description = @Description
   ,UncPath = @UncPath
   ,IsEnabled = @IsEnabled
   ,IsDeletable = @IsDeletable
WHERE Id = @Id;

    /*  Select all entity values to return
        ----------------------------------------------------
       */

SELECT * 
FROM [dbo].[tbl_FileSystem] 
WHERE Id = @Id

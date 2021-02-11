CREATE PROCEDURE [svc].[usp_FileSystem_Update] @Id UNIQUEIDENTIFIER,
	@FileSystemTypeId INT,
	@Name NVARCHAR(128),
	@Description NVARCHAR(256),
	@UncPath NVARCHAR(256),
	@IsEnabled BIT,
	@IsDeletable BIT
AS
SET NOCOUNT ON;

UPDATE [dbo].[tbl_FileSystem]
SET FileSystemTypeId = @FileSystemTypeId,
	Name = @Name,
	Description = @Description,
	UncPath = @UncPath,
	IsEnabled = @IsEnabled,
	IsDeletable = @IsDeletable
WHERE Id = @Id;

/*  Select all entity values to return
        ----------------------------------------------------
       */
SELECT *
FROM [dbo].[tbl_FileSystem]
WHERE Id = @Id

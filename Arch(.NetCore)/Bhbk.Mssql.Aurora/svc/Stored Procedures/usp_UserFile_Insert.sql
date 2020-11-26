





CREATE PROCEDURE [svc].[usp_UserFile_Insert]
     @IdentityId			UNIQUEIDENTIFIER
    ,@FolderId				UNIQUEIDENTIFIER
    ,@VirtualName			NVARCHAR (260) 
    ,@IsReadOnly			BIT
    ,@RealPath				NVARCHAR (MAX) 
    ,@RealFileName			NVARCHAR (260) 
	,@RealFileSize			BIGINT
    ,@HashSHA256			NVARCHAR (64) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @FILEID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_UserFile]
			(
			 Id         
			,IdentityId
			,FolderId
			,VirtualName  
			,IsReadOnly
			,RealPath
			,RealFileName
			,RealFileSize
			,HashSHA256
			,CreatedUtc
			,LastVerifiedUtc
			)
		VALUES
			(
			 @FILEID          
			,@IdentityId
			,@FolderId
			,@VirtualName
			,@IsReadOnly
			,@RealPath
			,@RealFileName
			,@RealFileSize
			,@HashSHA256
			,@CREATEDUTC
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_UserFile] WHERE Id = @FILEID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
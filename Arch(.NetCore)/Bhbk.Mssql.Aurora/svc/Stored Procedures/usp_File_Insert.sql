
CREATE PROCEDURE [svc].[usp_File_Insert]
     @UserId				UNIQUEIDENTIFIER
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

    	BEGIN TRANSACTION;

		DECLARE @FILEID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_File]
			(
			 Id         
			,UserId
			,FolderId
			,VirtualName  
			,IsReadOnly
			,RealPath
			,RealFileName
			,RealFileSize
			,HashSHA256
			,CreatedUtc
			,LastAccessedUtc
			,LastUpdatedUtc
			,LastVerifiedUtc
			)
		VALUES
			(
			 @FILEID          
			,@UserId
			,@FolderId
			,@VirtualName
			,@IsReadOnly
			,@RealPath
			,@RealFileName
			,@RealFileSize
			,@HashSHA256
			,@CREATEDUTC
			,@CREATEDUTC
			,@CREATEDUTC
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_File] 
			WHERE Id = @FILEID

    	COMMIT TRANSACTION;

    END TRY

    BEGIN CATCH

    	ROLLBACK TRANSACTION;
        THROW;

    END CATCH

END

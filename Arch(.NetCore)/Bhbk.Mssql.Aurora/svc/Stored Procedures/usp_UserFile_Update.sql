
CREATE PROCEDURE [svc].[usp_UserFile_Update]
	 @Id					UNIQUEIDENTIFIER
    ,@FolderId				UNIQUEIDENTIFIER
	,@IdentityId			UNIQUEIDENTIFIER
    ,@VirtualName			NVARCHAR (260) 
    ,@IsReadOnly			BIT
    ,@RealPath				NVARCHAR (MAX) 
    ,@RealFileName			NVARCHAR (260) 
	,@RealFileSize			BIGINT
    ,@HashSHA256			NVARCHAR (64) 
	,@LastAccessedUtc		DATETIMEOFFSET (7)
	,@LastUpdatedUtc		DATETIMEOFFSET (7)
	,@LastVerifiedUtc		DATETIMEOFFSET (7)

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        UPDATE [dbo].[tbl_UserFile]
        SET
			 VirtualName			= @VirtualName
			,IsReadOnly				= @IsReadOnly
			,RealPath				= @RealPath
			,RealFileName			= @RealFileName
			,RealFileSize			= @RealFileSize
			,HashSHA256				= @HashSHA256
			,LastAccessedUtc		= @LastAccessedUtc
            ,LastUpdatedUtc			= @LastUpdatedUtc
			,LastVerifiedUtc		= @LastVerifiedUtc
        WHERE Id = @Id AND FolderId = @FolderId AND IdentityId = @IdentityId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_UserFile] 
			WHERE Id = @Id AND FolderId = @FolderId AND IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
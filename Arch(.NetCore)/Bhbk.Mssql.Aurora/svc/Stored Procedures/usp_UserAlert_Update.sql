
CREATE PROCEDURE [svc].[usp_UserAlert_Update]
	 @Id					UNIQUEIDENTIFIER
	,@IdentityId			UNIQUEIDENTIFIER
    ,@OnDelete				BIT
    ,@OnDownload			BIT
    ,@OnUpload				BIT
    ,@ToDisplayName			NVARCHAR (256) 
    ,@ToEmailAddress		NVARCHAR (320) 
    ,@ToPhoneNumber			NVARCHAR (15) 
    ,@IsEnabled				BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_UserAlert]
        SET
			 OnDelete				= @OnDelete
			,OnDownload				= @OnDownload
			,OnUpload				= @OnUpload
			,ToDisplayName			= @ToDisplayName
			,ToEmailAddress			= @ToEmailAddress
			,ToPhoneNumber			= @ToPhoneNumber
			,IsEnabled				= @IsEnabled
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id AND IdentityId = @IdentityId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_UserAlert]
			WHERE Id = @Id AND IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE PROCEDURE [svc].[usp_User_Update]
     @IdentityId			UNIQUEIDENTIFIER
    ,@IdentityAlias			NVARCHAR (128) 
    ,@RequirePassword		BIT
    ,@RequirePublicKey		BIT
    ,@FileSystemType		NVARCHAR (16) 
    ,@FileSystemReadOnly	BIT
	,@QuotaInBytes			BIGINT
	,@QuotaUsedInBytes		BIGINT
    ,@Debugger				NVARCHAR (16) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_User]
        SET
			 IdentityAlias			= @IdentityAlias
			,RequirePassword		= @RequirePassword
			,RequirePublicKey		= @RequirePublicKey
			,FileSystemType			= @FileSystemType
			,FileSystemReadOnly		= @FileSystemReadOnly
			,QuotaInBytes			= @QuotaInBytes
			,QuotaUsedInBytes		= @QuotaUsedInBytes
			,Debugger				= @Debugger
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE IdentityId = @IdentityId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_User] WHERE IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
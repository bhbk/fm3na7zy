﻿
CREATE PROCEDURE [svc].[usp_UserLogin_Update]
     @IdentityId			UNIQUEIDENTIFIER
    ,@IdentityAlias			NVARCHAR (128) 
    ,@FileSystemType		NVARCHAR (16) 
	,@FileSystemChrootPath	NVARCHAR (64)
    ,@IsPasswordRequired	BIT
    ,@IsPublicKeyRequired	BIT
    ,@IsFileSystemReadOnly	BIT
	,@QuotaInBytes			BIGINT
	,@QuotaUsedInBytes		BIGINT
    ,@SessionMax            SMALLINT
    ,@SessionsInUse         SMALLINT
    ,@Debugger				NVARCHAR (16) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_UserLogin]
        SET
			 IdentityAlias			= @IdentityAlias
			,FileSystemType			= @FileSystemType
			,FileSystemChrootPath	= @FileSystemChrootPath
			,IsPasswordRequired		= @IsPasswordRequired
			,IsPublicKeyRequired	= @IsPublicKeyRequired
			,IsFileSystemReadOnly	= @IsFileSystemReadOnly
			,QuotaInBytes			= @QuotaInBytes
			,QuotaUsedInBytes		= @QuotaUsedInBytes
            ,SessionMax             = @SessionMax
            ,SessionsInUse          = @SessionsInUse
			,Debugger				= @Debugger
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE IdentityId = @IdentityId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_UserLogin] WHERE IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
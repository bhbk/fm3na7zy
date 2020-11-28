﻿


CREATE PROCEDURE [svc].[usp_PrivateKey_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@IdentityId			UNIQUEIDENTIFIER
    ,@PublicKeyId			UNIQUEIDENTIFIER
    ,@KeyValue				NVARCHAR (MAX) 
    ,@KeyAlgo				NVARCHAR (16) 
    ,@KeyPass				NVARCHAR (1024) 
    ,@KeyFormat				NVARCHAR (16) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        DECLARE @LASTUPDATED DATETIMEOFFSET (7) = GETUTCDATE()

        UPDATE [dbo].[tbl_PrivateKey]
        SET
			 PublicKeyId			= @PublicKeyId
			,KeyValue				= @KeyValue
			,KeyAlgo				= @KeyAlgo
			,KeyPass				= @KeyPass
			,KeyFormat				= @KeyFormat
			,IsEnabled				= @IsEnabled
			,IsDeletable			= @IsDeletable
            ,LastUpdatedUtc			= @LASTUPDATED
        WHERE Id = @Id AND IdentityId = @IdentityId

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_PrivateKey]
			WHERE Id = @Id AND IdentityId = @IdentityId

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END

CREATE PROCEDURE [svc].[usp_UserAlert_Insert]
     @IdentityId			UNIQUEIDENTIFIER
    ,@OnDelete				BIT
    ,@OnDownload			BIT
    ,@OnUpload				BIT
    ,@ToFirstName			NVARCHAR (128) 
    ,@ToLastName			NVARCHAR (128) 
    ,@ToEmailAddress		NVARCHAR (320) 
    ,@ToPhoneNumber			NVARCHAR (15) 
    ,@IsEnabled				BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @ALERTID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_UserAlert]
			(
			 Id         
			,IdentityId
			,OnDelete
			,OnDownload  
			,OnUpload
			,ToFirstName
			,ToLastName
			,ToEmailAddress
			,ToPhoneNumber
			,IsEnabled
			,CreatedUtc
			)
		VALUES
			(
			 @ALERTID          
			,@IdentityId
			,@OnDelete
			,@OnDownload
			,@OnUpload
			,@ToFirstName
			,@ToLastName
			,@ToEmailAddress
			,@ToPhoneNumber
			,@IsEnabled
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_UserAlert] WHERE Id = @ALERTID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
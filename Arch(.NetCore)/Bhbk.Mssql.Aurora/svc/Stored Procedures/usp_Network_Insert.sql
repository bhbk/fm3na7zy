
CREATE PROCEDURE [svc].[usp_Network_Insert]
     @IdentityId			UNIQUEIDENTIFIER
    ,@Address				NVARCHAR (128) 
    ,@Action				NVARCHAR (8) 
    ,@IsEnabled				BIT
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @NETWORKID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Network]
			(
			 Id         
			,IdentityId
			,Address   
			,Action
			,IsEnabled
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			 @NETWORKID          
			,@IdentityId
			,@Address
			,@Action
			,@IsEnabled
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_Network] WHERE Id = @NETWORKID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
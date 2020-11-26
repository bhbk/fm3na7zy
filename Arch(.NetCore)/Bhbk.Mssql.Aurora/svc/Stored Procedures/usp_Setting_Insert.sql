

CREATE PROCEDURE [svc].[usp_Setting_Insert]
     @IdentityId			UNIQUEIDENTIFIER
    ,@ConfigKey				NVARCHAR (MAX) 
    ,@ConfigValue			NVARCHAR (MAX) 
    ,@IsDeletable			BIT

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

		DECLARE @SETTINGID UNIQUEIDENTIFIER = NEWID()
        DECLARE @CREATEDUTC DATETIMEOFFSET (7) = GETUTCDATE()

		INSERT INTO [dbo].[tbl_Setting]
			(
			 Id         
			,IdentityId           
			,ConfigKey   
			,ConfigValue
			,IsDeletable
			,CreatedUtc
			)
		VALUES
			(
			 @SETTINGID          
			,@IdentityId         
			,@ConfigKey
			,@ConfigValue
			,@IsDeletable
			,@CREATEDUTC
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_Setting] WHERE Id = @SETTINGID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
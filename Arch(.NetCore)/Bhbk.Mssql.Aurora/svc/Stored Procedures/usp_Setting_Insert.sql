

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

		SELECT * FROM [svc].[uvw_Setting] WHERE [svc].[uvw_Setting].Id = @SETTINGID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END
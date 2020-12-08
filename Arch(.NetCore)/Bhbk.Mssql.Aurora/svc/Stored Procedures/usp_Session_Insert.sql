
CREATE PROCEDURE [svc].[usp_Session_Insert]
    @UserId						UNIQUEIDENTIFIER
   ,@CallPath					VARCHAR(256)
   ,@Details					VARCHAR(MAX)
   ,@LocalEndPoint				VARCHAR(128)
   ,@LocalSoftwareIdentifier	VARCHAR(128)
   ,@RemoteEndPoint				VARCHAR(128)
   ,@RemoteSoftwareIdentifier	VARCHAR(128)
   ,@IsActive					BIT

AS

SET NOCOUNT ON;

DECLARE @STATEID UNIQUEIDENTIFIER = NEWID()
DECLARE @CREATED DATETIMEOFFSET = GETUTCDATE()

INSERT INTO [dbo].[tbl_Session] 
	(Id
	,UserId
	,CallPath
	,Details
	,LocalEndPoint
	,LocalSoftwareIdentifier
	,RemoteEndPoint
	,RemoteSoftwareIdentifier
	,IsActive
	,CreatedUtc
	)
VALUES 
	(@STATEID
    ,@UserId
    ,@CallPath
    ,@Details
    ,@LocalEndPoint
	,@LocalSoftwareIdentifier
    ,@RemoteEndPoint
	,@RemoteSoftwareIdentifier
	,@IsActive
	,@CREATED
    );

    /*  Select all entity values to return
        ----------------------------------------------------
       */
	SELECT * FROM [dbo].[tbl_Session] WHERE Id = @STATEID
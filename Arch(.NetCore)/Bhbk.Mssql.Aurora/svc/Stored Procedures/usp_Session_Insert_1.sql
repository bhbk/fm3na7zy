
CREATE PROCEDURE [svc].[usp_Session_Insert]
    @IdentityId					uniqueidentifier
   ,@CallPath					varchar(256)
   ,@Details					varchar(MAX)
   ,@LocalEndPoint				varchar(128)
   ,@LocalSoftwareIdentifier	varchar(128)
   ,@RemoteEndPoint				varchar(128)
   ,@RemoteSoftwareIdentifier	varchar(128)

AS

SET NOCOUNT ON;

DECLARE @STATEID UNIQUEIDENTIFIER = NEWID()
DECLARE @CREATED DATETIMEOFFSET = GETUTCDATE()

INSERT INTO [dbo].[tbl_Session] 
	(Id
	,IdentityId
	,CallPath
	,Details
	,LocalEndPoint
	,LocalSoftwareIdentifier
	,RemoteEndPoint
	,RemoteSoftwareIdentifier
	,CreatedUtc
	)
VALUES 
	(@STATEID
    ,@IdentityId
    ,@CallPath
    ,@Details
    ,@LocalEndPoint
	,@LocalSoftwareIdentifier
    ,@RemoteEndPoint
	,@RemoteSoftwareIdentifier
	,@CREATED
    );

    /*  Select all entity values to return
        ----------------------------------------------------
       */
	SELECT * FROM [dbo].[tbl_Session] WHERE Id = @STATEID
/*  
Remove ChatHub Tables
*/

DROP TABLE [dbo].[ChatHubCam]
GO

DROP TABLE [dbo].[ChatHubSetting]
GO

DROP TABLE [dbo].[ChatHubIgnore]
GO

DROP TABLE [dbo].[ChatHubPhoto]
GO

DROP TABLE [dbo].[ChatHubMessage]
GO

DROP TABLE [dbo].[ChatHubConnection]
GO

DROP TABLE [dbo].[ChatHubRoomChatHubModerator]
GO

DROP TABLE [dbo].[ChatHubModerator]
GO

DROP TABLE [dbo].[ChatHubRoomChatHubWhitelistUser]
GO

DROP TABLE [dbo].[ChatHubWhitelistUser]
GO

DROP TABLE [dbo].[ChatHubRoomChatHubBlacklistUser]
GO

DROP TABLE [dbo].[ChatHubBlacklistUser]
GO

DROP TABLE [dbo].[ChatHubRoomChatHubUser]
GO

DROP TABLE [dbo].[ChatHubRoom]
GO

IF COL_LENGTH('dbo.User', 'UserType') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[User] DROP COLUMN [UserType]
END

GO

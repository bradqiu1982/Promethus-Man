USE [NPITrace]
GO
/****** Object:  Table [dbo].[n_User]    Script Date: 6/12/2018 8:57:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_User](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[Tel] [nvarchar](50) NOT NULL,
	[JobNum] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_User] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[n_UserGroup]    Script Date: 6/12/2018 8:57:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_UserGroup](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[GroupID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_UserGroup] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[n_UserPermissionRequest]    Script Date: 6/12/2018 8:57:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_UserPermissionRequest](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[MenuID] [int] NOT NULL,
	[FunctionID] [int] NOT NULL,
	[OperationID] [int] NOT NULL,
	[Comment] [nvarchar](max) NOT NULL,
	[Status] [tinyint] NOT NULL,
	[Approver] [int] NOT NULL,
	[Operator] [int] NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_UserPermissionRequest] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
ALTER TABLE [dbo].[n_User] ADD  CONSTRAINT [DF_n_User_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[n_UserGroup] ADD  CONSTRAINT [DF_n_UserGroup_Status]  DEFAULT ((1)) FOR [Status]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid 2 not verified' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_User', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_UserGroup', @level2type=N'COLUMN',@level2name=N'Status'
GO

/****** Object:  Table [dbo].[n_Function]    Script Date: 6/12/2018 8:55:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_Function](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[MenuID] [int] NOT NULL,
	[Url] [nvarchar](200) NOT NULL,
	[ImgUrl] [nvarchar](200) NOT NULL,
	[Status] [int] NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_Function] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[n_Group]    Script Date: 6/12/2018 8:55:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_Group](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Status] [int] NOT NULL,
	[Comment] [nvarchar](200) NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_Group] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[n_GroupRole]    Script Date: 6/12/2018 8:55:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_GroupRole](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GroupID] [int] NOT NULL,
	[RoleID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[Comment] [nvarchar](200) NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_GroupRole] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[n_Menu]    Script Date: 6/12/2018 8:55:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_Menu](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Url] [nvarchar](200) NOT NULL,
	[ImgUrl] [nvarchar](200) NOT NULL,
	[ParentID] [int] NOT NULL,
	[OrderID] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_Menu] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[n_Operation]    Script Date: 6/12/2018 8:55:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_Operation](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Status] [int] NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_Operation] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[n_Role]    Script Date: 6/12/2018 8:55:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_Role](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Status] [int] NOT NULL,
	[Comment] [nvarchar](200) NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_Role] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[n_RolePermission]    Script Date: 6/12/2018 8:55:58 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[n_RolePermission](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoleID] [int] NOT NULL,
	[MenuID] [int] NOT NULL,
	[FunctionID] [int] NOT NULL,
	[OperationID] [int] NOT NULL,
	[CreateAt] [datetime] NOT NULL,
	[UpdateAt] [datetime] NOT NULL
) ON [PRIMARY]

GO
SET IDENTITY_INSERT [dbo].[n_Function] ON 

INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (1, N'Yield', 1, N'/Project/ProjectYieldMain', N'/Content/images/PJ/Yield.png', 1, CAST(N'2018-06-05T11:38:29.000' AS DateTime), CAST(N'2018-06-06T11:02:13.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (2, N'FA', 1, N'/Project/ProjectFA', N'/Content/images/PJ/FA.png', 1, CAST(N'2018-06-06T11:40:36.000' AS DateTime), CAST(N'2018-06-06T11:40:36.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (3, N'RMA', 1, N'/Project/ProjectRMAStatus', N'/Content/images/PJ/RMA.png', 1, CAST(N'2018-06-06T11:41:05.000' AS DateTime), CAST(N'2018-06-06T11:41:05.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (4, N'Task', 1, N'/Project/ProjectIssues', N'/Content/images/PJ/Task.png', 1, CAST(N'2018-06-06T11:41:34.000' AS DateTime), CAST(N'2018-06-06T11:41:34.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (5, N'DebugTree', 1, N'/Project/ProjectError', N'/Content/images/PJ/DebugTreeicon.png', 1, CAST(N'2018-06-06T11:42:00.000' AS DateTime), CAST(N'2018-06-06T11:42:00.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (6, N'BurnIn', 1, N'/BurnIn/BurnInMainPage', N'/Content/images/PJ/BI.png', 1, CAST(N'2018-06-06T11:42:25.000' AS DateTime), CAST(N'2018-06-06T11:42:25.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (7, N'Critical Task', 1, N'/Project/ProjectSptTask', N'/Content/images/PJ/Spt.png', 1, CAST(N'2018-06-06T11:42:49.000' AS DateTime), CAST(N'2018-06-06T11:42:49.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (8, N'NPI Process', 1, N'/Project/ProjectNPI', N'/Content/images/PJ/NPIProcess.png', 1, CAST(N'2018-06-06T11:43:08.000' AS DateTime), CAST(N'2018-06-06T11:43:08.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (9, N'Project Stations', 1, N'/Project/ProjectStations', N'/Content/images/PJ/station.png', 1, CAST(N'2018-06-06T11:43:27.000' AS DateTime), CAST(N'2018-06-06T13:10:58.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (10, N'Project Reliability', 1, N'/Project/ProjectReliability', N'/Content/images/PJ/REL.png', 1, CAST(N'2018-06-06T11:44:20.000' AS DateTime), CAST(N'2018-06-06T11:44:20.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (11, N'Project Manage', 1, N'/Project/ProjectDash', N'/Content/images/PJ/PM.png', 1, CAST(N'2018-06-06T11:45:01.000' AS DateTime), CAST(N'2018-06-06T11:45:01.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (12, N'Project MileStone', 1, N'/Project/ProjectMileStone', N'/Content/images/PJ/milestone.png', 1, CAST(N'2018-06-06T11:45:23.000' AS DateTime), CAST(N'2018-06-06T11:45:23.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (13, N'I Task', 20, N'/User/Assign2Me', N'/Content/images/usercenter/Assign2Me.png', 1, CAST(N'2018-06-06T11:47:20.000' AS DateTime), CAST(N'2018-06-06T11:47:20.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (14, N'I Assign', 20, N'/User/IAssigned', N'/Content/images/usercenter/IAssigned.png', 1, CAST(N'2018-06-06T11:47:43.000' AS DateTime), CAST(N'2018-06-06T13:11:20.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (15, N'I Project', 20, N'/Project/ViewAll', N'/Content/images/usercenter/IProject.png', 1, CAST(N'2018-06-06T11:48:08.000' AS DateTime), CAST(N'2018-06-06T11:48:08.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (16, N'I Blog', 20, N'/User/IBLOG', N'/Content/images/usercenter/IBlog.png', 1, CAST(N'2018-06-06T11:48:50.000' AS DateTime), CAST(N'2018-06-06T11:48:50.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (17, N'I Learn', 20, N'/User/ILearn', N'/Content/images/usercenter/ILearn.png', 1, CAST(N'2018-06-06T11:49:13.000' AS DateTime), CAST(N'2018-06-06T11:49:13.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (18, N'I Tag', 20, N'/User/ITag', N'/Content/images/usercenter/ITag.png', 1, CAST(N'2018-06-06T11:52:49.000' AS DateTime), CAST(N'2018-06-06T11:52:49.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (19, N'I KPI', 20, N'/User/IKPI', N'/Content/images/usercenter/IKPI.png', 1, CAST(N'2018-06-06T11:53:11.000' AS DateTime), CAST(N'2018-06-06T11:53:11.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (20, N'Create Project', 2, N'/Project/CreateProject', N'', 1, CAST(N'2018-06-06T13:20:55.000' AS DateTime), CAST(N'2018-06-06T13:20:55.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (21, N'Create Task', 5, N'/Issue/CreateIssue', N'', 1, CAST(N'2018-06-06T13:21:23.000' AS DateTime), CAST(N'2018-06-06T13:21:23.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (22, N'Create RMA Task', 6, N'/Issue/CreateRMA', N'', 1, CAST(N'2018-06-06T13:21:45.000' AS DateTime), CAST(N'2018-06-06T13:21:45.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (23, N'Create FA', 7, N'/CustomerData/CommitFAData', N'', 1, CAST(N'2018-06-06T13:22:03.000' AS DateTime), CAST(N'2018-06-06T13:22:03.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (24, N'Search Task', 8, N'/Issue/SearchIssue', N'', 1, CAST(N'2018-06-06T13:22:30.000' AS DateTime), CAST(N'2018-06-06T13:22:30.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (25, N'Submit Vcsel PN Info', 10, N'/CustomerData/CommitVcselPNInfo', N'', 1, CAST(N'2018-06-06T13:22:51.000' AS DateTime), CAST(N'2018-06-06T13:22:51.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (26, N'Submit Vcsel Data', 11, N'/CustomerData/CommitVcselData', N'', 1, CAST(N'2018-06-06T13:23:17.000' AS DateTime), CAST(N'2018-06-06T13:23:17.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (27, N'Review Vcsel Data', 12, N'/CustomerData/CommitVcselData', N'', 1, CAST(N'2018-06-06T13:23:31.000' AS DateTime), CAST(N'2018-06-06T13:24:23.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (28, N'Review RMA Data', 13, N'/CustomerData/ReviewRMABackupData', N'', 1, CAST(N'2018-06-06T13:23:48.000' AS DateTime), CAST(N'2018-06-06T13:23:48.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (29, N'Review REL Data', 14, N'/CustomerData/ReviewRelBackupData', N'', 1, CAST(N'2018-06-06T13:25:07.000' AS DateTime), CAST(N'2018-06-06T13:25:07.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (30, N'Review IQE Data', 15, N'/CustomerData/ReviewIQEBackupData', N'', 1, CAST(N'2018-06-06T13:25:29.000' AS DateTime), CAST(N'2018-06-06T13:25:29.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (31, N'Review Wafer Info', 16, N'/CustomerData/ReviewWaferInfo', N'', 1, CAST(N'2018-06-06T13:25:44.000' AS DateTime), CAST(N'2018-06-06T13:25:44.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (32, N'Review Wafer Coord Data', 17, N'/CustomerData/ReviewWaferCoordData', N'', 1, CAST(N'2018-06-06T13:26:02.000' AS DateTime), CAST(N'2018-06-06T13:26:02.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (33, N'Debug Tree', 18, N'/DashBoard/DebugTree', N'', 1, CAST(N'2018-06-06T13:26:22.000' AS DateTime), CAST(N'2018-06-06T13:26:22.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (34, N'DownLoad ATE', 19, N'/CustomerData/DownLoadATETestData', N'', 1, CAST(N'2018-06-06T13:26:44.000' AS DateTime), CAST(N'2018-06-06T13:26:44.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (35, N'User Center', 21, N'/', N'', 1, CAST(N'2018-06-06T13:27:13.000' AS DateTime), CAST(N'2018-06-06T13:40:58.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (36, N'Add Project Critical Error', 22, N'/User/AddPJCriticalError', N'', 1, CAST(N'2018-06-06T13:27:50.000' AS DateTime), CAST(N'2018-06-06T13:27:50.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (37, N'I Status', 23, N'/User/MyStatus', N'', 1, CAST(N'2018-06-06T13:28:11.000' AS DateTime), CAST(N'2018-06-06T13:28:11.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (38, N'I Focus', 24, N'/User/ICare', N'', 1, CAST(N'2018-06-06T13:29:02.000' AS DateTime), CAST(N'2018-06-06T13:29:02.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (39, N'UserMatrix', 25, N'/CustomerData/CommitUserMatrix', N'', 1, CAST(N'2018-06-06T13:29:20.000' AS DateTime), CAST(N'2018-06-06T13:29:20.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (40, N'Add Share Tag', 26, N'/User/AddShareTag', N'', 1, CAST(N'2018-06-06T13:29:36.000' AS DateTime), CAST(N'2018-06-06T13:29:36.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (41, N'I Group', 27, N'/User/IGroup', N'', 1, CAST(N'2018-06-06T13:29:53.000' AS DateTime), CAST(N'2018-06-06T13:29:53.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (42, N'I Admire', 28, N'/User/IAdmire', N'', 1, CAST(N'2018-06-06T13:30:09.000' AS DateTime), CAST(N'2018-06-06T13:30:09.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (43, N'User Reviewed Info', 29, N'/User/UserReviewedInfo', N'', 1, CAST(N'2018-06-06T13:30:23.000' AS DateTime), CAST(N'2018-06-06T13:30:23.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (44, N'User Shared Info', 30, N'/User/UserSharedInfo', N'', 1, CAST(N'2018-06-06T13:30:38.000' AS DateTime), CAST(N'2018-06-06T13:30:38.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (45, N'KPI Summary', 31, N'/User/GetKPI', N'', 1, CAST(N'2018-06-06T13:30:50.000' AS DateTime), CAST(N'2018-06-06T13:30:50.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (46, N'VCSEL Monthly Yield', 33, N'/DataAnalyze/MonthlyVcsel', N'', 1, CAST(N'2018-06-06T13:31:32.000' AS DateTime), CAST(N'2018-06-06T13:31:32.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (47, N'VCSEL Wafer Distribution', 34, N'/DataAnalyze/WaferDistribution', N'', 1, CAST(N'2018-06-06T13:31:44.000' AS DateTime), CAST(N'2018-06-06T13:31:44.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (48, N'VCSEL RMA', 35, N'/DataAnalyze/VcselRMA', N'', 1, CAST(N'2018-06-06T13:31:57.000' AS DateTime), CAST(N'2018-06-06T13:32:06.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (49, N'Permission Approve', 38, N'/Permission/ApprovePermissionRequest', N'', 1, CAST(N'2018-06-07T16:39:05.000' AS DateTime), CAST(N'2018-06-07T16:39:25.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (50, N'Permission Deny', 38, N'/Permission/DenyPermissionRequest', N'', 1, CAST(N'2018-06-07T16:39:59.000' AS DateTime), CAST(N'2018-06-07T16:39:59.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (51, N'Permission Complete', 38, N'/Permission/CompletePermissionRequest', N'', 1, CAST(N'2018-06-07T16:40:45.000' AS DateTime), CAST(N'2018-06-07T16:40:45.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (52, N'Apply For Permission', 37, N'/Permission/ApplyForPermission', N'', 1, CAST(N'2018-06-07T16:41:46.000' AS DateTime), CAST(N'2018-06-07T16:41:46.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (53, N'Permission Request', 38, N'/Permission/PermissionRequest', N'', 1, CAST(N'2018-06-07T16:42:26.000' AS DateTime), CAST(N'2018-06-07T16:42:26.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (54, N'Permission Members', 39, N'/Permission/MemberList', N'', 1, CAST(N'2018-06-08T10:26:09.000' AS DateTime), CAST(N'2018-06-08T10:26:09.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (55, N'Permission Groups', 40, N'/Permission/GroupList', N'', 1, CAST(N'2018-06-08T10:26:25.000' AS DateTime), CAST(N'2018-06-08T10:26:25.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (56, N'Permission Role', 41, N'/Permission/RoleList', N'', 1, CAST(N'2018-06-08T10:26:44.000' AS DateTime), CAST(N'2018-06-08T10:26:44.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (57, N'Permission Menus', 42, N'/Permission/Menus', N'', 1, CAST(N'2018-06-08T10:27:00.000' AS DateTime), CAST(N'2018-06-08T10:27:00.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (58, N'Permission Functions', 43, N'/Permission/Functions', N'', 1, CAST(N'2018-06-08T10:27:18.000' AS DateTime), CAST(N'2018-06-08T10:27:18.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (59, N'Permission Operations', 44, N'/Permission/Operations', N'', 1, CAST(N'2018-06-08T10:27:42.000' AS DateTime), CAST(N'2018-06-08T16:20:28.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (60, N'OBA', 1, N'/Project/ProjectOBA', N'', 1, CAST(N'2018-06-08T15:14:58.000' AS DateTime), CAST(N'2018-06-08T15:14:58.000' AS DateTime))
INSERT [dbo].[n_Function] ([ID], [Name], [MenuID], [Url], [ImgUrl], [Status], [CreateAt], [UpdateAt]) VALUES (61, N'IQC', 1, N'/Project/ProjectIQC', N'', 1, CAST(N'2018-06-08T15:15:27.000' AS DateTime), CAST(N'2018-06-08T15:15:27.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[n_Function] OFF
SET IDENTITY_INSERT [dbo].[n_Group] ON 

INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (1, N'Super Admin', 1, N'Super Admin', CAST(N'2018-06-05T11:22:33.000' AS DateTime), CAST(N'2018-06-06T11:02:47.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (2, N'Admin', 1, N'Admin', CAST(N'2018-06-06T11:03:02.000' AS DateTime), CAST(N'2018-06-06T11:03:02.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (3, N'Manager', 1, N'Manager', CAST(N'2018-06-06T11:03:12.000' AS DateTime), CAST(N'2018-06-06T11:03:12.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (4, N'NPI Admin', 1, N'NPI Admin', CAST(N'2018-06-06T11:04:38.000' AS DateTime), CAST(N'2018-06-06T11:09:02.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (5, N'NPI General', 1, N'NPI General', CAST(N'2018-06-06T11:07:10.000' AS DateTime), CAST(N'2018-06-06T11:09:25.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (6, N'PE Admin', 1, N'PE Admin', CAST(N'2018-06-06T11:07:20.000' AS DateTime), CAST(N'2018-06-06T11:09:37.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (7, N'PE General', 1, N'PE General', CAST(N'2018-06-06T11:07:29.000' AS DateTime), CAST(N'2018-06-06T11:09:52.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (8, N'ME Admin', 1, N'ME Admin', CAST(N'2018-06-06T11:07:38.000' AS DateTime), CAST(N'2018-06-06T11:10:01.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (9, N'ME General', 1, N'ME General', CAST(N'2018-06-06T11:07:54.000' AS DateTime), CAST(N'2018-06-06T11:10:10.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (10, N'CQE Admin', 1, N'CQE Admin', CAST(N'2018-06-08T15:17:58.000' AS DateTime), CAST(N'2018-06-08T15:17:58.000' AS DateTime))
INSERT [dbo].[n_Group] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (11, N'CQE General', 1, N'CQE General', CAST(N'2018-06-08T15:18:09.000' AS DateTime), CAST(N'2018-06-08T15:18:09.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[n_Group] OFF
SET IDENTITY_INSERT [dbo].[n_GroupRole] ON 

INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (1, 1, 1, 1, N'', CAST(N'2018-06-06T10:07:10.000' AS DateTime), CAST(N'2018-06-06T10:07:10.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (2, 1, 2, 0, N'', CAST(N'2018-06-06T10:10:56.000' AS DateTime), CAST(N'2018-06-06T14:01:43.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (3, 2, 2, 1, N'', CAST(N'2018-06-06T14:02:50.000' AS DateTime), CAST(N'2018-06-06T14:02:50.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (4, 2, 3, 1, N'', CAST(N'2018-06-06T14:02:50.000' AS DateTime), CAST(N'2018-06-08T09:28:32.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (5, 1, 5, 1, N'', CAST(N'2018-06-08T08:52:56.000' AS DateTime), CAST(N'2018-06-08T08:52:56.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (6, 2, 6, 1, N'', CAST(N'2018-06-08T08:53:03.000' AS DateTime), CAST(N'2018-06-08T08:53:03.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (7, 2, 7, 0, N'', CAST(N'2018-06-08T10:51:41.000' AS DateTime), CAST(N'2018-06-11T11:12:00.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (8, 1, 7, 1, N'', CAST(N'2018-06-08T10:51:49.000' AS DateTime), CAST(N'2018-06-08T10:51:49.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (9, 3, 7, 1, N'', CAST(N'2018-06-08T10:52:03.000' AS DateTime), CAST(N'2018-06-08T10:52:03.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (10, 3, 5, 1, N'', CAST(N'2018-06-08T10:52:03.000' AS DateTime), CAST(N'2018-06-08T10:52:03.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (11, 5, 8, 1, N'', CAST(N'2018-06-08T10:52:54.000' AS DateTime), CAST(N'2018-06-08T10:52:54.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (12, 5, 4, 1, N'', CAST(N'2018-06-08T10:52:54.000' AS DateTime), CAST(N'2018-06-08T10:52:54.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (13, 5, 2, 1, N'', CAST(N'2018-06-08T10:54:12.000' AS DateTime), CAST(N'2018-06-08T10:54:12.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (14, 3, 2, 1, N'', CAST(N'2018-06-08T10:56:13.000' AS DateTime), CAST(N'2018-06-08T10:56:13.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (15, 10, 9, 1, N'', CAST(N'2018-06-08T15:18:41.000' AS DateTime), CAST(N'2018-06-08T15:18:41.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (16, 10, 8, 1, N'', CAST(N'2018-06-08T15:18:41.000' AS DateTime), CAST(N'2018-06-08T15:18:41.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (17, 10, 4, 1, N'', CAST(N'2018-06-08T15:18:41.000' AS DateTime), CAST(N'2018-06-08T15:18:41.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (18, 9, 4, 0, N'', CAST(N'2018-06-08T16:13:40.000' AS DateTime), CAST(N'2018-06-08T16:14:28.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (19, 9, 2, 1, N'', CAST(N'2018-06-08T16:14:28.000' AS DateTime), CAST(N'2018-06-08T16:14:28.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (20, 10, 10, 0, N'', CAST(N'2018-06-11T10:33:30.000' AS DateTime), CAST(N'2018-06-11T11:15:58.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (21, 2, 11, 1, N'', CAST(N'2018-06-11T11:12:43.000' AS DateTime), CAST(N'2018-06-11T11:12:43.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (22, 1, 11, 0, N'', CAST(N'2018-06-11T15:08:02.000' AS DateTime), CAST(N'2018-06-11T15:12:58.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (23, 1, 6, 1, N'', CAST(N'2018-06-11T15:08:02.000' AS DateTime), CAST(N'2018-06-11T15:12:58.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (24, 5, 6, 0, N'', CAST(N'2018-06-11T16:51:26.000' AS DateTime), CAST(N'2018-06-11T16:51:54.000' AS DateTime))
INSERT [dbo].[n_GroupRole] ([ID], [GroupID], [RoleID], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (25, 5, 11, 0, N'', CAST(N'2018-06-11T16:51:54.000' AS DateTime), CAST(N'2018-06-11T17:02:26.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[n_GroupRole] OFF
SET IDENTITY_INSERT [dbo].[n_Menu] ON 

INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (1, N'View Projects', N'/Project/ViewAll', N'', 3, 1, 1, CAST(N'2018-06-05T11:26:56.000' AS DateTime), CAST(N'2018-06-06T10:39:26.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (2, N'Create Projects', N'/Project/CreateProject', N'', 3, 2, 1, CAST(N'2018-06-06T10:12:54.000' AS DateTime), CAST(N'2018-06-06T10:39:36.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (3, N'Projects', N'/', N'', 0, 1, 1, CAST(N'2018-06-06T10:13:19.000' AS DateTime), CAST(N'2018-06-11T10:48:31.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (4, N'Task', N'/', N'', 0, 2, 1, CAST(N'2018-06-06T10:40:15.000' AS DateTime), CAST(N'2018-06-06T10:40:15.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (5, N'Create Task', N'/Issue/CreateIssue', N'', 4, 1, 1, CAST(N'2018-06-06T10:41:07.000' AS DateTime), CAST(N'2018-06-06T10:41:07.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (6, N'Create RMA', N'/Issue/CreateRMA', N'', 4, 2, 1, CAST(N'2018-06-06T10:42:00.000' AS DateTime), CAST(N'2018-06-06T10:42:00.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (7, N'Create FA', N'/CustomerData/CommitFAData', N'', 4, 3, 1, CAST(N'2018-06-06T10:42:31.000' AS DateTime), CAST(N'2018-06-06T10:42:31.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (8, N'Search Task', N'/Issue/SearchIssue', N'', 4, 4, 1, CAST(N'2018-06-06T10:42:48.000' AS DateTime), CAST(N'2018-06-06T10:42:48.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (9, N'3rd Party Data', N'/', N'', 0, 3, 1, CAST(N'2018-06-06T10:43:43.000' AS DateTime), CAST(N'2018-06-06T10:43:43.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (10, N'Submit Vcsel PN Info', N'/CustomerData/CommitVcselPNInfo', N'', 9, 1, 1, CAST(N'2018-06-06T10:44:01.000' AS DateTime), CAST(N'2018-06-06T10:44:01.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (11, N'Submit Vcsel Data', N'/CustomerData/CommitVcselData', N'', 9, 2, 1, CAST(N'2018-06-06T10:44:15.000' AS DateTime), CAST(N'2018-06-06T10:44:15.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (12, N'Review Vcsel Data', N'/CustomerData/ReviewVcselData', N'', 9, 3, 1, CAST(N'2018-06-06T10:44:31.000' AS DateTime), CAST(N'2018-06-06T10:44:31.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (13, N'Review RMA Data', N'/CustomerData/ReviewRMABackupData', N'', 9, 4, 1, CAST(N'2018-06-06T10:44:47.000' AS DateTime), CAST(N'2018-06-06T10:44:47.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (14, N'Review REL Data', N'/CustomerData/ReviewRelBackupData', N'', 9, 5, 1, CAST(N'2018-06-06T10:45:10.000' AS DateTime), CAST(N'2018-06-06T10:45:10.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (15, N'Review IQE Data', N'/CustomerData/ReviewIQEBackupData', N'', 9, 6, 1, CAST(N'2018-06-06T10:45:23.000' AS DateTime), CAST(N'2018-06-06T10:45:23.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (16, N'Review Wafer Info', N'/CustomerData/ReviewWaferInfo', N'', 9, 7, 1, CAST(N'2018-06-06T10:45:42.000' AS DateTime), CAST(N'2018-06-06T10:45:42.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (17, N'Review Wafer Coord Data', N'/CustomerData/ReviewWaferCoordData', N'', 9, 8, 1, CAST(N'2018-06-06T10:45:57.000' AS DateTime), CAST(N'2018-06-06T10:45:57.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (18, N'Debug Tree', N'/DashBoard/DebugTree', N'', 9, 9, 1, CAST(N'2018-06-06T10:46:13.000' AS DateTime), CAST(N'2018-06-06T10:46:13.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (19, N'DownLoad ATE', N'/CustomerData/DownLoadATETestData', N'', 9, 10, 1, CAST(N'2018-06-06T10:46:35.000' AS DateTime), CAST(N'2018-06-06T10:46:35.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (20, N'User Center', N'/', N'', 0, 4, 1, CAST(N'2018-06-06T10:48:24.000' AS DateTime), CAST(N'2018-06-06T10:48:24.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (21, N'User Center', N'/', N'', 20, 1, 1, CAST(N'2018-06-06T10:49:14.000' AS DateTime), CAST(N'2018-06-06T10:49:14.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (22, N'Add Project Critical Error', N'/User/AddPJCriticalError', N'', 20, 2, 1, CAST(N'2018-06-06T10:49:40.000' AS DateTime), CAST(N'2018-06-06T10:49:40.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (23, N'I Status', N'/User/MyStatus', N'', 20, 3, 1, CAST(N'2018-06-06T10:50:14.000' AS DateTime), CAST(N'2018-06-06T10:50:14.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (24, N'I Focus', N'/User/ICare', N'', 20, 4, 1, CAST(N'2018-06-06T10:50:38.000' AS DateTime), CAST(N'2018-06-06T10:50:38.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (25, N'UserMatrix', N'/CustomerData/CommitUserMatrix', N'', 20, 5, 1, CAST(N'2018-06-06T10:50:57.000' AS DateTime), CAST(N'2018-06-06T10:50:57.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (26, N'Add Share Tag', N'/User/AddShareTag', N'', 20, 6, 1, CAST(N'2018-06-06T10:51:09.000' AS DateTime), CAST(N'2018-06-06T10:51:09.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (27, N'I Group', N'I Group', N'', 20, 7, 1, CAST(N'2018-06-06T10:51:40.000' AS DateTime), CAST(N'2018-06-06T10:51:40.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (28, N'I Admire', N'/User/IAdmire', N'', 20, 8, 1, CAST(N'2018-06-06T10:51:54.000' AS DateTime), CAST(N'2018-06-06T10:51:54.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (29, N'User Reviewed Info', N'/User/UserReviewedInfo', N'', 20, 9, 1, CAST(N'2018-06-06T10:52:18.000' AS DateTime), CAST(N'2018-06-06T10:52:18.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (30, N'User Shared Info', N'/User/UserSharedInfo', N'', 20, 10, 1, CAST(N'2018-06-06T10:52:29.000' AS DateTime), CAST(N'2018-06-06T10:53:01.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (31, N'KPI Summary', N'/User/GetKPI', N'', 20, 11, 1, CAST(N'2018-06-06T10:52:47.000' AS DateTime), CAST(N'2018-06-06T10:52:47.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (32, N'Analysis', N'/', N'', 0, 5, 1, CAST(N'2018-06-06T10:53:35.000' AS DateTime), CAST(N'2018-06-06T10:53:35.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (33, N'VCSEL Monthly Yield', N'/DataAnalyze/MonthlyVcsel', N'', 32, 1, 1, CAST(N'2018-06-06T10:53:59.000' AS DateTime), CAST(N'2018-06-06T10:53:59.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (34, N'VCSEL Wafer Distribution', N'/DataAnalyze/WaferDistribution', N'', 32, 2, 1, CAST(N'2018-06-06T10:54:11.000' AS DateTime), CAST(N'2018-06-06T10:54:11.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (35, N'VCSEL RMA', N'/DataAnalyze/VcselRMA', N'', 32, 3, 1, CAST(N'2018-06-06T10:54:24.000' AS DateTime), CAST(N'2018-06-06T10:54:24.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (36, N'Permission', N'/Permission/ApplyForPermission', N'', 20, 12, 1, CAST(N'2018-06-07T16:26:56.000' AS DateTime), CAST(N'2018-06-07T16:27:16.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (37, N'Apply For Permission', N'/Permission/ApplyForPermission', N'', 36, 1, 1, CAST(N'2018-06-07T16:33:49.000' AS DateTime), CAST(N'2018-06-07T16:33:49.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (38, N'Permission Request', N'/Permission/PermissionRequest', N'', 36, 2, 1, CAST(N'2018-06-07T16:34:16.000' AS DateTime), CAST(N'2018-06-07T16:34:50.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (39, N'Members', N'/Permission/MemberList', N'', 36, 3, 1, CAST(N'2018-06-07T16:35:28.000' AS DateTime), CAST(N'2018-06-07T16:35:28.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (40, N'Groups', N'/Permission/GroupList', N'', 36, 4, 1, CAST(N'2018-06-07T16:35:59.000' AS DateTime), CAST(N'2018-06-07T16:35:59.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (41, N'Roles', N'/Permission/RoleList', N'', 36, 5, 1, CAST(N'2018-06-07T16:36:22.000' AS DateTime), CAST(N'2018-06-07T16:36:22.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (42, N'Menus', N'/Permission/Menus', N'', 36, 6, 1, CAST(N'2018-06-07T16:36:42.000' AS DateTime), CAST(N'2018-06-07T16:36:42.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (43, N'Functions', N'/Permission/Functions', N'', 36, 7, 1, CAST(N'2018-06-07T16:37:04.000' AS DateTime), CAST(N'2018-06-07T16:37:04.000' AS DateTime))
INSERT [dbo].[n_Menu] ([ID], [Name], [Url], [ImgUrl], [ParentID], [OrderID], [Status], [CreateAt], [UpdateAt]) VALUES (44, N'Operation', N'/Permission/Operations', N'', 36, 8, 1, CAST(N'2018-06-07T16:37:32.000' AS DateTime), CAST(N'2018-06-07T16:37:32.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[n_Menu] OFF
SET IDENTITY_INSERT [dbo].[n_Operation] ON 

INSERT [dbo].[n_Operation] ([ID], [Name], [Status], [CreateAt], [UpdateAt]) VALUES (1, N'All', 1, CAST(N'2018-06-05T11:39:53.000' AS DateTime), CAST(N'2018-06-06T13:54:10.000' AS DateTime))
INSERT [dbo].[n_Operation] ([ID], [Name], [Status], [CreateAt], [UpdateAt]) VALUES (2, N'Read', 1, CAST(N'2018-06-05T11:39:58.000' AS DateTime), CAST(N'2018-06-06T13:54:17.000' AS DateTime))
INSERT [dbo].[n_Operation] ([ID], [Name], [Status], [CreateAt], [UpdateAt]) VALUES (3, N'Write', 1, CAST(N'2018-06-05T11:40:21.000' AS DateTime), CAST(N'2018-06-06T13:54:25.000' AS DateTime))
INSERT [dbo].[n_Operation] ([ID], [Name], [Status], [CreateAt], [UpdateAt]) VALUES (4, N'None', 1, CAST(N'2018-06-06T13:16:23.000' AS DateTime), CAST(N'2018-06-06T13:54:30.000' AS DateTime))
INSERT [dbo].[n_Operation] ([ID], [Name], [Status], [CreateAt], [UpdateAt]) VALUES (5, N'Approved', 1, CAST(N'2018-06-06T16:55:59.000' AS DateTime), CAST(N'2018-06-06T16:55:59.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[n_Operation] OFF
SET IDENTITY_INSERT [dbo].[n_Role] ON 

INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (1, N'Super Role', 1, N'Super Role', CAST(N'2018-06-05T16:00:25.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (2, N'General Role', 1, N'General Role', CAST(N'2018-06-05T16:39:44.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (3, N'Test Role', 1, N'Test', CAST(N'2018-06-06T14:02:29.000' AS DateTime), CAST(N'2018-06-06T14:06:24.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (4, N'Permission General Role', 1, N'For general', CAST(N'2018-06-07T16:42:57.000' AS DateTime), CAST(N'2018-06-07T16:47:15.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (5, N'Permission  Approve Role', 1, N'Approve Role', CAST(N'2018-06-08T08:47:43.000' AS DateTime), CAST(N'2018-06-08T08:52:31.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (6, N'Permission Operate Role', 1, N'Operate ', CAST(N'2018-06-08T08:48:03.000' AS DateTime), CAST(N'2018-06-08T08:52:41.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (7, N'Permission Admin Menu', 1, N'Permission Admin Menu', CAST(N'2018-06-08T10:43:19.000' AS DateTime), CAST(N'2018-06-08T10:43:19.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (8, N'Permission General Menu', 1, N'Permission General Menu', CAST(N'2018-06-08T10:48:46.000' AS DateTime), CAST(N'2018-06-08T10:48:46.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (9, N'CQE Role', 1, N'CQE Role', CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (10, N'Eva Test role', 0, N'Test', CAST(N'2018-06-11T10:29:08.000' AS DateTime), CAST(N'2018-06-11T11:16:03.000' AS DateTime))
INSERT [dbo].[n_Role] ([ID], [Name], [Status], [Comment], [CreateAt], [UpdateAt]) VALUES (11, N'Permission Operate Menu', 1, N'Permission Operate Menu', CAST(N'2018-06-11T11:11:45.000' AS DateTime), CAST(N'2018-06-11T11:11:45.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[n_Role] OFF
SET IDENTITY_INSERT [dbo].[n_RolePermission] ON 

INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (171, 2, 1, 1, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (172, 2, 1, 2, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (173, 2, 1, 4, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (95, 1, 1, 3, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (96, 1, 1, 4, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (97, 1, 1, 5, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (98, 1, 1, 6, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (99, 1, 1, 7, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (100, 1, 1, 8, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (101, 1, 1, 9, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (102, 1, 1, 10, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (103, 1, 1, 11, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (104, 1, 1, 12, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (105, 1, 2, 20, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (106, 1, 5, 21, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (107, 1, 6, 22, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (108, 1, 7, 23, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (109, 1, 8, 24, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (110, 1, 10, 25, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (111, 1, 11, 26, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (112, 1, 12, 27, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (113, 1, 13, 28, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (114, 1, 14, 29, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (115, 1, 15, 30, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (116, 1, 16, 31, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (117, 1, 17, 32, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (118, 1, 18, 33, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (119, 1, 19, 34, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (120, 1, 22, 36, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (121, 1, 23, 37, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (122, 1, 24, 38, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (123, 1, 25, 39, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (124, 1, 26, 40, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (125, 1, 27, 41, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (126, 1, 28, 42, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (127, 1, 29, 43, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (128, 1, 30, 44, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (129, 1, 31, 45, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (130, 1, 33, 46, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (131, 1, 34, 47, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (132, 1, 35, 48, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (133, 1, 21, 35, 1, CAST(N'2018-06-06T13:41:15.000' AS DateTime), CAST(N'2018-06-06T13:41:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (140, 3, 1, 1, 1, CAST(N'2018-06-06T14:06:24.000' AS DateTime), CAST(N'2018-06-06T14:06:24.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (141, 3, 21, 35, 1, CAST(N'2018-06-06T14:06:24.000' AS DateTime), CAST(N'2018-06-06T14:06:24.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (142, 3, 13, 28, 2, CAST(N'2018-06-06T14:06:24.000' AS DateTime), CAST(N'2018-06-06T14:06:24.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (144, 4, 37, 52, 1, CAST(N'2018-06-07T16:47:15.000' AS DateTime), CAST(N'2018-06-07T16:47:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (145, 4, 38, 53, 1, CAST(N'2018-06-07T16:47:15.000' AS DateTime), CAST(N'2018-06-07T16:47:15.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (154, 5, 38, 49, 1, CAST(N'2018-06-08T08:52:31.000' AS DateTime), CAST(N'2018-06-08T08:52:31.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (155, 5, 38, 50, 1, CAST(N'2018-06-08T08:52:31.000' AS DateTime), CAST(N'2018-06-08T08:52:31.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (158, 6, 38, 51, 1, CAST(N'2018-06-08T08:52:41.000' AS DateTime), CAST(N'2018-06-08T08:52:41.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (156, 5, 37, 52, 1, CAST(N'2018-06-08T08:52:31.000' AS DateTime), CAST(N'2018-06-08T08:52:31.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (159, 6, 37, 52, 1, CAST(N'2018-06-08T08:52:41.000' AS DateTime), CAST(N'2018-06-08T08:52:41.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (157, 5, 38, 53, 1, CAST(N'2018-06-08T08:52:31.000' AS DateTime), CAST(N'2018-06-08T08:52:31.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (160, 6, 38, 53, 1, CAST(N'2018-06-08T08:52:41.000' AS DateTime), CAST(N'2018-06-08T08:52:41.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (161, 7, 39, 54, 1, CAST(N'2018-06-08T10:43:19.000' AS DateTime), CAST(N'2018-06-08T10:43:19.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (162, 7, 40, 55, 1, CAST(N'2018-06-08T10:43:19.000' AS DateTime), CAST(N'2018-06-08T10:43:19.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (163, 7, 41, 56, 1, CAST(N'2018-06-08T10:43:19.000' AS DateTime), CAST(N'2018-06-08T10:43:19.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (164, 7, 42, 57, 1, CAST(N'2018-06-08T10:43:19.000' AS DateTime), CAST(N'2018-06-08T10:43:19.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (165, 7, 43, 58, 1, CAST(N'2018-06-08T10:43:19.000' AS DateTime), CAST(N'2018-06-08T10:43:19.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (166, 7, 44, 59, 1, CAST(N'2018-06-08T10:43:19.000' AS DateTime), CAST(N'2018-06-08T10:43:19.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (167, 7, 37, 52, 1, CAST(N'2018-06-08T10:43:19.000' AS DateTime), CAST(N'2018-06-08T10:43:19.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (168, 7, 38, 53, 1, CAST(N'2018-06-08T10:43:19.000' AS DateTime), CAST(N'2018-06-08T10:43:19.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (169, 8, 37, 52, 1, CAST(N'2018-06-08T10:48:46.000' AS DateTime), CAST(N'2018-06-08T10:48:46.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (170, 8, 38, 53, 1, CAST(N'2018-06-08T10:48:46.000' AS DateTime), CAST(N'2018-06-08T10:48:46.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (174, 2, 1, 3, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (175, 2, 1, 5, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (176, 2, 1, 6, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (177, 2, 1, 7, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (178, 2, 1, 8, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (179, 2, 1, 9, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (180, 2, 1, 10, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (181, 2, 1, 11, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (182, 2, 1, 12, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (183, 2, 2, 20, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (184, 2, 5, 21, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (185, 2, 6, 22, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (186, 2, 8, 24, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (187, 2, 21, 35, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (188, 2, 22, 36, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (189, 2, 23, 37, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (190, 2, 24, 38, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (191, 2, 31, 45, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (192, 2, 33, 46, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (193, 2, 34, 47, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (194, 2, 35, 48, 1, CAST(N'2018-06-08T11:02:27.000' AS DateTime), CAST(N'2018-06-08T11:02:27.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (195, 9, 1, 4, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (196, 9, 1, 10, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (197, 9, 1, 60, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (198, 9, 1, 61, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (199, 9, 1, 3, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (200, 9, 5, 21, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (201, 9, 6, 22, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (202, 9, 8, 24, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (203, 9, 15, 30, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (204, 9, 21, 35, 1, CAST(N'2018-06-08T15:17:47.000' AS DateTime), CAST(N'2018-06-08T15:17:47.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (205, 10, 1, 1, 1, CAST(N'2018-06-11T10:29:08.000' AS DateTime), CAST(N'2018-06-11T10:29:08.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (206, 11, 38, 51, 1, CAST(N'2018-06-11T11:11:45.000' AS DateTime), CAST(N'2018-06-11T11:11:45.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (207, 11, 37, 52, 1, CAST(N'2018-06-11T11:11:45.000' AS DateTime), CAST(N'2018-06-11T11:11:45.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (208, 11, 39, 54, 1, CAST(N'2018-06-11T11:11:45.000' AS DateTime), CAST(N'2018-06-11T11:11:45.000' AS DateTime))
GO
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (209, 11, 40, 55, 1, CAST(N'2018-06-11T11:11:45.000' AS DateTime), CAST(N'2018-06-11T11:11:45.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (210, 11, 41, 56, 1, CAST(N'2018-06-11T11:11:45.000' AS DateTime), CAST(N'2018-06-11T11:11:45.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (211, 11, 42, 57, 1, CAST(N'2018-06-11T11:11:45.000' AS DateTime), CAST(N'2018-06-11T11:11:45.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (212, 11, 43, 58, 1, CAST(N'2018-06-11T11:11:45.000' AS DateTime), CAST(N'2018-06-11T11:11:45.000' AS DateTime))
INSERT [dbo].[n_RolePermission] ([ID], [RoleID], [MenuID], [FunctionID], [OperationID], [CreateAt], [UpdateAt]) VALUES (213, 11, 44, 59, 1, CAST(N'2018-06-11T11:11:45.000' AS DateTime), CAST(N'2018-06-11T11:11:45.000' AS DateTime))
SET IDENTITY_INSERT [dbo].[n_RolePermission] OFF
ALTER TABLE [dbo].[n_Function] ADD  CONSTRAINT [DF_n_Function_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[n_Group] ADD  CONSTRAINT [DF_n_Group_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[n_GroupRole] ADD  CONSTRAINT [DF_n_GroupRole_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[n_Menu] ADD  CONSTRAINT [DF_n_Menu_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[n_Operation] ADD  CONSTRAINT [DF_n_Operation_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[n_Role] ADD  CONSTRAINT [DF_n_Role_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[n_RolePermission] ADD  CONSTRAINT [DF_n_RolePermission_RoleID]  DEFAULT ((0)) FOR [RoleID]
GO
ALTER TABLE [dbo].[n_RolePermission] ADD  CONSTRAINT [DF_n_RolePermission_Type]  DEFAULT ((0)) FOR [MenuID]
GO
ALTER TABLE [dbo].[n_RolePermission] ADD  CONSTRAINT [DF_n_RolePermission_SourceID]  DEFAULT ((0)) FOR [FunctionID]
GO
ALTER TABLE [dbo].[n_RolePermission] ADD  CONSTRAINT [DF_n_RolePermission_OperationID]  DEFAULT ((0)) FOR [OperationID]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Function', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Group', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid  0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_GroupRole', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Menu', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Operation', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Role', @level2type=N'COLUMN',@level2name=N'Status'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 Menu 2 Operation 0 Common' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_RolePermission', @level2type=N'COLUMN',@level2name=N'MenuID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'MenuID/FuncID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_RolePermission', @level2type=N'COLUMN',@level2name=N'FunctionID'
GO

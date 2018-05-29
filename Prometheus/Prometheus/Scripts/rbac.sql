SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[n_Function](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](200) NOT NULL,
    [MenuID] [int] NOT NULL,
    [Url] [nvarchar](200) NOT NULL,
    [Status] [int] NOT NULL,
    [CreateAt] [datetime] NOT NULL,
    [UpdateAt] [datetime] NOT NULL,
 CONSTRAINT [PK_n_Function] PRIMARY KEY CLUSTERED 
(
    [ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[n_Function] ADD  CONSTRAINT [DF_n_Function_Status]  DEFAULT ((1)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Function', @level2type=N'COLUMN',@level2name=N'Status'
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

ALTER TABLE [dbo].[n_Group] ADD  CONSTRAINT [DF_n_Group_Status]  DEFAULT ((1)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Group', @level2type=N'COLUMN',@level2name=N'Status'
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

ALTER TABLE [dbo].[n_GroupRole] ADD  CONSTRAINT [DF_n_GroupRole_Status]  DEFAULT ((1)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid  0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_GroupRole', @level2type=N'COLUMN',@level2name=N'Status'
GO

CREATE TABLE [dbo].[n_Menu](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [Name] [nvarchar](50) NOT NULL,
    [Url] [nvarchar](200) NOT NULL,
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

ALTER TABLE [dbo].[n_Menu] ADD  CONSTRAINT [DF_n_Menu_Status]  DEFAULT ((1)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Menu', @level2type=N'COLUMN',@level2name=N'Status'
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

ALTER TABLE [dbo].[n_Operation] ADD  CONSTRAINT [DF_n_Operation_Status]  DEFAULT ((1)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Operation', @level2type=N'COLUMN',@level2name=N'Status'
GO

-- CREATE TABLE [dbo].[n_OperationPermission](
--     [ID] [int] IDENTITY(1,1) NOT NULL,
--     [Type] [int] NOT NULL,
--     [SourceID] [int] NOT NULL,
--     [OperationID] [int] NOT NULL,
--     [CreateAt] [datetime] NOT NULL,
--     [UpdateAt] [datetime] NOT NULL,
--  CONSTRAINT [PK_n_OperationPermission] PRIMARY KEY CLUSTERED 
-- (
--     [ID] ASC
-- )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
-- ) ON [PRIMARY]

-- GO

-- ALTER TABLE [dbo].[n_OperationPermission] ADD  CONSTRAINT [DF_n_OperationPermission_Type]  DEFAULT ((0)) FOR [Type]
-- GO

-- ALTER TABLE [dbo].[n_OperationPermission] ADD  CONSTRAINT [DF_n_OperationPermission_SourceID]  DEFAULT ((0)) FOR [SourceID]
-- GO

-- EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 Menu 2 Operation 0 Common' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_OperationPermission', @level2type=N'COLUMN',@level2name=N'Type'
-- GO

-- EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'MenuID/FuncID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_OperationPermission', @level2type=N'COLUMN',@level2name=N'SourceID'
-- GO

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

ALTER TABLE [dbo].[n_Role] ADD  CONSTRAINT [DF_n_Role_Status]  DEFAULT ((1)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_Role', @level2type=N'COLUMN',@level2name=N'Status'
GO

CREATE TABLE [dbo].[n_RolePermission](
    [ID] [int] IDENTITY(1,1) NOT NULL,
    [RoleID] [int] NOT NULL,
    [Type] [int] NOT NULL,
    [SourceID] [int] NOT NULL,
    [OperationID] [int] NOT NULL,
    [CreateAt] [datetime] NOT NULL,
    [UpdateAt] [datetime] NOT NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[n_RolePermission] ADD  CONSTRAINT [DF_n_RolePermission_RoleID]  DEFAULT ((0)) FOR [RoleID]
GO

ALTER TABLE [dbo].[n_RolePermission] ADD  CONSTRAINT [DF_n_RolePermission_Type]  DEFAULT ((0)) FOR [Type]
GO

ALTER TABLE [dbo].[n_RolePermission] ADD  CONSTRAINT [DF_n_RolePermission_SourceID]  DEFAULT ((0)) FOR [SourceID]
GO

ALTER TABLE [dbo].[n_RolePermission] ADD  CONSTRAINT [DF_n_RolePermission_OperationID]  DEFAULT ((0)) FOR [OperationID]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 Menu 2 Operation 0 Common' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_RolePermission', @level2type=N'COLUMN',@level2name=N'Type'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'MenuID/FuncID' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_RolePermission', @level2type=N'COLUMN',@level2name=N'SourceID'
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

ALTER TABLE [dbo].[n_User] ADD  CONSTRAINT [DF_n_User_Status]  DEFAULT ((1)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid 2 not verified' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_User', @level2type=N'COLUMN',@level2name=N'Status'
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

ALTER TABLE [dbo].[n_UserGroup] ADD  CONSTRAINT [DF_n_UserGroup_Status]  DEFAULT ((1)) FOR [Status]
GO

EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'1 valid 0 invalid' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'n_UserGroup', @level2type=N'COLUMN',@level2name=N'Status'
GO
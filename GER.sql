
GO
/****** Object:  Table [dbo].[Constituency]    Script Date: 21/12/2019 15:10:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Constituency](
	[ConstituencyName] [nvarchar](200) NOT NULL,
	[ConstituencyID] [int] IDENTITY(1,1) NOT NULL,
	[ONS_Code] [nchar](9) NOT NULL,
 CONSTRAINT [PK_Constituency] PRIMARY KEY CLUSTERED 
(
	[ConstituencyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CandidateResult]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CandidateResult](
	[CandidateName] [nvarchar](200) NOT NULL,
	[PartyCode] [nvarchar](10) NOT NULL,
	[Party] [nvarchar](200) NOT NULL,
	[ElectionID] [int] NOT NULL,
	[ConstituencyID] [int] NOT NULL,
	[Votes] [int] NOT NULL,
	[VoteShare] [decimal](3, 1) NULL,
	[VoteShareChange] [decimal](3, 1) NULL,
 CONSTRAINT [PK_CandidateResult] PRIMARY KEY CLUSTERED 
(
	[ElectionID] ASC,
	[ConstituencyID] ASC,
	[Party] ASC,
	[CandidateName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_con_candidate]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_con_candidate]
AS
SELECT        dbo.Constituency.ConstituencyName, dbo.CandidateResult.CandidateName
FROM            dbo.CandidateResult INNER JOIN
                         dbo.Constituency ON dbo.CandidateResult.ConstituencyID = dbo.Constituency.ConstituencyID
GROUP BY dbo.CandidateResult.CandidateName, dbo.Constituency.ConstituencyName
GO
/****** Object:  Table [dbo].[Party]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Party](
	[PartyName] [nvarchar](200) NOT NULL,
	[PartyID] [int] IDENTITY(1,1) NOT NULL,
	[PartyCode] [nvarchar](10) NOT NULL,
 CONSTRAINT [PK_Party] PRIMARY KEY CLUSTERED 
(
	[PartyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_winner]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_winner]
AS
SELECT        c.ConstituencyID, dbo.Party.PartyID, c.PartyCode, c.Votes
FROM            (SELECT        ConstituencyID, MAX(Votes) AS max_votes
                          FROM            dbo.CandidateResult
                          GROUP BY ConstituencyID) AS m INNER JOIN
                         dbo.CandidateResult AS c ON c.ConstituencyID = m.ConstituencyID AND c.Votes = m.max_votes INNER JOIN
                         dbo.Party ON c.PartyCode = dbo.Party.PartyCode
GO
/****** Object:  Table [dbo].[ConstituencyResult]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ConstituencyResult](
	[ConstituencyID] [int] NOT NULL,
	[ElectionID] [int] NOT NULL,
   [WinningParty] [nvarchar](200) NULL,
   [Headline] [nvarchar](200) NULL,
	[Electorate] [int] NULL,
	[Votes] [int] NULL,
	[Turnout] [decimal](3, 1) NULL,
	[TurnoutValid] [decimal](3, 1) NULL,
	[TurnoutAll] [decimal](3, 1) NULL,
	[Ballots] [int] NULL,
	[TurnoutBallot] [decimal](3, 1) NULL,
	[TurnoutChange] [decimal](3, 1) NULL,
	[Majority] [int] NULL,
	[Winner] [int] NULL,
 CONSTRAINT [PK_ConstituencyResult] PRIMARY KEY CLUSTERED 
(
	[ElectionID] ASC,
	[ConstituencyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Election]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Election](
	[ElectionYear] [int] NOT NULL,
	[ElectionDate] [smalldatetime] NULL,
	[ElectionID] [int] NOT NULL,
 CONSTRAINT [PK_Election] PRIMARY KEY CLUSTERED 
(
	[ElectionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [NonClusteredIndex-20191219-204503]    Script Date: 21/12/2019 15:10:03 ******/
CREATE UNIQUE NONCLUSTERED INDEX [NonClusteredIndex-20191219-204503] ON [dbo].[Party]
(
	[PartyCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [NonClusteredIndex-20191219-204617]    Script Date: 21/12/2019 15:10:03 ******/
CREATE UNIQUE NONCLUSTERED INDEX [NonClusteredIndex-20191219-204617] ON [dbo].[Party]
(
	[PartyName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[add_candidate_result]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[add_candidate_result] 
	@con_id int,
	@election_id int,
	@party_code nvarchar(10),
	@party nvarchar(200),
	@candidate nvarchar(200),
	@votes int,
	@vote_share decimal(3,1),
	@vote_share_change decimal(3,1)
AS
BEGIN
	
	insert into CandidateResult(
	ConstituencyID,
	ElectionID,
	PartyCode,
	Party,
	CandidateName,
	Votes,
	VoteShare,
	VoteShareChange)
	values (
	@con_id,
	@election_id,
	@party_code,
	@party,
	@candidate,
	@votes,
	@vote_share,
	@vote_share_change)
    
END
GO
/****** Object:  StoredProcedure [dbo].[add_constituency]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[add_constituency] 
	@gssid nchar(9),
	@con_name nvarchar(200)
AS
BEGIN
	
	insert into Constituency (
	ONS_Code,
	ConstituencyName)
	values (
	@gssid,
	@con_name)
    
END
GO
/****** Object:  StoredProcedure [dbo].[add_constituency_result]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[add_constituency_result] 
	@con_id int,
	@election_id int,
   @winning_party nvarchar(200),
   @headline nvarchar(200),
	@majority int,
	@electorate int,
	@turnout decimal(3,1),
	@turnout_change decimal(3,1)
AS
BEGIN
	
	insert into ConstituencyResult(
	ConstituencyID,
	ElectionID,
   WinningParty,
   Headline,
	Majority,
	Electorate,
	Turnout,
	TurnoutChange)
	values (
	@con_id,
	@election_id,
    @winning_party,
    @headline,
	@majority,
	@electorate,
	@turnout,
	@turnout_change)
    
END
GO
/****** Object:  StoredProcedure [dbo].[save_exception]    Script Date: 21/12/2019 15:10:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[save_exception](@ex nvarchar(4000)) as
insert into exceptions(extext, created_date)
values (@ex, current_timestamp)


if exists (select * from sys.views where name='vEncounterIdentifications') drop view dbo.vEncounterIdentifications;
GO

CREATE VIEW [dbo].[vEncounterIdentifications]
AS
SELECT        SubmitterCode,SubmitterClaimIdentification,TransactionId,IehpEncounterId,ReceiptId
FROM            titan.BizSqlB1_EdiManagement.dbo.EncounterIdentifications
union all
select SubmitterCode,SubmitterClaimIdentification,TransactionId,IehpEncounterId,ReceiptId
from titan.Venus_Bizprod_EDIManagement.dbo.EncounterIdentifications
GO

if exists (select * from sys.views where name='vTradingPartners') drop view dbo.vTradingPartners;
go

create view vTradingPartners as
select top (100) percent TradingPartnerId,TradingPArtnerName,TradingPartnerCode from (
select TradingPartnerId,TradingPartnerName,TradingPartnerCode,row_number() over(partition by TradingPartnerName order by TradingPartnerId) as rn 
from titan.BizSqlB1_EdiManagement.dbo.TradingPartners
) t
where t.rn=1
order by t.TradingPartnerId
go

if object_id('JsonDoc_Splitted') is null
begin
create table dbo.JsonDoc_Splitted
(
id bigint not null,
splitted bit not null
)
end
go

if object_id('EVRSplitterTable') is null
begin
CREATE TABLE EVRSplitterTable
(
	ID int identity(1,1) not null,
	EncounterReferenceNumber varchar(20) NOT NULL,
	IEHPEncounterId varchar(17) NOT NULL,
	EncounterStatus varchar(20) not null,
	CONSTRAINT PK_EVRSplitterTable PRIMARY KEY CLUSTERED (ID),
	Index IX_EVREncounterReferenceNumber nonclustered (EncounterReferenceNumber),
	Index IX_EVRIEHPEncounterId nonclustered (IEHPEncounterId)
)
end
go

IF object_id('EncounterTrack') is null
begin
CREATE TABLE [dbo].[EncounterTrack](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[JsonDocId] [nchar](10) NOT NULL,
	[TradingPartnerId] [int] NOT NULL,
	[FileName] [varchar](255) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
 CONSTRAINT [PK_EncounterTrack] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
end
GO


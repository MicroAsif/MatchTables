# MatchTables
A simple application which will sync the source and destination data, That means source and destnation data will be always same. 
this application will work as long as as the data source data table schema, destination table schema are same. 

# let's take a look of the scenario
<img src="https://github.com/MicroAsif/MatchTables/blob/main/Images/visualization.png" width="70%" height="70%">

# Setup
1. First change the connection string in the appsetting.json 
```
"ConnectionStrings": {
    "DefaultConnection": "server=.\\YOUR_SERVER_NAME;database=YOUR_DB_NAME;trusted_connection=true"
  }
 ```
 2. Create table as many as you want but same sure table schemas are same
 ```
CREATE TABLE [dbo].[SourceTable1](
	[SocialSecurityNumber] [nvarchar](50) NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[Department] [nvarchar](50) NULL,
 CONSTRAINT [PK_SourceTable1] PRIMARY KEY CLUSTERED 
(
	[SocialSecurityNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[SourceTable2](
	[SocialSecurityNumber] [nvarchar](50) NOT NULL,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[Department] [nvarchar](50) NULL,
 CONSTRAINT [PK_SourceTable2] PRIMARY KEY CLUSTERED 
(
	[SocialSecurityNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

 ```
 
 # Run the application 
 
```
 -s, --table1        Required. Input source table name to be processed.
 -t, --table2        Required. Input target table name to be processed.
 -p, --primarykey    Required. Input primarykey name to be processed.
 
  MatchTables.exe -s sourcetable1 -t sourcetaable2 -p SocialSecurityNumber
  
 ```






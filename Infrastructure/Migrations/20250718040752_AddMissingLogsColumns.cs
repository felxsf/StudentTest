using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddMissingLogsColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primero verificamos si la tabla existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[Logs] (
                        [Id] [int] IDENTITY(1,1) NOT NULL,
                        [TimeStamp] [datetime2](7) NOT NULL,
                        [Level] [nvarchar](50) NOT NULL,
                        [Message] [nvarchar](4000) NOT NULL,
                        [Exception] [nvarchar](4000) NULL,
                        [MachineName] [nvarchar](200) NULL,
                        [ThreadId] [int] NULL,
                        [ProcessId] [int] NULL,
                        [CorrelationId] [nvarchar](100) NULL,
                        [UserId] [nvarchar](100) NULL,
                        [IpAddress] [nvarchar](45) NULL,
                        [UserAgent] [nvarchar](500) NULL,
                        [Properties] [nvarchar](100) NULL,
                        CONSTRAINT [PK_Logs] PRIMARY KEY CLUSTERED ([Id] ASC)
                    )
                END
            ");

            // Si la tabla existe, verificamos y agregamos las columnas faltantes
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND type in (N'U'))
                BEGIN
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'MachineName')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] ADD [MachineName] [nvarchar](200) NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'ThreadId')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] ADD [ThreadId] [int] NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'ProcessId')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] ADD [ProcessId] [int] NULL;
                    END

                    -- Verificar y agregar otras columnas que podrían faltar
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'CorrelationId')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] ADD [CorrelationId] [nvarchar](100) NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'IpAddress')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] ADD [IpAddress] [nvarchar](45) NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'UserAgent')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] ADD [UserAgent] [nvarchar](500) NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'Properties')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] ADD [Properties] [nvarchar](100) NULL;
                    END
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No eliminamos la tabla en Down ya que podría contener datos importantes
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND type in (N'U'))
                BEGIN
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'MachineName')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] DROP COLUMN [MachineName];
                    END

                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'ThreadId')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] DROP COLUMN [ThreadId];
                    END

                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'ProcessId')
                    BEGIN
                        ALTER TABLE [dbo].[Logs] DROP COLUMN [ProcessId];
                    END
                END
            ");
        }
    }
}

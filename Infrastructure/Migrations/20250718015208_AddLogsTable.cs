using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Primero intentamos crear la tabla si no existe
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Logs' AND xtype='U')
                BEGIN
                    CREATE TABLE [Logs] (
                        [Id] int NOT NULL IDENTITY,
                        [TimeStamp] datetime2 NOT NULL,
                        [Level] nvarchar(50) NOT NULL,
                        [Message] nvarchar(4000) NOT NULL,
                        [Exception] nvarchar(4000) NULL,
                        [MachineName] nvarchar(200) NULL,
                        [ThreadId] int NULL,
                        [ProcessId] int NULL,
                        [CorrelationId] nvarchar(100) NULL,
                        [UserId] nvarchar(100) NULL,
                        [IpAddress] nvarchar(45) NULL,
                        [UserAgent] nvarchar(500) NULL,
                        [Properties] nvarchar(100) NULL,
                        CONSTRAINT [PK_Logs] PRIMARY KEY ([Id])
                    );
                END
            ");

            // Luego verificamos y agregamos las columnas si no existen
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'MachineName')
                BEGIN
                    ALTER TABLE [Logs] ADD [MachineName] nvarchar(200) NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'ThreadId')
                BEGIN
                    ALTER TABLE [Logs] ADD [ThreadId] int NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Logs]') AND name = 'ProcessId')
                BEGIN
                    ALTER TABLE [Logs] ADD [ProcessId] int NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sysobjects WHERE name='Logs' AND xtype='U')
                BEGIN
                    DROP TABLE [Logs];
                END
            ");
        }
    }
}

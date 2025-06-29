# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a C# WPF desktop application for managing food portion tickets (Gestión Talonarios). The application manages ticket sales for a food service, tracking traditional and vegan portions, payments, and deliveries.

## Technology Stack

- **Framework**: .NET 8.0
- **UI**: WPF (Windows Presentation Foundation)
- **Database**: SQL Server with Dapper ORM
- **Architecture**: Clean Architecture (Core, Data, UI layers)
- **Logging**: NLog
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Data Mapping**: AutoMapper
- **MVVM**: Custom implementation with PropertyChanged.Fody

## Build and Development Commands

```bash
# Build the solution
dotnet build GestionTalonarios.sln

# Build specific project
dotnet build GestionTalonarios.UI/GestionTalonarios.UI.csproj

# Run the application
dotnet run --project GestionTalonarios.UI/GestionTalonarios.UI.csproj

# Restore packages
dotnet restore GestionTalonarios.sln

# Publish for deployment
dotnet publish GestionTalonarios.UI/GestionTalonarios.UI.csproj -c Release -o publish/
```

## Project Architecture

### Layer Structure
- **GestionTalonarios.Core**: Business logic, models, DTOs, interfaces, and services
- **GestionTalonarios.Data**: Data access layer with repositories and database context
- **GestionTalonarios.UI**: WPF presentation layer with MVVM pattern

### Key Components

#### Core Models
- `Ticket`: Main entity representing food portion orders with traditional/vegan quantities (uses `Code` as primary key)
- `Client`: Customer information (uses `Id` as primary key)
- `Seller`: Vendor information (uses `Id` as primary key)

#### Services
- `TicketService`: Business logic for ticket operations (create, pay, deliver, search)
- `ClientService`: Client management operations

#### Repositories
- `TicketRepository`: Data access for ticket operations with Dapper
- `ClientRepository`: Data access for client operations
- `RepositoryBase`: Base repository with common CRUD operations

#### UI Architecture
- MVVM pattern with `ViewModelBase` and `RelayCommand`
- `MainViewModel`: Primary view model managing ticket display and operations
- `NavigationService`: Handles dialog and window navigation
- Auto-updating timer for refreshing portion counts every 30 seconds

### Database Configuration
- Connection string configured in `appsettings.json`
- SQL Server database: `Locro2025Prod`
- Dapper for data access with custom SQL queries
- Database setup script: `InsertsTalonarios.sql`

### Key Features
- Ticket creation with traditional and vegan portion differentiation
- Payment tracking and confirmation dialogs
- Delivery status management
- Search functionality by different criteria (SearchType enum)
- Real-time portion count updates
- Observation editing for tickets
- Logging with NLog (file and console outputs)

## Database Schema Notes
The Ticket entity uses column mapping attributes for database fields:
- `code`: Ticket identification number (PRIMARY KEY)
- `traditional_qty` / `vegan_qty`: Portion quantities by type
- `is_paid` / `is_delivered`: Status tracking
- `unit_cost`: Price per portion
- Foreign keys to `seller_id` and `client_id`

**Important**: The Ticket table uses `code` as the primary key instead of an auto-increment `id`. This was refactored to eliminate ID/Code confusion and ensure unique ticket codes.

## Development Notes
- The application uses PropertyChanged.Fody for automatic property change notifications
- Dependency injection is configured in the UI layer
- Error handling includes user-friendly MessageBox displays and detailed logging
- Timer-based updates ensure UI reflects current data state
- The application targets Windows-specific features (WPF)
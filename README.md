# RateMe ★ — Film & Series Rating Desktop Application

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![Visual Studio](https://img.shields.io/badge/Visual%20Studio-2019-5C2D91?style=flat&logo=visualstudio)](https://visualstudio.microsoft.com/vs/older-downloads/)
[![C#](https://img.shields.io/badge/Language-C%23%20WinForms-239120?style=flat&logo=c-sharp)](https://docs.microsoft.com/dotnet/csharp/)
[![Database](https://img.shields.io/badge/Database-SQL%20Server%20%7C%20LocalDB-CC292B?style=flat&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![API](https://img.shields.io/badge/API-OMDb%20Integration-E4A11B?style=flat)](https://www.omdbapi.com/)
[![Design System](https://img.shields.io/badge/Design-Starbucks%20Retail%20System-006241?style=flat)](rate-me-design.md)

**RateMe** is a full-featured desktop application built with **C# Windows Forms** (.NET Framework 4.7.2) and **Microsoft Visual Studio 2019**. The application is styled according to the **Starbucks Flagship Retail Design System**, featuring a warm café aesthetic, kinetic spring scroll physics, dual SQL Server / LocalDB storage with JSON fallback, live OMDb API integration, and an administrative governance portal.

> 📖 **AI Agent & Developer Runbook:** For complete project architecture, one-command terminal build scripts, and testing instructions, consult [`PROJECT_MANUAL.md`](PROJECT_MANUAL.md).

---

## 🎨 Visual Identity & Design System

The application implements the design tokens and atmospheric principles defined in [`rate-me-design.md`](rate-me-design.md):

| Surface / Element | Token Color | Hex Code | Role |
| :--- | :--- | :--- | :--- |
| **Page Canvas (Light)** | Neutral Warm | `#F2F0EB` | Primary background referencing warm café paper / ceramic walls |
| **Page Canvas (Dark)** | Dark Espresso | `#1E3932` | Midnight café dark-mode canvas |
| **Brand Primary** | Starbucks Green | `#006241` | Top navigation headers, section titles, and typography |
| **Brand Accent / CTAs** | Green Accent | `#00754A` | Primary filled pill buttons, active filters, floating CTAs |
| **Feature Band / Hero** | House Green | `#1E3932` | Deep near-black green feature banner and footer strips |
| **Rewards Ceremony** | Gold | `#CBA258` | Interactive 1-10 star rating selector, IMDb badges, admin marks |
| **Card Surface** | Ceramic White | `#FFFFFF` | Content card background with 12px rounded corners |
| **Destructive Actions** | Café Red | `#C82014` | Deletion and account suspension states |

### Micro-Interactions & Components
- **50px Pill Buttons:** Universal 50px border-radius full pills with anti-aliased GDI+ rendering and `scale(0.95)` press feedback.
- **12px Rounded Cards:** Flat-plus-lift geometry with layered ambient drop shadows (`0 0 0.5px rgba(0,0,0,0.14)` + `0 1px 1px rgba(0,0,0,0.24)`).
- **Floating "Frap" CTA Button:** Signature 56px circular button (`#00754A`) pinned to the bottom-right for instant title rating.
- **Kinetic Scroll Panel:** Custom scroll container providing spring-physics damping and smooth inertia scrolling inspired by Framer Motion.

---

## ✨ Features

### 1. 🔐 Authentication & Role-Based Access
- Secure password hashing using **PBKDF2** with **SHA-256** (10,000 iterations) and cryptographically secure **128-bit salt** generation (`RNGCryptoServiceProvider`).
- User session state management with live login/logout transitions.
- Roles: `User` and `Admin`.

### 2. 🎬 Explore & Curated Cinema
- **House Green Hero Banner:** *"Discover Your Next Obsession"* with quick CTA navigation.
- **Dynamic Category Filter:** Instant switching between `All Releases`, `Top Rated`, `Movies`, and `TV Series`.
- **Responsive Media Cards:** Live poster loading via background async downloader with memory caching, rating badges, release year, and genre tags.

### 3. 🔎 Debounced OMDb Live Search
- Integrated with the **OMDb API** (`cf367844`).
- Debounced 400ms instant search preventing network floods.
- Real-time results showing live film/series metadata, poster art, and IMDb rating scores.

### 4. ⭐ Interactive 1-10 Star Rating & Reviews
- Custom interactive 10-star rating bar with hover previews and click-to-commit selection.
- Detailed modal dialog displaying high-resolution posters, plot summaries, director, cast, runtime, Metascore, and IMDb ratings.
- Community reviews feed with real-time aggregate score re-calculation.

### 5. 📋 Watchlist & User Profile
- One-click watchlist toggle with persistent database tracking.
- Dedicated **Profile View** with user avatar initials, statistics (titles rated, watchlist count, review count, average given score), and editable bio.

### 6. ⚙️ Settings & Live Dark Mode
- Instant theme toggle between **Warm Cream / Ceramic** (`#F2F0EB`) and **Dark Espresso / House Green** (`#1E3932`).
- Notification preferences and database status indicators.

### 7. 🛠️ Administration & Governance Portal (`Admin ⚙`)
- **System Metrics Bar:** Real-time KPI cards displaying Total Users (with active/suspended counts), Total Catalog Items, Community Reviews, and Database Engine status.
- **User Management (CRUD):**
  - **Create:** Provision new user accounts with validated credentials and role assignment.
  - **Read & Filter:** Live search bar by username, display name, or email + role filter pills (`All Roles`, `Admins`, `Users`).
  - **Update:** Edit account information, role (`User` / `Admin`), and bio.
  - **Reset Password:** Admin-initiated password reset modal with secure re-hashing.
  - **Suspend / Activate:** Quick account status toggle.
  - **Delete:** Safe account deletion with cascade cleanup and self-deletion prevention.
- **Catalog Governance:**
  - Import new movies and TV series directly by **IMDb ID** (e.g. `tt0137523` for *Fight Club*, `tt0109830` for *Forrest Gump*).
  - Remove items from catalog.
- **Review Moderation:**
  - Inspect all user ratings and written reviews across all media titles with direct removal action.

---

## 🗄️ Database Architecture

The system features a **dual-mode database service**:
1. **Microsoft SQL Server / LocalDB:** Connects automatically to `(localdb)\MSSQLLocalDB` or `.\SQLEXPRESS`.
2. **Local JSON Fallback:** If SQL Server is unavailable, transparently transitions to a local JSON database so the application works seamlessly out-of-the-box.

### SQL Server Setup
Run [`RateMe.sql`](RateMe.sql) in SQL Server Management Studio (SSMS) or Visual Studio SQL Server Object Explorer:
- Creates `RateMeDb` database.
- Builds tables: `Users`, `MediaItems`, `Ratings`, `Watchlist`, `UserSettings`.
- Configures foreign keys, cascading deletes, unique constraints, and non-clustered performance indexes.
- Automatically creates stored views and triggers for aggregate score updates.
- Inserts curated seed movies, TV series, reviews, and administrator accounts.

---

## 🚀 Getting Started

### Prerequisites
- **Operating System:** Windows 10 or Windows 11
- **IDE:** Microsoft Visual Studio 2019 (Community, Professional, or Enterprise)
- **Target Framework:** .NET Framework 4.7.2

### Running in Visual Studio 2019
1. Clone the repository:
   ```bash
   git clone https://github.com/umang4zq/RateMe.git
   cd RateMe
   ```
2. Open **`RateMe.sln`** in Visual Studio 2019.
3. Verify project configuration is set to **Debug | Any CPU** (or **Release | Any CPU**).
4. Press **`F5`** (or click **Start**) to build and launch the application.

---

## 🔑 Default Credentials

| Role | Username | Password | Access Level |
| :--- | :--- | :--- | :--- |
| **Administrator** | `admin` | `Admin@123` | Full access to Admin Portal, User CRUD, Catalog Import, Review Moderation |
| **Standard User** | `john_doe` | `User@123` | Rating, Watchlist, Profile, Search, Reviews |

---

## 📂 Repository Structure

```
RateMe/
├── RateMe.sln                      # Visual Studio Solution File
├── RateMe.sql                      # T-SQL Database Schema & Seed Data Script
├── rate-me-design.md               # Starbucks Flagship Retail Design System Spec
├── tasks.md                        # Task Breakdown & Project Requirements
├── .gitignore                      # Git ignore rules for .NET / Visual Studio
├── README.md                       # Project Documentation
└── RateMe/                         # WinForms Application Project
    ├── App.config                  # App settings and connection strings
    ├── Program.cs                  # WinForms Main Entry Point
    ├── Form1.cs                    # Main Shell Form (Top Nav, View Swapping)
    ├── Form1.Designer.cs           # Form Designer Layout
    ├── RateMe.csproj               # Visual Studio Project File
    ├── Models/                     # Data Entities (User, MediaItem, Rating, etc.)
    ├── Services/                   # Business Services (Auth, Database, OMDb, Cache)
    ├── UI/                         # Styling & Controls
    │   ├── Theme.cs                # Design Tokens & Palette (Light/Dark)
    │   └── Controls/               # Custom Controls (PillButton, StarRating, etc.)
    └── Views/                      # Application Views & Dialogs
        ├── AdminView.cs            # Admin Management & Governance Portal
        ├── UserEditDialog.cs       # Admin User Create/Edit Modal
        ├── ExploreView.cs          # Curated Cinema & Categories
        ├── SearchView.cs           # Debounced OMDb Search
        ├── WatchlistView.cs        # User Watchlist Grid
        ├── ProfileView.cs          # User Profile & Stats
        ├── SettingsView.cs         # Settings & Dark Mode Switcher
        ├── MediaDetailDialog.cs    # Title Detail & Rating Dialog
        └── AuthDialog.cs           # Sign In / Sign Up Modal
```

---

## 📄 License
This project is for educational and portfolio demonstration purposes. All movie metadata and poster artwork are provided via the OMDb API. Design inspiration belongs to Starbucks Corporation.


# RateMe ★ — Complete Project Manual, Architecture & AI Runbook

> **Instructions for AI Agents & Developers:**  
> This file is the single source of truth for understanding, building, running, debugging, and testing the **RateMe** desktop project. When instructed to inspect and run this project, read this document in its entirety.

---

## 1. Project Overview & Tech Stack

| Specification | Details |
| :--- | :--- |
| **Application Type** | Windows Desktop Application (GUI) |
| **Language & Framework** | **C#**, **.NET Framework 4.7.2**, **Windows Forms (WinForms)** |
| **Primary IDE** | **Microsoft Visual Studio 2019** (Community, Professional, or Enterprise) |
| **Build Tool** | `MSBuild.exe` (v16.0 / Visual Studio 2019 Tools) |
| **Database Engine** | **Dual-Mode:** Microsoft SQL Server / LocalDB (`(localdb)\MSSQLLocalDB` or `.\SQLEXPRESS`) with automatic fallback to local JSON state storage (`rateme_data.json`) |
| **External API** | **OMDb API** (`http://www.omdbapi.com/`) for real-time cinema and TV metadata & poster art |
| **Default OMDb API Key**| `cf367844` |
| **Design System** | **Starbucks Flagship Retail Design System** (Tokens in `rate-me-design.md`) |
| **Repository URL** | [`https://github.com/umang4zq/RateMe.git`](https://github.com/umang4zq/RateMe.git) |

---

## 2. Quick Start: Build & Run in 60 Seconds

### A. One-Command Build & Run (PowerShell / Command Prompt)

To build and launch the application directly from the terminal without opening Visual Studio:

```powershell
# 1. Terminate any previous locked instance of RateMe
taskkill /F /IM RateMe.exe 2>$null

# 2. Build the solution using MSBuild (VS2019 Community path)
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe" RateMe.sln /p:Configuration=Debug /v:m

# 3. Launch the compiled executable
Start-Process ".\RateMe\bin\Debug\RateMe.exe"
```

*(Note for Visual Studio Professional/Enterprise installations: update path to `...\Professional\...` or `...\Enterprise\...` if Community is not present).*

### B. Running Inside Visual Studio 2019 GUI
1. Launch **Microsoft Visual Studio 2019**.
2. Open solution file: `RateMe.sln`.
3. Set configuration to **Debug** and platform to **Any CPU**.
4. Press **`F5`** (or click the green **Start** button).

---

## 3. Database Setup & Dual-Mode Storage Architecture

The application implements a resilient **Dual-Mode Database Service** (`RateMe/Services/DatabaseService.cs`):

### Mode 1: Microsoft SQL Server / LocalDB (Primary)
- The application automatically attempts to connect to:
  1. `Server=(localdb)\MSSQLLocalDB;Database=RateMeDb;Integrated Security=True;`
  2. `Server=.\SQLEXPRESS;Database=RateMeDb;Integrated Security=True;`
- **Schema & Migration Script:** [`RateMe.sql`](RateMe.sql)
  - Execute [`RateMe.sql`](RateMe.sql) in SQL Server Management Studio (SSMS) or Visual Studio SQL Server Object Explorer to create the `RateMeDb` database, schema tables (`Users`, `MediaItems`, `Ratings`, `Watchlist`, `UserSettings`), non-clustered indexes, views, and seed data.

### Mode 2: Local JSON Store (Zero-Config Fallback)
- If SQL Server is not installed or running, `DatabaseService` seamlessly catches the connection exception and falls back to a local JSON storage file (`rateme_data.json`) located in the application directory.
- On first launch without SQL, it automatically seeds all **25 curated cinema & series titles**, default administrator accounts, demo users, sample reviews, and watchlists.
- **Zero setup required to run and test.**

---

## 4. Default System Credentials

| Role | Username | Password | Purpose & Access Level |
| :--- | :--- | :--- | :--- |
| **Administrator** | `admin` | `Admin@123` | Unlocks the **Admin ⚙** portal (User CRUD, Catalog OMDb import, Review moderation). |
| **Demo User** | `cinema_fan` | `User@123` | Pre-configured regular user with rated titles and watchlist entries. |
| **Demo User 2** | `john_doe` | `User@123` | Standard user for testing registrations and ratings. |

---

## 5. Visual Identity & Starbucks Design System Tokens

The application strictly implements the design tokens documented in [`rate-me-design.md`](rate-me-design.md):

### Color Tokens
- **`#006241` (Starbucks Green):** Primary brand signal, headings, section titles, active states.
- **`#00754A` (Green Accent):** Primary filled pill buttons (`PillButtonStyle.PrimaryFilled`), floating CTA.
- **`#1E3932` (House Green):** Deep espresso green top navigation bar, feature hero band, dark surfaces.
- **`#CBA258` (Gold):** Interactive star ratings (1-10 stars), IMDb score badges, admin indicators.
- **`#F2F0EB` (Neutral Warm):** Primary page canvas in Light Mode (resembles café ceramic/paper).
- **`#12231F` (Dark Canvas):** Midnight dark mode page canvas.
- **`#C82014` (Destructive Red):** Delete buttons, warnings, and suspension toggles.

### Core Component Guidelines
- **50px Pill Buttons (`PillButton.cs`):** Universal 50px border-radius full pills with anti-aliased GDI+ rendering, active press tactile scale, and crisp parent-color clearing.
- **12px Rounded Cards (`RoundedCard.cs`):** Flat-plus-lift geometry with layered ambient drop shadows (`0 0 0.5px rgba(0,0,0,0.14)` + `0 1px 1px rgba(0,0,0,0.24)`).
- **Frap Floating Action Button (`FloatingFrapButton.cs`):** 56px circular floating action button pinned to bottom-right for quick search/rating.
- **Kinetic Scroll Panel (`KineticScrollPanel.cs`):** Spring damping physics with 60 FPS timer easing inspired by Framer Motion.
- **5 by 5 Grid Layout (`ExploreView.cs`):** `DoubleBufferedTableLayoutPanel` strictly locked to **5 columns** (each 20% width), rendering 5 cards per row across any screen size.

---

## 6. Full Project Structure & File Map

```
c:\Users\HP\source\repos\RateMe/
├── RateMe.sln                      # Visual Studio 2019 Solution File
├── RateMe.sql                      # Complete T-SQL schema, DDL, triggers, and seed script
├── rate-me-design.md               # Design System specification (Starbucks Flagship Retail)
├── tasks.md                        # Project requirements & task tracking checklist
├── PROJECT_MANUAL.md               # This document (Architecture & AI Runbook)
├── README.md                       # GitHub documentation & badge showcase
├── .gitignore                      # Git ignore rules for .NET / Visual Studio
│
└── RateMe/                         # Main Windows Forms Project
    ├── App.config                  # Application configuration & connection strings
    ├── Program.cs                  # Application entry point (Main)
    ├── Form1.cs                    # Main Shell Form (Top Nav, View Swapping, Auth UI)
    ├── Form1.Designer.cs           # Windows Forms Designer layout code
    ├── RateMe.csproj               # C# Project File (.NET Framework 4.7.2)
    │
    ├── Models/                     # Data Transfer Objects & Entities
    │   ├── User.cs                 # User entity & AdminStats model
    │   ├── MediaItem.cs            # Film & TV series entity
    │   ├── Rating.cs               # User review & score model
    │   ├── WatchlistItem.cs        # Watchlist tracking entity
    │   └── OmdbModels.cs           # OMDb API JSON response models
    │
    ├── Services/                   # Business Logic & Infrastructure
    │   ├── AuthService.cs          # PBKDF2 / SHA-256 (10,000 iterations) + 128-bit Salt hashing
    │   ├── DatabaseService.cs      # Dual-mode SQL Server / LocalDB & JSON database service
    │   ├── OmdbService.cs          # HttpClient OMDb search & title detail retrieval
    │   ├── ImageCache.cs           # Async poster downloader, memory cache & fallback cards
    │   └── SeedData.cs             # 25 curated films & series with live OMDb poster URLs
    │
    ├── UI/                         # Styling & Reusable Custom Controls
    │   ├── Theme.cs                # Design Tokens, Palette, Typography & Rounded Path Helpers
    │   └── Controls/
    │       ├── PillButton.cs       # 50px pill button with GDI+ anti-aliased rendering
    │       ├── ModernTextBox.cs    # Native Windows EM_SETCUEBANNER placeholder text box
    │       ├── RoundedCard.cs      # 12px card container with whisper drop shadows
    │       ├── StarRatingControl.cs# Interactive 1-10 Gold Star selector with hover feedback
    │       ├── RatingBadge.cs      # Score pill badge (Gold / Mint)
    │       ├── FloatingFrapButton.cs# 56px circular floating action button (Frap CTA)
    │       ├── KineticScrollPanel.cs# Spring damping kinetic scroll panel
    │       └── ToastNotification.cs# Animated toast alert banner (Success / Error)
    │
    └── Views/                      # Screen Views & Dialog Forms
        ├── ExploreView.cs          # 5 by 5 Grid media showcase with category filters
        ├── SearchView.cs           # Debounced OMDb instant live movie search
        ├── WatchlistView.cs        # Personal watchlist with Plan/Watching/Completed filters
        ├── ProfileView.cs          # User profile stats (rated count, reviews, bio editor)
        ├── SettingsView.cs         # Settings view, API key configuration, Light/Dark toggle
        ├── AdminView.cs            # Admin Governance Portal (User CRUD, Catalog, Moderation)
        ├── AuthDialog.cs           # Dedicated Sign In & Create Account modal dialog
        ├── UserEditDialog.cs       # Admin User Create/Edit modal dialog
        └── MediaDetailDialog.cs    # Title detail view, plot, cast, and rating submit form
```

---

## 7. Features & Functionality Verification Guide

When testing or verifying the application, run through this feature checklist:

### 1. Navigation Bar (Top Nav)
- **Brand Logo:** `RateMe ★` clicks through to the `Explore` view.
- **Main Public Tabs:** `Explore`, `Search`, `Watchlist`, `Profile`, and `Settings`.
  - Inactive tabs show **crisp white text** on House Green (`#1E3932`) with hover feedback.
  - Active tab highlights with a **solid white pill** and green text.
- **Separated Admin Button:** `Admin ⚙` is located on the right side of the navigation bar.
- **Auth Action Buttons:**
  - When logged out: shows dedicated **`Sign In`** (white outlined) and **`Join now`** (white filled) buttons.
  - When logged in: shows user profile pill (`👤 Display Name`) and **`Exit`** (sign out) button.

### 2. 5 by 5 Curated Media Grid (`ExploreView.cs`)
- **Strict 5 Columns:** The media cards are placed in a `DoubleBufferedTableLayoutPanel` with 5 columns (each 20% width).
- **25 Acclaimed Titles:**
  - 15 Movies: *Inception, The Dark Knight, Interstellar, The Shawshank Redemption, The Matrix, Oppenheimer, Pulp Fiction, Fight Club, Forrest Gump, The Godfather, The Lord of the Rings: Fellowship of the Ring, Dune: Part Two, Gladiator, Spirited Away, Spider-Man: Across the Spider-Verse*.
  - 10 Series: *Breaking Bad, Stranger Things, Game of Thrones, Chernobyl, Better Call Saul, The Sopranos, Severance, Succession, The Last of Us, Peaky Blinders*.
- **Filter Tabs:**
  - `All Releases`: Full 5 by 5 grid (25 titles).
  - `Top Rated ★`: Full 5 by 5 grid sorted by community rating.
  - `Movies`: 5 columns × 3 rows (15 titles).
  - `TV Series`: 5 columns × 2 rows (10 titles).

### 3. Authentication & Security (`AuthDialog.cs`, `AuthService.cs`)
- Passwords are encrypted with **PBKDF2** using **HMAC-SHA-256**, 10,000 iterations, and a cryptographically secure 128-bit salt (`RNGCryptoServiceProvider`).
- Dedicated modes:
  - **Sign In:** Shows `USERNAME OR EMAIL` and `PASSWORD` fields, plus a quick demo login button (`fan@rateme.app`).
  - **Create Account:** Shows `CHOOSE USERNAME`, `EMAIL ADDRESS`, `DISPLAY NAME`, and `PASSWORD (MIN. 6 CHARACTERS)`.
  - All input fields have visible uppercase labels and native Windows placeholder text.

### 4. Interactive Rating & Review (`MediaDetailDialog.cs`)
- Click any movie/series card to open the detail dialog.
- Shows high-resolution poster, director, cast, synopsis, runtime, Metascore, and IMDb rating.
- **1-10 Gold Star Selector:** Hover to preview rating labels (*"Masterpiece"*, *"Great"*, *"Good"*, etc.), click to select score.
- Submit a written review; aggregate scores recalculate in real-time.
- One-click **Add to Watchlist / In Watchlist** toggle button.

### 5. Debounced OMDb Live Search (`SearchView.cs`)
- Type any movie or TV series into the search box (e.g. *"Avatar"*, *"Batman"*, *"Star Wars"*).
- Debounces input by 500ms and queries the live OMDb API (`cf367844`).
- Results display real-time posters, metadata, and rating scores with click-to-rate capability.

### 6. Administration & Governance Portal (`AdminView.cs`)
- Click `Admin ⚙` on the navigation bar. If not signed in as an admin, enter `admin` / `Admin@123`.
- **System Metrics Bar:** Real-time counters for Total Users, Catalog Items, Community Reviews, and Database Engine status.
- **SubTab 1: User Management (CRUD):**
  - Instant search filter (by username, email, display name) + role filter pills (`All Roles`, `Admins`, `Users`).
  - `+ Add New User`: Provisions accounts with hashed passwords and role selection.
  - `Edit ✎`: Modifies display name, bio, role, and active status.
  - `Reset Pwd 🔑`: Direct password reset modal.
  - `Suspend ⊘` / `Activate ✓`: Account toggle.
  - `Delete ✕`: Permanent removal with self-deletion safety block.
- **SubTab 2: Catalog Governance & OMDb Import:**
  - Import any film or series by IMDb ID (e.g. `tt0137523` for *Fight Club*, `tt0109830` for *Forrest Gump*).
  - Delete titles from the catalog.
- **SubTab 3: Community Review Moderation:**
  - Lists all community reviews with rating badge, author, and timestamp.
  - `Remove Review 🗑` moderation button.

---

## 8. Common Troubleshooting & AI Gotchas

| Issue | Cause | Resolution |
| :--- | :--- | :--- |
| **`MSB3027 / MSB3021: Unable to copy file ... RateMe.exe locked by another process`** | Visual Studio 2019 or a previous background run has a file lock on `RateMe.exe`. | Run `taskkill /F /IM RateMe.exe` in terminal before compiling. |
| **Pill button white rectangle corners** | Placing custom `Control` inside transparent WinForms containers. | Always set `_flowNavTabs.BackColor = Theme.HouseGreen;` and let `PillButton` walk up the control tree to call `g.Clear(parentBg)`. |
| **Blank input boxes without placeholder text** | WinForms `TextBox` child control drawing on top of parent `OnPaint`. | Use native Windows `EM_SETCUEBANNER` (`SendMessage(handle, 0x1501, 1, placeholder)`) and provide explicit visible `Label` controls above every input. |
| **SQL Server connection failure** | Local SQL instance not started or LocalDB not created. | The application automatically falls back to local JSON storage (`rateme_data.json`), so no manual intervention is required. |

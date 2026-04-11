# Sprout 🌱

A social media platform with a calm, botanical aesthetic. Share your thoughts, follow people you care about, and grow your community — one sprout at a time.

---

## Theme & Design

Sprout uses an **organic-minimal naturalist** design language — think a social app that feels like a morning walk through a garden rather than a busy city street.

| Token | Value | Usage |
|---|---|---|
| Cream | `#F1F3E0` | Page background |
| Sage Light | `#D2DCB6` | Borders, hover states |
| Sage Mid | `#A1BC98` | Avatars, secondary UI |
| Sage Dark | `#778873` | CTAs, active nav, primary text |
| Terracotta | `#e07a5f` | Likes, notifications (warm accent) |

- **Font:** Nunito (400–900 weight) — rounded, friendly, readable
- **Radius:** 3-step system — pill buttons, card panels, element inputs
- **Layout:** 3-column app layout (left sidebar 240px · main feed · right sidebar 280px)
- **Auth pages:** 2-panel card design with blurred sage blob backgrounds

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 10) |
| Language | C# |
| Database | PostgreSQL (hosted on [Neon](https://neon.tech)) |
| ORM | Entity Framework Core |
| Auth | ASP.NET Core Identity |
| UI Components | [DuneUI](https://www.nuget.org/packages/DuneUI) |
| Architecture | Repository + Unit of Work pattern |

---

## Features

### Live
- **Sign Up / Log In / Log Out** — full Identity-based auth with secure cookie sessions
- **Home Feed** — view all posts sorted by newest first
- **Create Post (Sprout)** — share what's on your mind
- **Repost** — reshare posts from others
- **Delete Post** — remove your own sprouts
- **Comments** — reply to any post
- **Likes** — like posts and comments
- **Profile Dropdown** — navbar avatar opens a menu with View Profile and Log Out

### In Progress / Planned
- **Profile Page** — dynamic user profile with post history, follower/following counts
- **Settings** — edit your name, bio, location, profile image
- **Like Toggle** — unlike posts (currently increment-only)
- **Follow System** — follow/unfollow users, update follower counts
- **Notifications** — real-time alerts for likes, follows, reposts, comments
- **Messages** — direct conversations between users
- **Search** — find posts and users by keyword
- **Bookmarks** — save posts to read later
- **Explore** — discover trending content and suggested users

---

## Project Structure

```
Sprout/
├── Controllers/          # MVC controllers (Home, Login, SignUp, Profile, etc.)
├── Models/
│   ├── DBModels/         # Entity models (Post, Comment, Like, Follow, etc.)
│   └── ViewModels/       # View-specific models (PostViewModel, ProfileViewModel, etc.)
├── Views/
│   ├── Home/             # Feed page
│   ├── Login/            # Sign in page
│   ├── SignUp/           # Registration page
│   ├── Profile/          # User profile page
│   ├── Settings/         # Account settings page
│   ├── Notifications/    # Notifications page
│   ├── Messages/         # Direct messages page
│   └── Shared/           # _Navbar, _SidebarLeft, _SidebarRight partials
├── Services/             # Business logic (PostService, CommentService, LikeService)
├── Repository/           # Generic EFRepository + ApplicationDbContext
├── UnitOfWork/           # IUnitOfWork — lazy-loads all repositories
├── Migrations/           # EF Core migration history
└── wwwroot/              # Static assets (CSS, JS, fonts)
```

---

## Data Models

```
ApplicationUser (Identity)
    └── UserProfile (1:1)
            ├── Posts (1:N)
            │     └── Comments (1:N, threaded)
            ├── Likes (1:N) — on Posts or Comments
            ├── Follows (self-referencing)
            ├── Notifications (recipient + actor)
            ├── Bookmarks (1:N)
            └── ConversationParticipants → Conversations → Messages
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A PostgreSQL database (or a free [Neon](https://neon.tech) project)

### Setup

1. **Clone the repo**
   ```bash
   git clone https://github.com/Utchash007/Spout.git
   cd Spout
   ```

2. **Add your database connection**

   Create a `.env` file in the project root:
   ```
   pgConn="Host=...;Database=...;Username=...;Password=...;SSL Mode=VerifyFull;"
   ```

3. **Apply migrations**
   ```bash
   dotnet ef database update
   ```

4. **Run**
   ```bash
   dotnet run
   ```
   App runs at `https://localhost:7233` / `http://localhost:5274`

---

## Architecture Notes

Two valid call paths are used throughout the project:

```
Controller → UnitOfWork → EFRepository<T> → DbContext → PostgreSQL
Controller → Service → UnitOfWork → EFRepository<T> → DbContext → PostgreSQL
```

The **Service layer** handles complex business logic (e.g. creating a post, toggling a like). Simple CRUD goes directly through UnitOfWork. All repositories are lazy-loaded and share a single `DbContext` per request.

---

## License

MIT

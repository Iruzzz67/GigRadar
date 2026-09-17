# GIGRADAR_APP_SPEC.md

> **Dokumen tunggal (single source of truth) untuk konsep produk GigRadar.**
> Semua dokumen konsep lama — `GIGRADAR_ROLE_SYSTEM.md`, `GIGRADAR_MOBILE_APP_NET_MAUI.md`,
> `GIGRADAR_DESIGN_SYSTEM.md`, `GIGRADAR_MULTIPLATFORM_FIX.md`, `GPS_LOCATION.md`,
> `REKAPAN_PROJECT.md`, `REKAPAN_PROJECT_TERBARU.md`, `GigRadar android.md`,
> `migrate sqlite to postgre.md` — telah dilebur ke dalam spec ini (Lampiran A–K)
> dan dihapus dari root agar folder proyek tetap bersih.
> Tidak ada konten yang hilang: bagian yang tidak tercakup di Bab 1–84 dilestariakan di lampiran.

# GigRadar
## Music Ecosystem, Artist Social Platform, Event Organizer & Ticketing

**Platform:** .NET MAUI  
**Client:** Android, Windows, iOS, macOS  
**Backend:** ASP.NET Core Web API  
**ORM:** Entity Framework Core  
**Database:** PostgreSQL (produksi & development — Npgsql + EF Core Migrations; lihat Lampiran F). Deviasi lama ke SQLite sudah diakhiri di branch `feat/postgresql-migration`.  
**Architecture:** MVVM + Service Layer + REST API  
**Authentication:** JWT + Refresh Token  
**Core Roles:** USER, ARTIST, EO, ADMIN

---

# 1. Product Vision

GigRadar adalah platform ekosistem musik yang menghubungkan:

- Penikmat musik (User)
- Artist / Band
- Event Organizer (EO)
- Admin platform

GigRadar tidak hanya berfungsi sebagai pencari gig. Platform mencakup:

1. Music discovery
2. Artist social platform
3. Event discovery
4. Event management
5. Ticketing
6. Artist verification
7. EO verification
8. Artist merchandise
9. Social interaction
10. Platform moderation
11. Recommendation
12. Analytics

## Core Concept

```text
                         GIGRADAR
                            |
        +-------------------+-------------------+
        |                   |                   |
       USER              ARTIST                 EO
        |                   |                   |
        |             Social Profile       Event Management
        |             Music / Release       Ticketing
        |             Gig Activity          Portfolio
        |             Merchandise           Event Promotion
        |                   |                   |
        +-------------------+-------------------+
                            |
                          ADMIN
                            |
              Verification + Moderation
              Account + Event Management
              Ticket + Platform Monitoring
```

## Product Positioning

> GigRadar adalah platform ekosistem musik lokal yang mempertemukan penikmat musik, artist, dan event organizer melalui discovery musik, social feed artist, event management, ticketing, merchandise, dan sistem verifikasi terpercaya.

---

# 2. Product Principles

GigRadar harus:

- Music-first
- Local-community focused
- Artist friendly
- EO friendly
- User friendly
- Verification-based
- Secure
- Scalable
- Multiplatform
- Tidak terasa seperti clone Spotify, Instagram, atau Eventbrite
- Tidak menggunakan UI generik yang terasa seperti template AI

GigRadar harus memiliki identitas visual sendiri.

---

# 3. Role System

GigRadar memiliki tepat 4 role utama:

```text
USER
ARTIST
EO
ADMIN
```

Role harus dikontrol oleh backend.

Client MAUI tidak boleh menjadi sumber kebenaran untuk permission.

```text
MAUI Client
    |
    | JWT
    v
ASP.NET Core API
    |
    +-- Role
    +-- Permission
    +-- Verification
    +-- Ownership
```

---

# 4. USER ROLE

## Tujuan

User adalah penikmat musik dan pengunjung event.

## Fitur

### Authentication

- Register
- Login
- Logout
- Refresh token
- Forgot password
- Change password
- Session management

### Profile

- Profile photo
- Username
- Display name
- Bio
- Location
- Favorite genres
- Followed artists

### Discovery

- Discover gigs
- Discover artists
- Discover venues
- Search event
- Search artist
- Search venue
- Nearby events
- Recommended events
- Upcoming events
- New releases

### Social

- Follow artist
- Unfollow artist
- Like post
- Comment
- View artist profile
- View artist releases
- View artist gigs
- View merchandise

### Ticketing

- View ticket types
- Buy ticket
- View order
- View ticket
- QR ticket
- Ticket history
- Event reminder

---

# 5. USER NAVIGATION

```text
UserShell

Home
Discover
Following
Tickets
Profile
```

## Home

Content:

- Search
- Nearby gigs
- Recommended gigs
- Followed artist activity
- New releases
- Popular events
- Music feed

## Discover

Tabs:

- Events
- Artists
- Venues

## Following

- Followed artists
- Upcoming gigs
- Recent artist posts

## Tickets

- Active tickets
- Upcoming events
- Ticket history

## Profile

- Account information
- Music preferences
- Location
- Followed artists
- Order history
- Settings

---

# 6. ARTIST ROLE

Artist adalah musisi solo, band, group, atau music act.

Artist memiliki social profile seperti platform sosial khusus musik.

## Artist Features

### Profile

- Artist name
- Profile photo
- Cover image
- Bio
- Location
- Genre
- Members
- Social links
- Verification badge
- Followers
- Following

### Social Feed

Artist dapat membuat:

- Daily update
- Gig activity
- Photo
- Video
- New song announcement
- New album announcement
- Merchandise promotion
- General announcement

### Music

- Songs
- Albums
- EP
- Cover artwork
- Release date
- Genre
- Streaming links

### Gig

- Upcoming gigs
- Gig history
- Event invitations
- Accept invitation
- Reject invitation
- Event information

### Merchandise

- Product catalog
- Product image
- Product name
- Description
- Price
- Stock
- External purchase link / internal order system

### Analytics

- Profile views
- Followers
- Follower growth
- Post engagement
- Gig interest
- Release engagement
- Merchandise views

---

# 7. ARTIST REGISTRATION

Artist tidak boleh langsung mendapatkan status verified.

Flow:

```text
Register
    |
    v
Choose Artist
    |
    v
Artist Profile Setup
    |
    v
Connect Spotify (optional/recommended)
    |
    v
Add supporting information
    |
    v
Submit Verification
    |
    v
ADMIN REVIEW
    |
    +---- APPROVED
    |
    +---- REJECTED
```

## Artist Application

Fields:

- Artist name
- Legal/representative name
- Username
- Email
- Phone
- Location
- Genre
- Bio
- Profile photo
- Cover photo
- Spotify link/account connection
- Social media links
- Music links
- Supporting information

Spotify data is supporting evidence, not automatic proof of identity.

---

# 8. ARTIST VERIFICATION

## Verification Status

```text
NOT_SUBMITTED
PENDING
UNDER_REVIEW
VERIFIED
REJECTED
```

## Account Status

```text
ACTIVE
SUSPENDED
BANNED
```

These statuses must be stored separately.

## Verification Rules

- User cannot impersonate a verified artist.
- Only Admin can approve Artist verification.
- Backend must enforce verification permissions.
- Verification decision must be recorded in AuditLogs.
- Rejected applications may be resubmitted according to platform rules.

## Verified Badge

Verified Artist:

```text
Artist Name ✓
```

Unverified Artist:

```text
Artist Name
```

---

# 9. SPOTIFY INTEGRATION

Artist registration may support Spotify account connection.

## Flow

```text
Artist
    |
    v
Connect Spotify
    |
    v
Spotify Authorization
    |
    v
Authorized data
    |
    v
GigRadar Artist Profile
```

Use Spotify OAuth and only request required scopes/data.

Spotify connection must not automatically make an Artist verified.

The Admin remains responsible for verification.

---

# 10. ARTIST SOCIAL PROFILE

Artist profile should visually behave like a music-focused social network.

```text
+--------------------------------+
|          COVER IMAGE           |
|                                |
|       ARTIST PROFILE PHOTO     |
|                                |
|       BAND / ARTIST NAME ✓     |
|       Rock • Depok             |
|                                |
|       [Follow] [Share]         |
+--------------------------------+
| Followers | Releases | Gigs    |
+--------------------------------+
| Posts | Music | Gigs | Shop    |
+--------------------------------+
|                                |
|           POST FEED            |
|                                |
+--------------------------------+
```

Avoid excessive:

- Glassmorphism
- Gradients
- Floating cards
- Animations
- Decorative 3D objects
- Neon effects everywhere

Content hierarchy must remain primary.

---

# 11. ARTIST POST TYPES

```text
DAILY_UPDATE
GIG_ACTIVITY
PHOTO
VIDEO
NEW_SONG
NEW_ALBUM
MERCHANDISE
ANNOUNCEMENT
```

Each post may contain:

- Text
- Image
- Video reference
- Music reference
- Event reference
- Merchandise reference

Interactions:

- Like
- Comment
- Share
- Save (optional)

---

# 12. EO ROLE

EO means Event Organizer.

EO is responsible for organizing and managing events.

## EO Features

### Organizer Profile

- Organization name
- Logo
- Description
- Location
- Contact
- Social links
- Website
- Verification badge

### Events

- Create event
- Edit event
- Delete draft
- Publish
- Cancel
- Manage lineup
- Manage venue
- Manage tickets
- Monitor ticket sales
- View event statistics

### Artist Management

- Search artists
- Invite artist
- View artist profile
- Track invitation
- Confirm lineup

### Analytics

- Event views
- Ticket sales
- Revenue
- Attendance
- Conversion
- Artist lineup
- Event performance

---

# 13. EO REGISTRATION

EO registration must use verification.

```text
Register
    |
    v
Choose EO
    |
    v
Organizer Profile
    |
    v
Portfolio
    |
    v
Submit Verification
    |
    v
ADMIN REVIEW
    |
    +---- APPROVED
    |
    +---- REJECTED
```

---

# 14. EO PORTFOLIO

EO can submit previous event work as portfolio evidence.

## Portfolio Fields

- Organization name
- Description
- Logo
- Founded year
- Location
- Website
- Social media
- Contact
- Previous events
- Event posters
- Event photos
- Event documentation links
- Event descriptions
- Supporting documents

## Portfolio Structure

```text
OrganizerPortfolio
    |
    +-- PortfolioItem
            |
            +-- Event name
            +-- Date
            +-- Venue
            +-- Description
            +-- Poster
            +-- Photos
            +-- Video/documentation link
```

---

# 15. EO VERIFICATION

## Status

```text
NOT_SUBMITTED
PENDING
UNDER_REVIEW
VERIFIED
REJECTED
```

Only Admin can verify EO.

Verified EO receives:

```text
Organizer Name ✓
```

Backend must prevent unverified EO accounts from accessing protected organizer capabilities where verification is required.

---

# 16. ADMIN ROLE

Admin is the platform management and moderation role.

Admin interface should be a dedicated management dashboard.

## Admin Responsibilities

- Account management
- Artist verification
- EO verification
- Event monitoring
- Ticket monitoring
- Content moderation
- Reports
- Audit logs
- Platform statistics
- User management
- Artist management
- EO management

---

# 17. ADMIN NAVIGATION

```text
AdminShell

Dashboard
Users
Artists
Organizers
Verification
Events
Tickets
Reports
Audit Logs
Settings
```

---

# 18. ADMIN DASHBOARD

Display:

```text
Total Users
Total Artists
Verified Artists
Total EO
Verified EO
Total Events
Active Events
Tickets Sold
Pending Verification
Reports
```

Optional charts:

- User growth
- Artist growth
- EO growth
- Event creation
- Ticket sales
- Revenue overview

---

# 19. ADMIN USER CRUD

Admin can:

- Create account
- Read account
- Update account
- Delete account
- Suspend account
- Activate account
- View profile
- View account status
- View role
- View creation date

## CRUD

```text
CREATE
READ
UPDATE
DELETE
```

Deletion should preferably use soft delete for important platform records.

Example:

```text
IsDeleted
DeletedAt
DeletedBy
```

---

# 20. ADMIN ARTIST MANAGEMENT

Admin can:

- View all Artists
- Search Artist
- Filter by verification status
- View Artist profile
- View Artist music
- View Artist posts
- View Artist gigs
- View verification application
- Approve
- Reject
- Suspend
- Reactivate
- Edit administrative data
- Delete/deactivate account

---

# 21. ADMIN EO MANAGEMENT

Admin can:

- View all EO
- Search EO
- Filter verification
- View organizer profile
- View portfolio
- View events
- View ticket activity
- Approve
- Reject
- Suspend
- Reactivate
- Edit administrative data
- Delete/deactivate account

---

# 22. ADMIN VERIFICATION CENTER

Dedicated page:

```text
Verification Center

Artist Applications
    Pending: 12

Organizer Applications
    Pending: 7
```

## Review Detail

```text
Applicant
Profile
Identity/Representative Information
Spotify Connection
Social Links
Music Links
Portfolio
Previous Events
Supporting Information
Verification History

[Approve]
[Reject]
```

Every decision must create an AuditLog.

---

# 23. EVENT SYSTEM

Event is a core platform entity.

## Event Fields

- EventId
- OrganizerId
- Name
- Description
- Poster
- StartDate
- EndDate
- VenueId
- Status
- MinimumAge (if applicable)
- Terms
- CreatedAt
- UpdatedAt

## Event Status

```text
DRAFT
PENDING_REVIEW (optional)
PUBLISHED
SOLD_OUT
ONGOING
COMPLETED
CANCELLED
```

---

# 24. EVENT CREATION

EO flow:

```text
Create Event
    |
    v
Basic Information
    |
    v
Venue
    |
    v
Lineup
    |
    v
Ticket Types
    |
    v
Save Draft
    |
    v
Publish
```

If platform moderation is enabled:

```text
Draft
  |
  v
Submit Review
  |
  v
Admin Review
  |
  +-- Approved --> Published
  |
  +-- Rejected --> Draft
```

---

# 25. EVENT LINEUP

Relationship:

```text
Event
  |
  +-- EventArtist
          |
          +-- Artist
          +-- Performance order
          +-- Set time
          +-- Invitation status
```

EO can invite Artists.

Artist can:

- Accept
- Reject
- View event details

---

# 26. EVENT INVITATION

```text
EO
 |
 | invitation
 v
Artist
 |
 +-- Accept
 |
 +-- Reject
```

Statuses:

```text
PENDING
ACCEPTED
REJECTED
CANCELLED
```

---

# 27. VENUE

Venue fields:

- VenueId
- Name
- Address
- City
- Latitude
- Longitude
- Description
- Capacity
- Image
- Contact

Venue can be used for:

- Map
- Event detail
- Nearby discovery
- Distance calculation

---

# 28. TICKETING

Ticket types:

```text
EARLY_BIRD
PRESALE
REGULAR
VIP
FESTIVAL_PASS
OTHER
```

TicketType fields:

- TicketTypeId
- EventId
- Name
- Description
- Price
- Stock
- Sold
- SaleStart
- SaleEnd
- Status

---

# 29. TICKET PURCHASE FLOW

```text
User
  |
  v
Event Detail
  |
  v
Choose Ticket
  |
  v
Order
  |
  v
Payment
  |
  v
Payment Confirmation
  |
  v
Ticket Generated
  |
  v
QR Ticket
```

Payment integration must be handled by the backend.

Never trust payment status supplied by the MAUI client.

---

# 30. TICKET MONITORING

Admin can view:

- Event
- Organizer
- Ticket type
- Price
- Stock
- Sold
- Remaining
- Order status
- Payment status
- Ticket status

EO can view their own event ticket data.

User can view only their own tickets.

---

# 31. MERCHANDISE

Artist can create products.

## Merchandise

Fields:

- MerchandiseId
- ArtistId
- Name
- Description
- Image
- Price
- Stock
- ExternalPurchaseUrl
- Status
- CreatedAt

## MVP

Use external purchase/contact link if a full marketplace is not yet required.

Later:

```text
Product
   |
   v
Cart
   |
   v
Order
   |
   v
Payment
   |
   v
Fulfillment
```

---

# 32. SOCIAL SYSTEM

Core entities:

```text
Posts
PostLikes
Comments
Follows
```

User can follow Artist.

Artist can publish content.

User can interact with Artist content.

Admin can moderate reported content.

---

# 33. DISCOVERY

User can discover:

- Events
- Artists
- Venues
- Releases
- Posts

Search supports:

```text
Artist
Event
Venue
Genre
Location
```

Example:

```text
punk depok
```

---

# 34. LOCATION SYSTEM

Location is used for event discovery.

Features:

- Nearby events
- Distance from user
- Map view
- Venue location
- Search by city

MAUI should request location permission only when required.

Do not continuously track location unless a future feature explicitly needs it.

---

# 35. RECOMMENDATION ENGINE

Initial recommendation should use deterministic weighted scoring.

```text
Genre Match       30%
Distance          25%
Followed Artist   20%
Past Interaction  10%
Popularity        10%
Date Relevance     5%
```

Formula:

```text
RecommendationScore =
    GenreScore * 0.30
    + DistanceScore * 0.25
    + FollowScore * 0.20
    + InteractionScore * 0.10
    + PopularityScore * 0.10
    + DateScore * 0.05
```

Do not start with complex ML.

ML can be introduced after sufficient interaction data exists.

---

# 36. NOTIFICATION SYSTEM

Types:

```text
NEW_GIG_NEARBY
FOLLOWED_ARTIST_NEW_GIG
ARTIST_NEW_RELEASE
ARTIST_NEW_POST
EVENT_REMINDER
TICKET_PURCHASE
TICKET_UPDATE
EVENT_CANCELLED
ARTIST_INVITATION
ARTIST_VERIFICATION
EO_VERIFICATION
ADMIN_NOTICE
```

---

# 37. REPORTING & MODERATION

Users should be able to report:

- Artist
- EO
- Event
- Post
- Comment
- Merchandise

Report categories:

```text
SPAM
IMPERSONATION
FRAUD
INAPPROPRIATE_CONTENT
MISLEADING_INFORMATION
OTHER
```

Admin can:

- View reports
- Investigate
- Resolve
- Dismiss
- Suspend account
- Remove content
- Add moderation note

---

# 38. AUDIT LOG

Important Admin actions must be recorded.

Examples:

```text
ADMIN_APPROVED_ARTIST
ADMIN_REJECTED_ARTIST
ADMIN_APPROVED_EO
ADMIN_REJECTED_EO
ADMIN_UPDATED_USER
ADMIN_DELETED_USER
ADMIN_SUSPENDED_ACCOUNT
ADMIN_UPDATED_EVENT
ADMIN_CANCELLED_EVENT
```

Fields:

- AuditLogId
- AdminUserId
- Action
- EntityType
- EntityId
- Description
- CreatedAt
- IP/device metadata where appropriate

Never log passwords or access tokens.

---

# 39. DATABASE MODEL

Recommended entities:

```text
Users
Roles
UserRoles
RefreshTokens
UserPreferences

Artists
ArtistMembers
ArtistGenres
ArtistVerifications

Organizers
OrganizerVerifications
OrganizerPortfolios
PortfolioItems

Genres

Songs
Albums

Posts
PostLikes
Comments
Follows

Venues

Events
EventArtists
EventInvitations

TicketTypes
Orders
OrderItems
Tickets

Merchandise
MerchandiseOrders

Notifications

Reports
AuditLogs
```

---

# 40. USER MODEL

```csharp
public class User
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? ProfileImageUrl { get; set; }

    public string? Bio { get; set; }

    public string? Location { get; set; }

    public string Role { get; set; } = "User";

    public string AccountStatus { get; set; } = "ACTIVE";

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
```

---

# 41. ARTIST MODEL

```csharp
public class Artist
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string ArtistName { get; set; } = string.Empty;

    public string? Bio { get; set; }

    public string? ProfileImageUrl { get; set; }

    public string? CoverImageUrl { get; set; }

    public string? Location { get; set; }

    public string VerificationStatus { get; set; } = "NOT_SUBMITTED";

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
```

---

# 42. ORGANIZER MODEL

```csharp
public class Organizer
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string OrganizationName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LogoUrl { get; set; }

    public string? Location { get; set; }

    public string? WebsiteUrl { get; set; }

    public string VerificationStatus { get; set; } = "NOT_SUBMITTED";

    public bool IsVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
```

---

# 43. EVENT MODEL

```csharp
public class Event
{
    public Guid Id { get; set; }

    public Guid OrganizerId { get; set; }

    public Guid VenueId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? PosterUrl { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
```

---

# 44. FOLLOW MODEL

```csharp
public class Follow
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid ArtistId { get; set; }

    public DateTime CreatedAt { get; set; }
}
```

Unique constraint:

```text
UserId + ArtistId
```

---

# 45. POST MODEL

```csharp
public class Post
{
    public Guid Id { get; set; }

    public Guid ArtistId { get; set; }

    public string Type { get; set; } = "GENERAL";

    public string Content { get; set; } = string.Empty;

    public string? MediaUrl { get; set; }

    public Guid? EventId { get; set; }

    public Guid? MerchandiseId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
```

---

# 46. DATABASE RELATIONSHIP

```text
User
 |
 +---- Artist
 |       |
 |       +---- Songs
 |       +---- Albums
 |       +---- Posts
 |       +---- Merchandise
 |       +---- ArtistVerification
 |
 +---- Organizer
         |
         +---- Events
         +---- Portfolio
         +---- OrganizerVerification

User
 |
 +---- Follows ----> Artist
 |
 +---- Orders -----> Event
 |
 +---- Comments ----> Posts

Event
 |
 +---- Organizer
 +---- Venue
 +---- EventArtists
 +---- TicketTypes
```

---

# 47. API ARCHITECTURE

```text
.NET MAUI
    |
    v
REST API
    |
    v
ASP.NET Core
    |
    +-- Authentication
    +-- Authorization
    +-- Users
    +-- Artists
    +-- Organizers
    +-- Verification
    +-- Events
    +-- Tickets
    +-- Posts
    +-- Merchandise
    +-- Recommendations
    +-- Notifications
    +-- Reports
    |
    v
EF Core
    |
    v
Database
```

---

# 48. API ENDPOINTS

## Authentication

```http
POST /api/auth/register
POST /api/auth/login
POST /api/auth/logout
POST /api/auth/refresh
POST /api/auth/forgot-password
POST /api/auth/reset-password
```

## Users

```http
GET /api/users/me
PUT /api/users/me
GET /api/users/me/preferences
PUT /api/users/me/preferences
```

## Artists

```http
GET /api/artists
GET /api/artists/{id}
POST /api/artists
PUT /api/artists/{id}
DELETE /api/artists/{id}

POST /api/artists/{id}/follow
DELETE /api/artists/{id}/follow
```

## Artist Verification

```http
POST /api/artists/verification
GET /api/artists/verification/status
```

## Spotify

```http
GET /api/integrations/spotify/connect
GET /api/integrations/spotify/callback
DELETE /api/integrations/spotify/disconnect
```

## Organizer

```http
GET /api/organizers
GET /api/organizers/{id}
POST /api/organizers
PUT /api/organizers/{id}
DELETE /api/organizers/{id}
```

## Organizer Verification

```http
POST /api/organizers/verification
GET /api/organizers/verification/status
POST /api/organizers/portfolio
PUT /api/organizers/portfolio/{id}
DELETE /api/organizers/portfolio/{id}
```

## Events

```http
GET /api/events
GET /api/events/{id}
POST /api/events
PUT /api/events/{id}
DELETE /api/events/{id}
POST /api/events/{id}/publish
POST /api/events/{id}/cancel
```

## Event Discovery

```http
GET /api/events/nearby
GET /api/events/search
GET /api/events/recommended
```

## Event Invitations

```http
POST /api/events/{id}/invitations
POST /api/invitations/{id}/accept
POST /api/invitations/{id}/reject
```

## Posts

```http
GET /api/artists/{id}/posts
POST /api/artists/me/posts
PUT /api/posts/{id}
DELETE /api/posts/{id}

POST /api/posts/{id}/like
DELETE /api/posts/{id}/like

GET /api/posts/{id}/comments
POST /api/posts/{id}/comments
```

## Music

```http
GET /api/artists/{id}/songs
POST /api/artists/me/songs
PUT /api/songs/{id}
DELETE /api/songs/{id}

GET /api/artists/{id}/albums
POST /api/artists/me/albums
PUT /api/albums/{id}
DELETE /api/albums/{id}
```

## Merchandise

```http
GET /api/artists/{id}/merchandise
POST /api/artists/me/merchandise
PUT /api/merchandise/{id}
DELETE /api/merchandise/{id}
```

## Tickets

```http
GET /api/events/{id}/tickets
POST /api/orders
GET /api/orders/{id}
GET /api/tickets/me
```

## Notifications

```http
GET /api/notifications
POST /api/notifications/{id}/read
POST /api/notifications/read-all
```

---

# 49. ADMIN API

## User CRUD

```http
GET /api/admin/users
GET /api/admin/users/{id}
POST /api/admin/users
PUT /api/admin/users/{id}
DELETE /api/admin/users/{id}
POST /api/admin/users/{id}/suspend
POST /api/admin/users/{id}/activate
```

## Artist Management

```http
GET /api/admin/artists
GET /api/admin/artists/{id}
PUT /api/admin/artists/{id}
DELETE /api/admin/artists/{id}
POST /api/admin/artists/{id}/suspend
```

## EO Management

```http
GET /api/admin/organizers
GET /api/admin/organizers/{id}
PUT /api/admin/organizers/{id}
DELETE /api/admin/organizers/{id}
POST /api/admin/organizers/{id}/suspend
```

## Verification

```http
GET /api/admin/verifications/artists
GET /api/admin/verifications/organizers

POST /api/admin/verifications/artists/{id}/approve
POST /api/admin/verifications/artists/{id}/reject

POST /api/admin/verifications/organizers/{id}/approve
POST /api/admin/verifications/organizers/{id}/reject
```

## Event Monitoring

```http
GET /api/admin/events
GET /api/admin/events/{id}
PUT /api/admin/events/{id}
POST /api/admin/events/{id}/cancel
```

## Ticket Monitoring

```http
GET /api/admin/tickets
GET /api/admin/orders
GET /api/admin/events/{id}/ticket-sales
```

## Reports

```http
GET /api/admin/reports
GET /api/admin/reports/{id}
POST /api/admin/reports/{id}/resolve
POST /api/admin/reports/{id}/dismiss
```

---

# 50. MAUI SERVICE LAYER

Recommended services:

```text
IAuthService
IUserService
IArtistService
IOrganizerService
IVerificationService
IEventService
ITicketService
IPostService
IMusicService
IMerchandiseService
IFollowService
ILocationService
IRecommendationService
INotificationService
IReportService
IAdminService
```

Architecture:

```text
Pages
  |
  v
ViewModels
  |
  v
Services
  |
  v
ApiClient
  |
  v
ASP.NET Core API
```

Pages must not directly call HttpClient.

---

# 51. MAUI PROJECT STRUCTURE

Recommended:

```text
GigRadar/
│
├── App.xaml
├── App.xaml.cs
├── MauiProgram.cs
│
├── Models/
│   ├── User.cs
│   ├── Artist.cs
│   ├── Organizer.cs
│   ├── Event.cs
│   ├── Venue.cs
│   ├── Song.cs
│   ├── Album.cs
│   ├── Post.cs
│   ├── Merchandise.cs
│   ├── Ticket.cs
│   ├── Order.cs
│   ├── Notification.cs
│   └── Report.cs
│
├── DTOs/
│   ├── Auth/
│   ├── Users/
│   ├── Artists/
│   ├── Organizers/
│   ├── Events/
│   ├── Tickets/
│   ├── Posts/
│   └── Admin/
│
├── Pages/
│   ├── Auth/
│   ├── User/
│   ├── Artist/
│   ├── Organizer/
│   ├── Admin/
│   └── Shared/
│
├── ViewModels/
│   ├── Auth/
│   ├── User/
│   ├── Artist/
│   ├── Organizer/
│   ├── Admin/
│   └── Shared/
│
├── Services/
│   ├── Auth/
│   ├── User/
│   ├── Artist/
│   ├── Organizer/
│   ├── Verification/
│   ├── Events/
│   ├── Tickets/
│   ├── Posts/
│   ├── Music/
│   ├── Merchandise/
│   ├── Location/
│   ├── Recommendation/
│   └── Notifications/
│
├── Components/
│   ├── GigCard.xaml
│   ├── ArtistCard.xaml
│   ├── EventCard.xaml
│   ├── PostCard.xaml
│   ├── TicketCard.xaml
│   ├── MerchandiseCard.xaml
│   ├── GenreChip.xaml
│   └── VerificationBadge.xaml
│
├── Navigation/
│   └── AppNavigationService.cs
│
├── Constants/
│   ├── ApiConstants.cs
│   ├── AppConstants.cs
│   └── RoleConstants.cs
│
└── Resources/
    ├── Fonts/
    ├── Images/
    ├── Styles/
    └── Raw/
```

---

# 52. BACKEND PROJECT STRUCTURE

Recommended separate solution:

```text
GigRadar.sln

src/
├── GigRadar.Mobile/
│
├── GigRadar.Api/
│   ├── Controllers/
│   ├── Middleware/
│   ├── Extensions/
│   └── Program.cs
│
├── GigRadar.Application/
│   ├── DTOs/
│   ├── Interfaces/
│   ├── Services/
│   ├── Validators/
│   └── UseCases/
│
├── GigRadar.Domain/
│   ├── Entities/
│   ├── Enums/
│   └── Interfaces/
│
└── GigRadar.Infrastructure/
    ├── Data/
    ├── Repositories/
    ├── Identity/
    ├── Integrations/
    └── Migrations/
```

---

# 53. AUTHORIZATION

Backend must enforce:

```text
USER
    |
    +-- user permissions

ARTIST
    |
    +-- artist permissions
    +-- verification required

EO
    |
    +-- organizer permissions
    +-- verification required

ADMIN
    |
    +-- platform permissions
```

Example:

```text
POST /api/events
```

Requires:

```text
Authenticated
AND
Role = EO
AND
Organizer verified
```

Example:

```text
POST /api/admin/users
```

Requires:

```text
Authenticated
AND
Role = ADMIN
```

---

# 54. PERMISSION MATRIX

| Feature | User | Artist | EO | Admin |
|---|---:|---:|---:|---:|
| Browse events | ✓ | ✓ | ✓ | ✓ |
| Search artists | ✓ | ✓ | ✓ | ✓ |
| Follow artist | ✓ | ✓ | ✓ | ✓ |
| Buy ticket | ✓ | ✓ | ✓ | ✓ |
| Create post | - | ✓ | Optional | Moderate |
| Manage music | - | ✓ | - | Moderate |
| Manage merchandise | - | ✓ | - | Moderate |
| Create event | - | - | ✓ | ✓ |
| Manage ticket | - | - | ✓ | ✓ |
| Invite artist | - | - | ✓ | ✓ |
| Submit artist verification | - | ✓ | - | - |
| Submit EO verification | - | - | ✓ | - |
| Verify artist | - | - | - | ✓ |
| Verify EO | - | - | - | ✓ |
| User CRUD | - | - | - | ✓ |
| Artist management | - | - | - | ✓ |
| EO management | - | - | - | ✓ |
| Event monitoring | - | Own/related | Own | ✓ |
| Ticket monitoring | Own | Own/related | Own | ✓ |
| Reports | Create | Create | Create | Manage |
| Audit logs | - | - | - | ✓ |

---

# 55. NAVIGATION SEPARATION

After login:

```text
if Role == USER
    -> UserShell

if Role == ARTIST
    -> ArtistShell

if Role == EO
    -> OrganizerShell

if Role == ADMIN
    -> AdminShell
```

Never show unrelated navigation.

Example:

Artist should not see:

```text
Admin Users
Admin Verification
Admin Audit Logs
```

User should not see:

```text
EO Event Management
Artist Analytics
Admin Dashboard
```

---

# 56. UI DESIGN SYSTEM

GigRadar visual direction:

- Modern
- Music-focused
- Urban
- Local
- Energetic
- Clean
- Content-first

Do not copy exact layouts from reference apps.

## UI Rules

- Strong typography hierarchy
- Clear event information
- Consistent spacing
- Limited color palette
- Meaningful accent color
- Accessible contrast
- Consistent buttons
- Consistent cards
- Consistent form fields
- Responsive layouts

Avoid:

- Excessive gradients
- Excessive glassmorphism
- Huge decorative 3D elements
- Too many floating cards
- Excessive animation
- Tiny text
- Information overload

---

# 57. CORE COMPONENTS

## Event Card

Show:

- Poster
- Event name
- Date
- Venue
- Distance
- Genre
- Starting ticket price
- Event status

## Artist Card

Show:

- Profile image
- Artist name
- Verification badge
- Genre
- Location
- Followers
- Follow button

## Post Card

Show:

- Artist
- Verification badge
- Timestamp
- Content
- Media
- Like
- Comment
- Share

## Merchandise Card

Show:

- Product image
- Name
- Price
- Stock status
- Buy button

## Verification Badge

Only display after backend verification status is VERIFIED.

---

# 58. RESPONSIVE DESIGN

The MAUI application must support:

- Android portrait
- Android landscape
- Windows desktop
- Windows resizing
- iOS
- macOS

Avoid excessive fixed pixel dimensions.

Use:

- Grid
- FlexLayout
- StackLayout
- CollectionView
- ScrollView
- Adaptive layouts

Windows UI should not simply be a stretched phone layout.

---

# 59. MAP

Map functionality:

- Event location
- Venue location
- Nearby events
- Distance
- Event discovery

Avoid loading every event marker at once.

Use clustering/pagination if necessary.

---

# 60. OFFLINE & CACHING

Cache lightweight data:

- Artist profile
- Event preview
- Venue
- Genres
- User preferences

Ticket QR should ideally remain accessible offline after successful purchase.

Server remains source of truth.

---

# 61. PERFORMANCE

Priorities:

1. Fast startup
2. Smooth scrolling
3. Low memory usage
4. Low battery usage
5. Efficient network calls

Avoid:

- Continuous GPS
- Huge images
- Loading every event
- Excessive animations
- Nested CollectionViews
- Heavy 3D everywhere
- Unnecessary API calls

Use:

- Pagination
- Image caching
- Lazy loading
- Async operations
- API caching where appropriate

---

# 62. THREE.JS / 3D

Three.js/3D should be optional enhancement.

Good use cases:

- Artist hero
- Music visualization
- Event atmosphere
- Interactive onboarding
- Special event visualizer

Do not use 3D for:

- Basic buttons
- CRUD tables
- Forms
- Navigation
- Ticket lists
- Admin data tables

A lightweight 2D fallback must exist.

---

# 63. ADMIN UI

Admin UI should prioritize information density and efficiency.

Use:

- Tables
- Filters
- Search
- Status chips
- Confirmation dialogs
- Detail panels
- Charts
- Pagination
- Bulk operations where safe

Example:

```text
Users

Search [________________]

Role     Status      Actions

User     Active      View Edit Suspend
Artist   Verified    View Edit Suspend
EO       Pending     Review
```

---

# 64. ADMIN CRUD SAFETY

Dangerous actions must require confirmation.

Example:

```text
Delete account?

This action affects:
- Profile
- Posts
- Events
- Orders
- Tickets
```

Prefer soft deletion for records required for financial/audit history.

Never physically delete financial records merely to hide them from the UI.

---

# 65. VALIDATION

Client validation improves UX.

Backend validation is mandatory.

Examples:

- Required fields
- Email format
- Event dates
- Ticket price
- Ticket stock
- Artist verification data
- EO portfolio data
- Ownership checks

Never trust client validation alone.

---

# 66. ERROR HANDLING

Use consistent API errors.

```text
200 OK
201 Created
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
422 Unprocessable Entity
429 Too Many Requests
500 Internal Server Error
```

MAUI must display human-readable messages.

Do not expose stack traces to users.

---

# 67. SECURITY

Requirements:

- JWT access token
- Refresh token
- Secure token storage
- Password hashing
- Server-side role authorization
- Ownership validation
- Rate limiting
- Input validation
- File upload validation
- HTTPS
- Audit logs
- No secrets in client code

Never log:

- Passwords
- Access tokens
- Payment secrets
- Sensitive personal data

---

# 68. FILE UPLOADS

Used for:

- Artist profile
- Artist cover
- Artist posts
- Album artwork
- EO logo
- EO portfolio
- Event poster
- Merchandise image

Backend must validate:

- File type
- File size
- File name
- Image dimensions where appropriate

Do not trust file extension alone.

---

# 69. ANALYTICS

## Artist

- Profile views
- Followers
- Follower growth
- Post views
- Post engagement
- Gig interest
- Release engagement
- Merchandise views

## EO

- Event views
- Ticket sold
- Revenue
- Attendance
- Conversion
- Event engagement

## Admin

- Registered users
- Artists
- EO
- Events
- Active events
- Tickets
- Verification requests
- Reports
- Platform activity

---

# 70. USER JOURNEY

```text
Launch
  |
Login/Register
  |
Choose User
  |
Music Preferences
  |
Location Permission
  |
User Home
  |
Discover
  |
Event Detail
  |
Artist / Venue
  |
Buy Ticket
  |
Ticket
```

---

# 71. ARTIST JOURNEY

```text
Register
  |
Choose Artist
  |
Artist Setup
  |
Connect Spotify
  |
Submit Verification
  |
Admin Review
  |
Verified
  |
Artist Dashboard
  |
Social Feed
  |
Music
  |
Gigs
  |
Merchandise
```

---

# 72. EO JOURNEY

```text
Register
  |
Choose EO
  |
Organizer Setup
  |
Portfolio
  |
Submit Verification
  |
Admin Review
  |
Verified
  |
EO Dashboard
  |
Create Event
  |
Venue
  |
Lineup
  |
Tickets
  |
Publish
  |
Monitor Sales
```

---

# 73. ADMIN JOURNEY

```text
Login
  |
Admin Dashboard
  |
+-- Users
+-- Artists
+-- Organizers
+-- Verification
+-- Events
+-- Tickets
+-- Reports
+-- Audit Logs
```

---

# 74. DEVELOPMENT PHASES

## Phase 1: Foundation

- MAUI project
- ASP.NET Core API
- Database
- Authentication
- JWT
- Role system

## Phase 2: Role Separation

- UserShell
- ArtistShell
- OrganizerShell
- AdminShell

## Phase 3: Admin

- Dashboard
- User CRUD
- Artist management
- EO management
- Verification center

## Phase 4: Artist

- Artist profile
- Verification
- Spotify connection
- Social feed
- Music
- Gigs

## Phase 5: EO

- Organizer profile
- Portfolio
- Verification
- Event CRUD
- Artist invitation
- Lineup

## Phase 6: Event Discovery

- Event listing
- Search
- Location
- Map
- Event detail

## Phase 7: Ticketing

- Ticket types
- Orders
- Payment integration
- QR tickets

## Phase 8: Social

- Posts
- Likes
- Comments
- Follow
- Notifications

## Phase 9: Merchandise

- Artist shop
- Product catalog
- External purchase link

## Phase 10: Recommendation

- Weighted recommendation
- Interaction tracking
- Personalization

## Phase 11: Moderation

- Reports
- Content moderation
- Audit logs

## Phase 12: Optional Enhancement

- Advanced analytics
- ML recommendation
- Three.js
- Advanced merchandise marketplace

---

# 75. MVP PRIORITY

The MVP should prioritize:

```text
1. Authentication
2. Four-role system
3. Role-specific navigation
4. Admin dashboard
5. User CRUD
6. Artist verification
7. EO verification
8. EO portfolio
9. Artist profile
10. Artist social feed
11. Event CRUD
12. Event discovery
13. Venue
14. Follow Artist
15. Basic ticketing
```

Do not start with:

- Advanced ML
- Complex marketplace
- Heavy Three.js
- Chat
- Streaming
- Complex recommendation models

Build the platform foundation first.

---

# 76. FUTURE FEATURES

Possible future modules:

- Artist booking marketplace
- EO ↔ Artist contract workflow
- Full merchandise marketplace
- Event check-in
- QR scanner
- Live event status
- Artist collaboration
- Fan community
- Chat
- Music streaming
- Advanced analytics
- Push notification
- AI-assisted recommendations
- Personalized event calendar
- Venue management
- Sponsorship marketplace

---

# 77. TESTING

## Unit Tests

Test:

- Recommendation
- Validation
- Permission logic
- Business rules

## API Tests

Test:

- Authentication
- Authorization
- CRUD
- Verification
- Event ownership
- Ticket ownership

## ViewModel Tests

Test:

- Loading
- Commands
- Error states
- Navigation state

## UI Tests

Test:

- Login
- Role navigation
- Artist flow
- EO flow
- Admin CRUD
- Ticket flow

## Security Tests

Test:

```text
User cannot access Admin
Artist cannot access Admin
EO cannot access Admin
Unverified Artist cannot use protected Artist features
Unverified EO cannot use protected EO features
EO cannot modify another EO's event
Artist cannot modify another Artist's profile
User cannot access another user's private ticket data
```

---

# 78. DEFINITION OF DONE

A feature is complete only when:

- UI exists
- ViewModel exists
- Service exists
- API exists where required
- Database model exists where required
- Validation exists
- Loading state exists
- Error state exists
- Empty state exists
- Authorization exists
- Ownership validation exists
- Android tested
- Windows tested
- No major compile errors
- No obvious regression

---

# 79. ARCHITECTURAL RULES

Mandatory:

1. Pages contain UI logic only.
2. ViewModels manage state and commands.
3. Services handle API/business interaction.
4. Backend owns authorization.
5. Backend validates ownership.
6. DTOs are preferred for API contracts.
7. Do not expose database entities directly where inappropriate.
8. Do not hardcode roles or credentials.
9. Do not put API calls directly in XAML code-behind.
10. Do not duplicate services.
11. Do not duplicate components.
12. Preserve Android and Windows compatibility.
13. Prefer small, testable services.
14. Keep verification logic server-side.
15. Keep payment status server-side.
16. Keep ticket ownership server-side.

---

# 80. AI CODING AGENT INSTRUCTIONS

Any coding agent such as Claude, Gemini, DeepSeek, or another coding assistant must read this specification before modifying GigRadar.

## Before modifying code

1. Inspect the existing project.
2. Identify current architecture.
3. Identify reusable components.
4. Identify existing services.
5. Identify existing models.
6. Identify existing navigation.
7. Do not duplicate existing functionality.

## Implementation

- Follow MVVM.
- Maintain role separation.
- Use services.
- Use DTOs.
- Keep business logic outside Pages.
- Keep authorization in backend.
- Preserve Android support.
- Preserve Windows support.
- Avoid unnecessary rewrites.
- Make the smallest safe change.
- Build after modifications.
- Fix compile errors before moving forward.

## UI

Do not create generic AI-generated UI.

Avoid:

- Excessive gradients
- Excessive glassmorphism
- Excessive rounded cards
- Excessive animations
- Decorative UI without purpose
- Overly complex 3D

Prioritize:

- Typography
- Hierarchy
- Spacing
- Content
- Usability
- Accessibility
- GigRadar identity

## Backend

Never trust:

```text
ClientRole
ClientVerification
ClientPaymentStatus
ClientOwnership
```

The backend must determine all of them.

## Verification

Artist and EO verification must always be server-controlled.

## Admin

Admin endpoints must require Admin authorization.

---

# 81. CURRENT IMPLEMENTATION ORDER

Implement in this exact order:

```text
STEP 1
Authentication

STEP 2
Four Role System

STEP 3
Role-specific Navigation

STEP 4
Admin Dashboard

STEP 5
Admin User CRUD

STEP 6
Artist Registration + Verification

STEP 7
EO Registration + Portfolio + Verification

STEP 8
Artist Profile

STEP 9
Artist Social Feed

STEP 10
Event + Venue

STEP 11
EO Event Management

STEP 12
Artist ↔ EO Invitation

STEP 13
Event Discovery

STEP 14
Location + Map

STEP 15
Follow Artist

STEP 16
Ticketing

STEP 17
Notifications

STEP 18
Merchandise

STEP 19
Reports + Moderation

STEP 20
Recommendation Engine

STEP 21
Analytics

STEP 22
Optional Three.js
```

---

# 82. FINAL PRODUCT ARCHITECTURE

```text
                         GIGRADAR
                            |
        +-------------------+-------------------+
        |                   |                   |
       USER              ARTIST                 EO
        |                   |                   |
    Discovery           Social               Events
    Following           Music                Ticketing
    Ticket              Gigs                 Portfolio
    Feed                Merch                Lineup
        |                   |                   |
        +-------------------+-------------------+
                            |
                          ADMIN
                            |
       +--------------------+--------------------+
       |                    |                    |
   Accounts           Verification          Moderation
       |                    |                    |
     CRUD             Artist / EO             Reports
       |                    |                    |
       +--------------------+--------------------+
                            |
                         EVENTS
                            |
                    +-------+-------+
                    |               |
                  ARTIST           TICKET
                    |               |
                    +-------+-------+
                            |
                           USER
```

## Central Idea

The core relationship is:

```text
USER
  |
  | discovers / follows / buys
  v
ARTIST <------ EVENT ------> EO
  |                           |
  |                           |
  +------ SOCIAL ------------+
              |
            ADMIN
              |
       Verification
       Moderation
       Management
```

GigRadar should ultimately feel like a **local music ecosystem**, not a collection of unrelated features.

---

# 83. NON-NEGOTIABLE PRODUCT RULES

1. Exactly four primary roles: User, Artist, EO, Admin.
2. Artist and EO verification is mandatory for verified status.
3. Admin controls verification.
4. Admin has dedicated CRUD and management tools.
5. Backend controls authorization.
6. Artist profile behaves like a music-focused social profile.
7. EO profile includes portfolio.
8. Event is a central entity connecting Artist, EO, and User.
9. User can discover events and Artists.
10. User can purchase tickets.
11. Artist can publish music, gig activity, posts, and merchandise.
12. EO can create events, manage lineup, and tickets.
13. Admin can monitor events and tickets.
14. Admin can manage accounts.
15. Reports and audit logs must exist for platform safety.
16. Android and Windows compatibility must be preserved.
17. Advanced ML and heavy 3D are not MVP requirements.
18. UI must have a distinct GigRadar identity.
19. Security and server-side authorization take priority over convenience.
20. Every feature must include loading, error, empty, validation, and authorization states where applicable.

---

# 84. IMPLEMENTATION PRINCIPLE

```text
FUNCTIONALITY
      ↓
DATA MODEL
      ↓
API
      ↓
AUTHORIZATION
      ↓
STATE MANAGEMENT
      ↓
UI
      ↓
VALIDATION
      ↓
TESTING
      ↓
ANIMATION
      ↓
OPTIONAL 3D
```

Do not reverse this order.

The foundation must be stable before visual enhancements are added.

---

# LAMPIRAN A — DESIGN SYSTEM

> Sumber: GIGRADAR_DESIGN_SYSTEM.md (hasil *design architecture pass*). Keputusan di sini
> diterapkan di .NET MAUI (mobile-first); konsep "Three.js" pada brief diterjemahkan menjadi
> visualisasi native yang hemat daya (`GraphicsView`), karena produk ini bukan aplikasi web.

## A.1 Prinsip Desain

1. **Tipografi adalah identitas.** Heading memakai Space Grotesk (geometris, tegas, khas
   produk musik/tekno), body memakai Inter. Tidak ada font "startup AI" generik.
2. **Imagery editorial.** Poster event dan foto artist dipakai besar. Bukan grid kartu identik.
3. **Hierarki, bukan dekorasi.** Setiap elemen menjawab "kenapa ini ada?". Kalau jawabannya
   hanya "biar keren" — hapus.
4. **Densitas yang disengaja.** Informasi padat saat berguna (daftar event, admin),
   lapang saat membaca (hero, artist profile).
5. **Kontras kuat.** Dark theme zinc + aksen "signal lime" yang dipakai strategis,
   bukan neon di mana-mana.
6. **Motion punya fungsi.** Sweep radar, transisi halaman, konfirmasi tiket.
   Tidak ada floating/partikel acak.
7. **Aksesibilitas & performa.** Kontras cukup, target sentuh ≥ 44px, dukungan reduced
   motion, dan visualisasi radar berhenti saat tidak terlihat.

## A.2 Analisis Produk & Role

| Role | Kebutuhan inti | "Rumah" di aplikasi |
|---|---|---|
| **User / pendengar** | Discover gig, radar sekitar, detail event/artist/venue, beli tiket, tiket digital, profil | `UserShell` — tab Discover / Radar / Tickets / Profile |
| **Artist** | Profil artist, dashboard, jadwal gig, musik, audience | `ArtistShell` |
| **Event Organizer** | Buat/kelola event, tipe tiket, stok, analitik penjualan | `EOShell` |
| **Admin** | Moderasi user/artist/EO/event, laporan, statistik sistem | `AdminShell` |

Satu bahasa visual, empat varian navigasi. Role non-User memakai token yang sama tapi
layout dashboard yang *information-dense* dan tidak dekoratif.

## A.3 Information Architecture (User Shell)

```text
GIGRADAR
├── Auth
│   ├── Login / Register
│   └── Onboarding (pilih genre → kota/lokasi)
├── USER SHELL (4 tab)
│   ├── Discover (Home)
│   │   ├── Hero "Malam ini di {kota}" + event unggulan
│   │   ├── Dekat kamu (horizontal)
│   │   ├── Radar preview → buka tab Radar
│   │   ├── Artist untuk kamu (editorial)
│   │   ├── Minggu ini (list tanggal)
│   │   └── Genre chips → filter Radar
│   ├── Radar
│   │   ├── Visualisasi node event di sekitar user
│   │   ├── Filter: genre · radius · "malam ini"
│   │   ├── Preview event terpilih → EventDetail
│   │   └── List "Gig terdekat" + buka Google Maps
│   ├── Tickets (My Tickets)
│   │   └── Ticket → digital ticket
│   └── Profile
│       ├── Data diri, edit kota
│       ├── Favorit
│       └── (EO/Admin) pintu dashboard
├── USER FLOW UTAMA (route global)
│   EventDetail → TicketSelection → Checkout → TicketSuccess
│   ArtistDetail · VenueDetail (via maps)
├── ARTIST SHELL — Overview · Events · Create Gig · Audience · Profile
├── EO SHELL — Overview · Events · Create Event · Tickets · Venues · Analytics · Settings
└── ADMIN SHELL — Users · Artists · Organizers · Events · Reports · Moderation
```

Route navigasi (`Shells/Routes.cs`): `EventDetailPage`, `ArtistDetailPage`,
`TicketSelectionPage`, `CheckoutPage`, `TicketSuccessPage`, dst.

## A.4 User Flows

```text
Onboarding (genre) → Home
  → Discover: hero/nearby/radar preview
  → Radar: tap node → preview → EventDetail
  → EventDetail: lineup → artist; venue → maps; beli → tiket
  → TicketSelection (pilih tier) → Checkout (data diri, umur ≥ 17) → TicketSuccess (digital ticket)
  → Tickets tab (riwayat, barcode)
```

Flow EO: Buat Event → kelola tipe tiket/stok → publikasi → pantau penjualan → validasi QR.
Flow Admin: moderasi konten/report, kelola user/EO/artist, statistik sistem.

## A.5 Visual Language

- **Mood:** malam, skena, panggung kecil, peta kota. Zinc gelap (#0A0A0B) bukan hitam pekat.
- **Aksen "signal lime"** `#A3FF12` — metafora radar GIGRADAR. Dipakai untuk: CTA utama,
  angka kunci (harga, jarak), titik user di radar, status "aktif". Tidak dipakai untuk border
  dekoratif.
- **Genre punya warna.** Warna genre (A.6) dipakai konsisten di radar node, chip genre,
  dan label. Bukan pelangi neon — palet redup yang tetap bisa dibedakan.
- **Sudut:** 14px untuk kartu, 8px elemen kecil, 999px hanya untuk avatar/chip pill.
- **Shadow:** hampir tidak ada. Hierarki dibuat lewat warna permukaan dan border tipis.
- **Tipografi:** display Space Grotesk (hero, nama event, angka), body Inter.
  Label seksi memakai *eyebrow* uppercase + letter-spacing (gaya editorial).

## A.6 Design Tokens — Warna

| Token | Value | Pemakaian |
|---|---|---|
| `BackgroundColor` / Ink | `#0A0A0B` | latar aplikasi |
| `SurfaceColor` | `#131316` | kartu, seksi |
| `CardColor` | `#18181B` | kartu di atas surface |
| `CardElevatedColor` | `#1E1E23` | elemen terangkat, input |
| `BorderColor` | `#26262C` | border default |
| `BorderStrong` | `#34343C` | border fokus/interaktif |
| `TextPrimary` | `#F4F4F5` | teks utama |
| `TextSecondary` | `#A1A1AA` | teks pendukung |
| `TextMuted` | `#71717A` | metadata, placeholder |
| `PrimaryColor` / Accent | `#A3FF12` | aksen "signal lime" |
| `AccentSoft` | `#1C2A0D` | latar tint aksen |
| `OnAccent` | `#0A0A0B` | teks di atas aksen |
| `DangerColor` | `#F43F5E` | error, tiket habis |
| `WarningColor` | `#F59E0B` | peringatan, stok menipis |
| `SuccessColor` | `#22C55E` | sukses, tiket active |

### Warna genre (radar node + chip)

| Genre | Warna | Genre | Warna |
|---|---|---|---|
| Indie | `#F4F4F5` | Jazz | `#F2C14E` |
| Alternative | `#A3FF12` | Folk | `#D9A05B` |
| Rock | `#E4572E` | Electronic | `#2DD4BF` |
| Metal | `#9CA3AF` | Pop | `#F472B6` |
| Punk | `#E11D48` | Hip-Hop | `#A78BFA` |
| Hardcore | `#EF476F` | Shoegaze | `#7DD3FC` |
| Emo | `#C084FC` | default | `#A1A1AA` |

## A.7 Design Tokens — Tipografi, Spasi, Radius, Motion

| Style | Font | Ukuran | Pemakaian |
|---|---|---|---|
| Display | SpaceGroteskBold | 34 | hero, nama event besar |
| Title | SpaceGroteskBold | 26–28 | judul halaman |
| Heading | SpaceGroteskMedium | 20–22 | judul seksi/event |
| Subheading | InterSemiBold | 17 | subjudul, kartu utama |
| Body | InterRegular | 14–15 | teks isi |
| Caption | InterMedium | 12–13 | metadata |
| Meta | InterMedium | 10–11 | eyebrow, label kecil (letter-spacing 1.2) |
| Numeric | SpaceGroteskMedium | bervariasi | harga, jarak, angka statistik |

- Spasi basis **4px**: 4 · 8 · 12 · 16 · 20 · 24 · 32 · 40 · 48 · 64.
- Padding halaman mobile **20px**; antar seksi **28–32px**; desktop 24–48px.
- Radius: kartu **14**, input/button **12**, kecil **8**, pill **999**.
- Motion: radar sweep ~8°/frame saat tab aktif, **berhenti saat tab tidak terlihat** dan saat
  `reduced motion` aktif (sweep diganti cincin statis); transisi halaman default platform;
  konfirmasi tiket: check-in brief (opacity/scale sekali); tanpa parallax/floating/partikel.

## A.8 Komponen & State

| Komponen | Elemen | State |
|---|---|---|
| Button | Primary (lime, teks gelap), Outline, Ghost, Danger, Small, Buy (sticky) | default · pressed · disabled · loading |
| Chip | genre filter, pill | default · selected (lime) |
| Card | Frame `Card` style | default · pressed (opacity) |
| EventCard (hero) | poster besar + tanggal + nama + venue + harga | — |
| EventRow | blok tanggal (day/month) + info + harga | — |
| ArtistCard | foto portrait + nama + genre + kota | — |
| Input | Entry/Editor dengan `SurfaceColor` box | default · focus · error |
| SectionHeader | eyebrow + judul + aksi "Lihat semua" | — |
| TicketCard | digital ticket + barcode + status | Active/Used |
| Empty/Loading/Error | state teks + aksi | — |

A11y: target ≥ 44px, label semantik untuk GraphicsView (radar punya fallback list),
focus state terlihat (border `BorderStrong`), kontras ≥ 4.5:1 untuk teks.

## A.9 Spesifikasi Layar (ringkas)

- **Login**: wordmark besar, form kotak, CTA lime. Tanpa hero stock photo.
- **Onboarding**: "Pilih yang kamu dengar" — grid genre (pill besar, icon), lalu pilih kota.
- **Discover (Home)**: header (wordmark + lokasi + avatar) → hero malam ini → dekat kamu
  (horizontal) → preview radar → artist untuk kamu → minggu ini (list tanggal) → genre chips.
- **Explore**: search event/artis/venue (debounce), filter genre · tanggal · radius, **switch
  view List / Grid / Radar**.
- **Radar**: header + filter + radar + preview + list terdekat.
- **ArtistDetail**: hero cover editorial (scrim), statistik (pengikut/track/album), bio,
  gig mendatang (dari API), track dengan preview, personel.
- **EventDetail**: hero poster (scrim), genre chip + status, nama display, meta baris
  (tanggal/jam/venue/jarak), deskripsi, LINEUP (row artist → detail), TIKET (tier + stok),
  venue block, **sticky buy bar**.
- **TicketSelection**: tier sebagai row radio (nama, deskripsi, harga, sisa stok, SOLD OUT).
- **Checkout**: ringkasan pesanan + form data diri; verifikasi inline.
- **TicketSuccess**: digital ticket — perforasi (notch + dashed), event, tanggal, venue,
  tier, atas nama, barcode, kode QR, status "ACTIVE".
- **Artist/EO/Admin**: token sama, layout dense, tanpa dekorasi.

## A.10 Arsitektur Komponen Visual

```text
GigRadarMobile/
├── Helpers/
│   ├── GeoHelper.cs            # Haversine, bearing, format jarak
│   ├── GenreColors.cs          # map genre → warna
│   ├── RadarDrawable.cs        # IDrawable radar (rings, sweep, node)
│   ├── RadarState.cs           # filter lintas-tab (genre chip Home → Radar)
│   └── ReducedMotion.cs        # deteksi reduced motion per platform
├── ViewModels/  RadarViewModel (ex-MapViewModel), HomeViewModel, EventDetailViewModel, ...
├── Views/       RadarPage (ex-MapPage), HomePage, EventDetailPage, TicketSuccessPage, ...
└── Shells/      UserShell (tab Discover | Radar | Tickets | Profile), EOShell, ArtistShell, AdminShell
```

Logika tetap di ViewModel/Service; halaman hanya konsumsi.

---

# LAMPIRAN B — RADAR: SPESIFIKASI VISUALISASI NATIVE

Brief meminta pengalaman Three.js; karena produk adalah .NET MAUI, konsep diterjemahkan
ke **`GraphicsView` + `IDrawable`** yang ringan dan lintas platform:

| Konsep brief (Three.js) | Implementasi MAUI |
|---|---|
| Globe 3D yang bisa dirotasi | Radar planar 2D — rotasi tidak relevan di mobile; zoom = pilih radius (10/25/50 km) |
| Radar pulses di kota aktif | **Sweep line** berputar + cincin radius berlabel (jarak km) |
| Event sebagai titik/node | Node bulat; **radius node ∝ popularitas** (views/saves), **warna = genre** |
| Posisi berdasar jarak | Haversine dari titik user → koordinat polar (jarak → radius, bearing → sudut) |
| Tap node → preview | `StartInteraction` → hit-test node → kartu preview di bawah radar |
| Filter dinamis | Chip genre + toggle "Malam ini" + pilihan radius → redraw node |
| Progressive enhancement / fallback | Jika list node kosong → empty state; radar **tetap punya list** "Gig terdekat" sebagai akses utama (radar tidak pernah jadi satu-satunya cara akses info) |
| Performa (lazy, pause, DPR, disposal) | Render hanya saat tab aktif; `Invalidate()` per frame sweep; timer dihentikan saat `OnDisappearing`/`IsVisible=false`; `DeviceDisplay.MainDisplayInfo.Density` dipakai untuk ukuran minimum node; **reduced motion → sweep mati** |

Node yang dipilih diberi cincin + label nama event di atasnya. Jarak antar node dipakai
secara visual (bukan presisi peta) — akurasi tetap di list dan Google Maps.

---

# LAMPIRAN C — GPS & DEVICE LOCATION SPECIFICATION

> Sumber: GPS_LOCATION.md — spesifikasi implementasi lokasi untuk GigRadarMobile.

## C.1 Tujuan & Aturan

1. Aplikasi meminta izin lokasi **hanya ketika fitur yang membutuhkan lokasi digunakan**.
2. Latitude/longitude diambil via `Microsoft.Maui.Devices.Sensors.Geolocation`.
3. Kompatibel: Android, iOS, Mac Catalyst/macOS, Windows.
4. Jika GPS tidak tersedia, aplikasi tetap dapat digunakan dengan fallback yang jelas.
5. Lokasi dipakai untuk: Radar, Gig terdekat, rekomendasi berbasis lokasi, filter radius, event nearby.
6. Jangan minta `LocationAlways` atau background location.
7. Jangan kirim lokasi ke backend tanpa kebutuhan fitur/persetujuan user.
8. Jangan simpan riwayat lokasi secara permanen.

## C.2 Arsitektur

`LocationService` menjadi satu-satunya abstraction yang digunakan ViewModel untuk mengakses
perangkat. Jangan memanggil `Geolocation.Default.GetLocationAsync()` langsung dari banyak
Page/ViewModel — gunakan service agar permission, timeout, fallback, dan error handling konsisten.

```csharp
public sealed class LocationService
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    public bool IsLocationEnabled => Geolocation.Default.IsEnabled;

    public async Task<bool> RequestPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status == PermissionStatus.Granted) return true;
        if (status == PermissionStatus.Denied) return false;
        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
    }

    public async Task<Location?> GetCurrentLocationAsync(
        GeolocationAccuracy accuracy = GeolocationAccuracy.Medium,
        CancellationToken cancellationToken = default)
    {
        if (!await RequestPermissionAsync()) return null;
        if (!Geolocation.Default.IsEnabled) return null;
        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(RequestTimeout);
            var request = new GeolocationRequest(accuracy, RequestTimeout);
#if IOS
            request.RequestFullAccuracy = false;
#endif
            return await Geolocation.Default.GetLocationAsync(request, timeoutCts.Token);
        }
        catch (PermissionException)        { return null; }
        catch (FeatureNotSupportedException) { return null; }
        catch (FeatureNotEnabledException)  { return null; }
        catch (OperationCanceledException)  { return null; }
    }

    public async Task<Location?> GetLastKnownLocationAsync()
    {
        try { return await Geolocation.Default.GetLastKnownLocationAsync(); }
        catch { return null; }
    }
}
```

Register di `MauiProgram.cs`: `builder.Services.AddSingleton<LocationService>();`
(jangan menghapus DI yang sudah ada).

## C.3 Konfigurasi Platform

**Android** (`Platforms/Android/AndroidManifest.xml`):

```xml
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<uses-feature android:name="android.hardware.location" android:required="false" />
<uses-feature android:name="android.hardware.location.gps" android:required="false" />
<uses-feature android:name="android.hardware.location.network" android:required="false" />
```

Jangan menambahkan `ACCESS_BACKGROUND_LOCATION` kecuali ada fitur background yang sah.

**iOS & Mac Catalyst** (`Info.plist`):

```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>GigRadar menggunakan lokasi Anda untuk menampilkan gig dan event musik terdekat.</string>
```

**Mac Catalyst** (`Entitlements.plist`):

```xml
<key>com.apple.security.personal-information.location</key>
<true/>
```

**Windows**: pakai API MAUI yang sama (tanpa implementasi terpisah). Desktop Windows harus
punya location services aktif — kode wajib menangani `IsEnabled == false`.

## C.4 Permission Flow & UX

```text
User membuka Radar / Nearby
        → cek Location Service (Disabled → pesan)
        → cek LocationWhenInUse (Granted → ambil lokasi; Denied → penjelasan + Settings;
          Unknown → RequestAsync → Granted/Denied → fallback)
```

Sebelum system permission popup, tampilkan UI singkat: "Aktifkan lokasi — GigRadar
menggunakan lokasi Anda untuk menemukan gig musik terdekat" + CTA `[ Aktifkan Lokasi ]`.
Jika ditolak: "Anda tetap dapat menggunakan GigRadar..." Jangan popup permission berulang.

## C.5 Integrasi Nearby API & Radar

Endpoint: `GET /api/events/nearby?lat=&lng=&radius=` (backend Haversine).

```text
Device GPS → LocationService → ApiService → GET /api/events/nearby → Radar / Home / Explore
```

Validasi client: latitude −90..90, longitude −180..180, radius `Math.Clamp(radius, 1, 100)`.
Server wajib validasi juga — jangan percaya nilai client.

Radar memakai lokasi device sebagai pusat: jarak (Haversine) + bearing → koordinat polar node.
Jangan pakai lokasi dummy saat GPS tersedia; `GeoHelper` dipertahankan.

## C.6 Fallback

Aplikasi **tidak boleh crash** saat: permission ditolak, GPS mati, provider tidak ada,
emulator tanpa mock location, timeout, posisi tidak tersedia.

```text
GPS berhasil → "Gig terdekat dari lokasi Anda"
GPS gagal    → gunakan kota/profil user → "Pilih lokasi secara manual"
             → tetap tampilkan event global / berdasarkan search
```

Catatan: `City` hanya fallback kasar, bukan pengganti koordinat realtime.

Mode lokasi: `LocationMode.Device` atau `LocationMode.ManualCity` (perjalanan, desktop
tanpa GPS, permission ditolak, lihat gig kota lain). Jangan campur mode tanpa indikator UI.

## C.7 Accuracy, Tracking & Privasi

- Home/Nearby/Radar: `GeolocationAccuracy.Medium`. Presisi tinggi hanya bila fitur benar-benar butuh. Jangan selalu `Best`.
- iOS reduced accuracy: tangani `Location.ReducedAccuracy` (nearby tetap jalan, jangan klaim presisi).
- Foreground listening (`StartListeningForegroundAsync`) hanya bila Radar butuh update saat user bergerak; stop saat halaman tidak memerlukan.
- ❌ Jangan: kirim lokasi berkala ke server, simpan history GPS, background GPS, minta LocationAlways, expose koordinat user ke user lain, log lat/lng di production.
- ✅ Boleh: lokasi untuk nearby search, pusat radar, kirim lat/lng hanya saat request nearby, HTTPS, buang data lokasi setelah selesai.

State ViewModel: `LocationStatus` enum (`Unknown, RequestingPermission, PermissionDenied, Disabled, Available, Unavailable, Error`) + `CurrentLatitude/CurrentLongitude` + `IsUsingDeviceLocation`. UI memetakan status ke pesan ("📍 Menggunakan lokasi Anda", "📍 Aktifkan lokasi", dst).

## C.8 Testing Matrix & Definition of Done

- **Android device**: GPS aktif/mati, permission pertama kali/denied/granted, airplane mode, outdoor/indoor, weak GPS, network only. **Emulator**: mock location (`adb devices`).
- **iPhone**: Allow Once/When In Use, location disabled, reduced accuracy, precise location, denied.
- **Windows**: Location ON/OFF, desktop tanpa GPS, network location, manual fallback.
- **Mac Catalyst**: Location Services ON, prompt, denied, entitlement benar.

DoD: permission benar di semua platform, tidak ada hardcoded lat/lng user, radar pakai device location, nearby memanggil endpoint, denied/disabled/timeout/null ditangani, fallback manual tersedia, tidak ada background tracking/history default, UI menunjukkan status lokasi, semua platform diuji.

---

# LAMPIRAN D — MULTI-PLATFORM BUILD & CONFIGURATION

> Sumber: GIGRADAR_MULTIPLATFORM_FIX.md. GigRadar harus benar-benar aplikasi **.NET MAUI
> multi-platform**, bukan aplikasi Windows yang kebetulan memakai target MAUI.

## D.1 Target Resmi

```xml
<TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0</TargetFrameworks>
```

- Windows hanya dapat dibuild di OS Windows; iOS/Mac Catalyst membutuhkan macOS.
- Jangan menghapus target platform lain hanya karena tidak muncul di dropdown Visual Studio — periksa `dotnet --info`, `dotnet workload list`, `dotnet workload install maui` dulu.
- Satu project `GigRadarMobile`, satu codebase, platform-specific code hanya di `Platforms/`.
- Hindari branch UI per platform (`if (OperatingSystem.IsWindows())` untuk seluruh UI = salah). Halaman shared: LoginPage, HomePage, EventDetailPage, ArtistDetailPage, TicketPage, ProfilePage, dst.
- Windows unpackaged boleh: `<WindowsPackageType>None</WindowsPackageType>` (tidak menjadikan project Windows-only).

## D.2 API URL Per Platform

`Services/ApiConfiguration.cs` — satu sumber, dipakai ApiService (`_httpClient.BaseAddress`):

```csharp
#if ANDROID
    public const string BaseUrl = "http://10.0.2.2:5000";   // emulator → host PC
#elif WINDOWS
    public const string BaseUrl = "http://localhost:5000";
#elif IOS || MACCATALYST
    public const string BaseUrl = "http://localhost:5000";  // simulator; device fisik → IP PC
#else
    public const string BaseUrl = "http://localhost:5000";
#endif
```

Device fisik: PC dan device harus satu Wi-Fi, gunakan `http://192.168.x.x:5000` (ganti IP sesuai PC), dan API harus listen ke network interface:

```bash
dotnet run --project GigRadarApi --urls "http://0.0.0.0:5000"
```

`localhost` tidak boleh dipakai sebagai alamat API universal — artinya berbeda di emulator, device fisik, dan PC.

## D.3 Launcher & Script

`GigRadarLauncher` (console) = utility development: menjalankan `GigRadarApi` di
`http://localhost:5000`, menunggu port terbuka, membuka Swagger. Launcher **bukan** aplikasi
MAUI dan tidak boleh menjadi dependency aplikasi. `StartGigRadar.bat` → launcher;
`StartMobileApp.bat` → build/exe Windows app.

## D.4 Clean Build & Test Per Platform

```bash
dotnet nuget locals all --clear
dotnet clean
dotnet restore
dotnet build GigRadarMobile -f net10.0-windows10.0.19041.0
dotnet build GigRadarMobile -f net10.0-android      # adb devices dulu
dotnet build GigRadarMobile -f net10.0-ios          # butuh macOS
dotnet build GigRadarMobile -f net10.0-maccatalyst  # butuh macOS
```

Uji platform satu per satu, jangan sekaligus. Jika bin/obj terkunci, tutup Visual Studio dulu.

## D.5 Checklist Multi-Platform

- Project: `UseMaui=true`, `SingleProject=true`, 4 target framework, tidak Windows-only, folder `Platforms/` lengkap, `MauiProgram.cs` tidak Windows-specific.
- Android: SDK + emulator terdeteksi, INTERNET permission, `10.0.2.2` untuk emulator, IP PC untuk device fisik.
- iOS/Mac Catalyst: macOS + workload tersedia.
- API: port 5000 aktif, CORS untuk development, production HTTPS.
- Target akhir: satu aplikasi MAUI, 4 target platform, 1 API, 1 shared codebase (Models/Views/ViewModels/Services/Auth sama semua).

---

# LAMPIRAN E — ANDROID PRODUCTION APK

> Sumber: GigRadar android.md — task build signed production APK.

## E.1 Rules Wajib

- Baca struktur repository + file kunci (csproj, MauiProgram, ApiConfiguration, AndroidManifest, MainActivity) sebelum mengubah.
- Jangan hapus fitur, jangan ubah schema DB/auth sembarangan, jangan commit password/secret/keystore.
- Jangan pakai `localhost`, `10.0.2.2`, atau IP LAN sebagai endpoint production; jangan buat fake API; jangan klaim sukses sebelum build Release benar-benar dilakukan.
- Git: kerjakan di branch (mis. `feat/android-production-apk-cli`), jangan langsung ke `main`.

## E.2 Release Configuration

Target command:

```bash
dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-android -c Release
dotnet publish GigRadarMobile/GigRadarMobile.csproj -f net10.0-android -c Release
```

- Output harus `.apk` (bukan hanya `.aab`): `<AndroidPackageFormat>apk</AndroidPackageFormat>` untuk Release.
- Tanpa development secrets, tanpa cleartext HTTP (`android:usesCleartextTraffic="false"`), tanpa debugging aktif.
- Jangan aggressive trimming bila merusak reflection/serialization/MAUI runtime — prioritas APK stabil.
- App identity tetap: `ApplicationTitle=GigRadar`, `ApplicationId=com.gigradar.app`, version dapat dinaikkan (`ApplicationDisplayVersion` = human-readable, `ApplicationVersion` = versionCode).
- Icon & splash memakai asset GigRadar (bukan default MAUI).
- Manifest: `INTERNET` wajib; location permission hanya bila dipakai; `MainLauncher=true`.

## E.3 API Configuration (Release)

```csharp
#if DEBUG && ANDROID
public const string BaseUrl = "http://10.0.2.2:5000";
#elif ANDROID
public const string BaseUrl = "https://YOUR-PRODUCTION-API-DOMAIN"; // wajib diganti sebelum release
#elif WINDOWS
public const string BaseUrl = "http://localhost:5000";
#else
public const string BaseUrl = "https://YOUR-PRODUCTION-API-DOMAIN";
#endif
```

Jangan mengarang domain fiktif yang seolah aktif; dokumentasikan bahwa URL harus diganti.

## E.4 Error Handling di Production

`ApiService` harus menangani: No Internet, Timeout, HTTP 401/403/404/500, invalid response,
malformed JSON. Tanpa stack trace ke user. 401 → flow logout/session expired.
Contoh pesan: "Tidak dapat terhubung ke server. Periksa koneksi internet dan coba lagi."

## E.5 JWT / Auth di Release

Alur `Login → JWT → AuthService → ApiService → Authorization: Bearer` tetap bekerja.
Jangan hardcode JWT/password/secret, jangan tampilkan token di UI/log. Evaluasi SecureStorage
untuk token; jangan refactor besar bila berisiko merusak login flow yang ada.

## E.6 Signing

Jangan commit keystore. Tambahkan `*.keystore`, `*.jks` ke `.gitignore`. Buat keystore:

```bash
keytool -genkeypair -v -keystore gigradar-release.keystore -alias gigradar \
  -keyalg RSA -keysize 2048 -validity 10000
```

Signing via environment variables (jangan hardcode store/key pass di csproj):

```text
GIGRADAR_KEYSTORE_PATH
GIGRADAR_KEYSTORE_PASSWORD
GIGRADAR_KEY_ALIAS
GIGRADAR_KEY_PASSWORD
```

Untuk CI/CD gunakan GitHub Actions Secrets: `ANDROID_KEYSTORE_BASE64`,
`ANDROID_KEYSTORE_PASSWORD`, `ANDROID_KEY_ALIAS`, `ANDROID_KEY_PASSWORD` — tanpa print ke log.
Workflow `.github/workflows/android-release.yml`: checkout → setup .NET → install MAUI Android
workload → restore → publish Release → artifact APK.

Local build script: `scripts/build-android-release.ps1` (restore → build/publish Release →
tampilkan lokasi APK → gagal dengan pesan jelas).

## E.7 Install & Validasi

```bash
adb devices
adb install -r <path-to-apk>
```

Instalasi manual: copy APK → buka → izinkan instalasi → install → buka GigRadar.
Validasi APK: file ada, size > 0, package ID `com.gigradar.app`, versionCode/Name benar,
Release configuration (gunakan `apkanalyzer` bila tersedia).

## E.8 Production Checklist & Jangan Palsu Sukses

Checklist (docs/ANDROID_PRODUCTION.md): HTTPS production API, tanpa localhost/10.0.2.2,
cleartext disabled, JWT/login/register/event/artist/ticket/EO dashboard/location/icon/splash
tested, Release build sukses, APK ter-install di device fisik, API connection via mobile
data/Wi-Fi, signing verified, tanpa secret ter-commit.

Wajib uji **device fisik** (install → open → login → API → JWT → events → artist → tickets →
profile → logout), bukan hanya emulator. Jika command gagal, laporkan `BUILD FAILED` +
command/error/file/saran perbaikan — JANGAN mengatakan "Build berhasil". Jika environment
tidak punya SDK/JDK/MAUI workload, deteksi dan laporkan. Jika production API belum ada:
tandai `BLOCKER` dan kerjakan bagian yang tidak membutuhkan API.

Laporan akhir memuat: branch, commit, target, konfigurasi, APK + size, package, version,
build PASS/FAIL, signing PASS/NOT CONFIGURED, API DEBUG/PRODUCTION/BLOCKED, physical device
PASS/NOT TESTED, files changed, commands executed, problems, next steps.

---

# LAMPIRAN F — DATABASE: POSTGRESQL & MIGRASI

> Sumber: migrate sqlite to postgre.md. Status saat ini: migrasi **sudah dilakukan** di
> branch `feat/postgresql-migration` — backend memakai `Npgsql.EntityFrameworkCore.PostgreSQL`
> dan skema dimiliki EF Core Migrations (`db.Database.Migrate()` di `Program.cs`),
> menggantikan pola lama `EnsureCreated()`. `TicketSchemaBootstrap` tinggal seed/idempotent
> tambahan, bukan penambal skema.

## F.1 Arsitektur Database Saat Ini

```text
.NET MAUI (Android/iOS/Mac/Windows)
        │ HTTPS
        ▼
ASP.NET Core Web API
        │ EF Core + Npgsql
        ▼
PostgreSQL
```

- MAUI TIDAK PERNAH terhubung langsung ke PostgreSQL — hanya via API.
- Connection string TIDAK disimpan di `appsettings.json`. Development: `dotnet user-secrets`
  (`ConnectionStrings:DefaultConnection`). Production: environment variable
  `DATABASE_URL` / `ConnectionStrings__DefaultConnection`.
- Jangan commit password PostgreSQL, JWT secret, atau kredensial database ke Git.
- Production connection string memakai `SSL Mode=Require` bila provider mendukung.

## F.2 Aturan yang Dipertahankan dari Panduan Migrasi

- Jangan hapus database/migration lama sebelum audit; jangan ubah API contract tanpa alasan.
- Perhatikan kompatibilitas PostgreSQL: `bool`, `DateTime` (UTC konsisten), `decimal`
  (precision eksplisit untuk harga/fee, mis. `[Precision(18, 2)]`), `string` max-length,
  `Guid`, sequence/identity setelah import data.
- Cari SQL spesifik provider (`PRAGMA`, `INSERT OR REPLACE`, `datetime('now')`,
  `AUTOINCREMENT`) dan sesuaikan bila masih ada.
- Authentication (register/login/BCrypt/JWT/roles) harus tetap berfungsi setelah perubahan DB.
- Seed data: hati-hati duplicate key, GUID, DateTime, unique constraint, password hash (bukan plaintext).
- Setelah perubahan besar: `dotnet build` harus hijau sebelum lanjut.
- Health check `/health` boleh ditambahkan, tanpa expose informasi sensitif.
- Jangan log password/JWT/refresh token/connection string lengkap.

## F.3 Validasi Saat Ini

```bash
dotnet build GigRadarApi/GigRadarApi.csproj
dotnet test
```

Validasi minimal: koneksi PostgreSQL, migration sukses, register/login/JWT, CRUD event,
flow tiket, MAUI ↔ API connection, tidak ada dependency SQLite yang tersisa, tidak ada
kredensial di source code.

---

# LAMPIRAN G — ROLE EXPERIENCE DETAILS

> Sumber: GIGRADAR_ROLE_SYSTEM.md — detail pengalaman yang tidak diulang di Bab 4–17.

## G.1 User Experience

Fokus: `Discover → Follow → Buy → Attend`. UI sederhana dan berorientasi discovery.

Home (pusat discovery): event berdasarkan lokasi, event terdekat, malam ini, akhir pekan,
rekomendasi, artist berdasarkan genre, artist yang diikuti, event favorit, event populer.

```text
HOME
├── Search
├── Location
├── Recommended Gigs
├── Nearby Gigs
├── Tonight
├── This Weekend
├── Popular Artists
└── Followed Artists
```

Profile: Edit Profile · Music Preferences · Favorite Events · Followed Artists · My Tickets · Logout.

## G.2 Artist Experience & Journey

Fokus: `Create → Share → Build Audience → Perform`.

Artist Dashboard (pusat pengelolaan karier): jumlah follower, lagu, album, post, gigs
mendatang, performa konten, event yang diikuti.

```text
ARTIST DASHBOARD

Followers       1,250
Songs              12
Albums              2
Upcoming Gigs       4

Upcoming:
- Local Underground Fest
- Jakarta Music Night
```

**Artist Journey** — dokumentasi perkembangan artist (fitur pembeda GigRadar):

```text
Artist Journey
2024  Band terbentuk.
2025  Single pertama dirilis. Showcase pertama.
2026  Album pertama. Festival pertama.
```

Data: tahun/tanggal, judul, deskripsi, gambar, kategori:
`Formation, Release, Gig, Achievement, Album, Single, Other`.

Artist dapat mengelola: nama, foto, cover, bio, genre, social media, anggota band, kontak,
lokasi, link musik. Profil publik dilihat User dan EO. Artist yang masuk lineup event
melihat event tersebut di halaman gigs mereka.

## G.3 EO Experience

Fokus: `Create → Promote → Sell → Manage → Analyze`. UI operasional dan berorientasi data.

```text
EVENT ORGANIZER DASHBOARD
My Events       12
Published        8
Upcoming         4
Tickets Sold   840
```

EO dapat melihat event publik milik EO lain (view only) untuk memantau ekosistem, tetapi
hanya dapat mengelola event miliknya sendiri:

```text
EO A
  ├── Event A1 → OWN    → Manage
  ├── Event A2 → OWN    → Manage
  ├── Event B1 → PUBLIC → View Only (EO B)
  └── Event C1 → PUBLIC → View Only (EO C)
```

Form Create Event: Event Name, Description, Poster, Date, Start/End Time, Venue, City,
Genre, Artists, Ticket Types, External Ticket Link, Status. Event baru dibuat sebagai
**Draft**, kemudian EO publish.

## G.4 Admin Experience

Fokus: `Monitor → Moderate → Manage → Analyze`. Admin = pusat kontrol platform
(lihat Bab 16–22 untuk CRUD, Verification Center, dan Audit Log).

## G.5 Event Lifecycle & Ticket Flow (ringkas)

```text
Draft → Published → SoldOut / Completed
```

- Draft: belum tampil publik. Published: tampil di discovery, dapat dibeli.
- SoldOut: event ada, tiket habis. Completed: selesai.
- Ticket: Event Detail → Selection → Checkout → Payment → Ticket Success → My Tickets.
  Tiket menampilkan event, venue, tanggal/jam, type, order number, barcode/QR.

## G.6 UX Fokus Per Role

| Role | Fokus | Orientasi UI |
|---|---|---|
| User | Discover → Follow → Buy → Attend | Discovery |
| Artist | Create → Share → Build Audience → Perform | Musik, konten, follower, gigs |
| EO | Create → Promote → Sell → Manage → Analyze | Operasional & data |
| Admin | Monitor → Moderate → Manage | Kontrol platform |

---

# LAMPIRAN H — EXTENDED ENTITY REFERENCE

> Sumber: GIGRADAR_ROLE_SYSTEM.md §28 — tabel pengembangan social/artist yang melengkapi
> Bab 39–45. Semua tabel mengikuti konvensi: primary key, timestamp `CreatedAt`/`UpdatedAt`,
> unique constraint di relasi many-to-many.

```text
ArtistPosts      : Id, ArtistId, Title, Content, ImageUrl, CreatedAt, UpdatedAt, IsPublished
ArtistAlbums     : Id, ArtistId, Title, CoverUrl, Description, ReleaseDate, CreatedAt
ArtistMembers    : Id, ArtistId, Name, Role, PhotoUrl, JoinedAt
ArtistFollowers  : Id, ArtistId, UserId, CreatedAt (unique: ArtistId + UserId)
ArtistGenres     : ArtistId, GenreId
ArtistJourney    : Id, ArtistId, Title, Description, Date, ImageUrl, Category
PostLikes        : Id, PostId, UserId, CreatedAt
PostComments     : Id, PostId, UserId, Content, CreatedAt, UpdatedAt
EventViews       : Id, EventId, UserId, ViewedAt
EventAnalytics   : Id, EventId, Date, Views, TicketSales, Revenue, Favorites
```

---

# LAMPIRAN I — INTEGRASI EKSTERNAL & MODEL BISNIS

> Sumber: GIGRADAR_MOBILE_APP_NET_MAUI.md.

## I.1 Integrasi Eksternal (opsional/per fase)

- **Google Maps Platform** — peta & lokasi (MVP: daftar + link Google Maps eksternal via `Launcher`, lihat Bab 34/59).
- **Firebase Cloud Messaging** — push notification (Bab 36).
- **Spotify Web API** — dukung verifikasi artist (Bab 9); OAuth, minimal scope, bukan bukti identitas otomatis.
- **Payment gateway Indonesia** (Midtrans/Xendit) — pembayaran tiket (Bab 29); status pembayaran selalu server-side.
- **Object storage** — poster, foto artist, aset event (Bab 68).
- **Audio preview** — preview lagu artist (play/pause/progress/link platform musik); durasi mengikuti aturan API musik.

## I.2 Onboarding

```text
Welcome → Pilih Kota/Lokasi → Pilih Genre Favorit → Pilih Artist Favorit
        → Izinkan Lokasi → Home
```

Genre seed: Indie, Alternative, Rock, Punk, Hardcore, Shoegaze, Emo, Metal, Jazz, Folk,
Electronic, Pop. Data onboarding = initial preference profile untuk rekomendasi.

## I.3 Model Bisnis

- **Komisi ticketing**: 2–5% dari transaksi.
- **Promoted gigs**: EO membayar untuk featured event / posisi rekomendasi / campaign;
  konten berbayar wajib diberi penanda jelas.
- **Insight untuk EO**: data agregat — genre populer, kota permintaan tinggi, waktu event
  populer, tren pencarian. Analitik selalu agregat/anonim sesuai ketentuan privasi.
- **Crowdfunding / support artist** (fase lanjut): nominal dukungan, target, progress,
  daftar proyek, riwayat — semua transaksi via payment gateway resmi.

## I.4 Roadmap Tingkat Tinggi (menggantikan roadmap dokumen lama)

```text
Phase 1 MVP    : MAUI, Login, Onboarding, Home, Event, Search, Map/Radar, Artist,
                 Ticket, basic recommendation
Phase 2        : Payment gateway, push notification, favorite, follow, community, EO dashboard
Phase 3        : Personalized recommendation, audio preview, crowdfunding, analytics
Phase 4        : ML recommendation, multi-city expansion, advanced personalization
```

(Detail urutan implementasi teknis: Bab 74, 75, 81.)

---

# LAMPIRAN J — STATUS IMPLEMENTASI SAAT INI

> Sumber: REKAPAN_PROJECT.md & REKAPAN_PROJECT_TERBARU.md, direkonsiliasi dengan kode
> di branch `feat/postgresql-migration`.

## J.1 Komponen

| Komponen | Teknologi | Keterangan |
|---|---|---|
| `GigRadarApi` | ASP.NET Core Web API (.NET 8) | REST + JWT + Swagger, EF Core + Npgsql (PostgreSQL) |
| `GigRadarMobile` | .NET MAUI (.NET 10) + MVVM | Android, iOS, Mac Catalyst, Windows; CommunityToolkit.Mvvm |
| `GigRadarLauncher` | .NET Console (.NET 8) | Menjalankan API di localhost:5000 + buka Swagger |

## J.2 Yang Sudah Ada

- Auth register/login (JWT + BCrypt), onboarding genre, profil & preferensi.
- List & detail event, artist + audio preview UI, nearby/tonight/weekend, rekomendasi rule-based.
- **Alur beli tiket 3 tahap** (pilih tipe → data diri & bayar → barcode) + My Tickets.
- **Tipe tiket per event** (stok & harga) + link pembelian eksternal (TicketLink).
- **Verifikasi pembeli**: nama ≥ 3, telepon ≥ 9 digit, email valid, umur ≥ 17, cek stok (stok −1 otomatis).
- **CRUD event lengkap** dengan `CreatedBy` dari JWT; status Published/Draft/SoldOut/Completed; blokir pembelian non-Published.
- **Dashboard & profil EO/Admin**: statistik, kelola tipe tiket, buat event, tandai habis/selesai, hapus event.
- **Role-based authorization + cek kepemilikan** (EO hanya event sendiri; selain itu 403).
- **Validasi QR tiket** (`POST /api/tickets/validate`, EO/Admin).
- Radar native (`GraphicsView` + `RadarDrawable`), Explore search/filter List/Grid/Radar,
  design system token diterapkan (Lampiran A).
- Database: PostgreSQL + EF Migrations (Lampiran F); akun seed Admin & EO.

## J.3 Perlu Disempurnakan

- Pembayaran masih simulasi (belum ada payment gateway).
- Barcode tiket masih pola visual (bukan QR image standar); validasi via kode QR teks.
- Peta interaktif native → masih daftar + link Google Maps eksternal.
- Home belum memakai endpoint recommended/nearby server (filter di client).
- Audio preview belum memutar lagu sungguhan.
- Form edit event di UI belum ada (backend PUT sudah ada).
- Register yang memohon role Artist/EO harus tetap melalui alur aplikasi + verifikasi (Bab 7, 13; implementasi `AuthService` saat ini sudah mengikuti pola ini).

## J.4 Belum Ada (Fase Lanjut)

Payment gateway, push notification (FCM), social feed lengkap (posts/likes/comments UI),
merchandise, verification center UI Admin, komunitas/follow venue-EO, crowdfunding,
analytics penuh, ML rekomendasi, audio streaming, multi-city, edit line-up dari UI.

## J.5 Cara Menjalankan (cepat)

```bash
# Backend (Swagger di http://localhost:5000/swagger)
dotnet run --project GigRadarApi --urls http://localhost:5000

# Mobile (Windows)
dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-windows10.0.19041.0

# Android
dotnet build GigRadarMobile/GigRadarMobile.csproj -f net10.0-android
```

Akun seed: `admin@gigradar.com` / `admin123` (Admin), `eo@gigradar.com` / `eo123` (EO).

---

# LAMPIRAN K — PETA DOKUMENTASI

```text
GIGRADAR_APP_SPEC.md      ← DOKUMEN TUNGGAL (bab 1–84 + lampiran A–K)
README.md                 ← Ringkasan proyek, cara menjalankan, endpoint
GEMINI.md                 ← Workspace rules untuk AI agents (konvensi kode & build)
docs/ANDROID_PRODUCTION.md← Checklist produksi Android (Lampiran E)
```

Dokumen berikut sudah dihapus dari root karena isinya melebur ke sini:

| Dokumen lama | Dialihkan ke |
|---|---|
| GIGRADAR_ROLE_SYSTEM.md | Bab 3–17, 53–55; Lampiran G, H |
| GIGRADAR_MOBILE_APP_NET_MAUI.md | Bab 1, 33–36; Lampiran I |
| GIGRADAR_DESIGN_SYSTEM.md | Bab 56–57, 62; Lampiran A, B |
| GIGRADAR_MULTIPLATFORM_FIX.md | Bab 58, 61; Lampiran D |
| GPS_LOCATION.md | Bab 34; Lampiran C |
| GigRadar android.md | Lampiran E |
| migrate sqlite to postgre.md | Lampiran F |
| REKAPAN_PROJECT.md / REKAPAN_PROJECT_TERBARU.md | Lampiran J |

Referensi kode ke dokumen lama (mis. `GIGRADAR_ROLE_SYSTEM.md §26`) tetap valid sebagai
sitasi sejarah; rujukan ke depan adalah `GIGRADAR_APP_SPEC.md`.

---

# END OF GIGRADAR APP SPEC

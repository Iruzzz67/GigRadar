# RECLUB RESEARCH → GIGRADAR

> **Purpose:** Competitive/product research for adapting ideas from
> Reclub into GigRadar.\
> **Date:** 2026-09-28\
> **Scope:** Reclub's publicly documented product capabilities, mapped
> to GigRadar's Artist ↔ Event Organizer (EO) marketplace/community
> model.

------------------------------------------------------------------------

## 1. Executive Summary

Reclub is a sports-community platform that centralizes discovery, clubs,
activities/meets, competitions, communication, payments, member
management, and community growth. Its official product material
describes four major product areas: **Clubs, Meets, Competitions, and
Community**. Its current Help Center also documents discovery across
meets/clubs/users/competitions/coaches, venue discovery,
RSVP/invitations, payments, member/organizer management, and
support/reporting workflows.

For GigRadar, the important lesson is not to clone Reclub's sports
mechanics. The reusable product patterns are:

1.  **Discovery first** --- users should quickly find relevant
    opportunities/people.
2.  **Structured communities** --- groups can become persistent networks
    rather than one-off events.
3.  **Availability + RSVP/waitlist** --- participation state should be
    explicit.
4.  **Organizer tooling** --- EO needs operational tools, not only a
    public profile.
5.  **Contextual communication** --- chat should be connected to an
    event/booking.
6.  **Reputation and history** --- completed activities create useful
    trust signals.
7.  **Venue/location intelligence** --- location should be a first-class
    discovery dimension.
8.  **Network effects** --- follow/community/activity mechanisms can
    make the platform useful between bookings.

### Decision legend

  -----------------------------------------------------------------------
  Decision                            Meaning
  ----------------------------------- -----------------------------------
  **ADAPT**                           The Reclub pattern maps directly to
                                      GigRadar with mostly
                                      domain-language changes.

  **MODIFY**                          The idea is useful, but GigRadar
                                      needs a materially different
                                      workflow/data model.

  **SKIP**                            Reclub's feature is too
                                      sport-specific or creates
                                      unnecessary complexity for the
                                      current GigRadar scope.

  **NEW FOR GIGRADAR**                Not a direct Reclub feature;
                                      proposed because it solves a core
                                      Artist ↔ EO problem.
  -----------------------------------------------------------------------

------------------------------------------------------------------------

# 2. Reclub Feature Inventory

The following inventory is based on Reclub's public website, Google Play
listing, and Help Center. Reclub describes itself as a platform for
sports communities, with tools to run clubs, organize meets, manage
competitions, and build community. Its app listing also highlights
discovery, meets, club management, competitions, and chat.

## 2.1 Core Product Features

  ---------------------------------------------------------------------------------------------
  Reclub capability      Evidence                        GigRadar decision GigRadar
                                                                           interpretation
  ---------------------- ------------------------------- ----------------- --------------------
  Discover sports near   Discover tab + filters          **ADAPT**         Discover gigs,
  you                                                                      artists, EOs,
                                                                           venues, communities

  Search/filter by       Discovery docs                  **MODIFY**        Filter by genre,
  sport/location/skill                                                     location, budget,
                                                                           date, event type,
                                                                           availability

  Clubs                  Club creation + member          **MODIFY**        Artist communities,
                         management                                        EO organizations,
                                                                           music collectives

  Club member management Club tools                      **ADAPT**         Manage artists,
                                                                           staff,
                                                                           collaborators,
                                                                           applicants

  Meets/events           Create and manage activities    **ADAPT**         Gig/event creation
                                                                           and management

  RSVP                   Event participation             **ADAPT**         Artist application /
                                                                           EO invitation /
                                                                           booking acceptance

  Waitlist               Meet waitlist                   **ADAPT**         Artist waitlist when
                                                                           event slots are full

  Invitations            Centralized invitations         **ADAPT**         EO invites artist;
                                                                           artist receives gig
                                                                           invitation

  Team assignments       Meet team assignment            **MODIFY**        Lineup/slot
                                                                           assignment,
                                                                           performance
                                                                           schedule, stage
                                                                           order

  Competitions           Brackets/leagues/registration   **SKIP**          Not central to
                                                         initially         ordinary gig booking

  Match/player tracking  Sports match tracking           **SKIP**          Replace with
                                                                           gig/booking history

  Chat                   Athlete communication           **ADAPT**         Artist ↔ EO
                                                                           contextual messaging

  Community feed /       Community sharing               **MODIFY**        Gig posts, artist
  moments                                                                  updates, event
                                                                           recaps, portfolio
                                                                           content

  Payments               Payment                         **MODIFY**        Booking deposits,
                         methods/subscription/spot                         invoices, payouts,
                         confirmation                                      payment status

  Venue discovery        Venue-first discovery           **ADAPT**         Venue-first
                                                                           gig/event discovery

  Location/map           Venue/map exploration           **ADAPT**         Event/venue/artist
                                                                           geographic discovery

  Automatic              Club/event notifications        **ADAPT**         Booking,
  notifications                                                            application,
                                                                           message, event and
                                                                           availability alerts

  Organizer roles        Club organizer/admin concepts   **MODIFY**        EO owner, manager,
                                                                           staff, event
                                                                           coordinator

  Reporting              Report player/community issue   **ADAPT**         Report user, event,
                                                                           fraud, inappropriate
                                                                           content

  Account deletion/help  Support workflow                **ADAPT**         Standard
                                                                           account/privacy
                                                                           lifecycle

  Support/feedback       Help center + feedback          **ADAPT**         In-app support and
                                                                           issue reporting
  ---------------------------------------------------------------------------------------------

------------------------------------------------------------------------

# 3. Feature-by-Feature Mapping

## 3.1 Discover

### Reclub

Reclub provides a Discover experience for finding meets, clubs, users,
competitions, and coaches. Filters include dimensions such as sport,
location, and skill level.

### GigRadar

**Decision: ADAPT**

Make Discover the primary marketplace surface.

### Proposed information architecture

``` text
DISCOVER
├── Gigs
├── Artists
├── Event Organizers
├── Venues
├── Communities
└── My Recommendations
```

### Filters

``` text
Date
Location
Radius
Genre
Event Type
Budget
Duration
Artist Type
Availability
Rating
Experience
```

### UX principle

Do not force users to know exactly what they want.

Support searches such as:

-   "artist for wedding"
-   "rock band near Depok"
-   "DJ under Rp 3M"
-   "available 24 October"
-   "EO looking for acoustic duo"

------------------------------------------------------------------------

# 4. Clubs → GigRadar Communities

### Reclub

Clubs are persistent communities. Organizers create a club, configure
it, invite members, manage activities, and communicate with members.

### GigRadar

**Decision: MODIFY**

A literal "Club" model is too sports-specific. Use **Community** or
**Collective**.

Examples:

``` text
Indie Musicians Depok
Jakarta Wedding Musicians
Local DJ Community
Independent Event Organizers
Campus Music Community
```

### Community capabilities

``` text
Community
├── Members
├── Events
├── Posts
├── Announcements
├── Chat
├── Rules
└── Admins
```

### Why it matters

A marketplace can become empty between transactions. Communities create
reasons to return even when a user is not currently booking a gig.

------------------------------------------------------------------------

# 5. Meets → Gigs

### Reclub

A Meet is a scheduled activity with details, participants,
RSVP/waitlist, and organizer controls.

### GigRadar

**Decision: ADAPT**

A Gig should become a first-class object.

``` text
Gig
├── Event
├── Date
├── Time
├── Venue
├── Genre
├── Budget
├── Required Artists
├── Requirements
├── Application Status
├── Booking Status
└── Communication
```

### Gig lifecycle

``` text
DRAFT
  ↓
OPEN
  ↓
APPLICATIONS
  ↓
SHORTLISTED
  ↓
NEGOTIATION
  ↓
OFFERED
  ↓
CONFIRMED
  ↓
COMPLETED
  ↓
REVIEWED
```

This lifecycle should become one of GigRadar's central domain workflows.

------------------------------------------------------------------------

# 6. RSVP → Application / Invitation / Booking

### Reclub

Players can RSVP to a meet and respond to invitations.

### GigRadar

**Decision: MODIFY**

GigRadar needs multiple participation states.

``` text
Artist
 ├── APPLY
 ├── ACCEPT INVITATION
 ├── DECLINE
 ├── WITHDRAW
 └── ACCEPT BOOKING

EO
 ├── INVITE
 ├── SHORTLIST
 ├── REJECT
 ├── SEND OFFER
 └── CONFIRM BOOKING
```

Do not collapse these into a single `RSVP` boolean.

Recommended state model:

``` text
ApplicationStatus
- Pending
- Shortlisted
- Rejected
- Withdrawn

BookingStatus
- Proposed
- Negotiating
- Confirmed
- Cancelled
- Completed
```

------------------------------------------------------------------------

# 7. Waitlist → Artist Waitlist

### Reclub

When an activity reaches capacity, users can be waitlisted.

### GigRadar

**Decision: ADAPT**

Useful when an event has limited artist slots.

Example:

``` text
Rock Festival
Artist slots: 5/5

Waitlist:
1. Reality Band
2. Midnight Noise
3. Echo Project
```

If a confirmed artist cancels, the EO can invite the next eligible
artist.

------------------------------------------------------------------------

# 8. Competitions

### Reclub

Reclub supports tournament registration, brackets, leagues, match
generation, and results.

### GigRadar

**Decision: SKIP initially**

Most GigRadar transactions are not competitions.

Do not add tournament infrastructure to the MVP.

### Possible future use

Could become useful if GigRadar later supports:

-   Battle of Bands
-   DJ competitions
-   talent contests
-   audition rounds
-   festival selection stages

Until then, keep this outside the core architecture.

------------------------------------------------------------------------

# 9. Team Assignment → Lineup / Slot Management

### Reclub

Reclub can assign players/teams for activities.

### GigRadar

**Decision: MODIFY**

For an event, the equivalent is a **performance lineup**.

``` text
Event
│
├── Main Stage
│   ├── 19:00 Artist A
│   ├── 20:00 Artist B
│   └── 21:00 Artist C
│
└── Acoustic Stage
    ├── 19:30 Artist D
    └── 20:30 Artist E
```

This becomes valuable for festivals and multi-artist events.

------------------------------------------------------------------------

# 10. Chat

### Reclub

Chat connects community members and helps coordinate activities.

### GigRadar

**Decision: ADAPT**

But GigRadar should make chat **context-aware**.

Instead of:

``` text
Artist ↔ EO
```

Use:

``` text
Conversation
├── Event
├── Application
├── Booking
├── Attachments
├── Contract
└── Payment context
```

Example:

``` text
Indie Night
Reality Band ↔ EO

Budget: Rp 3.000.000
Date: 20 Oct
Status: Negotiating

[View Booking]
```

This prevents important transaction information from getting lost in
generic chat.

------------------------------------------------------------------------

# 11. Community Moments → Gig Feed

### Reclub

Reclub provides community-oriented social sharing.

### GigRadar

**Decision: MODIFY**

Create a professional activity feed.

Possible post types:

``` text
🎵 New Release
🎤 Gig Completed
📸 Event Recap
📢 Looking for Members
🔥 Available for Booking
🎪 New Event
```

Avoid turning GigRadar into a generic social-media clone.

The feed should reinforce the marketplace.

------------------------------------------------------------------------

# 12. Venue Discovery

### Reclub

Reclub added venue-first discovery: users can start from a physical
venue and see associated clubs and upcoming activities. It supports
venue profiles, map pins, activities, clubs, distance, and venue
filters.

### GigRadar

**Decision: ADAPT**

This is highly relevant.

Create:

``` text
VENUES
├── Cafes
├── Hotels
├── Wedding Halls
├── Concert Halls
├── Bars
├── Outdoor Venues
└── Event Spaces
```

Venue profile:

``` text
Venue
├── Photos
├── Address
├── Capacity
├── Facilities
├── Equipment
├── Upcoming Events
├── Past Events
├── Resident Artists
├── Reviews
└── Contact / Booking
```

------------------------------------------------------------------------

# 13. Location / Map

### Reclub

Location is used in discovery and venue exploration.

### GigRadar

**Decision: ADAPT**

Use maps for:

-   nearby gigs
-   nearby venues
-   artists near event location
-   EO locations
-   community discovery

Important:

**Do not expose exact private addresses unnecessarily.**

For artist discovery, initially show approximate location or city/area.

------------------------------------------------------------------------

# 14. Notifications

### Reclub

Reclub uses automatic notifications to keep club members informed about
activities.

### GigRadar

**Decision: ADAPT**

Notification categories:

``` text
BOOKING
- Offer received
- Offer accepted
- Booking confirmed
- Booking cancelled

GIG
- New matching gig
- Application update
- Event reminder

MESSAGE
- New message

AVAILABILITY
- Artist became available
- EO request

COMMUNITY
- New event
- New post
```

Notifications should deep-link directly to the relevant object.

------------------------------------------------------------------------

# 15. Payments

### Reclub

Reclub documents payment methods, payment flows, subscription benefits,
refunds, and payment confirmation for activities.

### GigRadar

**Decision: MODIFY**

Payments are more important in GigRadar because the platform is
connecting commercial gigs.

Recommended future flow:

``` text
EO
 ↓
Booking Offer
 ↓
Deposit
 ↓
Booking Confirmed
 ↓
Event Completed
 ↓
Remaining Payment
 ↓
Artist Payout
```

For MVP, implement payment status before implementing a complicated
payout system.

``` text
PaymentStatus
- Unpaid
- DepositPending
- DepositPaid
- FullyPaid
- Refunded
- Failed
```

------------------------------------------------------------------------

# 16. Ratings / Reputation

### Reclub

Reclub has community recognition and sports performance/rating
integrations in some contexts.

### GigRadar

**Decision: NEW FOR GIGRADAR**

Build a two-sided reputation model.

### Artist reputation

``` text
Performance
Communication
Punctuality
Professionalism
Quality
```

### EO reputation

``` text
Communication
Payment Reliability
Event Organization
Professionalism
Venue Accuracy
```

This is important because GigRadar is a two-sided marketplace.

Do not create a single simplistic "star score" as the only trust signal.

------------------------------------------------------------------------

# 17. Artist Availability

### Reclub analogue

Reclub is built around scheduled activities and participation.

### GigRadar

**Decision: NEW FOR GIGRADAR**

Create an explicit availability calendar.

``` text
Available
Tentative
Booked
Unavailable
```

EO can query:

``` text
Date: 24 Oct
Genre: Rock
Location: Depok
Budget: Rp 2–4M

→ show artists available on that date
```

This should feed the matching engine.

------------------------------------------------------------------------

# 18. AI Matching

### Reclub analogue

Reclub has discovery and filters, but GigRadar can take the concept
further.

### GigRadar

**Decision: NEW FOR GIGRADAR**

Create a matching layer.

``` text
Artist ↔ Gig

Signals:
- Genre
- Location
- Availability
- Budget
- Event type
- Experience
- Equipment
- Past bookings
- Preferences
- Rating/reputation
```

Example:

``` text
92% Match

Rock Festival
✓ Genre
✓ Available
✓ Budget
✓ Location
✓ Experience
```

The score should be explainable.

Do not show an unexplained AI number without telling the user which
factors contributed.

------------------------------------------------------------------------

# 19. Search

### Reclub analogue

Discovery uses structured filters.

### GigRadar

**Decision: ADAPT + MODIFY**

Support both:

### Structured search

``` text
Genre: Jazz
Location: Jakarta
Budget: 3–5M
Date: 24 Oct
```

### Natural language search

``` text
"cari band jazz untuk wedding
di Jakarta tanggal 24 Oktober
budget 5 juta"
```

The natural-language layer can later translate user intent into
structured search filters.

------------------------------------------------------------------------

# 20. Organizer / Staff Roles

### Reclub

Reclub supports club administration and organizer concepts.

### GigRadar

**Decision: MODIFY**

EO organizations should support role-based access.

``` text
EO Owner
│
├── Event Manager
├── Booking Manager
├── Finance
└── Staff
```

Permissions:

``` text
Events
Artists
Bookings
Payments
Messages
Reports
Organization Settings
```

This becomes important once an EO manages multiple events.

------------------------------------------------------------------------

# 21. Reporting & Moderation

### Reclub

The Help Center documents player reporting and support workflows.

### GigRadar

**Decision: ADAPT**

Implement:

``` text
Report User
Report Event
Report Message
Report Review
Report Fraud
Report Copyright/Content
```

Admin workflow:

``` text
Report
 ↓
Moderation Queue
 ↓
Review
 ↓
Action
 ↓
Audit Log
```

------------------------------------------------------------------------

# 22. Support & Feedback

### Reclub

Reclub provides in-app support and issue-reporting workflows.

### GigRadar

**Decision: ADAPT**

Create:

``` text
Help Center
FAQ
Report Problem
Contact Support
Report Transaction
Account & Privacy
```

Support requests should carry context automatically:

``` text
User
Event ID
Booking ID
Conversation ID
Error
Timestamp
```

This reduces support friction.

------------------------------------------------------------------------

# 23. Features to SKIP from Reclub

These should not be copied into the initial GigRadar roadmap.

  -----------------------------------------------------------------------
  Reclub concept          Decision                Reason
  ----------------------- ----------------------- -----------------------
  Sports-specific player  **SKIP**                Wrong domain
  ratings                                         

  Match result tracking   **SKIP**                Not needed for ordinary
                                                  gigs

  Tournament brackets     **SKIP**                Not MVP

  DUPR integration        **SKIP**                Sports-specific

  Team generator          **SKIP**                Replace with
                                                  lineup/crew tools

  Sport skill levels      **SKIP**                Replace with
                                                  genre/experience

  Sport-specific          **SKIP**                Adds unnecessary domain
  competition logic                               complexity
  -----------------------------------------------------------------------

------------------------------------------------------------------------

# 24. GigRadar Features Reclub Does Not Directly Solve

These are proposed as **NEW FOR GIGRADAR**.

## 24.1 Artist Portfolio

``` text
Artist
├── Bio
├── Genres
├── Photos
├── Videos
├── Audio
├── Social Links
├── Previous Events
├── Equipment
├── Technical Rider
└── Booking Price Range
```

## 24.2 Event Requirements

EO should specify:

``` text
Genre
Artist Type
Set Duration
Number of Sets
Equipment
Stage Size
Sound System
Lighting
Crew Requirements
Budget
```

## 24.3 Contract Management

``` text
Booking
 ↓
Contract Draft
 ↓
Artist Approval
 ↓
EO Approval
 ↓
Signed
```

## 24.4 Invoice

``` text
Invoice
├── Artist
├── EO
├── Event
├── Amount
├── Tax/Fees
├── Due Date
└── Status
```

## 24.5 Cancellation Policy

Booking should have explicit cancellation rules.

``` text
CancellationPolicy
├── Artist cancellation
├── EO cancellation
├── Notice period
├── Deposit handling
└── Refund rules
```

## 24.6 Artist Crew Management

A band may contain multiple people.

``` text
Band
├── Manager
├── Vocalist
├── Guitarist
├── Bassist
└── Drummer
```

The account should not force every performer to operate as an
independent booking entity.

------------------------------------------------------------------------

# 25. Recommended GigRadar Navigation

## Artist

``` text
Home
Discover
Bookings
Messages
Profile
```

Home:

``` text
Good morning, Reality Band

🔥 Recommended gigs
📅 Your availability
💬 Recent messages
📋 Active bookings
⭐ Profile performance
```

## EO

``` text
Home
Discover Artists
My Events
Messages
Organization
```

Home:

``` text
Good morning, Event Organizer

📅 Upcoming events
🎤 Artist recommendations
📨 Applications
💬 Conversations
💰 Booking/payment status
```

------------------------------------------------------------------------

# 26. Recommended Data Model

A possible high-level domain model:

``` text
User
├── ArtistProfile
├── EOProfile
└── OrganizationMembership

Artist
├── Genres
├── Portfolio
├── Availability
├── Equipment
├── Reviews
└── BookingHistory

Organization
├── Members
├── Roles
├── Events
└── Reviews

Event
├── Venue
├── Requirements
├── Gigs
├── Applications
├── Invitations
└── Bookings

Gig
├── Event
├── ArtistRequirement
├── Applications
├── Offers
└── Booking

Booking
├── Artist
├── Organization
├── Event
├── Contract
├── Payment
├── Messages
└── Reviews

Venue
├── Location
├── Facilities
├── Events
└── Reviews

Community
├── Members
├── Posts
├── Events
└── Moderators
```

------------------------------------------------------------------------

# 27. Recommended Backend Boundaries

For a clean architecture, keep domains separated.

``` text
Identity
  └── Users / Authentication / Authorization

Artists
  └── Profiles / Portfolio / Availability

Organizations
  └── EO / Staff / Roles

Events
  └── Event / Gig / Venue

Discovery
  └── Search / Filters / Recommendations

Applications
  └── Apply / Invite / Shortlist

Bookings
  └── Offer / Confirmation / Cancellation

Messaging
  └── Conversations / Messages

Payments
  └── Invoice / Deposit / Payment / Payout

Reviews
  └── Artist Reviews / EO Reviews

Communities
  └── Communities / Posts / Members

Notifications
  └── Push / Email / In-App

Moderation
  └── Reports / Audit / Safety
```

------------------------------------------------------------------------

# 28. MVP Priority

## P0 --- Core Marketplace

Implement first:

-   Artist profile
-   EO profile
-   Event creation
-   Gig creation
-   Discover
-   Search
-   Filters
-   Artist availability
-   Application
-   Invitation
-   Shortlist
-   Booking
-   Chat
-   Notifications

## P1 --- Trust & Operations

Then:

-   Reviews
-   Two-sided reputation
-   Payment status
-   Invoice
-   Contract
-   Cancellation policy
-   Venue profiles
-   Map
-   EO staff roles

## P2 --- Network Effects

Then:

-   Follow
-   Communities
-   Activity feed
-   Event sharing
-   Artist/EO networking
-   Saved venues
-   Saved artists

## P3 --- Intelligence

Then:

-   AI matching
-   Natural language search
-   Smart recommendations
-   Pricing suggestions
-   Personalized discovery
-   Matching explanations

------------------------------------------------------------------------

# 29. Suggested MVP User Journeys

## Artist

``` text
Register
 ↓
Choose Artist
 ↓
Create Profile
 ↓
Add Genre + Location
 ↓
Add Portfolio
 ↓
Set Availability
 ↓
Discover Gigs
 ↓
Apply
 ↓
Chat with EO
 ↓
Receive Offer
 ↓
Accept
 ↓
Perform
 ↓
Complete Booking
 ↓
Receive Review
```

## EO

``` text
Register
 ↓
Choose Event Organizer
 ↓
Create Organization
 ↓
Create Event
 ↓
Define Artist Requirements
 ↓
Publish Gig
 ↓
Receive Applications
 ↓
Review Artists
 ↓
Shortlist
 ↓
Chat
 ↓
Send Offer
 ↓
Confirm Booking
 ↓
Manage Event
 ↓
Complete Event
 ↓
Review Artist
```

------------------------------------------------------------------------

# 30. UI/UX Direction Inspired by Reclub

Do not copy Reclub's visual identity.

Adapt the **interaction principles**.

### Principle 1 --- Discovery is prominent

The first meaningful action should be finding:

-   a gig
-   an artist
-   an EO
-   a venue
-   a community

### Principle 2 --- Cards should be actionable

Artist card:

``` text
┌───────────────────────────────┐
│ PHOTO                         │
│ Reality Band             ⭐4.8│
│ Pop • Rock                    │
│ Jakarta                      │
│ Available Oct 24             │
│ From Rp 2.5M                 │
│                               │
│ [View Profile] [Invite]      │
└───────────────────────────────┘
```

Gig card:

``` text
┌───────────────────────────────┐
│ 🎸 Rock Festival              │
│ 📍 Depok                      │
│ 📅 24 Oct                     │
│ 💰 Rp 2–4M                    │
│ 🔥 94% Match                  │
│                               │
│ [View Gig]                    │
└───────────────────────────────┘
```

### Principle 3 --- Reduce dead-end screens

Every important object should provide a next action.

Artist:

``` text
View → Message → Invite → Book
```

Gig:

``` text
View → Apply → Chat → Book
```

Venue:

``` text
View → Events → Artists → Create Event
```

------------------------------------------------------------------------

# 31. Matching Architecture

Start deterministic before introducing an LLM.

``` text
Candidate Score

Genre Match       30%
Availability      25%
Location          15%
Budget            15%
Experience        10%
Reputation         5%
```

Then expose the explanation:

``` text
94% Match

✓ Genre matches
✓ Available on event date
✓ Within target budget
✓ 12 km from venue
✓ Similar events completed
```

Later:

``` text
Rules Engine
     ↓
Feature Extraction
     ↓
Recommendation Model
     ↓
LLM Explanation
```

The LLM should explain/reason over structured matching signals rather
than becoming the source of truth for booking eligibility.

------------------------------------------------------------------------

# 32. Search Architecture

Recommended flow:

``` text
User Query
   ↓
Intent Parser
   ↓
Structured Filters
   ↓
Search
   ↓
Ranking
   ↓
Results
```

Example:

``` text
"butuh band jazz untuk wedding
di Jakarta 24 Oktober budget 5 juta"
```

becomes:

``` json
{
  "type": "artist",
  "genre": ["jazz"],
  "eventType": "wedding",
  "location": "Jakarta",
  "date": "2026-10-24",
  "maxBudget": 5000000
}
```

------------------------------------------------------------------------

# 33. Security Considerations

Because GigRadar will handle commercial relationships:

-   Use role-based authorization.
-   Validate ownership of events/bookings before mutations.
-   Never trust client-side role claims.
-   Protect private artist/EO contact information.
-   Do not expose private addresses by default.
-   Audit booking/payment state transitions.
-   Protect chat attachments.
-   Rate-limit authentication and messaging endpoints.
-   Validate uploaded media.
-   Store secrets outside source control.
-   Log security-relevant actions without logging sensitive
    credentials/tokens.

------------------------------------------------------------------------

# 34. Metrics to Track

Do not measure only downloads.

### Marketplace metrics

``` text
Gig → Application conversion
Application → Shortlist
Shortlist → Booking
Booking → Completion
```

### Artist metrics

``` text
Profile completion
Portfolio completion
Response rate
Application rate
Booking rate
Cancellation rate
```

### EO metrics

``` text
Event creation
Artist invitation rate
Application review time
Booking completion
Repeat booking
```

### Network metrics

``` text
Follow creation
Community joins
Messages per booking
Repeat interactions
```

------------------------------------------------------------------------

# 35. Final Feature Decision Matrix

  Feature                   Decision                             Priority
  ------------------------- ------------------------------------ ----------
  Discover                  **ADAPT**                            P0
  Search                    **ADAPT**                            P0
  Filters                   **MODIFY**                           P0
  Clubs                     **MODIFY → Communities**             P2
  Meets                     **ADAPT → Gigs**                     P0
  RSVP                      **MODIFY → Applications/Bookings**   P0
  Waitlist                  **ADAPT**                            P1
  Invitations               **ADAPT**                            P0
  Team assignment           **MODIFY → Lineup**                  P1
  Competitions              **SKIP**                             P3
  Match tracking            **SKIP**                             ---
  Chat                      **ADAPT**                            P0
  Community feed            **MODIFY**                           P2
  Venue discovery           **ADAPT**                            P1
  Map                       **ADAPT**                            P1
  Notifications             **ADAPT**                            P0
  Payments                  **MODIFY**                           P1
  Organizer roles           **MODIFY**                           P1
  Reporting                 **ADAPT**                            P1
  Support                   **ADAPT**                            P1
  Artist portfolio          **NEW**                              P0
  Availability calendar     **NEW**                              P0
  Two-sided reviews         **NEW**                              P1
  Contract                  **NEW**                              P1
  Invoice                   **NEW**                              P1
  Cancellation policy       **NEW**                              P1
  AI matching               **NEW**                              P3
  Natural language search   **NEW**                              P3
  Pricing recommendation    **NEW**                              P3
  Artist crew management    **NEW**                              P2

------------------------------------------------------------------------

# 36. Recommended Development Order

``` text
PHASE 1
Core marketplace
│
├── Artist Profile
├── EO Profile
├── Event
├── Gig
├── Discover
├── Search
├── Availability
├── Application
├── Invitation
├── Booking
├── Chat
└── Notification

        ↓

PHASE 2
Trust + operations
│
├── Reviews
├── Reputation
├── Venue
├── Map
├── Payment
├── Invoice
├── Contract
├── Cancellation
└── EO Staff Roles

        ↓

PHASE 3
Community
│
├── Follow
├── Communities
├── Feed
├── Event Sharing
└── Networking

        ↓

PHASE 4
Intelligence
│
├── Matching
├── Recommendation
├── Natural Language Search
├── Price Recommendation
└── AI Assistant
```

------------------------------------------------------------------------

# 37. Key Product Principle

The central lesson from Reclub for GigRadar is:

> **Turn fragmented coordination into one structured workflow.**

For Reclub:

``` text
Find Sport
 ↓
Find Club
 ↓
Find Activity
 ↓
RSVP
 ↓
Play
 ↓
Track
 ↓
Community
```

For GigRadar:

``` text
Discover
 ↓
Find Artist / Gig
 ↓
Match
 ↓
Apply / Invite
 ↓
Negotiate
 ↓
Book
 ↓
Perform
 ↓
Pay
 ↓
Review
 ↓
Build Reputation
 ↓
Find the next Gig
```

That should be the backbone of the GigRadar product.

------------------------------------------------------------------------

# 38. Sources

The research in this document uses publicly available Reclub
documentation and app information:

-   Reclub official product site: https://reclub.co/
-   Reclub Help Center: https://help.reclub.co/
-   Reclub "What is Reclub?":
    https://help.reclub.co/hc/reclub-help/articles/1765846073-what-is-reclub
-   Reclub Discovery documentation:
    https://help.reclub.co/hc/reclub-help/articles/1765845551-find-meets-clubs-users-competitions-coaches
-   Reclub Venue Discover documentation:
    https://help.reclub.co/hc/reclub-help/articles/1773823454-venue-discover
-   Reclub Club creation documentation:
    https://help.reclub.co/hc/reclub-help/articles/1765845000-how-do-i-create-a-club
-   Reclub Meets product page: https://reclub.co/platform/meets
-   Reclub Google Play listing:
    https://play.google.com/store/apps/details?id=co.reclub

### Research note

Feature availability can change over time. This document reflects
publicly documented capabilities found during the research date above.
It should be treated as a product-research baseline, not a claim that
every feature is available to every Reclub user, region, or account
type.

------------------------------------------------------------------------

# 39. Implementation Instruction for GigRadar CLI

Use this document as a product-design reference, **not as an instruction
to copy Reclub's implementation or visual identity**.

When implementing:

1.  Inspect the existing GigRadar architecture first.
2.  Preserve existing authentication/security boundaries unless there is
    a concrete reason to change them.
3.  Map existing entities to the proposed domain model.
4.  Implement P0 features before P1/P2/P3.
5.  Add database migrations incrementally.
6.  Add API contracts before UI integration.
7.  Add authorization tests for Artist, EO, staff, and admin roles.
8.  Add integration tests for:
    -   application flow
    -   invitation flow
    -   booking state transitions
    -   cancellation
    -   payment state
    -   review permissions
9.  Implement responsive UI for both Artist and EO workflows.
10. Keep matching explainable and deterministic before adding AI.
11. Do not implement sports-specific Reclub functionality unless it has
    a clear GigRadar use case.
12. Keep the UI inspired by the **interaction patterns**, not Reclub's
    branding/assets.

## Definition of Done

A feature is considered complete when:

-   Backend domain model exists.
-   Authorization rules exist.
-   API endpoint/contract exists.
-   Database migration exists.
-   UI state/loading/error/empty states exist.
-   Validation exists.
-   Unit/integration tests exist.
-   Mobile/responsive layout works.
-   Accessibility basics are covered.
-   Audit/security considerations are documented.


---

# 40. MASTER PROMPT — GITHUB COPILOT / COPILOT CLI

> Copy bagian prompt di bawah ini ke GitHub Copilot/Copilot CLI setelah repository GigRadar dibuka.

```text
You are the lead software engineer and product engineer working on the GigRadar project.

Your job is to improve the existing GigRadar application by using this document
(RECLUB_RESEARCH_GIGRADAR.md) as product research and implementation guidance.

IMPORTANT:
- Do NOT copy Reclub's branding, source code, assets, text, or proprietary implementation.
- Reclub is only a product/UX reference.
- Adapt the underlying interaction patterns to GigRadar's Artist ↔ Event Organizer domain.
- Inspect the existing GigRadar repository before modifying anything.
- Do not blindly rewrite existing architecture.
- Preserve working functionality unless a change is required by the new design.
- Prefer incremental, testable changes.
- Do not invent APIs, database tables, services, or framework features without inspecting the repository first.

==================================================
1. FIRST: AUDIT THE EXISTING PROJECT
==================================================

Before writing code, inspect:

- solution/project structure
- frontend projects
- backend projects
- API controllers/endpoints
- application/use-case layer
- domain entities
- infrastructure layer
- database configuration
- migrations
- authentication
- authorization
- DTOs
- repositories
- services
- dependency injection
- navigation
- existing UI components
- styling/design system
- tests
- configuration files
- environment variables
- deployment configuration

Create a short internal implementation map:

1. Current architecture
2. Existing Artist functionality
3. Existing EO functionality
4. Existing Event functionality
5. Existing authentication/authorization
6. Existing booking/application functionality
7. Existing UI/navigation
8. Features that already exist and can be reused
9. Features that are missing
10. Risks or breaking changes

Do not start a large refactor before this audit.

==================================================
2. PRODUCT GOAL
==================================================

Transform GigRadar into a professional marketplace/community platform connecting:

ARTISTS
↕
GIGS / EVENTS
↕
EVENT ORGANIZERS

The core workflow should become:

Discover
→ Find Artist / Gig
→ Match
→ Apply / Invite
→ Negotiate
→ Book
→ Perform
→ Pay
→ Review
→ Build Reputation
→ Find the next Gig

The two primary user experiences are:

A. ARTIST
B. EVENT ORGANIZER / EO

They must have different dashboards, priorities, actions, and navigation.

==================================================
3. IMPLEMENTATION PRIORITY
==================================================

Implement in this order.

P0 — CORE MARKETPLACE

1. Artist Profile
2. EO Profile
3. Event
4. Gig
5. Discover
6. Search
7. Filters
8. Artist Availability
9. Application
10. Invitation
11. Shortlist
12. Booking
13. Contextual Chat
14. Notifications

P1 — TRUST & OPERATIONS

15. Reviews
16. Two-sided Reputation
17. Venue Profile
18. Map/Location
19. Payment Status
20. Invoice
21. Contract
22. Cancellation Policy
23. EO Staff Roles

P2 — COMMUNITY

24. Follow
25. Communities
26. Activity Feed
27. Event Sharing
28. Networking
29. Artist Crew Management

P3 — INTELLIGENCE

30. Matching Engine
31. Recommendation Engine
32. Natural Language Search
33. Pricing Recommendation
34. AI Assistant

Do NOT implement P2/P3 before P0 is stable unless the existing project already supports the required foundation.

==================================================
4. ARTIST EXPERIENCE
==================================================

Artist navigation should prioritize:

Home
Discover
Bookings
Messages
Profile

Artist Home should surface:

- recommended gigs
- availability
- active applications
- active bookings
- unread messages
- upcoming events
- profile completeness
- reputation
- relevant opportunities

Artist Profile should support:

- display name
- artist/band name
- profile image
- cover image
- bio
- genres
- location
- price range
- portfolio
- audio
- video
- social links
- previous gigs
- equipment
- technical rider
- availability
- experience
- reviews
- booking history
- response rate
- cancellation rate

Do not make Artist Profile a simple social profile.
It must function as a professional booking profile.

==================================================
5. EO EXPERIENCE
==================================================

EO navigation should prioritize:

Home
Discover Artists
My Events
Messages
Organization

EO Home should surface:

- upcoming events
- open gigs
- artist recommendations
- pending applications
- shortlisted artists
- active negotiations
- bookings
- payment status
- messages

EO should be able to:

- create organization
- create event
- create gig requirements
- discover artists
- filter artists
- shortlist artists
- invite artists
- message artists
- send offers
- confirm bookings
- manage event lineup
- manage staff
- manage payment status
- review artists after completed gigs

==================================================
6. EVENT / GIG MODEL
==================================================

Treat Event and Gig as structured business objects.

Suggested relationship:

Event
  └── one or more Gigs
        ├── requirements
        ├── applications
        ├── invitations
        ├── offers
        └── booking

Gig should contain relevant requirements such as:

- date
- start time
- end time
- venue
- genre
- event type
- budget
- artist type
- number of artists
- set duration
- equipment
- sound requirements
- stage requirements
- audience size
- additional requirements

Avoid storing critical business state only as free-form text.

==================================================
7. BOOKING STATE MACHINE
==================================================

Implement explicit state transitions.

Application:

Pending
→ Shortlisted
→ Rejected

or

Pending
→ Withdrawn

Booking:

Proposed
→ Negotiating
→ Confirmed
→ Completed

Alternative terminal state:

Any valid pre-completion state
→ Cancelled

Do not use a single boolean such as:

isBooked = true

for the entire booking lifecycle.

Every state transition must be validated server-side.

==================================================
8. AVAILABILITY
==================================================

Implement Artist availability as structured data.

Suggested states:

Available
Tentative
Booked
Unavailable

Availability must be queryable by:

- date
- time range
- location if applicable

The matching/search system should be able to exclude artists who are unavailable.

Do not rely only on client-side calendar logic.

==================================================
9. DISCOVERY
==================================================

Create a strong Discover experience.

Top-level discovery:

- Gigs
- Artists
- EOs
- Venues
- Communities

Search filters should support:

- date
- location
- radius
- genre
- event type
- budget
- availability
- artist type
- experience
- rating/reputation

Every result must have an obvious next action.

Artist:

View Profile
→ Message
→ Invite / Book

Gig:

View Gig
→ Apply

Venue:

View Venue
→ View Events
→ Create Event

==================================================
10. MATCHING ENGINE
==================================================

Start with deterministic matching before AI.

Suggested initial weighted model:

Genre Match       30%
Availability      25%
Location          15%
Budget             15%
Experience         10%
Reputation          5%

The exact weights are configurable and should not be hard-coded throughout the application.

Matching results must be explainable.

Example:

94% Match

✓ Genre matches
✓ Available on event date
✓ Within target budget
✓ Near event location
✓ Relevant experience

Do not display unexplained AI scores.

Create a service boundary for matching so the algorithm can evolve later.

==================================================
11. CONTEXTUAL CHAT
==================================================

Do not build chat as an isolated generic messaging system.

Where possible, associate conversations with:

- Event
- Gig
- Application
- Booking

Example UI:

Indie Night
Reality Band ↔ Event Organizer

Budget: Rp 3.000.000
Date: 20 Oct
Status: Negotiating

[View Booking]

Chat must support:

- text
- attachments if already supported by architecture
- unread state
- timestamps
- event/booking context

Never expose sensitive internal data through client-side messages.

==================================================
12. REVIEWS & REPUTATION
==================================================

Implement two-sided reviews.

Artist reviews EO:

- communication
- payment reliability
- organization
- professionalism
- venue accuracy

EO reviews Artist:

- performance
- communication
- punctuality
- professionalism
- quality

Avoid making one raw star rating the only trust mechanism.

Keep review ownership and eligibility server-side:

Only users involved in a completed booking should be able to review each other.

==================================================
13. VENUES
==================================================

Venue should be a reusable domain object.

Suggested fields:

- name
- location
- approximate/public address
- capacity
- facilities
- equipment
- photos
- description
- events
- reviews

Avoid exposing private addresses unnecessarily.

==================================================
14. COMMUNITIES
==================================================

Do not directly copy the Reclub "Club" concept.

Use GigRadar Communities.

Examples:

- Indie Musicians Depok
- Jakarta Wedding Musicians
- Local DJ Community
- Event Organizer Community

Community features:

- members
- posts
- announcements
- events
- chat
- moderators
- rules

Community should support the marketplace, not become an unrelated social network.

==================================================
15. NOTIFICATIONS
==================================================

Create actionable notifications.

Examples:

New Gig Match
Application Accepted
Application Rejected
Artist Invitation
Booking Offer
Booking Confirmed
Booking Cancelled
New Message
Payment Update
Event Reminder
Availability Update

Every notification should deep-link to the relevant entity.

==================================================
16. SECURITY
==================================================

Security is mandatory.

Verify authorization server-side for:

- event ownership
- organization membership
- booking access
- application access
- payment access
- review eligibility
- message access
- staff permissions

Never trust:

- client role claims
- client user IDs
- client organization IDs
- client booking IDs
- hidden form fields
- UI-only authorization

Do not expose:

- passwords
- access tokens
- refresh tokens
- secrets
- private credentials

Use the repository's existing authentication/security architecture unless there is a documented reason to improve it.

==================================================
17. UI/UX RULES
==================================================

The redesign should feel:

- modern
- professional
- visually clean
- comfortable for long sessions
- responsive
- mobile-friendly
- easy to scan
- action-oriented

Do not copy Reclub's visual identity.

Use the research only for interaction patterns.

Important UI states:

- loading
- empty
- error
- success
- disabled
- permission denied
- offline if applicable

Every important screen should have a clear primary CTA.

Artist and EO interfaces should feel related but not identical.

==================================================
18. API DESIGN
==================================================

Before implementing frontend calls, inspect existing API conventions.

Reuse existing:

- naming conventions
- authentication
- DTO pattern
- error format
- validation
- pagination
- dependency injection
- response conventions

Potential endpoints conceptually include:

GET    /artists
GET    /artists/{id}
POST   /artists/profile
PUT    /artists/profile

GET    /gigs
GET    /gigs/{id}
POST   /gigs
PUT    /gigs/{id}

POST   /gigs/{id}/applications
POST   /gigs/{id}/invitations
POST   /applications/{id}/shortlist

POST   /bookings
GET    /bookings/{id}
POST   /bookings/{id}/confirm
POST   /bookings/{id}/cancel
POST   /bookings/{id}/complete

GET    /availability
PUT    /availability

GET    /conversations
GET    /conversations/{id}/messages
POST   /conversations/{id}/messages

Do not create these exact endpoints blindly.
Adapt them to the existing GigRadar API architecture.

==================================================
19. DATABASE
==================================================

Inspect existing entities first.

Potential entities:

User
ArtistProfile
Organization
OrganizationMember
Event
Gig
Venue
Availability
Application
Invitation
Booking
Conversation
Message
Review
Payment
Invoice
Contract
Community
CommunityMember
Post
Notification
Report

Avoid premature database normalization/refactoring.

Add migrations incrementally.

Never delete existing production data merely to simplify a migration.

==================================================
20. TESTING
==================================================

Every new business workflow must have tests.

Minimum tests:

Authentication
Authorization
Artist profile
EO profile
Event creation
Gig creation
Availability
Application
Invitation
Shortlisting
Booking state transitions
Cancellation
Review permissions
Message authorization
Organization staff permissions

Critical invariant examples:

- Artist cannot modify another artist's availability.
- EO cannot modify another EO's event.
- Artist cannot confirm an offer that was not addressed to them.
- User cannot review a booking they were not part of.
- Cancelled bookings cannot be completed.
- Completed bookings cannot return to pending.
- Unauthorized users cannot access private booking conversations.

==================================================
21. IMPLEMENTATION PROCESS
==================================================

Follow this exact process for each feature:

STEP 1
Inspect existing implementation.

STEP 2
Identify reusable components/entities/services.

STEP 3
Write a short implementation plan.

STEP 4
Implement backend/domain changes.

STEP 5
Implement API changes.

STEP 6
Implement frontend/UI changes.

STEP 7
Implement loading/error/empty states.

STEP 8
Add tests.

STEP 9
Run existing test suite.

STEP 10
Run build.

STEP 11
Fix regressions.

STEP 12
Summarize changed files and remaining work.

Do not modify unrelated files.

==================================================
22. UI IMPLEMENTATION ORDER
==================================================

Start with:

1. Artist Dashboard
2. EO Dashboard
3. Discover
4. Artist Profile
5. Gig Detail
6. Event Detail
7. Availability
8. Application flow
9. Booking flow
10. Chat
11. Notifications

Then:

12. Venue
13. Reviews
14. Payment
15. Contract
16. Communities
17. Follow
18. Feed

Finally:

19. Matching
20. AI recommendations
21. Natural-language search

==================================================
23. EMPTY STATES
==================================================

Do not leave empty pages blank.

Examples:

Artist has no gigs:

"No matching gigs yet.
Complete your profile and availability to improve recommendations."

EO has no applicants:

"No applications yet.
Try improving your gig requirements or inviting matching artists."

No messages:

"No conversations yet.
Apply to a gig or invite an artist to start a conversation."

No availability:

"Set your availability so EOs can find you for suitable gigs."

==================================================
24. ACCESSIBILITY
==================================================

Ensure:

- readable typography
- sufficient contrast
- keyboard navigation where applicable
- semantic controls
- accessible form labels
- meaningful validation messages
- non-color-only status indicators
- usable touch targets
- screen-reader-friendly labels

==================================================
25. PERFORMANCE
==================================================

Avoid:

- unnecessary API calls
- N+1 queries
- loading entire result sets
- excessive image payloads
- repeated matching calculations

Use:

- pagination
- server-side filtering
- caching where appropriate
- optimized queries
- image optimization
- debounced search where appropriate

Do not add infrastructure complexity without evidence it is needed.

==================================================
26. DEFINITION OF DONE
==================================================

A feature is complete only when:

[ ] Backend implementation exists
[ ] Authorization exists
[ ] Validation exists
[ ] Database migration exists if needed
[ ] API contract exists
[ ] Frontend implementation exists
[ ] Loading state exists
[ ] Empty state exists
[ ] Error state exists
[ ] Success state exists
[ ] Responsive layout works
[ ] Accessibility basics are covered
[ ] Unit tests exist where appropriate
[ ] Integration tests exist for critical workflows
[ ] Build passes
[ ] Existing tests pass
[ ] No unrelated regressions were introduced
[ ] Security implications have been checked

==================================================
27. IMPORTANT COPILOT BEHAVIOR
==================================================

Do not respond with a huge speculative implementation.

Work incrementally.

Before major changes, inspect the relevant files.

When uncertain about the current architecture:
- search the repository
- inspect existing patterns
- reuse established conventions

When a feature already exists:
- improve/refactor it instead of duplicating it.

When a proposed feature conflicts with the existing architecture:
- explain the conflict
- propose the smallest safe architectural change
- implement only after understanding dependencies.

When a test fails:
- diagnose the root cause
- do not simply weaken/delete the test.

When a build fails:
- fix the underlying problem
- do not bypass the build.

When modifying authentication or authorization:
- treat the change as security-sensitive
- inspect all affected flows.

==================================================
28. FIRST TASK
==================================================

Do NOT immediately implement every feature.

Start by auditing the repository.

Then produce:

A. Current architecture summary
B. Current Artist flow
C. Current EO flow
D. Current Event/Gig flow
E. Current authentication flow
F. Current booking/application flow
G. Existing reusable components
H. Missing P0 features
I. Recommended implementation order
J. Files that will likely need modification

After the audit, begin with the highest-priority missing P0 feature.

Do not move to P1 until P0 foundations are stable.

==================================================
29. FINAL GOAL
==================================================

The finished GigRadar experience should make this possible:

ARTIST:

Profile
→ Availability
→ Discover matching gigs
→ Apply
→ Communicate
→ Accept offer
→ Perform
→ Get paid
→ Receive review
→ Build reputation
→ Get better recommendations

EO:

Organization
→ Create event
→ Define requirements
→ Discover artists
→ Receive recommendations
→ Invite / shortlist
→ Communicate
→ Send offer
→ Confirm booking
→ Manage lineup
→ Pay
→ Review artist
→ Rebook suitable artists

Build the system around these two journeys.

Do not build GigRadar as a copy of Reclub.

Build GigRadar as a professional Artist ↔ EO marketplace inspired by the useful interaction patterns identified in this research.
```

---

# 41. COPILOT CLI — SHORT START COMMAND

Jika ingin memberikan instruksi singkat setelah membuka repository:

```text
Read RECLUB_RESEARCH_GIGRADAR.md completely.

First audit the existing GigRadar architecture, authentication,
Artist flow, EO flow, Event/Gig flow, application/booking flow,
database, API, frontend, and tests.

Do not implement everything at once.

Create an implementation plan based on the P0 → P1 → P2 → P3
priority in the document.

Start with the highest-priority missing P0 feature.

Reuse the existing architecture and patterns.
Do not copy Reclub branding, assets, source code, or proprietary implementation.

After each feature:
1. implement backend/domain changes
2. implement API changes
3. implement UI
4. add loading/empty/error states
5. add tests
6. run tests
7. run build
8. summarize changed files

Begin by auditing the repository and report the current architecture
and the first P0 implementation task.
```

---

# 42. COPILOT IMPLEMENTATION RULE

The most important instruction is:

```text
RESEARCH → AUDIT → PLAN → IMPLEMENT → TEST → BUILD → REVIEW
```

Do not skip the **AUDIT** phase.

Do not implement all features in one pass.

The purpose of this document is to give Copilot enough product context to make incremental, architecture-aware decisions while working inside the existing GigRadar codebase.

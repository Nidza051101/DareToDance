# D2D — Dance Studio System

## Goals

The system enables user registration and login, access control through
permissions, membership validity verification, and linking users to dance
groups. This ensures that only authorized users have access to the system,
while the studio's operations remain organized and secure.

## Problems

The problems the system currently faces are the lack of registration and
login into the system, permissions have not been defined, there is no way
to verify memberships, and it is unclear whether a participant belongs to
one group or can be part of multiple groups at the same time. These
shortcomings currently prevent the controlled and secure use of the system.

## User Stories

A user should be able to register with an email address and password to
gain access to the system, and then log in easily whenever needed. Those
with the appropriate permissions should be able to grant or revoke other
users' access rights, while staff at the entrance should be able to scan a
user's membership QR code and immediately see whether it's valid, with no
charge involved at that moment. Instructors, in turn, should be linked to
the groups they lead, so their work is clearly organized within the
system.

## Functional Requirements

### 5.1 Registration and Login

The system allows a new user to register by providing their full name,
email address, phone number (optional), and password, with the password
always stored hashed, never in plain text. The email must be unique for
each user, and login is performed using a combination of email address and
password. Each user also has a status (active, inactive, or blocked) that
determines whether they are allowed to log in at all.

### 5.2 Permission-based Authorization

The system does not use fixed roles as an authorization mechanism; instead,
each user has their own set of permissions. A permission is a named entity
(e.g., manage_users, manage_groups, scan_membership) with a description,
and the relationship between users and permissions is many-to-many,
through the USER_PERMISSION table. Access rights in the application are
always checked based on the specific permission name, not the role name.

### 5.3 Membership and QR Check-in

A user can have one or more memberships, each with a validity period
defined by a start and end date, as well as a unique QR code. Scanning the
QR code checks whether the membership is currently active and whether the
current date falls within its validity period, without triggering any
payment. The membership status (active, expired, or suspended) is updated
independently of the payment process, which is outside the scope of this
module.

### 5.4 Groups

A user with the appropriate permission can be assigned as an instructor to
one or more groups they lead. It remains an open question whether a
participant belongs to only one group or can be part of multiple groups at
the same time, which is something that still needs to be defined.

## Data Model

The module relies on the following tables from the ER diagram: USER,
PERMISSION, USER_PERMISSION, MEMBERSHIP, GROUP_TABLE. Detailed fields are
defined in the previously delivered ER diagram and schema documentation.

## Non-functional Requirements

The system's non-functional requirements are that the module only tracks
status and validity period, not payment processing, scheduling and
individual class sessions, per-session attendance tracking, or support for
multiple dance studios. This clearly limits the module's scope to the core
user functionalities, avoiding unnecessary complexity in the early stage
of development.

## Open Questions

Several questions remain open. It is still unclear whether a participant
can belong to multiple groups at the same time. It
also needs to be decided whether permissions should be grouped into
predefined sets ("templates") to make assignment easier, instead of
selecting individual permissions manually. Additionally, it hasn't been
determined whether email verification is required during registration, or
whether a password reset / forgot-password feature is needed.

## Success Metrics

Success will be measured by the login success rate (the percentage of
login attempts completed without errors), the time between scanning the
QR code and confirming membership validity (latency), and the number of
users with correctly assigned permissions, reflected in the absence of
incorrect-access incidents.
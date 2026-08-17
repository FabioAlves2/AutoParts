# AutoParts

Windows Forms client for an auto parts retailer's inventory and order system,
backed by SQL Server. The interesting problem isn't the catalogue — it's
compatibility: a given part fits certain vehicles, and a vehicle is defined
partly by its engine, so "which parts fit this car" is a join across four
tables that has to stay correct as stock moves.

Coursework project — Databases, BSc Computer and Informatics Engineering,
University of Aveiro. Built with a course partner.

<!-- ![Screenshot](docs/screenshot.png) -->
<!-- ![ER diagram](docs/er-diagram.png) -->

## What it does

A single-window app (tabbed) covering the full lifecycle of the store:

- **Admin & Customer management** — register, list, filter and edit people,
  split between `AP_Administrator` and `AP_Customer` records
- **Catalogue** — parts, categories and specs (weight, dimensions, diameter),
  with filtering by name/category
- **Vehicles & engines** — register vehicles and engines, and set which parts
  are compatible with which vehicle (the `AP_Compatibility` bridge)
- **Suppliers & stock** — record incoming stock from a supplier, which keeps
  `AP_Part.Stock` up to date
- **Orders** — build an order (cart), submit it via `AP_CreateOrder`, and see
  order history resolved into readable line items

## Schema

The database itself isn't part of this repo — it lives on the SQL Server
instance the app connects to. 14 tables, organised in four groups:

- **Catalogue** — `AP_Part`, `AP_Category`, `AP_Specs`
- **Vehicles** — `AP_Vehicle`, `AP_Engine`, and `AP_Compatibility` as the
  many-to-many bridge between parts and the vehicles they fit
- **People** — `AP_Person` as the base, specialised into `AP_Customer` and
  `AP_Administrator`
- **Commerce** — `AP_Order_Table`, `AP_Order_item`, `AP_Rates`, `AP_Supplier`,
  `AP_Stocks`

Three views do the heavy lifting for the UI instead of assembling joins in
C#: `AP_VehicleWithEngine` (vehicle + engine flattened into one row),
`AP_PartDetailsView` (part, specs, compatibility, target vehicle and average
rating in one place), and `AP_OrderDetailsView` (order lines resolved into
part name, manufacturer, unit price and quantity).

Stock is kept denormalised on `AP_Part` for fast catalogue reads, while
`AP_Stocks` remains the record of what arrived and from whom; triggers keep
the two in sync when stock is added or an order is placed.

## What I would do differently

**The connection string used to be hardcoded, credentials and all, straight
into `Form1.cs`.** It's now read from an environment variable instead (see
Running it, below), and the old value has been scrubbed from the git
history. Lesson learned the hard way: secrets don't belong in source, even
for a coursework project that's "just going to be graded" — this one ended
up needing a history rewrite before the repo could go public.

**Everything lives in one 2700-line `Form1.cs`.** Every tab's data access,
filtering and validation logic sits in a single partial class. It works, but
splitting each tab into its own UserControl with its own data-access class
would make the compatibility logic (the actual hard part of this domain)
easier to find and test independently of the UI.

**Some triggers on the server only handle single-row inserts** (they read
`inserted` into scalar variables instead of doing a set-based
`UPDATE ... FROM inserted`), so a multi-row `INSERT ... SELECT` would
silently process just one row. This is the kind of bug that passes every
manual test done through the app's UI — since the app always inserts one
row at a time — and would only surface with a bulk import.

## Stack

.NET 8 · Windows Forms · `System.Data.SqlClient` · SQL Server / T-SQL

## Running it

1. You'll need a SQL Server instance with the schema described above already
   created (this repo doesn't include the DDL — it targets an existing
   database).
2. Set the connection string as an environment variable before launching:

   ```powershell
   $env:AUTOPARTS_CONNECTION_STRING = "Data Source=<server>;uid=<user>;password=<password>"
   ```

3. Build and run:

   ```powershell
   dotnet run --project AutoParts
   ```

The app throws a clear error on startup if the environment variable isn't
set, instead of silently failing on the first query.

## Security note

This repo's history was rewritten on 2026-08-17 to remove a hardcoded
database credential that was committed early in the project. If you cloned
this repo before that date, discard that clone and re-clone — the old
commits containing the credential no longer exist on the remote, but may
still be cached locally. The exposed credential has been rotated.

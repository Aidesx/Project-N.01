# E-Commerce Web Platform (Project-N.01)

An e-commerce web application designed to study and practice the **MVC (Model-View-Controller)** architectural pattern using **ASP.NET Core (.NET 8)** and **Entity Framework Core**.

---

## 👥 Contributors
* [@Aidesx](https://github.com/Aidesx)
* [@TheRuizz006](https://github.com/TheRuizz006)

---

## 🎯 Project Objectives
* Explore and implement modern **MVC** architecture in a real-world web application.
* Gain hands-on experience with **ASP.NET Core 8**, Entity Framework Core (Code-First), and Microsoft SQL Server.
* Build foundational e-commerce workflows, including secure user authentication, product catalog management, shopping cart operations, role-based authorization, and third-party service integrations (e-wallets and email notifications).

---

## 🛠 Tech Stack
* **Framework & Runtime:** C# / .NET 8 (ASP.NET Core MVC)
* **Database & ORM:** Microsoft SQL Server, Entity Framework Core (Code-First)
* **Authentication & Security:** ASP.NET Core Identity, Google OAuth 2.0, Cookie Authentication
* **Integrations:** MoMo Payment Gateway API, SMTP Email Service
* **Frontend:** Razor Views (.cshtml), HTML5, CSS3, JavaScript, Bootstrap, jQuery
* **Containerization:** Docker support (`Dockerfile`)

---

## ⚡ High-Level Features

### 1. Storefront (Customer Experience)
* **Product Catalog:** Browse products, filter by categories and brands, search by keyword, and paginate results.
* **Product Details:** Detailed specifications, product reviews and ratings, wishlists, and item comparisons.
* **Shopping Cart:** Add, update, and remove items with dynamic order total calculations and regional delivery rate adjustments.
* **Checkout & Payments:** Support for Cash on Delivery (COD) and online payments via **MoMo E-Wallet**.
* **Account Management:** User registration with email confirmation, secure credential login, and one-click Google OAuth sign-in.

### 2. Admin Panel
* **Product Management:** Full CRUD operations for products, including image uploads and pricing.
* **Catalog Management:** Organize and manage product categories and brands.
* **Order Tracking:** Review customer orders, inspect order details, and update shipping/processing statuses.
* **User & Role Administration:** Manage system accounts and assign user roles with policy-based authorization.

---

## 🚀 Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* Microsoft SQL Server (or SQL Server Express / LocalDB)

### Configuration
1. Clone the repository.
2. Update the database connection string in `appsettings.json` under `ConnectionStrings:ConnectDB`.
3. *(Optional)* Configure credentials for MoMo API, Email SMTP, and Google OAuth if testing those features locally.

### Database Setup & Run
Run the following commands in the project root directory:

```bash
# Restore dependencies
dotnet restore

# Apply Entity Framework Core migrations to your SQL Server database
dotnet ef database update

# Start the web server
dotnet run
```

Once running, navigate to the local URL displayed in the terminal output (e.g., `https://localhost:7xxx` or `http://localhost:5xxx`).

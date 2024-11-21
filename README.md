# Blog Platform with Clean Architecture (.NET 8)

This project is a blog platform built with .NET 8, following the principles of Clean Architecture. 
It provides a robust and maintainable foundation for creating and managing blog content. 
This project focuses on the backend implementation, providing a RESTful API for future frontend integration.

## Features

* **User Authentication and Authorization:**
    * Secure user registration and login with password management.
    * JWT (JSON Web Token) authentication for secure API access.
    * Refresh token implementation for extended sessions.
    * Role-based authorization for granular access control.
* **User Management:**
    * Create, read, update, and delete user profiles.
    * Assign and manage user roles.
* **Blogging Functionality:**
    * Create, read, update, and delete blog posts.
    * Add and manage comments on posts.
* **Clean Architecture with Repository Pattern:**  The application is designed using the Repository Pattern to abstract data access and improve maintainability.

## Roles and Permissions

The platform has three user roles with different levels of access:

* **Readers:**
    * Can view all posts and comments.
    * Can write comments on posts.
    * Can delete their own comments.

* **Author:**
    * Has all the permissions of a Reader.
    * Can create and publish blog posts.
    * Can update and delete their own posts.
    * Can delete any comments on their own posts.

* **Admin:**
    * Has all the permissions of an Author and Reader.
    * Can delete any post or comment.
    * Can update their own posts.

## Technologies Used

* .NET 8
* ASP.NET Core Identity
* JWT Authentication
* Entity Framework Core
* Repository Pattern
* SQL Server Database 

## Getting Started

1. **Clone the repository:** `git clone https://github.com/omar-salah-elshafey/BlogPlatformCleanArchitecture`
2. **Database Setup:**
    * Configure the database connection string, Email Configuration, and JWT Configuration in `appsettings.json`.
    * Run database migrations to create the necessary tables.
3. **Build and Run:**
    * Build the project using your preferred IDE or command-line tools.
    * Run the application.

## Usage

* **Registration:** Create a new user account.
* **Login:** Authenticate with your credentials to access the platform.
* **MAnage Posts:** Write and publish blog posts, Edit posts, and delete them.
* **Manage Comments:** Add comments to posts and manage existing comments.

## Contributing

Contributions are welcome! Please feel free to submit pull requests for bug fixes, new features, or improvements.

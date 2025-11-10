# 🍽️ Menulo – Hệ thống quản lý & gọi món nhà hàng (Razor Pages + Clean Architecture .NET 8)

![Stars](https://img.shields.io/github/stars/menuloapp/menulo?style=for-the-badge)
![License](https://img.shields.io/badge/license-Non--Commercial-red?style=for-the-badge)
![.NET Version](https://img.shields.io/badge/.NET-8.0-blueviolet?style=for-the-badge)
![CI/CD](https://img.shields.io/github/actions/workflow/status/menuloapp/menulo/dotnet.yml?label=CI%2FCD&style=for-the-badge)

> Menulo là một ứng dụng web được xây dựng bằng **ASP.NET Core 8**, **Razor Pages**, và **Clean Architecture**.  
> Mục tiêu của dự án là giúp các nhà hàng dễ dàng quản lý **thực đơn**, **bàn**, **đơn hàng**, và **doanh thu** theo thời gian thực.  
> Menulo tích hợp SignalR để đồng bộ hóa dữ liệu giữa nhân viên và khách hàng một cách trực quan.

---

## ⚙️ Kiến trúc hệ thống

```
├── Menulo.Domain          # Entities, Domain logic
├── Menulo.Application     # Use cases, DTOs, Services, Interfaces
├── Menulo.Infrastructure  # EF Core, Identity, Repositories, External Storage
└── Menulo (Presentation)  # Razor Pages UI, Controllers, Routing, Static Assets
```

**Mục tiêu:** Tách biệt business logic, dễ mở rộng và bảo trì.

---

## 🧩 Công nghệ sử dụng

| Thành phần | Mô tả |
|-------------|-------|
| **.NET 8 (ASP.NET Core Razor Pages)** | Framework chính cho backend & frontend server-side |
| **Entity Framework Core** | ORM quản lý dữ liệu và migration |
| **SignalR** | Real-time communication giữa nhân viên và khách |
| **Bootstrap 5 + FontAwesome** | Giao diện người dùng hiện đại |
| **SweetAlert2, DataTables, Select2** | Trải nghiệm UI/UX nâng cao |
| **Google OAuth** | Đăng nhập bảo mật |
| **Clean Architecture Pattern** | Cấu trúc chuẩn, dễ bảo trì |

---

## 🔔 Tính năng nổi bật

- 🧾 Quản lý danh mục & món ăn CRUD đầy đủ  
- 🍽️ Quản lý bàn & đơn hàng real-time (SignalR)  
- 🔐 Đăng nhập Google OAuth + phân quyền  
- 💬 Thông báo âm thanh & rung thiết bị  
- 🎨 Giao diện responsive, thân thiện người dùng

---

## 🧠 Clean Code & SOLID

- Clean Architecture  
- Dependency Inversion  
- Reusable UI Components  
- AutoMapper Profiles theo Feature  

---

## ⚠️ Bản quyền

```
© 2025 Menulo Project - All rights reserved.
```
**Chỉ được phép sử dụng cho mục đích học tập / phi thương mại.**  
Mọi hình thức thương mại hóa Menulo mà không có sự cho phép đều **bị nghiêm cấm**.

Liên hệ: **quangthong211101@gmail.com**

---

## 🤝 Đóng góp

1. Fork repo  
2. Tạo nhánh mới `feature/<tên-tính-năng>`  
3. Gửi Pull Request

---

## 🌐 Thông tin

| Mục | Thông tin |
|-----|------------|
| **Tác giả** | Dev lor |
| **Email** | menulo.dev@proton.me |
| **Năm phát hành** | 2025 |
| **Phiên bản .NET** | .NET 8 LTS |
| **Giấy phép** | Phi thương mại (Non-Commercial License) |

---

> ❤️ _“Clean Architecture, Clean Food.”_

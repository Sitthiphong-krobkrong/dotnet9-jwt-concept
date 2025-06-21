# dotnet9-jwt-concept

> **.NET 9 JWT Authentication & Authorization Example**

โปรเจกต์ตัวอย่างสำหรับเรียนรู้การใช้งาน **JWT Authentication** ใน ASP.NET Core (.NET 9)  
เหมาะสำหรับผู้เริ่มต้นที่ต้องการศึกษาหรือใช้เป็นโครงสร้างตั้งต้น

---

## ✨ Features

---

## 📁 Project Structure

- `Program.cs` – จุดเริ่มต้นของแอป, กำหนด Service และ Middleware
- `Helper/JwtHelper.cs` – ฟังก์ชันสำหรับสร้างและตรวจสอบ JWT
- `Models/Core/AppSettings.cs` – คลาสสำหรับ Mapping ค่าจาก `appsettings.json`
- `Controllers/UserController.cs` – ตัวอย่าง Controller สำหรับ Authentication
- `Core/UserService.cs` – ตัวอย่าง Service สำหรับจัดการผู้ใช้
- `Core/DbContext.cs` – ตัวอย่าง Context สำหรับเชื่อมต่อข้อมูล
- `ExceptionHandlingMiddleware` – Middleware สำหรับจัดการ Exception ทั่วทั้งแอป

---
## 🧑‍💻 Usage Example

1. **Login**  
   ส่งข้อมูลผู้ใช้ไปที่ `/api/user/login` เพื่อรับ JWT Token

2. **Access Protected Endpoint**  
   ส่ง JWT Token ใน Header เพื่อเข้าถึง API ที่ต้องการ Authentication

---

> Created for learning and quick-start template purposes.
- .NET 9 (ASP.NET Core Web API)
- JWT Token Authentication & Authorization
- Centralized error response (Global Exception Middleware)
- Profile/Custom claims in JWT
- Strongly-typed configuration via `appsettings.json`

---

## 🚀 Getting Started

### 1. Clone & Restore

```bash
git clone https://github.com/Sitthiphong-krobkrong/dotnet9-jwt-concept.git
cd dotnet9-jwt-concept
dotnet restore

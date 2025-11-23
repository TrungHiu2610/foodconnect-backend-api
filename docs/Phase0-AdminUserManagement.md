# Phase 0 - Admin User Management APIs

## Tổng quan

Phase 0 implement nhóm API quản lý tài khoản người dùng dành cho Admin, bao gồm search, pagination, xem chi tiết, và thay đổi trạng thái người dùng với audit log.

## API Endpoints

### 1. Lấy danh sách người dùng (GET)

**Endpoint:** `GET /api/Admin/GetUserList`

**Query Parameters:**

- `searchTerm` (string, optional): Tìm kiếm theo tên, email, hoặc số điện thoại
- `status` (UserStatusEnum, optional): Lọc theo trạng thái (Pending = 0, Active = 1, Banned = 2, Locked = 3)
- `role` (RoleEnum, optional): Lọc theo role (Admin = 1, Seller = 2, Buyer = 3)
- `pageNumber` (int, default = 1): Số trang
- `pageSize` (int, default = 10): Số item mỗi trang
- `sortBy` (string, default = "CreatedAtUtc"): Sắp xếp theo field (FullName, Email, Status, CreatedAtUtc)
- `isDescending` (bool, default = true): Sắp xếp giảm dần

**Response:**

```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "statusCode": 200,
  "data": {
    "users": [
      {
        "id": "guid",
        "fullName": "string",
        "email": "string",
        "phoneNumber": "string",
        "avatarUrl": "string",
        "roles": ["Admin", "Seller"],
        "status": 1,
        "createdAtUtc": "2024-01-01T00:00:00Z"
      }
    ],
    "totalCount": 100,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 10
  }
}
```

### 2. Lấy chi tiết người dùng (GET)

**Endpoint:** `GET /api/Admin/GetUserDetail/{userId}`

**Route Parameters:**

- `userId` (Guid): ID của người dùng

**Response:**

```json
{
  "success": true,
  "message": "User detail retrieved successfully",
  "statusCode": 200,
  "data": {
    "id": "guid",
    "fullName": "string",
    "email": "string",
    "phoneNumber": "string",
    "avatarUrl": "string",
    "dateOfBirth": "2000-01-01T00:00:00Z",
    "gender": "Male",
    "roles": ["Seller", "Buyer"],
    "status": 1,
    "provider": 0,
    "createdAtUtc": "2024-01-01T00:00:00Z",
    "updatedAtUtc": "2024-01-01T00:00:00Z",
    "totalOrders": 50,
    "totalSpending": 1000000,
    "shopId": "guid",
    "shopName": "My Shop"
  }
}
```

### 3. Thay đổi trạng thái người dùng (PUT)

**Endpoint:** `PUT /api/Admin/ChangeUserStatus`

**Request Body:**

```json
{
  "userId": "guid",
  "newStatus": 3,
  "reason": "Vi phạm chính sách"
}
```

**Response:**

```json
{
  "success": true,
  "message": "User status changed successfully",
  "statusCode": 200,
  "data": {
    "userId": "guid",
    "oldStatus": 1,
    "newStatus": 3,
    "changedBy": "Admin Name",
    "changedAt": "2024-01-01T00:00:00Z",
    "reason": "Vi phạm chính sách"
  }
}
```

## Database Changes

### New Table: UserStatusAuditLogs

Bảng lưu lịch sử thay đổi trạng thái người dùng

**Columns:**

- `Id` (Guid, PK)
- `UserId` (Guid, FK -> Users)
- `OldStatus` (UserStatusEnum)
- `NewStatus` (UserStatusEnum)
- `ChangedByUserId` (Guid, FK -> Users)
- `Reason` (string, nullable)
- `ChangedAtUtc` (DateTime)
- `CreatedAtUtc` (DateTime)
- `UpdatedAtUtc` (DateTime, nullable)
- `CreatedBy` (Guid, nullable)
- `UpdatedBy` (Guid, nullable)
- `IsDeleted` (bool)

## Technical Implementation

### Architecture Pattern

Sử dụng Clean Architecture với CQRS pattern:

1. **Domain Layer:**

   - Entity: `UserStatusAuditLog.cs`
   - Enums: `UserStatusEnum`, `RoleEnum`

2. **Application Layer:**

   - DTOs: `UserManagementResponse.cs`, `UserManagementListResponse.cs`, `UserDetailResponse.cs`, `UserStatusChangeResponse.cs`
   - Queries: `GetUserListQuery`, `GetUserDetailQuery`
   - Commands: `ChangeUserStatusCommand`
   - Handlers: `GetUserListQueryHandler`, `GetUserDetailQueryHandler`, `ChangeUserStatusCommandHandler`
   - Repository Interface: `IUserStatusAuditLogRepository`

3. **Infrastructure Layer:**

   - Repository: `UserStatusAuditLogRepository`
   - DbContext: Thêm `DbSet<UserStatusAuditLog>`

4. **API Layer:**
   - Controller: `AdminController`

### Features

- ✅ Search & Pagination
- ✅ Filter by Status and Role
- ✅ Sort by multiple fields
- ✅ View user details with statistics (total orders, total spending)
- ✅ Change user status (Active/Locked/Banned)
- ✅ Audit log for status changes
- ✅ Transaction support for data consistency
- ✅ Role-based authorization (Admin only)

### Security

- Yêu cầu role Admin: `[Authorize(Roles = "Admin")]`
- Audit log ghi nhận admin thực hiện thay đổi
- Transaction đảm bảo tính toàn vẹn dữ liệu

## Next Steps

1. Tạo migration: `Add-Migration AddUserStatusAuditLog`
2. Update database: `Update-Database`
3. Test các API endpoints
4. Tiến hành Phase 1 - Seller Statistics

# userinterface 專案說明

本專案為簡易使用者介面，採用 **簡易MVC 架構**，提供基本的註冊與登入 API。 //其實只有M和C

---

## API 說明

### 1. `/User/Register`

**Request 範例**
```json
{
  "ID": 114514,  // 可選
  "username": "bigred",
  "password": "msg_is_king_of_flavor",
  "email": "kang0726@gmail.com"
}
```

**Response 範例**
```json
{
  "message": "註冊成功",
  "userId": 114514
}
```

---

### 2. `/User/Login`

**Request 範例**
```json
{
  "email": "kang0926@gmail.com",
  "password": "msg_is_king_of_flavor"
}
```

**Response 範例**
```json
{
    "message": "登入成功",
    "userId": 9,
    "username": "kang0926",
    "roles": [
        "admin"
    ],
    "permissions": [
        "edit_user",
        "delete_user",
        "permission_editing",
        "view"
    ],
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiI5Iiwic3ViIjoiRkBnbWFpbC5jb20iLCJlbWFpbCI6IkZAZ21haWwuY29tIiwidW5pcXVlX25hbWUiOiJrYW5nMDkyNiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6ImFkbWluIiwicGVybSI6WyJlZGl0X3VzZXIiLCJkZWxldGVfdXNlciIsInBlcm1pc3Npb25fZWRpdGluZyIsInZpZXciXSwiZXhwIjoxNzU1OTM1NTg1LCJpc3MiOiJtZSIsImF1ZCI6InlvdXJfbW9tIn0.SabeC4HPKcBaGPpXZxnVujAyQLmJQsJjJ6hMuOyctgo"
}
```

---

### 3. `/User/Delete`
需要 Bearer Token
**Request 範例**
```json
{
    "user_ID": 20050224
}
```

**Response 範例**
```json
{
    "message": "刪除成功"
}
```
### 4. `/User/all`
需要 Bearer Token
**Get 範例**
```json
[
    {
        "id": 2,
        "name": "test",
        "status": "test",
        "email": "kang0926@gmail.com"
    },
    {
        "id": 4,
        "name": "kang0926",
        "status": "bigred",
        "email": "RRR@gmail.com"
    },
    {
        "id": 5,
        "name": "kang0926",
        "status": "users",
        "email": "FFF@gmail.com"
    }
]
```
### 5. `/User/Update
需要 Bearer Token
**Request 範例**
```json
{
    "Id":2,
    "name":"test" // 可選
}
```

**Response 範例**
```json
{
    "message": "更新成功"
}
```


### 5. `/User/AssignRole`
需要 Bearer Token
```json
{
  "userId": 3,
  "roleId": 1
}
```
---

## 密碼加密方法

- 註冊時會自動產生一組隨機 salt（儲存於 `msg` 欄位）
- 密碼加密方式：`SHA256(password + salt)`，結果以 Base64 字串儲存於 `password_hash` 欄位
- 登入時根據資料庫中的 salt 重新計算雜湊值比對

---

## 架構流程圖

```
Program.cs
   ↓
UserController.cs
   ↓
IUserService.cs (介面)
   ↓
UserService.cs (實作)
   ↓
User.cs (資料模型)
```

---

## 已改用 RBAC
以角色 (roles) + 權限 (permissions) 取代原本使用者欄位 status。  
JWT 內含：
- ClaimTypes.Role: 多個角色
- perm: 多個權限 (edit_user, delete_user, permission_editing, view)

| API | 需求權限 |
|-----|----------|
| /User/all | view |
| /User/Update | edit_user |
| /User/Delete | delete_user |
| /User/AssignRole | permission_editing |


### 權限資料表示意
roles, permissions, role_permissions, user_roles 已存在，註冊預設自動指派 user 角色 (名稱為 user)。

---

## 備註

- 若於實驗室電腦操作，MySQL 連線資訊可使用：`172.16.3.50:13306`
- 我有保留.http但是我還是使用postman來測試
- 改用dto來傳輸
- 未來/updata 中根據權限不同使用者的可修改資料量(但jwt要先完成)
- 新增刪除角色
- 讓user可以改自己的資料
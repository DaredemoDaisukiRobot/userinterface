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
  "password": "msg_is_king_of_flavor"
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
  "ID": 114514,
  "password": "msg_is_king_of_flavor"
}
```

**Response 範例**
```json
{
  "message": "登入成功",
  "username": "bigred"
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

## 備註

- 若於實驗室電腦操作，MySQL 連線資訊可使用：`172.16.3.50:13306`
- 我有保留.http但是我還是使用postman來測試
- 未來嘗試使用cookie保留使用者資料

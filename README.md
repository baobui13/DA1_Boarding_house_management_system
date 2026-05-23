# QUẢN LÝ TRỌ

Hướng dẫn pull repo về và chạy local cho frontend + backend.

## Yêu cầu

- Node.js 20+
- .NET SDK 10
- PostgreSQL connection string hợp lệ

## Cấu trúc

- `FE`: frontend Vite
- `Backend_Boarding_house_management_system/Backend_Boarding_house_management_system`: ASP.NET Core backend

## Chạy backend

1. Tạo file `Backend_Boarding_house_management_system/Backend_Boarding_house_management_system/appsettings.Development.json`
2. Copy nội dung từ `Backend_Boarding_house_management_system/Backend_Boarding_house_management_system/appsettings.Development.example.json`
3. Điền các giá trị thật cho:
   - `ConnectionStrings:DefaultConnection`
   - `Authentication:Google`
   - `Jwt`
   - `CloudinarySettings`
4. Chạy backend:

```bash
cd Backend_Boarding_house_management_system/Backend_Boarding_house_management_system
dotnet run
```

Backend dev mặc định chạy ở:

- `http://localhost:5046`
- `https://localhost:7134`

Swagger mở tại:

- `http://localhost:5046`

## Chạy frontend

1. Tạo file `FE/.env`
2. Copy nội dung từ `FE/.env.example`
3. Cài package và chạy:

```bash
cd FE
npm install
npm run dev
```

Frontend mặc định gọi API qua:

- `VITE_API_BASE_URL=http://localhost:5046/api`

## Lưu ý CORS

Backend đã mở sẵn cho các origin local phổ biến:

- `http://localhost:3000`
- `http://localhost:4173`
- `http://localhost:5173`
- `http://localhost:5174`
- `http://localhost:5175`
- các biến thể `127.0.0.1` tương ứng

Nếu frontend chạy cổng khác, hãy thêm origin đó vào CORS trong `Program.cs`.

## Nếu không lấy được data

Kiểm tra lần lượt:

1. Backend có đang chạy ở `http://localhost:5046` không
2. `appsettings.Development.json` đã được tạo và điền đúng chưa
3. Frontend có file `FE/.env` chưa
4. `VITE_API_BASE_URL` có đúng là `http://localhost:5046/api` không
5. Console browser có báo lỗi `CORS`, `401`, `500` hay không

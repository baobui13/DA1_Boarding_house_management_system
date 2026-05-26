# AI Chatbot Microservice

Microservice Python (FastAPI) cho chức năng AI Chatbot trong hệ thống Quản lý Phòng Trọ.

## Cài đặt

```bash
cd ai-service

# Tạo virtual environment
python3 -m venv venv
source venv/bin/activate  # macOS/Linux
# venv\Scripts\activate   # Windows

# Cài đặt dependencies
pip install -r requirements.txt

# Cấu hình môi trường
cp .env.example .env
# Mở .env và điền API keys của bạn
```

## Cấu hình API Keys

Chỉnh sửa file `.env`:

```env
# Lấy miễn phí tại https://aistudio.google.com/
GEMINI_API_KEY=your-key-here

# Lấy miễn phí tại https://huggingface.co/settings/tokens
# (Không bắt buộc - có rule-based fallback)
HUGGINGFACE_TOKEN=your-token-here
```

## Chạy service

```bash
uvicorn main:app --reload --port 8000
```

Service sẽ chạy tại: http://localhost:8000

## API Endpoints

| Method | Endpoint | Mô tả |
|--------|----------|-------|
| GET | `/` | Thông tin service |
| GET | `/health` | Health check |
| POST | `/chat` | Tư vấn AI cho tenant/landlord |
| POST | `/analyze-emotion` | Phân tích cảm xúc văn bản |
| POST | `/complaint` | Tiếp nhận & phân tích khiếu nại |

## Swagger UI

Truy cập http://localhost:8000/docs để xem và test API trực tiếp.

## Lưu ý

- Nếu chưa có `GEMINI_API_KEY`, service vẫn chạy được với rule-based fallback
- Nếu chưa có `HUGGINGFACE_TOKEN`, phân tích cảm xúc dùng từ khóa (rule-based)
- Thêm API keys vào `.env` để có kết quả chính xác hơn

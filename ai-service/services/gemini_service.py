import os
from typing import Optional, List
import google.generativeai as genai
from dotenv import load_dotenv

load_dotenv()

GEMINI_API_KEY = os.getenv("GEMINI_API_KEY", "")

SYSTEM_PROMPT = """Bạn là trợ lý AI của hệ thống quản lý phòng trọ. 
Nhiệm vụ của bạn là hỗ trợ tenant (người thuê trọ) và landlord (chủ trọ) 
giải quyết các vấn đề liên quan đến:
- Thông tin phòng trọ, giá thuê, tiện ích
- Hợp đồng thuê (ký kết, gia hạn, chấm dứt)
- Hóa đơn và thanh toán (điện, nước, phí quản lý)
- Lịch hẹn xem phòng
- Gửi khiếu nại và phản ánh
- Quy trình và thủ tục liên quan

Hãy trả lời ngắn gọn, thân thiện và chuyên nghiệp bằng tiếng Việt.
Nếu câu hỏi nằm ngoài phạm vi hệ thống quản lý phòng trọ, hãy lịch sự từ chối và hướng dẫn lại.
Khi tiếp nhận khiếu nại, hãy thể hiện sự đồng cảm và hướng dẫn cách gửi khiếu nại chính thức qua hệ thống."""

FALLBACK_RESPONSES = {
    "hóa đơn": "Để xem hóa đơn, bạn vào mục 'Hóa đơn' trong menu chính. Nếu có thắc mắc về hóa đơn, vui lòng liên hệ chủ trọ hoặc gửi khiếu nại qua hệ thống.",
    "hợp đồng": "Thông tin hợp đồng của bạn có thể xem trong mục 'Hợp đồng'. Nếu cần gia hạn hoặc chấm dứt hợp đồng, vui lòng liên hệ chủ trọ.",
    "khiếu nại": "Để gửi khiếu nại, vào mục 'Khiếu nại' → 'Tạo khiếu nại mới'. Mô tả chi tiết vấn đề để chúng tôi có thể hỗ trợ nhanh nhất.",
    "lịch hẹn": "Để đặt lịch hẹn xem phòng, bạn vào mục 'Lịch hẹn' và chọn thời gian phù hợp.",
    "thanh toán": "Thanh toán có thể thực hiện qua mục 'Thanh toán' trong ứng dụng. Chúng tôi hỗ trợ nhiều hình thức thanh toán khác nhau.",
    "default": "Xin chào! Tôi là trợ lý AI của hệ thống quản lý phòng trọ. Tôi có thể hỗ trợ bạn về hóa đơn, hợp đồng, khiếu nại, và lịch hẹn. Bạn cần hỗ trợ gì?",
}

def _get_fallback_response(message: str) -> str:
    msg_lower = message.lower()
    for keyword, response in FALLBACK_RESPONSES.items():
        if keyword in msg_lower:
            return response
    return FALLBACK_RESPONSES["default"]

async def chat_with_gemini(message: str, history: Optional[List[dict]] = None, context: Optional[str] = None) -> str:
    load_dotenv(override=True)
    api_key = os.getenv("GEMINI_API_KEY", "")

    if not api_key or api_key == "your-gemini-api-key-here":
        return _get_fallback_response(message)

    try:
        genai.configure(api_key=api_key)
        
        system_instruction = SYSTEM_PROMPT
        if context and context.strip():
            system_instruction += (
                f"\n\n[DỮ LIỆU HỆ THỐNG THỰC TẾ CỦA NGƯỜI DÙNG HIỆN TẠI]:\n{context}\n\n"
                "QUY TẮC BẮT BUỘC:\n"
                "1. Bạn CHỈ được sử dụng dữ liệu thực tế được cung cấp ở trên để trả lời các câu hỏi về hợp đồng, hóa đơn, lịch hẹn, tiện ích, khiếu nại.\n"
                "2. TUYỆT ĐỐI KHÔNG tự bịa đặt, tự vẽ ra hoặc tự sinh dữ liệu giả (không được tự tạo tên phòng, mã hợp đồng giả, số tiền giả, ngày tháng giả, hoặc trạng thái giả) nếu dữ liệu trên không đề cập đến.\n"
                "3. Nếu dữ liệu hệ thống trên KHÔNG có hoặc trống (ví dụ: ghi 'Không tìm thấy hợp đồng', hoặc không có lịch hẹn/khiếu nại nào), bạn phải trả lời lịch sự rằng hệ thống hiện tại chưa ghi nhận thông tin này của người dùng, tuyệt đối không tự bịa ra thông tin giả để trả lời."
            )

        model = genai.GenerativeModel(
            model_name="gemini-flash-latest",
            system_instruction=system_instruction,
        )

        chat = model.start_chat(history=history or [])
        response = chat.send_message(message)
        return response.text

    except Exception as e:
        import traceback
        print(f"[Error] Gemini Chat API failed: {e}")
        traceback.print_exc()
        error_str = str(e).lower()
        if "api_key" in error_str or "invalid" in error_str or "permission" in error_str:
            return "API key Gemini không hợp lệ. Vui lòng kiểm tra lại."
        if "quota" in error_str or "rate" in error_str or "resource" in error_str:
            return "Tính năng sinh câu trả lời đang tạm ngưng do API Key của Google đạt giới hạn miễn phí (20 tin nhắn/phút). Vui lòng đợi 30 giây rồi thử lại."
        return _get_fallback_response(message)

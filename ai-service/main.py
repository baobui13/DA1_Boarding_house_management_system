import os
from typing import List
from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from dotenv import load_dotenv

from services.gemini_service import chat_with_gemini, SYSTEM_PROMPT
from services.emotion_service import analyze_emotion

load_dotenv()

app = FastAPI(
    title="AI Chatbot Service",
    version="1.0.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://localhost:5046",
        "http://localhost:5047",
        "http://localhost:5173",
        "http://localhost:5174",
        "http://localhost:3000",
    ],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class ChatMessage(BaseModel):
    role: str
    text: str

class ChatRequest(BaseModel):
    message: str
    history: List[ChatMessage] = []
    user_role: str = "tenant"
    context: str = ""

class ChatResponse(BaseModel):
    reply: str
    emotion: dict
    suggestions: list[str]
    prompt: str = ""
    context: str = ""

class EmotionRequest(BaseModel):
    text: str

class ComplaintRequest(BaseModel):
    content: str
    tenant_id: str = ""
    tenant_name: str = "Ẩn danh"

class ComplaintAnalysisResponse(BaseModel):
    emotion: dict
    urgency: str
    suggested_category: str
    suggested_response: str

TENANT_SUGGESTIONS = [
    "Hỏi về hóa đơn",
    "Xem hợp đồng",
    "Đặt lịch hẹn",
    "Gửi khiếu nại",
]

LANDLORD_SUGGESTIONS = [
    "Xem danh sách tenant",
    "Thống kê thu chi",
    "Quản lý hợp đồng",
    "Xem thông báo",
]

URGENCY_RESPONSE_TEMPLATE = {
    "high": "Chúng tôi đã ghi nhận vấn đề của bạn với mức độ ưu tiên CAO. Chủ trọ sẽ liên hệ bạn trong vòng 2 giờ.",
    "medium": "Khiếu nại của bạn đã được tiếp nhận. Chúng tôi sẽ xử lý trong vòng 24 giờ làm việc.",
    "low": "Cảm ơn bạn đã phản hồi. Chúng tôi sẽ xem xét và phản hồi sớm nhất có thể.",
}

EMOTION_TO_CATEGORY = {
    "anger": "urgent_complaint",
    "disgust": "quality_complaint",
    "fear": "safety_complaint",
    "sadness": "general_complaint",
    "joy": "positive_feedback",
    "neutral": "general_inquiry",
    "surprise": "general_inquiry",
}

@app.get("/")
async def root():
    return {
        "service": "AI Chatbot Service",
        "version": "1.0.0",
        "status": "running",
    }

@app.get("/health")
async def health():
    return {"status": "ok"}

@app.post("/chat", response_model=ChatResponse)
async def chat(request: ChatRequest):
    try:
        history = [
            {"role": msg.role, "parts": [{"text": msg.text}]}
            for msg in request.history
        ]

        emotion = await analyze_emotion(request.message)
        dominant_emotion = emotion.get("label", "neutral")
        dominant_emotion_vi = emotion.get("label_vi", "bình thường")

        emotion_context = f"[TRẠNG THÁI CẢM XÚC CỦA NGƯỜI DÙNG]: {dominant_emotion_vi.upper()} ({dominant_emotion}).\n"
        if dominant_emotion == "anger":
            emotion_context += "LƯU Ý: Khách thuê đang cực kỳ TỨC GIẬN, bực bội hoặc khó chịu. Bạn PHẢI phản hồi bằng thái độ vô cùng lịch sự, từ tốn, nhận trách nhiệm, xoa dịu cơn giận của họ trước, tuyệt đối không đôi co và đề xuất hướng xử lý khẩn cấp."
        elif dominant_emotion == "fear":
            emotion_context += "LƯU Ý: Người dùng đang LO LẮNG, SỢ HÃI hoặc BẤT AN. Bạn cần trả lời với thái độ trấn an, thấu hiểu, cung cấp thông tin an toàn rõ ràng để họ cảm thấy an tâm."
        elif dominant_emotion == "sadness":
            emotion_context += "LƯU Ý: Người dùng đang BUỒN BÃ, THẤT VỌNG hoặc GẶP HOÀN CẢNH KHÓ KHĂN. Hãy trả lời đầy sự đồng cảm, chia sẻ và đề xuất các giải pháp hỗ trợ thiết thực, tử tế."
        elif dominant_emotion == "joy":
            emotion_context += "LƯU Ý: Người dùng đang VUI MỪNG, HÀI LÒNG hoặc gửi lời khen. Hãy phản hồi với thái độ niềm nở, tích cực, cảm ơn họ chân thành để thắt chặt mối quan hệ."
        else:
            emotion_context += "Hãy giữ phong thái chuyên nghiệp, thân thiện và hỗ trợ như bình thường."

        full_context = f"{emotion_context}\n\n{request.context}"

        reply = await chat_with_gemini(request.message, history, full_context)

        suggestions = (
            TENANT_SUGGESTIONS if request.user_role == "tenant" else LANDLORD_SUGGESTIONS
        )

        return ChatResponse(
            reply=reply,
            emotion=emotion,
            suggestions=suggestions,
            prompt=SYSTEM_PROMPT,
            context=request.context
        )

    except Exception as e:
        print(f"[Error] Chat endpoint failed: {e}")
        raise HTTPException(status_code=500, detail=f"Internal Server Error: {str(e)}")

@app.post("/analyze-emotion")
async def emotion_endpoint(request: EmotionRequest):
    if not request.text or not request.text.strip():
        raise HTTPException(status_code=400, detail="Văn bản không được để trống.")

    result = await analyze_emotion(request.text)
    return result

@app.post("/complaint", response_model=ComplaintAnalysisResponse)
async def analyze_complaint(request: ComplaintRequest):
    if not request.content or not request.content.strip():
        raise HTTPException(status_code=400, detail="Nội dung khiếu nại không được để trống.")

    emotion = await analyze_emotion(request.content)
    urgency = emotion.get("urgency", "low")
    label = emotion.get("label", "neutral")

    return ComplaintAnalysisResponse(
        emotion=emotion,
        urgency=urgency,
        suggested_category=EMOTION_TO_CATEGORY.get(label, "general_complaint"),
        suggested_response=URGENCY_RESPONSE_TEMPLATE.get(urgency, URGENCY_RESPONSE_TEMPLATE["low"]),
    )

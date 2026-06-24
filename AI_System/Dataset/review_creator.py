import pandas as pd
from openai import OpenAI
import os
import time
import random
import json
import csv
import re # Đã thêm thư viện Regex để xử lý tách phần suy nghĩ

# ==========================================
# CẤU HÌNH API & FILE
# ==========================================
# Điền API Key của bạn vào đây
client = OpenAI(
    api_key="",
    base_url="https://openrouter.ai/api/v1"
)

INPUT_FILE = r"D:\UIT\DoAn1\AiTraining\ABSA_Metadata.csv"
OUTPUT_FILE = r"D:\UIT\DoAn1\AiTraining\Generated_ABSA_Dataset.csv"

# Chuẩn hóa nhãn tiếng Anh cho JSON
SENTIMENT_MAP = {
    1: "positive",
    0: "neutral",
    -1: "negative"
}

LENGTH_TYPES = [
    ("Ngắn", "1-2 câu"),
    ("Trung bình", "3-4 câu"),
    ("Dài", "5-7 câu")
]
LENGTH_WEIGHTS = [0.3, 0.5, 0.2]

EMOTION_LEVELS = ["Nhẹ", "Vừa", "Mạnh"]
EMOTION_WEIGHTS = [0.4, 0.4, 0.2]

BANNED_WORDS = [
    "RoomQuality", "Noise", "Utilities", "Security", "Landlord", 
    "Location", "Price", "Positive", "Negative", "Neutral"
]

# ==========================================
# CẤU HÌNH PROMPT: SYSTEM VÀ TÍNH CÁCH
# ==========================================

# ĐÃ NÂNG CẤP THÊM BƯỚC THINKING VÀO SYSTEM PROMPT
SYSTEM_PROMPT_BASE = """Bạn là một người dùng thực tế tại Việt Nam đang viết bình luận (review) phòng trọ trên các hội nhóm Facebook/Chợ Tốt.

Để đảm bảo chất lượng dữ liệu, bạn PHẢI tuân thủ quy trình 2 bước sau:

BƯỚC 1: SUY NGHĨ (THINKING)
Đặt phần suy nghĩ của bạn vào trong cặp thẻ <thinking> ... </thinking>. Hãy tự phân tích một cách ngắn gon về các khía cạnh của phòng trọ dựa trên dữ liệu đầu vào (JSON) mà bạn nhận được. Phân tích phải ngắn gọn, không tốn nhiều token và thời gian Cụ thể:
- Danh sách các khía cạnh cần viết (có giá trị positive, neutral, negative) là gì?
- mức độ chi tiết trong nhận xét về từng khía cạnh là gì? (ví dụ: chỉ nêu chung chung hay đi sâu vào từng điểm nhỏ như phòng có cửa sổ không, có nóng không, có gần bệnh viện, có bị đột nhập không...)
- Danh sách các khía cạnh bị cấm (có giá trị "no comment") là gì? (Phải tự nhắc nhở bản thân KHÔNG ĐƯỢC nhắc đến chúng).
- Nếu TẤT CẢ đều là "no comment", bạn sẽ viết ngắn gọn là không quan tâm, không có gì nhận xét hay viết xàm về chủ đề gì ngoài lề?
- Phong cách nhân vật, độ dài và mức cảm xúc cần thể hiện là gì?
- chú ý show dont tell, ví dụ đùng nói thẳng là tôi thích..., aspect nào tốt như chủ trọ tốt, wifi tốt, chất lượng phòng tốt,..., mà hãy kể chi tiết như chủ trọ hay tặng đồ ăn, tường cách âm không tốt, cửa sổ lớn nên rất thoáng mát, wifi như rùa bò,...

BƯỚC 2: VIẾT REVIEW (REVIEW_TEXT)
Viết đoạn văn review BÊN NGOÀI và NẰM SAU thẻ đóng </thinking>. 
LUẬT TỐI CAO CẦN TUÂN THỦ:
- Bằng tiếng Việt đời thường, tự nhiên, liền mạch.
- Chỉ viết về các khía cạnh có yêu cầu, tuyệt đối bỏ qua các khía cạnh "no comment". Việc viết thừa sẽ làm hỏng dữ liệu huấn luyện!
- Hạn chế đề cập trực tiếp như chất lượng phòng tốt, chủ trọ xấu tính,... mà hãy để cập bằng chi tiết nhỏ như cửa sổ lớn nên rất thoáng mát, chủ trọ hay tặng đồ ăn, wifi như rùa bò,... để tăng tính chân thực và đa dạng cho dữ liệu, tránh đề cập trực tiếp sẽ làm dữ liệu bị cứng nhắc, thiếu tự nhiên.
- Nếu tất cả aspects đều là "no comment", có thể viết về tất cả chủ đề dù là nói xàm không liên quan đến thuê trọ nhưng không được phép nhắc đến bất kỳ khía cạnh nào.
- KHÔNG dùng gạch đầu dòng, không liệt kê, không bê nguyên các từ tiếng Anh (như RoomQuality) vào bài viết."""

PERSONAS = [
    "Sinh viên Gen Z: Giọng điệu trẻ trung, hơi nhây, hay dùng từ lóng mạng (khum, chê, đỉnh chóp, 10 điểm không có nhưng) và thỉnh thoảng gõ teencode nhẹ.",
    "Dân văn phòng bận rộn: Viết ngắn gọn, súc tích, đi thẳng vào vấn đề. Quan tâm nhiều đến tính thực dụng, sự yên tĩnh sau giờ làm.",
    "Người kỹ tính (khó ở): Câu cú dài dòng, hay soi xét tiểu tiết. Khi chê thì dùng từ ngữ rất xéo xắt, mỉa mai. Khi khen thì cũng khen rất dè dặt.",
    "Người dễ dãi, xuề xòa: Giọng điệu bình dân, dễ tính. Hay dùng các từ 'tạm ổn', 'cũng được', 'giá này đòi hỏi gì thêm'. Viết rất ngắn.",
    "Người có gia đình / Vợ chồng trẻ: Giọng điệu chín chắn, thường hay đứng trên góc độ sinh hoạt gia đình, có con nhỏ để đánh giá.",
    "Người review dạo tấu hài: Văn phong hài hước, hay so sánh ví von tấu hài (ví dụ: nóng như cái lò gạch, mạng load chậm hơn rùa, phòng đẹp như khách sạn 5 sao).",
    "Người viết sai chính tả (nhẹ): Đóng vai một người lao động phổ thông, câu cú đôi khi thiếu chủ ngữ, sai dấu ngã/hỏi hoặc viết tắt (ko, dc, r, v...)."
]

def get_random_persona():
    return random.choice(PERSONAS)

def is_valid_review(text):
    if not text or len(text) < 10:
        return False
    for word in BANNED_WORDS:
        if word.lower() in text.lower():
            return False
    return True

# ==========================================
# HÀM TẠO USER PROMPT DẠNG JSON
# ==========================================
def build_user_prompt(row):
    input_json_dict = {}
    for col in row.index:
        if col != "Num_Aspects":
            val = row[col]
            input_json_dict[col] = SENTIMENT_MAP[val] if val != -99 else "no comment"

    length_name, length_desc = random.choices(LENGTH_TYPES, weights=LENGTH_WEIGHTS, k=1)[0]
    emotion = random.choices(EMOTION_LEVELS, weights=EMOTION_WEIGHTS, k=1)[0]

    json_payload_string = json.dumps(input_json_dict, indent=2)

    return f"""Dữ liệu đầu vào (Aspect & Sentiment JSON):
{json_payload_string}

Yêu cầu định dạng văn bản:
- Độ dài mong muốn: {length_desc}
- Mức độ biểu đạt cảm xúc: {emotion}

Hãy thực hiện Bước 1 (Suy nghĩ trong <thinking>) và Bước 2 (Viết review ngoài thẻ) ngay dưới đây:"""

# ==========================================
# HÀM GỌI API & LỌC TEXT
# ==========================================
def generate_review(user_prompt, persona, max_retries=3):
    system_content = f"{SYSTEM_PROMPT_BASE}\n\n[PHONG CÁCH BẠN PHẢI ĐÓNG VAI LẦN NÀY]:\n{persona}"
    
    for attempt in range(max_retries):
        try:
            response = client.chat.completions.create(
                model="deepseek/deepseek-v4-flash", 
                messages=[
                    {"role": "system", "content": system_content},
                    {"role": "user", "content": user_prompt}
                ],
                temperature=0.5, 
                max_tokens=600 # Tăng Token lên 600 để đủ chỗ cho phần AI tự suy nghĩ
            )
            raw_output = response.choices[0].message.content.strip()

            # Dùng Regex để loại bỏ toàn bộ chuỗi nằm trong thẻ <thinking>...</thinking>
            clean_review = re.sub(r'<thinking>.*?</thinking>', '', raw_output, flags=re.DOTALL).strip()

            # Nếu mô hình lỡ sinh ra chữ "Review:" ở đầu thì cắt luôn
            if clean_review.lower().startswith("review:"):
                clean_review = clean_review[7:].strip()

            if is_valid_review(clean_review):
                return clean_review
            else:
                raise ValueError("Review chứa từ khóa cấm hoặc sau khi cắt thẻ thinking thì bị ngắn")
                
        except Exception as e:
            print(f"\n[Lỗi] Thử lại lần {attempt + 1}/{max_retries}. Chi tiết: {e}")
            time.sleep(3)
            
    return "ERROR_GENERATION"

# ==========================================
# THỰC THI CHÍNH
# ==========================================
if __name__ == "__main__":
    df = pd.read_csv(INPUT_FILE)
    total_rows = len(df)
    columns_list = list(df.columns) + ["Persona", "Review_Text"]
    
    if os.path.exists(OUTPUT_FILE):
        df_done = pd.read_csv(OUTPUT_FILE)
        start_index = len(df_done)
        print(f"Phát hiện file đã tồn tại. Bắt đầu chạy tiếp từ dòng {start_index}...")
        file_mode = 'a'
        write_header = False
    else:
        start_index = 0
        file_mode = 'w'
        write_header = True
        print("Đã tạo file output mới. Bắt đầu sinh văn bản...")

    with open(OUTPUT_FILE, mode=file_mode, newline='', encoding='utf-8-sig') as f:
        writer = csv.DictWriter(f, fieldnames=columns_list)
        
        if write_header:
            writer.writeheader()

        for index in range(start_index, total_rows):
            row = df.iloc[index]
            user_prompt = build_user_prompt(row)
            current_persona = get_random_persona()
            
            print(f"Đang sinh dòng {index + 1}/{total_rows} | Aspects: {row['Num_Aspects']} ...", end=" ", flush=True)
            
            start_time = time.time()
            review_text = generate_review(user_prompt, current_persona)
            time_taken = time.time() - start_time
            
            row_dict = row.to_dict()
            row_dict["Persona"] = current_persona.split(":")[0] 
            row_dict["Review_Text"] = review_text
            
            writer.writerow(row_dict)
            f.flush()
            
            print(f"Xong! ({time_taken:.1f}s)")
            
            time.sleep(1)
            
    print("\nHOÀN THÀNH TOÀN BỘ DATASET!")
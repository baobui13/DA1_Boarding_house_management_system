import os
import sys
import torch
import torch.nn as nn
import torch.optim as optim
from torch.utils.data import Dataset, DataLoader
from transformers import AutoTokenizer, AutoModel
from datasets import load_dataset
import random
import json

# Them duong dan import
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from services.emotion_service import BertBiLSTMEmotionClassifier, EMOTION_LABELS

# Du lieu goc tieng Viet (50 mau)
VI_DATA_ORIGINAL = [
    ("Tháng này anh tính tiền điện điêu thế", 0),
    ("Phòng bên cạnh ồn ào quá không ngủ được", 0),
    ("Báo sửa ống nước cả tuần không ai đến, làm ăn kiểu gì vậy", 0),
    ("Sao tự nhiên lại tăng giá phòng", 0),
    ("Cọc tiền rồi mà giờ kêu hết phòng là sao", 0),
    ("Thái độ phục vụ quá kém", 0),
    ("Anh làm ăn kiểu gì mà để xe bị xước hết thế này", 0),
    ("Nhắn tin không thèm trả lời luôn à", 0),
    ("Tháng này lương chậm, chắc tôi không đóng đủ tiền trọ", 5),
    ("Chuyển trọ buồn quá, quen chỗ này rồi", 5),
    ("Mới bị đuổi việc, anh thư thả cho em vài hôm nhé", 5),
    ("Phòng dột hết rồi ướt hết đồ đạc", 5),
    ("Tháng này đóng tiền mạng mà chẳng dùng được bao nhiêu", 5),
    ("mái nhà đang dột mưa", 5),
    ("mái nhà bị dột nước mỗi khi trời mưa lớn", 5),
    ("mưa to làm nước ngập hết cả phòng trọ rồi", 5),
    ("phòng dột nước mưa ướt hết chăn màn giường chiếu", 5),
    ("nước chảy từ trần nhà xuống ướt hết cả đồ", 5),
    ("mái nhà dột nát quá anh ơi", 5),
    ("trần nhà bị thấm dột nước mưa chảy ròng ròng", 5),
    ("Phòng mát mẻ sạch sẽ tuyệt vời", 3),
    ("Chủ nhà nhiệt tình ghê", 3),
    ("Tôi rất thích phòng này", 3),
    ("Giá cả hợp lý, ưng bụng lắm", 3),
    ("Khu này an ninh tốt, yên tĩnh", 3),
    ("Cảm ơn anh đã sửa giúp em cái vòi nước nhé", 3),
    ("Phòng đẹp y như hình luôn", 3),
    ("Nhà vệ sinh bốc mùi hôi thối quá", 1),
    ("Phòng nhiều rác bẩn thỉu kinh khủng", 1),
    ("Cống trào ngược dơ dáy thật sự", 1),
    ("Khu vực vứt rác chung tởm quá", 1),
    ("Mùi ẩm mốc không thể ngửi nổi", 1),
    ("Chuột bọ chạy khắp nơi gớm quá", 1),
    ("Hành lang tối om sợ quá", 2),
    ("Khu này hay mất trộm lo ghê", 2),
    ("Có người lạ rình rập ở cổng", 2),
    ("Nửa đêm có tiếng gõ cửa ớn lạnh", 2),
    ("Khóa cổng lỏng lẻo sợ trộm vào", 2),
    ("Ổ điện chập chờn sợ cháy chập quá", 2),
    ("Ôi phòng mới xây đẹp bất ngờ", 6),
    ("Không ngờ khu này lại rộng rãi thế", 6),
    ("Tự nhiên được giảm giá phòng bất ngờ ghê", 6),
    ("Tôi muốn xem hợp đồng thuê", 4),
    ("Anh có ở nhà không để tôi qua lấy chìa khóa", 4),
    ("Cho tôi hỏi tiền phòng tháng này bao nhiêu", 4),
    ("Gửi xe ở đâu vậy anh", 4),
    ("Wifi pass là gì thế", 4)
]

# Du lieu tieng Anh tuong duong
EN_DATA_ORIGINAL = [
    ("You calculated the electricity bill so dishonestly this month.", 0),
    ("The next room is too noisy, I can't sleep.", 0),
    ("I reported the plumbing issue a week ago and no one came, what kind of service is this?", 0),
    ("Why did you suddenly increase the room rent?", 0),
    ("I already deposited money but now you say the room is gone, what's going on?", 0),
    ("The service attitude is extremely poor.", 0),
    ("What kind of management is this to let my motorbike get scratched like this?", 0),
    ("You didn't even bother to reply to my messages?", 0),
    ("My salary is late this month, I probably can't pay the full rent.", 5),
    ("Moving out is so sad, I am used to this place.", 5),
    ("I just got laid off, please give me a few extra days for rent.", 5),
    ("The room is leaking everywhere and all my belongings are wet.", 5),
    ("I paid for the internet this month but barely got to use it.", 5),
    ("The roof is currently leaking rain.", 5),
    ("The roof leaks water every time it rains heavily.", 5),
    ("Heavy rain flooded my entire rented room.", 5),
    ("The room leaks rain water, soaking all my blankets and bedding.", 5),
    ("Water is dripping from the ceiling, wetting all my things.", 5),
    ("The roof is so dilapidated and leaking, landlord.", 5),
    ("The ceiling is leaking and rainwater is dripping down continuously.", 5),
    ("My rented room has a leak.", 5),
    ("The ceiling of my rented room is leaking.", 5),
    ("The room is cool, clean, and wonderful.", 3),
    ("The landlord is so enthusiastic and helpful.", 3),
    ("I really like this room.", 3),
    ("The price is reasonable, I'm very satisfied.", 3),
    ("This area has good security and is very quiet.", 3),
    ("Thank you for fixing the water tap for me.", 3),
    ("The room is beautiful, just like the pictures.", 3),
    ("The toilet smells so bad and stinky.", 1),
    ("The room is full of trash and terribly dirty.", 1),
    ("The sewer is overflowing, it's truly filthy.", 1),
    ("The shared trash disposal area is disgusting.", 1),
    ("The musty smell is unbearable.", 1),
    ("Mice and bugs are running everywhere, so gross.", 1),
    ("The hallway is pitch black, so scary.", 2),
    ("This area is prone to theft, I'm so worried.", 2),
    ("There is a stranger lurking at the gate.", 2),
    ("There is a chilling knock at the door in the middle of the night.", 2),
    ("The gate lock is loose, I'm afraid thieves will get in.", 2),
    ("The power outlet is flickering, I'm so afraid of a short circuit fire.", 2),
    ("Oh, the newly built room is surprisingly beautiful.", 6),
    ("I didn't expect this area to be so spacious.", 6),
    ("Getting a room discount out of nowhere is a pleasant surprise.", 6),
    ("I want to see the lease contract.", 4),
    ("Are you home so I can come over to get the keys?", 4),
    ("May I ask how much the rent is this month?", 4),
    ("Where do I park my vehicle, landlord?", 4),
    ("What is the wifi password?", 4)
]

HF_LABEL_MAPPING = {
    0: 5,  # sadness -> sadness
    1: 3,  # joy -> joy
    2: 3,  # love -> joy
    3: 0,  # anger -> anger
    4: 2,  # fear -> fear
    5: 6   # surprise -> surprise
}

class EmotionDataset(Dataset):
    def __init__(self, texts, labels):
        self.texts = texts
        self.labels = labels

    def __len__(self):
        return len(self.texts)

    def __getitem__(self, idx):
        return self.texts[idx], self.labels[idx]

def prepare_data():
    print("Dang tai dataset 'dair-ai/emotion'...")
    try:
        hf_dataset = load_dataset("dair-ai/emotion", trust_remote_code=True)
    except Exception as e:
        print(f"[ERROR] Khong the tai dataset: {e}")
        sys.exit(1)

    train_texts = [item['text'] for item in hf_dataset['train']]
    train_labels = [HF_LABEL_MAPPING[item['label']] for item in hf_dataset['train']]

    val_texts = [item['text'] for item in hf_dataset['validation']]
    val_labels = [HF_LABEL_MAPPING[item['label']] for item in hf_dataset['validation']]

    # Doc du lieu tieng Viet tu json
    vi_json_path = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "data/vietnamese_boarding_house_emotions.json")
    vi_data = []
    if os.path.exists(vi_json_path):
        print(f"Nap du lieu tieng Viet tu: {vi_json_path}")
        with open(vi_json_path, "r", encoding="utf-8") as f:
            vi_data = json.load(f)
            
    for text, label in VI_DATA_ORIGINAL:
        vi_data.append({"text": text, "label": label})

    # Doc du lieu tieng Anh tu json
    en_json_path = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "data/english_boarding_house_emotions.json")
    en_data = []
    if os.path.exists(en_json_path):
        print(f"Nap du lieu tieng Anh tu: {en_json_path}")
        with open(en_json_path, "r", encoding="utf-8") as f:
            en_data = json.load(f)
            
    for text, label in EN_DATA_ORIGINAL:
        en_data.append({"text": text, "label": label})

    # Cân bằng dữ liệu (upscale để tương xứng với tập English chung của HF)
    # Nhân bản cả hai tập Anh-Việt phòng trọ lên 15 lần
    upscale_factor = 15
    vi_train_texts = [item["text"] for item in vi_data] * upscale_factor
    vi_train_labels = [item["label"] for item in vi_data] * upscale_factor
    
    en_train_texts = [item["text"] for item in en_data] * upscale_factor
    en_train_labels = [item["label"] for item in en_data] * upscale_factor

    print(f"Upscaling du lieu tieng Viet: {len(vi_data)} -> {len(vi_train_texts)}")
    print(f"Upscaling du lieu tieng Anh: {len(en_data)} -> {len(en_train_texts)}")

    train_texts.extend(vi_train_texts)
    train_labels.extend(vi_train_labels)
    
    train_texts.extend(en_train_texts)
    train_labels.extend(en_train_labels)

    # Shuffle tập train
    combined = list(zip(train_texts, train_labels))
    random.shuffle(combined)
    train_texts, train_labels = zip(*combined)

    print(f"Tong du lieu huan luyen: {len(train_texts)}")

    
    return (
        EmotionDataset(train_texts, train_labels),
        EmotionDataset(val_texts, val_labels)
    )

def main():
    device = torch.device("mps" if torch.backends.mps.is_available() else "cpu")
    print(f"Thiet bi huan luyen: {device}")

    train_dataset, val_dataset = prepare_data()
    train_loader = DataLoader(train_dataset, batch_size=64, shuffle=True)
    val_loader = DataLoader(val_dataset, batch_size=64, shuffle=False)

    print("Dang tai BERT...")
    tokenizer = AutoTokenizer.from_pretrained("bert-base-multilingual-cased")
    bert_model = AutoModel.from_pretrained("bert-base-multilingual-cased").to(device)
    bert_model.eval()  # Freeze BERT

    # Khởi tạo Classifier BiLSTM
    model = BertBiLSTMEmotionClassifier(num_classes=7).to(device)

    # Dùng learning rate nhỏ hơn (2e-4 thay vì 1e-3) để ngăn mô hình nhớ vẹt dữ liệu tiếng Việt
    optimizer = optim.AdamW(model.parameters(), lr=2e-4, weight_decay=1e-2)
    criterion = nn.CrossEntropyLoss()

    EPOCHS = 4  # Chỉ huấn luyện 4 epochs để tránh overfitting
    best_val_loss = float('inf')
    model_dir = "models"
    os.makedirs(model_dir, exist_ok=True)
    model_path = os.path.join(model_dir, "boarding_house_emotion_model.pth")

    print("Bat dau huan luyen...")
    for epoch in range(EPOCHS):
        model.train()
        train_loss = 0.0
        correct_train = 0
        total_train = 0

        for texts, labels in train_loader:
            optimizer.zero_grad()

            inputs = tokenizer(
                list(texts),
                padding=True,
                truncation=True,
                max_length=64,
                return_tensors="pt"
            ).to(device)

            with torch.no_grad():
                bert_outputs = bert_model(**inputs)
                hidden_states = bert_outputs.last_hidden_state

            labels = torch.tensor(labels).to(device)

            # Forward qua BiLSTM Classifier
            outputs = model(hidden_states, inputs.get('attention_mask'))
            loss = criterion(outputs, labels)

            # Backward & Step
            loss.backward()
            optimizer.step()

            train_loss += loss.item() * len(texts)
            _, predicted = torch.max(outputs, 1)
            correct_train += (predicted == labels).sum().item()
            total_train += len(texts)

        avg_train_loss = train_loss / total_train
        train_acc = correct_train / total_train

        # Validation
        model.eval()
        val_loss = 0.0
        correct_val = 0
        total_val = 0

        with torch.no_grad():
            for texts, labels in val_loader:
                inputs = tokenizer(
                    list(texts),
                    padding=True,
                    truncation=True,
                    max_length=64,
                    return_tensors="pt"
                ).to(device)
                
                bert_outputs = bert_model(**inputs)
                hidden_states = bert_outputs.last_hidden_state
                labels = torch.tensor(labels).to(device)

                outputs = model(hidden_states, inputs.get('attention_mask'))
                loss = criterion(outputs, labels)

                val_loss += loss.item() * len(texts)
                _, predicted = torch.max(outputs, 1)
                correct_val += (predicted == labels).sum().item()
                total_val += len(texts)

        avg_val_loss = val_loss / total_val
        val_acc = correct_val / total_val

        print(f"Epoch {epoch+1:02d}/{EPOCHS} | Train Loss: {avg_train_loss:.4f} - Acc: {train_acc:.4f} | Val Loss: {avg_val_loss:.4f} - Acc: {val_acc:.4f}")

        if avg_val_loss < best_val_loss:
            best_val_loss = avg_val_loss
            torch.save(model.state_dict(), model_path)
            print(f"Da luu model tai: {model_path}")

    print("Hoan tat huan luyen!")

if __name__ == "__main__":
    main()

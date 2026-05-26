import os
import re
import torch
import torch.nn as nn
from typing import Optional, Dict, List, Tuple
import asyncio
from concurrent.futures import ThreadPoolExecutor

EMOTION_LABELS = ["anger", "disgust", "fear", "joy", "neutral", "sadness", "surprise"]

EMOTION_LABEL_VI = {
    "anger": "tức giận",
    "disgust": "ghét bỏ",
    "fear": "lo sợ",
    "joy": "vui mừng",
    "neutral": "bình thường",
    "sadness": "buồn bã",
    "surprise": "ngạc nhiên",
}

URGENCY_MAP = {
    "anger": "high",
    "disgust": "high",
    "fear": "medium",
    "joy": "low",
    "neutral": "low",
    "sadness": "medium",
    "surprise": "low",
}

EMOTION_COLOR = {
    "anger": "#ef4444",
    "disgust": "#f97316",
    "fear": "#eab308",
    "joy": "#22c55e",
    "neutral": "#94a3b8",
    "sadness": "#3b82f6",
    "surprise": "#a855f7",
}

class BertBiLSTMEmotionClassifier(nn.Module):
    # Model phan tich cam xuc BERT + BiLSTM
    def __init__(
        self,
        bert_hidden_size: int = 768,
        lstm_hidden_size: int = 256,
        num_layers: int = 2,
        num_classes: int = 7,
        dropout: float = 0.3,
    ):
        super().__init__()
        self.bilstm = nn.LSTM(
            input_size=bert_hidden_size,
            hidden_size=lstm_hidden_size,
            num_layers=num_layers,
            batch_first=True,
            bidirectional=True,
            dropout=dropout if num_layers > 1 else 0,
        )
        self.dropout = nn.Dropout(dropout)
        self.attention = nn.Linear(lstm_hidden_size * 2, 1)
        self.classifier = nn.Linear(lstm_hidden_size * 2, num_classes)

    def forward(
        self,
        bert_hidden_states: torch.Tensor,
        attention_mask: Optional[torch.Tensor] = None,
    ) -> torch.Tensor:
        lstm_out, _ = self.bilstm(bert_hidden_states)
        attn_weights = self.attention(lstm_out).squeeze(-1)

        if attention_mask is not None:
            attn_weights = attn_weights.masked_fill(attention_mask == 0, float("-inf"))

        attn_weights = torch.softmax(attn_weights, dim=-1)
        attended = torch.bmm(
            attn_weights.unsqueeze(1), lstm_out
        ).squeeze(1)

        attended = self.dropout(attended)
        logits = self.classifier(attended)
        return logits

class BertBiLSTMEmotionService:
    # Service phan tich cam xuc
    def __init__(self):
        self._tokenizer = None
        self._bert_model = None
        self._bilstm_model = None
        self._device = None
        self._loaded = False
        self._loading = False
        self._executor = ThreadPoolExecutor(max_workers=2)

    def _load_models(self) -> bool:
        if self._loaded:
            return True

        try:
            from transformers import AutoTokenizer, AutoModel

            print("[INFO] Loading BERT model...")
            MODEL_NAME = "bert-base-multilingual-cased"

            self._tokenizer = AutoTokenizer.from_pretrained(MODEL_NAME)
            self._bert_model = AutoModel.from_pretrained(MODEL_NAME)
            self._bert_model.eval()

            self._bilstm_model = BertBiLSTMEmotionClassifier(
                bert_hidden_size=768,
                lstm_hidden_size=256,
                num_layers=2,
                num_classes=7,
                dropout=0.3,
            )

            model_path = os.path.join(os.path.dirname(__file__), "../models/boarding_house_emotion_model.pth")
            self._is_fine_tuned = False
            
            if torch.backends.mps.is_available():
                self._device = torch.device("mps")
                print("[INFO] Using Apple Silicon MPS acceleration")
            else:
                self._device = torch.device("cpu")
                print("[INFO] Using CPU")

            if os.path.exists(model_path):
                print(f"[INFO] Fine-tuned model loaded from {model_path}")
                self._bilstm_model.load_state_dict(torch.load(model_path, map_location=self._device))
                self._is_fine_tuned = True
            else:
                print("[WARN] Fine-tuned model not found, using untrained weights.")

            self._bert_model.to(self._device)
            self._bilstm_model.to(self._device)
            self._bilstm_model.eval()

            self._loaded = True
            print("[INFO] BERT + BiLSTM model ready.")
            return True

        except ImportError:
            print("[WARN] transformers package not installed. Using fallback.")
            return False
        except Exception as e:
            print(f"[ERROR] Failed to load model: {e}. Using fallback.")
            return False

    def _encode_with_bert_bilstm(self, text: str) -> Dict:
        if not self._loaded:
            return {}

        try:
            inputs = self._tokenizer(
                text,
                return_tensors="pt",
                max_length=128,
                truncation=True,
                padding=True,
            )
            inputs = {k: v.to(self._device) for k, v in inputs.items()}

            with torch.no_grad():
                bert_output = self._bert_model(**inputs)
                hidden_states = bert_output.last_hidden_state

                logits = self._bilstm_model(
                    hidden_states,
                    attention_mask=inputs.get("attention_mask"),
                )

                probs = torch.softmax(logits, dim=-1).squeeze(0).cpu().numpy()

            scores = {label: float(probs[i]) for i, label in enumerate(EMOTION_LABELS)}
            return scores

        except Exception as e:
            print(f"[ERROR] Inference failed: {e}")
            return {}

    def _rule_enhance(self, text: str, bert_scores: Dict) -> Dict:
        # Luat ho tro phan tich tu khoa tuong ung tung cam xuc
        RULES = {
            "anger": [
                r"tức|giận|bực|khó chịu|tệ|vô lý|sai|quá đáng|phẫn nộ|ức chế|không chịu được|nóng|cáu|điên|cục",
                r"angry|furious|outraged|unacceptable|terrible",
            ],
            "sadness": [
                r"buồn|thất vọng|tiếc|khổ|chán|đau lòng|không vui|tủi|khóc|dột|ngập|ướt|mưa",
                r"sad|disappointed|upset|unhappy|regret",
            ],
            "joy": [
                r"vui|hài lòng|tốt|cảm ơn|tuyệt|ổn|được|thích|hài|ok|hay|tuyệt vời|khen",
                r"happy|satisfied|great|thanks|good|nice|excellent",
            ],
            "disgust": [
                r"kinh|tởm|ghét|chán ghét|không thể chịu|dơ|bẩn|hôi|thối|tởm lợm",
                r"disgusting|horrible|awful|nasty",
            ],
            "fear": [
                r"lo|sợ|lo lắng|lo ngại|bất an|hoang mang|sợ hãi|run",
                r"worried|afraid|concerned|scared|anxious",
            ],
            "neutral": [
                r"cho tôi|xem|hợp đồng|hóa đơn|lịch hẹn|ở đâu|bao nhiêu|wifi|chìa khóa|gửi xe|tài khoản|thông tin|yêu cầu|hỏi|tra cứu|khi nào|thủ tục|phòng trọ|studio|địa chỉ",
                r"show me|contract|bill|appointment|where|how much|wifi|key|parking|account|information|inquiry|check|when|procedure|address",
            ],
        }

        text_lower = text.lower()
        rule_boost = {label: 0.0 for label in EMOTION_LABELS}

        for emotion, patterns in RULES.items():
            for pattern in patterns:
                if re.search(pattern, text_lower):
                    rule_boost[emotion] += 0.25

        if sum(rule_boost.values()) == 0.0:
            rule_boost["neutral"] = 0.5

        boost_multiplier = 0.2 if getattr(self, "_is_fine_tuned", False) else 1.0

        if bert_scores:
            combined = {}
            for label in EMOTION_LABELS:
                bert_val = bert_scores.get(label, 1.0 / len(EMOTION_LABELS))
                rule_val = rule_boost.get(label, 0.0) * boost_multiplier
                combined[label] = 0.7 * bert_val + 0.3 * rule_val

            total = sum(combined.values())
            if total > 0:
                combined = {k: v / total for k, v in combined.items()}
            return combined
        else:
            total = sum(rule_boost.values())
            if total > 0:
                return {k: v / total for k, v in rule_boost.items()}
            return {"neutral": 1.0}

    def _sync_analyze(self, text: str) -> Dict:
        if not self._loaded:
            self._load_models()

        bert_scores = self._encode_with_bert_bilstm(text)
        combined_scores = self._rule_enhance(text, bert_scores)

        dominant = max(combined_scores, key=lambda k: combined_scores[k])
        score = combined_scores[dominant]

        source = "bert-bilstm+rules" if self._loaded else "rule-based"

        return {
            "label": dominant,
            "label_vi": EMOTION_LABEL_VI.get(dominant, dominant),
            "score": round(score, 3),
            "urgency": URGENCY_MAP.get(dominant, "low"),
            "color": EMOTION_COLOR.get(dominant, "#94a3b8"),
            "source": source,
            "model": "bert-base-multilingual-cased + BiLSTM" if self._loaded else "rule-based-fallback",
            "all_scores": {k: round(v, 3) for k, v in sorted(
                combined_scores.items(), key=lambda x: -x[1]
            )},
        }

    async def analyze(self, text: str) -> Dict:
        loop = asyncio.get_event_loop()
        result = await loop.run_in_executor(self._executor, self._sync_analyze, text)
        return result

_service: Optional[BertBiLSTMEmotionService] = None

def get_emotion_service() -> BertBiLSTMEmotionService:
    global _service
    if _service is None:
        _service = BertBiLSTMEmotionService()
    return _service

async def analyze_emotion(text: str) -> Dict:
    service = get_emotion_service()
    return await service.analyze(text)

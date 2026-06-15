# ABSA (Aspect-Based Sentiment Analysis) Inference Service

Dedicated lightweight FastAPI microservice to serve the trained **Two-Head Shared Gated Fusion** PhoBERT model for Vietnamese property/boarding-house reviews.

## Model
- Base: `vinai/phobert-base` + LoRA adapter
- Two heads (as provided by training):
  - `aspect_head.pt`: detects whether each aspect is mentioned in the review (presence / binary-ish)
  - `sentiment_head.pt`: predicts sentiment (Positive / Negative / Neutral) for aspects
- Location: `../property_review_absa_model/TwoHead_Shared_GatedFusion_LAST_20260613_201336/`

The .NET backend calls this service when a tenant creates a `Rating`. Results are stored as `RatingAspect` rows and aggregated into `PropertyAspectScore` (used for recommendations).

## Quick Start (Windows + PowerShell)

1. **Create & activate venv** (from repo root or this folder):
```powershell
cd AI_System\absa_service
python -m venv .venv
.\.venv\Scripts\Activate.ps1
```

2. **Install torch CPU first (recommended for dev, avoids huge CUDA download)**:
```powershell
pip install torch --index-url https://download.pytorch.org/whl/cpu
```

3. **Install the rest**:
```powershell
pip install -r requirements.txt
```

4. **(Optional) Copy env**:
```powershell
copy .env.example .env
# Edit .env if you want to override MODEL_DIR or threshold
```

5. **Run the service** (default port 8001):
```powershell
uvicorn main:app --host 0.0.0.0 --port 8001 --reload
```

   Or with env:
```powershell
$env:MODEL_DIR="..\property_review_absa_model\TwoHead_Shared_GatedFusion_LAST_20260613_201336"; uvicorn main:app --port 8001 --reload
```

6. **Test manually** (in another PS or browser):
   - Health: `http://localhost:8001/health`
   - Analyze (PowerShell):
```powershell
Invoke-RestMethod -Method Post -Uri http://localhost:8001/analyze -ContentType "application/json" -Body (@{
  content = "Phòng rất sạch sẽ, rộng rãi, có gác lửng. Wifi mạnh nhưng thỉnh thoảng chập chờn. Chủ trọ thân thiện hỗ trợ nhanh. Hơi ồn vì gần đường."
  stars = 4
} | ConvertTo-Json -Depth 3)
```

Expected: several aspects with `RoomQuality: Positive`, `Wifi: mixed but leaning`, `Landlord: Positive`, `Noise: Negative`, plus `Overall`.

## API

### POST /analyze
Request:
```json
{
  "content": "string (Vietnamese review text, required)",
  "stars": 4   // optional, used for Overall bias + fallback
}
```

Response:
```json
{
  "aspects": [
    { "aspect": "RoomQuality", "sentiment": "Positive", "confidence": 0.91 },
    { "aspect": "Wifi", "sentiment": "Negative", "confidence": 0.67 },
    ...
  ]
}
```

Only aspects the model considers "mentioned" (above `ASPECT_THRESHOLD`) are returned. `Overall` is always synthesized (stars-based or majority) to match existing system expectations.

### GET /health
Returns `{ "status": "ok", "model_loaded": true }`

## Aspect Mapping (must match .NET Enums.ReviewAspect)
Python list (fixed order for the 10-aspect model output):
```python
ASPECT_NAMES = [
    "RoomQuality", "Noise", "Wifi", "Utilities", "Parking",
    "Security", "Environment", "Landlord", "Location", "Price"
]
```
- `Overall` is added server-side (not part of the 10).
- Sentiments map to `Positive` / `Negative` / `Neutral` (exactly as `RatingAttitude`).

## Configuration / Env
- `MODEL_DIR`: path to the folder containing `aspect_head.pt`, `sentiment_head.pt`, `lora_adapter/`, `config.json`.
- `DEVICE`: auto (default), cpu, or cuda.
- `ASPECT_THRESHOLD`: 0.5 default. Raise for stricter "mentioned" detection.
- The service is intentionally unauthenticated (localhost dev only). Add bearer or restrict host in prod.

## Integration with .NET Backend
- Backend config (`appsettings.Development.json`):
  ```json
  "AspectAnalysis": {
    "Enabled": true,
    "PythonServiceUrl": "http://localhost:8001/analyze",
    "RequestTimeoutSeconds": 10,
    "AspectPresenceThreshold": 0.5,
    "FallbackToKeywordOnError": true
  }
  ```
- On rating create the backend calls this URL (with fallback to keyword mock if service is down or times out).
- Startup check is non-fatal (warning only).

## Troubleshooting
- "No module named transformers" → activate venv.
- Slow first inference → normal (model + tokenizer load). Subsequent are faster.
- CUDA OOM or driver issues → force `DEVICE=cpu`.
- Tokenizer warning about `sentencepiece` → `pip install sentencepiece protobuf`.
- Path issues on Windows → use forward slashes or raw strings in .env, or absolute path.
- Model weights not found → verify `MODEL_DIR` points inside the `TwoHead_Shared_GatedFusion...` folder (or its parent).

## Notes for Model Authors
The inference.py contains a **skeleton** for loading:
- PhoBERT base + PEFT LoRA
- The two `.pt` heads + any gated fusion logic you used in training.

**You will likely need to adapt** the `GatedFusion` / head classes and the `forward` / `predict` method to exactly match how you saved `aspect_head.pt` and `sentiment_head.pt` during training (state_dict vs full module, exact submodule names, whether fusion weights are in the heads, etc.).

Look for `TODO: ADAPT HEAD LOADING / FORWARD` comments.

Once adapted, the rest of the pipeline (mapping to .NET enums, Overall synthesis, confidence, threshold) should work as-is.

## License / Scope
Internal for DA1 Boarding House project. Not for production public exposure without auth + rate limiting.

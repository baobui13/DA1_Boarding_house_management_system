"""
main.py
FastAPI entrypoint for the ABSA inference microservice.

Run:
    uvicorn main:app --port 8001 --reload

See README.md for full Windows/PowerShell instructions and model adaptation notes.
"""

import os
import logging
from typing import Optional, List

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field

from inference import analyze, get_predictor

# -------------------------------------------------------------------
# Logging
# -------------------------------------------------------------------
logging.basicConfig(
    level=os.getenv("LOG_LEVEL", "INFO").upper(),
    format="%(asctime)s | %(levelname)-7s | %(name)s | %(message)s",
)
logger = logging.getLogger("absa_service")

# -------------------------------------------------------------------
# Schemas (match the contract the .NET backend expects)
# -------------------------------------------------------------------
class AnalyzeRequest(BaseModel):
    content: str = Field(..., min_length=1, description="Raw review text from tenant (Vietnamese)")
    stars: Optional[int] = Field(None, ge=1, le=5, description="Overall star rating (1-5), helps Overall + bias")


class AspectResult(BaseModel):
    aspect: str
    sentiment: str  # Positive | Negative | Neutral  (exact match to C# RatingAttitude)
    confidence: Optional[float] = None


class AnalyzeResponse(BaseModel):
    aspects: List[AspectResult]


class HealthResponse(BaseModel):
    status: str = "ok"
    model_loaded: bool
    device: Optional[str] = None


# -------------------------------------------------------------------
# App
# -------------------------------------------------------------------
app = FastAPI(
    title="Boarding House ABSA Inference",
    description="Two-head (aspect presence + sentiment) PhoBERT model for Vietnamese rental reviews. "
                "Called by the .NET backend on every tenant Rating creation.",
    version="1.0.0",
)

# CORS: permissive for local dev (backend + possible local FE tools)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.on_event("startup")
async def startup_event():
    logger.info("Starting ABSA service...")
    # Eager load the model so first request is fast and we surface load errors immediately
    try:
        predictor = get_predictor()
        logger.info("Model loaded successfully on startup. device=%s", predictor.device)
    except Exception as ex:
        logger.exception("Failed to load ABSA model on startup: %s", ex)
        # Do not crash the process — .NET has fallback. But log clearly.
        logger.warning("Service will still start. Requests will use fallback behavior or return errors until model loads.")


@app.get("/health", response_model=HealthResponse, tags=["meta"])
async def health():
    try:
        p = get_predictor()
        return HealthResponse(status="ok", model_loaded=True, device=str(p.device))
    except Exception:
        return HealthResponse(status="ok", model_loaded=False, device=None)


@app.post("/analyze", response_model=AnalyzeResponse, tags=["analysis"])
async def analyze_endpoint(req: AnalyzeRequest):
    """
    Main entry point called by the .NET RatingService.

    The .NET side will:
      - POST {content, stars?}
      - Receive {aspects: [...]}
      - Map directly to RatingAspect entities (aspect name must be valid ReviewAspect)
      - Synthesize Overall if missing (this service already adds it)
    """
    try:
        raw_aspects = analyze(req.content, req.stars)
        # Normalize to our Pydantic shape (confidence can be None)
        aspects = [
            AspectResult(
                aspect=a["aspect"],
                sentiment=a["sentiment"],
                confidence=a.get("confidence"),
            )
            for a in raw_aspects
        ]
        logger.debug("Analyzed content len=%d -> %d aspects", len(req.content or ""), len(aspects))
        return AnalyzeResponse(aspects=aspects)
    except Exception as ex:
        logger.exception("Inference failed: %s", ex)
        # Return 503 so .NET knows to trigger its keyword fallback cleanly
        raise HTTPException(status_code=503, detail=f"ABSA inference error: {str(ex)}")


if __name__ == "__main__":
    # For local `python main.py` usage (not the normal path — prefer uvicorn)
    import uvicorn
    port = int(os.getenv("PORT", "8001"))
    uvicorn.run("main:app", host="0.0.0.0", port=port, reload=True)

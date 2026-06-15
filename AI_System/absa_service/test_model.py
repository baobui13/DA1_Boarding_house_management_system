"""
Quick test script to run the real two-head ABSA model on sample reviews.
Run with:
    python test_model.py
"""
import os
import sys
import json
import torch
import warnings

warnings.filterwarnings("ignore")

# Patch the strict torch version check (temporary)
try:
    import transformers.utils.import_utils as iu
    iu.check_torch_load_is_safe = lambda: None
except Exception:
    pass

sys.path.insert(0, os.path.dirname(__file__))
from inference import get_predictor, analyze

def main():
    # === Choose which model to test ===
    # GatedFusion model (user request to test this one):
    model_dir = r"D:\UIT\DoAn1\DA1_Boarding_house_management_system\AI_System\property_review_absa_model\TwoHead_Shared_GatedFusion_LAST_20260613_201336"

    # CondAttn model:
    # model_dir = r"D:\UIT\DoAn1\DA1_Boarding_house_management_system\AI_System\property_review_absa_model\TwoHead_Shared_CondAttn_LAST_20260613_204010"

    print(f"Testing model from: {os.path.basename(model_dir)}")

    print("Loading Two-Head ABSA model (using notebook-exact ABSAFeatureExtractor)...")
    device = "cuda" if torch.cuda.is_available() else "cpu"
    predictor = get_predictor(model_dir=model_dir, device=device, aspect_threshold=0.45)
    print(f"Loaded on {predictor.device}\n")

    test_cases = [
        ("Positive-leaning review", 
         "Phòng trọ sạch sẽ, rộng rãi và có gác lửng tiện lợi. Wifi mạnh, mạng ổn định. Tuy nhiên hơi ồn ào vì gần đường lớn. Chủ trọ thân thiện, hỗ trợ nhanh chóng. Giá thuê hợp lý, đáng tiền.", 
         4),
        ("Negative review", 
         "Phòng bẩn, chật chội, wifi rất yếu hay mất mạng. Chủ trọ khó tính, không hỗ trợ. Giá thì đắt so với chất lượng.", 
         2),
        ("Mixed review", 
         "Phòng khá sạch và thoáng, vị trí trung tâm tiện đi lại. Nhưng điện nước hay bị cắt, chủ nhà thì hơi khó tính.", 
         3),
    ]

    for title, text, stars in test_cases:
        print("=" * 70)
        print(f"{title} (stars={stars})")
        print("Text:", text)
        print("-" * 70)

        results = analyze(text, stars=stars)
        print(f"Detected {len(results)} aspects:\n")
        for item in results:
            conf = item.get("confidence")
            conf_str = f"{conf:.3f}" if conf is not None else "n/a"
            print(f"  {item['aspect']:13s}  {item['sentiment']:8s}   conf={conf_str}")

        print()

if __name__ == "__main__":
    main()

import random
import pandas as pd
from collections import defaultdict

# ==========================================
# CONFIG
# ==========================================

ASPECTS = [
    "RoomQuality",
    "Noise",
    "Wifi",
    "Utilities",
    "Parking",
    "Security",
    "Environment",
    "Landlord",
    "Location",
    "Price"
]

# số review theo số aspect
DISTRIBUTION = {
    0: 250,
    1: 1250,
    2: 1500,
    3: 1100,
    4: 500,
    5: 200,
    6: 100,
    7: 50,
    8: 25,
    9: 15,
    10: 10
}

# ==========================================
# TÍNH TARGET OCCURRENCE
# ==========================================

total_occurrence = sum(
    k * n
    for k, n in DISTRIBUTION.items()
)

target_per_aspect = total_occurrence // len(ASPECTS)

print("Total occurrence:", total_occurrence)
print("Target per aspect:", target_per_aspect)

# ==========================================
# TARGET SENTIMENT
# 40 / 30 / 30
# ==========================================

target_sentiment = {}

for asp in ASPECTS:

    positive = int(target_per_aspect * 0.40)
    neutral = int(target_per_aspect * 0.30)

    negative = (
        target_per_aspect
        - positive
        - neutral
    )

    target_sentiment[asp] = {
        1: positive,
        0: neutral,
        -1: negative
    }

# ==========================================
# COUNTERS
# ==========================================

aspect_counter = defaultdict(int)

sentiment_counter = {
    asp: defaultdict(int)
    for asp in ASPECTS
}

dataset = []

# ==========================================
# CHỌN ASPECT CÒN THIẾU NHIỀU NHẤT
# ==========================================

def choose_aspects(k):

    remaining = []

    for asp in ASPECTS:

        remain = (
            target_per_aspect
            - aspect_counter[asp]
        )

        remaining.append(
            (asp, max(remain, 0))
        )

    remaining.sort(
        key=lambda x: x[1],
        reverse=True
    )

    candidates = []

    for asp, score in remaining:

        if score > 0:
            candidates.append(asp)

    if len(candidates) < k:
        candidates = ASPECTS.copy()

    return random.sample(candidates, k)

# ==========================================
# CHỌN SENTIMENT CÒN THIẾU
# ==========================================

def choose_sentiment(aspect):

    remain = {}

    for s in [-1, 0, 1]:

        remain[s] = (
            target_sentiment[aspect][s]
            - sentiment_counter[aspect][s]
        )

    remain = {
        k: max(v, 0)
        for k, v in remain.items()
    }

    total = sum(remain.values())

    if total == 0:
        return random.choice([-1, 0, 1])

    sentiments = list(remain.keys())
    weights = list(remain.values())

    return random.choices(
        sentiments,
        weights=weights,
        k=1
    )[0]

# ==========================================
# GENERATE
# ==========================================

for k, count in DISTRIBUTION.items():

    for _ in range(count):

        row = {
            asp: -99
            for asp in ASPECTS
        }

        row["Num_Aspects"] = k

        if k > 0:

            selected = choose_aspects(k)

            for asp in selected:

                sentiment = choose_sentiment(
                    asp
                )

                row[asp] = sentiment

                aspect_counter[asp] += 1

                sentiment_counter[asp][sentiment] += 1

        dataset.append(row)

# ==========================================
# SAVE
# ==========================================

df = pd.DataFrame(dataset)

columns = (
    ["Num_Aspects"]
    + ASPECTS
)

df = df[columns]

df.to_csv(
    "ABSA_Metadata.csv",
    index=False
)

print("Saved.")

# ==========================================
# REPORT
# ==========================================

print("\n=== Aspect Count ===")

for asp in ASPECTS:

    print(
        asp,
        aspect_counter[asp]
    )

print("\n=== Sentiment Count ===")

for asp in ASPECTS:

    print(
        f"\n{asp}"
    )

    print(
        "Negative:",
        sentiment_counter[asp][-1]
    )

    print(
        "Neutral:",
        sentiment_counter[asp][0]
    )

    print(
        "Positive:",
        sentiment_counter[asp][1]
    )
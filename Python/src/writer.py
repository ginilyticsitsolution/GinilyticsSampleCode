import csv
import os

def save_to_csv(data, filepath="data/services.csv"):
    os.makedirs(os.path.dirname(filepath), exist_ok=True)
    with open(filepath, mode="w", newline='', encoding='utf-8') as f:
        writer = csv.DictWriter(f, fieldnames=["title", "description"])
        writer.writeheader()
        writer.writerows(data)
    print(f"✅ Data saved to {filepath}")

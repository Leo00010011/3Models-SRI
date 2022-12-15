import json
import ir_datasets
import ir_measures
from ir_measures import *

# cranfield
# vaswani

qrels = ir_datasets.load('vaswani').qrels_iter()
run = json.load(open(".\\qrels_save"))

results = ir_measures.calc_aggregate(
    [
        P(rel=1, judged_only=True)@30,
        R(rel=1, judged_only=True)@30,
        SetF(rel=1, judged_only=True)
    ],
    qrels, run)

r_value = 0
p_value = 0
beta = 1
for key, value in results.items():
    r_value = r_value if key.NAME != 'R' else value
    p_value = p_value if key.NAME != 'P' else value
    print(f'{key}: {value}')

print(f"F:{(1 + beta**2) * p_value * r_value / (beta**2 * p_value + r_value)}")
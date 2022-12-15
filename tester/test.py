import ir_datasets
import pytrec_eval
import json
import math

from pytrec_eval_ext import supported_nicknames

# data_set = ir_datasets.load(input())

# evaluator = pytrec_eval.RelevanceEvaluator(
#     data_set.qrels_dict(), {'map', 'ndcg'})

# file = open(".\\qrels_save")

# max_map = -math.inf
# max_ndcg = -math.inf
# min_map = math.inf
# min_ndcg = math.inf
# m_map = 0
# m_ndcg = 0
# for i in evaluator.evaluate(json.loads(file.readline())):
#     m_map += i.map
#     m_ndcg += i.ndcg

#     min_map = min(min_map, i.map)
#     min_map = min(min_map, i.map)

# file.close()

print(supported_nicknames.item())
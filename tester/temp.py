import json
import ir_datasets
import ir_measures
from ir_measures import *

# cranfield
# vaswani

qrels = ir_datasets.load('cranfield').qrels_iter()
run = json.load(open(".\\qrels_save"))

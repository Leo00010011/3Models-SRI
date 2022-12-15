import ir_datasets
import json

data_set = ir_datasets.load(input())

docs_data_save = {}
for i in data_set.docs_iter():
    docs_data_save[i.doc_id] = {
        "doc_id": i.doc_id,
        "title": i.title,
        "text": i.text,
    }

queries_data_save = {}
for i in data_set.queries_iter():
    queries_data_save[i.query_id] = {
        "query_id": i.query_id,
        "text": i.text
    }

docs_file = open(".\\docs_save", 'w')
docs_file.write(json.dumps(docs_data_save))
docs_file.close()

queries_file = open(".\\queries_save", 'w')
queries_file.write(json.dumps(queries_data_save))
queries_file.close()
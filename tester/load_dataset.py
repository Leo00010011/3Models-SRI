import ir_datasets
import json

# cranfield
# vaswani

data_set = ir_datasets.load('vaswani')

docs_data_save = {}
for i in data_set.docs_iter():
    docs_data_save[i.doc_id] = {
        "doc_id": i.doc_id,
        "title": i.title if i._asdict().get("title") else i.doc_id,
        "text": i.text,
    }

count = 1
queries_data_save = {}
for i in data_set.queries_iter():
    queries_data_save[f'{count}'] = {
        "query_id": f'{count}',
        "text": i.text
    }
    count += 1

count = 0
qrels_data_save = {}
for i in data_set.qrels_iter():
    qrels_data_save[f'{count}'] = {
        'query_id': i.query_id,
        'doc_id': i.doc_id,
        'relevance': i.relevance,
        'iteration': i.iteration
    }
    count += 1

docs_file = open(".\\docs_save", 'w')
docs_file.write(json.dumps(docs_data_save))
docs_file.close()

queries_file = open(".\\queries_save", 'w')
queries_file.write(json.dumps(queries_data_save))
queries_file.close()

qrels_file = open(".\\qrels_save", 'w')
qrels_file.write(json.dumps(qrels_data_save))
qrels_file.close()

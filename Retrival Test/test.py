import ir_datasets



data_set = ir_datasets.load('vaswani')

for doc in data_set.docs_iter():
    print(doc.doc_id)
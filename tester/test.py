import json
import ir_datasets
import ir_measures
from ir_measures import *

# cranfield
# vaswani

qrels = ir_datasets.load('cranfield').qrels_iter()
run = json.load(open(".\\qrels_save"))

# indices por donde cortar después de ordenar
test_cutoff = [20, 30, 50]
# relación a tener en cuenta para el corte
test_rel = [1, 2, 3]

# medidas a realizar
results = ir_measures.calc_aggregate(
    [
        # la P es precisión y la R recobrado

        # grupo de corte por 20
        P(rel=test_rel[0])@test_cutoff[0],
        P(rel=test_rel[1])@test_cutoff[0],
        P(rel=test_rel[2])@test_cutoff[0],
        R(rel=test_rel[0])@test_cutoff[0],
        R(rel=test_rel[1])@test_cutoff[0],
        R(rel=test_rel[2])@test_cutoff[0],

        # grupo de corte por 30
        P(rel=test_rel[0])@test_cutoff[1],
        P(rel=test_rel[1])@test_cutoff[1],
        P(rel=test_rel[2])@test_cutoff[1],
        R(rel=test_rel[0])@test_cutoff[1],
        R(rel=test_rel[1])@test_cutoff[1],
        R(rel=test_rel[2])@test_cutoff[1],

        # grupo de corte por 50
        P(rel=test_rel[0])@test_cutoff[2],
        P(rel=test_rel[1])@test_cutoff[2],
        P(rel=test_rel[2])@test_cutoff[2],
        R(rel=test_rel[0])@test_cutoff[2],
        R(rel=test_rel[1])@test_cutoff[2],
        R(rel=test_rel[2])@test_cutoff[2],
    ],
    # qrels a comparar y run es el resultado del modelo
    qrels, run)

# conjunto de medidas hechas
metric_set = []

results_file = open(".\\results.txt", 'a')
results_file.write('\ncranfield results:\n')
for key, value in results.items():
    # guarda el valor, la medida y todos sus parámetros
    metric_result = {'value': value, 'metric': key.NAME}
    metric_result.update(key.params.items())
    metric_set.append(metric_result)

# ordena las medidas por métrica, rel y corte para hayar la medida F
metric_set.sort(key=lambda x: x['metric'])
metric_set.sort(key=lambda x: x['rel'])
metric_set.sort(key=lambda x: x['cutoff'])

beta = 1

# cálculo de la medida F por cada rel en un corte dado
for i in range(0, len(metric_set), 2):
    results_file.write(f"{metric_set[i]['metric']}(rel={metric_set[i]['rel']})@{metric_set[i]['cutoff']}: {metric_set[i]['value']}\n")
    results_file.write(f"{metric_set[i + 1]['metric']}(rel={metric_set[i + 1]['rel']})@{metric_set[i + 1]['cutoff']}: {metric_set[i + 1]['value']}\n")

    F = (1 + beta**2) * metric_set[i]['value'] * metric_set[i + 1]['value'] / (beta**2 * metric_set[i]['value'] + metric_set[i + 1]['value'])
    results_file.write(f"F(rel={metric_set[i]['rel']})@{metric_set[i]['cutoff']}: {F}\n\n")

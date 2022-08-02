import random
import sys
from itertools import combinations, groupby

import networkx as nx


def main(number_of_routers, probability_of_link):

    graph = gnp_random_connected_graph(number_of_routers, probability_of_link)
    positions = nx.kamada_kawai_layout(graph)

    print(len(positions.values()))
    sys.stdout.flush()
    for pos in positions:
        print(str((positions[pos][0] + 1) / 2) + "," + str((positions[pos][1] + 1) / 2))
        sys.stdout.flush()

    for edge in graph.edges:
        print(str(edge[0]) + "-" + str(edge[1]))
        sys.stdout.flush()

    exit(0)


def gnp_random_connected_graph(num_nodes, prob_edge):
    edges = combinations(range(num_nodes), 2)
    g = nx.Graph()
    g.add_nodes_from(range(num_nodes))
    if prob_edge <= 0:
        return g
    if prob_edge >= 1:
        return nx.complete_graph(num_nodes, create_using=g)
    for _, node_edges in groupby(edges, key=lambda x: x[0]):
        node_edges = list(node_edges)
        random_edge = random.choice(node_edges)
        g.add_edge(*random_edge)
        for e in node_edges:
            if random.random() < prob_edge:
                g.add_edge(*e)
    return g


if __name__ == "__main__":
    n = int(sys.argv[1])
    p = float(sys.argv[2].replace(',', '.'))
    main(n, p)

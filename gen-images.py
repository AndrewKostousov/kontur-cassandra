import re
import os
import sys
import json
from os import path
from datetime import timedelta
from datetime import datetime

import numpy as np
import matplotlib.pylab as plt


# BENCHMARKS_WORKING_DIR = path.join('TimeSeries', 'Benchmarks', 'bin', 'Debug')

if len(sys.argv) < 2:
    print("Error: expected path to Benchmarks working dir as first parameter")
    print("Usage: gen-images.py [PATH]")
    exit(1)

BENCHMARKS_WORKING_DIR = sys.argv[1]

DATA_DIR = path.join(BENCHMARKS_WORKING_DIR, 'Benchmarks', 'Raw data')
RESULTS_DIR = path.join(BENCHMARKS_WORKING_DIR, 'Benchmarks', 'Results', 'Images')


# see: http://stackoverflow.com/questions/3232701/using-json-to-serialize-deserialize-timespan
TIMESPAN_PATTERN = re.compile(
r"""
    ^[-]?                             # indicates negative timespan
    P                                 # must be the first characted
    (D(?P<days>\d+))?                 # optional days part starting with T, integer
    (T                                # optional time part starting with T
        ((?P<hours>\d+)H)?            # optional hours, integer format
        ((?P<minutes>\d+)M)?          # optional minutes, integer format
        ((?P<seconds>\d+(\.\d+)?)S)?  # optional seconds, floating point number
    )?$
""", re.X)


# sample string, that should match this pattern: "/Date(123)/"
DATETIME_OFFSET_PATTERN = re.compile(
r"""
    /Date\(
        (?P<ticks>\d+)
    \)/
""", re.X)


TIMESPAN_TYPES = {
    'days': int,
    'hours': int,
    'minutes': int,
    'seconds': float,
}


def create_figure(xlabel, ylabel):
    figure, ax = plt.subplots(1, 1)

    figure.set_figheight(8)
    figure.set_figwidth(15)

    ax.set_xlabel(xlabel)
    ax.set_ylabel(ylabel)

    return figure, ax


def normalize(time_sequence):
    t0 = time_sequence[0]
    return [t - t0 for t in time_sequence]


class Payload(object):
    def __init__(self, data):
        assert isinstance(data, dict)
        self.__dict__ = {k: self.build_object(v) for k, v in data.items()}

    def build_object(self, data):
        if isinstance(data, dict):
            return Payload(data)

        if isinstance(data, list):
            return [self.build_object(x) for x in data]

        if isinstance(data, str):
            return self.parse_time(data)

        return data

    def parse_time(self, data):
        timespan_match = TIMESPAN_PATTERN.match(data)
        if timespan_match:
            groupdict = timespan_match.groupdict()
            kwargs = {k: TIMESPAN_TYPES[k](v) for k, v in timespan_match.groupdict().items() if v}
            return timedelta(**kwargs).total_seconds() * 1000
        datetimeoffset_match = DATETIME_OFFSET_PATTERN.match(data)
        if datetimeoffset_match:
            ticks = int(datetimeoffset_match.groupdict()['ticks'])
            return timedelta(microseconds=ticks).total_seconds() * 1000
        return data

    def __str__(self):
        return "{\n\t" + "\n\t".join("{} {};".format(type(v).__name__, k) for k, v in self.__dict__.items()) + "\n}"

    def describe(self):
        print(str(self))

    def __repr__(self):
        return "Payload\n" + str(self)


def load_json(path):
    with open(path, 'r') as f:
        return Payload(json.load(f))


def load_fixture(fixture):
    fixture_dir = os.path.join(DATA_DIR, fixture)

    get_filename = lambda x: os.path.splitext(x)[0]
    load_data = lambda x: load_json(os.path.join(fixture_dir, x))

    return {
        get_filename(benchmark): load_data(benchmark)
        for benchmark in os.listdir(fixture_dir)
    }


def load_fixtures(fixtures):
    fixtures = {
        fixture: load_fixture(fixture) for fixture in fixtures
    }

    benchmarks = {}

    for fixture in fixtures.keys():
        for benchmark, data in fixtures[fixture].items():
            if benchmark in benchmarks:
                benchmarks[benchmark][fixture] = data
            else:
                benchmarks[benchmark] = {fixture: data}

    return benchmarks


def load_benchmarks():
    fixtures = os.listdir(DATA_DIR)

    print("Found fixtures:\n" if fixtures else "No fixtures were found")

    for index, fixture in enumerate(fixtures):
        print("{}). {}".format(index, fixture))

    print()

    benchmarks = load_fixtures(fixtures)
    print("Load OK")

    return benchmarks


def print_benchmarks(benchmarks):
    print("\nBenchmarks:\n")

    for benchmark in benchmarks:
        print(benchmark)

    print("\nFixtures:\n")

    benchmark = next(iter(benchmarks))

    for fixture in benchmarks[benchmark]:
        print(fixture)

    print()


def create_writers_latency_images(benchmarks):
    latency_results_dir = path.join(RESULTS_DIR, 'Writers Latency')

    for benchmark_name, benchmark in benchmarks.items():
        figure, ax = create_figure('Time', 'Writers Latency')
        benchmark_results_dir = path.join(latency_results_dir, benchmark_name)

        if not os.path.isdir(benchmark_results_dir):
            os.makedirs(benchmark_results_dir)

        for index, (fixture_name, data) in enumerate(benchmark.items()):

            all_workers_latency = []

            for worker_index, worker in enumerate(data.Writers.Measurements):
                time = normalize([x.Start.DateTime for x in worker])
                worker_latency = [x.Latency for x in worker]

                all_workers_latency += worker_latency

                label = fixture_name if worker_index == 0 else ''
                ax.plot(time, worker_latency, 'C{}'.format(index), label=label)

            fixture_figure, fixture_ax = create_figure('Writers Latency', 'Operations Count')

            fixture_ax.hist(all_workers_latency, 50, color='C{}'.format(index))
            fixture_ax.legend([fixture_name])

            fixture_figure.savefig(path.join(benchmark_results_dir, fixture_name + '.png'))

        ax.legend()
        figure.savefig(path.join(benchmark_results_dir, 'Writers Latency.png'))


def create_readers_latency_images(benchmarks):
    latency_results_dir = path.join(RESULTS_DIR, 'Readers Latency')

    for benchmark_name, benchmark in benchmarks.items():

        data = next(iter(benchmark.values())).Readers.Measurements
        if len(data) == 0:
            continue

        figure, ax = create_figure('Time', 'Readers Latency')
        benchmark_results_dir = path.join(latency_results_dir, benchmark_name)

        if not os.path.isdir(benchmark_results_dir):
            os.makedirs(benchmark_results_dir)

        for index, (fixture_name, data) in enumerate(benchmark.items()):

            all_workers_latency = []

            for worker_index, worker in enumerate(data.Readers.Measurements):
                time = normalize([x.Start.DateTime for x in worker])
                worker_latency = [x.Latency for x in worker]

                all_workers_latency += worker_latency

                label = fixture_name if worker_index == 0 else ''
                ax.plot(time, worker_latency, 'C{}'.format(index), label=label)

            fixture_figure, fixture_ax = create_figure('Readers Latency', 'Operations Count')

            fixture_ax.hist(all_workers_latency, 50, color='C{}'.format(index))
            fixture_ax.legend([fixture_name])

            fixture_figure.savefig(path.join(benchmark_results_dir, fixture_name + '.png'))

        ax.legend()
        figure.savefig(path.join(benchmark_results_dir, 'Readers Latency.png'))


def create_writers_throughput_images(benchmarks):
    for benchmark_name, benchmark in benchmarks.items():

        figure, ax = create_figure('Time', 'Writers Throughput')

        for index, (fixture_name, fixture) in enumerate(benchmark.items()):

            for worker_index, worker in enumerate(fixture.Writers.Measurements):

                time = []
                throughput = []

                start = worker[0].Start.DateTime
                global_start = start
                step = 0.05
                counter = 0

                for measurement in worker:
                    if measurement.Start.DateTime > start + step:
                        time.append(start - global_start)
                        throughput.append(counter)

                        start = measurement.Start.DateTime
                        counter = 0

                    counter += measurement.Throughput

                label = fixture_name if worker_index == 0 else ""
                ax.plot(time, throughput, "C{}".format(index), label=label)

        ax.legend()
        benchmark_results_dir = path.join(RESULTS_DIR, 'Writers Throughput')

        if not path.isdir(benchmark_results_dir):
            os.makedirs(benchmark_results_dir)

        figure.savefig(path.join(benchmark_results_dir, benchmark_name + ".png"))


def create_end_to_end_latency_images(benchmarks):
    latency_results_dir = path.join(RESULTS_DIR, 'End To End Latency')

    for benchmark_name, benchmark in benchmarks.items():

        data = next(iter(benchmark.values())).Readers.WriteToReadLatency
        if len(data) == 0:
            continue

        figure, ax = create_figure('Operations', 'End To End Latency')

        benchmark_results_dir = path.join(latency_results_dir, benchmark_name)

        if not os.path.isdir(benchmark_results_dir):
            os.makedirs(benchmark_results_dir)

        for index, (fixture_name, fixture) in enumerate(benchmark.items()):

            latency = fixture.Readers.WriteToReadLatency[0]

            figure, axs = plt.subplots(2, 1)

            figure.set_figheight(10)
            figure.set_figwidth(15)

            axs[0].plot(latency, color="C{}".format(index))
            axs[0].legend([fixture_name])
            axs[0].set_xlabel("Operation")
            axs[0].set_ylabel("End to End Latency")

            axs[1].hist(latency, 50, color="C{}".format(index))
            axs[1].legend([fixture_name])
            axs[1].set_xlabel("End to End Latency")
            axs[1].set_ylabel("Operations Count")

            figure.savefig(path.join(benchmark_results_dir, fixture_name + ".png"))


def create_writers_count_relations_images(benchmarks):
    sorted_benchmarks = [(name, data) for name, data in benchmarks.items()]
    sorted_benchmarks = [b for b in sorted_benchmarks if next(iter(b[1].values())).Readers.WorkersCount == 1]
    sorted_benchmarks = sorted(sorted_benchmarks, key=lambda x: next(iter(x[1].values())).Writers.WorkersCount)

    lfigure, lax = create_figure("Writers Count", "Average Writers Latency")
    rfigure, rax = create_figure("Writers Count", "Average Readers Latency")
    tfigure, tax = create_figure("Writers Count", "Average Writers Throughput")
    efigure, eax = create_figure("Writers Count", "Average End To End Latency")

    writers_count = dict()
    average_latency = dict()
    average_rlatency = dict()
    average_throughput = dict()
    average_ete = dict()

    for benchmark_name, benchmark in sorted_benchmarks:
        for index, (fixture_name, fixture) in enumerate(benchmark.items()):
            if fixture_name in writers_count:
                writers_count[fixture_name].append(fixture.Writers.WorkersCount)
                average_latency[fixture_name].append(fixture.Writers.AverageLatency)
                average_throughput[fixture_name].append(fixture.Writers.AverageThroughput)
                average_ete[fixture_name].append(fixture.Readers.AverageEndToEndLatency)
                average_rlatency[fixture_name].append(fixture.Readers.AverageLatency)
            else:
                writers_count[fixture_name] = [fixture.Writers.WorkersCount]
                average_latency[fixture_name] = [fixture.Writers.AverageLatency]
                average_throughput[fixture_name] = [fixture.Writers.AverageThroughput]
                average_ete[fixture_name] = [fixture.Readers.AverageEndToEndLatency]
                average_rlatency[fixture_name] = [fixture.Readers.AverageLatency]


    for index, fixture_name in enumerate(next(iter(benchmarks.values()))):
        lax.plot(writers_count[fixture_name], average_latency[fixture_name], "C{}".format(index), label=fixture_name)
        tax.plot(writers_count[fixture_name], average_throughput[fixture_name], "C{}".format(index), label=fixture_name)
        eax.plot(writers_count[fixture_name], average_ete[fixture_name], "C{}".format(index), label=fixture_name)
        rax.plot(writers_count[fixture_name], average_rlatency[fixture_name], "C{}".format(index), label=fixture_name)

    lax.legend()
    tax.legend()
    eax.legend()
    rax.legend()

    lfigure.savefig(path.join(RESULTS_DIR, 'Writers Count - Writers Latency.png'))
    tfigure.savefig(path.join(RESULTS_DIR, 'Writers Count - Writers Throughput.png'))
    efigure.savefig(path.join(RESULTS_DIR, 'Writers Count - End To End Latency.png'))
    rfigure.savefig(path.join(RESULTS_DIR, 'Writers Count - Readers Latency.png'))


def create_readers_count_relations_images(benchmarks):
    sorted_benchmarks = [(name, data) for name, data in benchmarks.items()]
    sorted_benchmarks = [b for b in sorted_benchmarks if next(iter(b[1].values())).Writers.WorkersCount == 1]
    sorted_benchmarks = sorted(sorted_benchmarks, key=lambda x: next(iter(x[1].values())).Readers.WorkersCount)

    lfigure, lax = create_figure("Readers Count", "Average Writers Latency")
    rfigure, rax = create_figure("Readers Count", "Average Readers Latency")
    tfigure, tax = create_figure("Readers Count", "Average Writers Throughput")
    efigure, eax = create_figure("Readers Count", "Average End To End Latency")

    readers_count = dict()
    average_latency = dict()
    average_rlatency = dict()
    average_throughput = dict()
    average_ete = dict()

    for benchmark_name, benchmark in sorted_benchmarks:
        for index, (fixture_name, fixture) in enumerate(benchmark.items()):
            if fixture_name in readers_count:
                readers_count[fixture_name].append(fixture.Readers.WorkersCount)
                average_latency[fixture_name].append(fixture.Writers.AverageLatency)
                average_throughput[fixture_name].append(fixture.Writers.AverageThroughput)
                average_ete[fixture_name].append(fixture.Readers.AverageEndToEndLatency)
                average_rlatency[fixture_name].append(fixture.Readers.AverageLatency)
            else:
                readers_count[fixture_name] = [fixture.Readers.WorkersCount]
                average_latency[fixture_name] = [fixture.Writers.AverageLatency]
                average_throughput[fixture_name] = [fixture.Writers.AverageThroughput]
                average_ete[fixture_name] = [fixture.Readers.AverageEndToEndLatency]
                average_rlatency[fixture_name] = [fixture.Readers.AverageLatency]


    for index, fixture_name in enumerate(next(iter(benchmarks.values()))):
        lax.plot(readers_count[fixture_name], average_latency[fixture_name], "C{}".format(index), label=fixture_name)
        tax.plot(readers_count[fixture_name], average_throughput[fixture_name], "C{}".format(index), label=fixture_name)
        eax.plot(readers_count[fixture_name], average_ete[fixture_name], "C{}".format(index), label=fixture_name)
        rax.plot(readers_count[fixture_name], average_rlatency[fixture_name], "C{}".format(index), label=fixture_name)

    lax.legend()
    tax.legend()
    eax.legend()
    rax.legend()

    lfigure.savefig(path.join(RESULTS_DIR, 'Readers Count - Writers Latency.png'))
    tfigure.savefig(path.join(RESULTS_DIR, 'Readers Count - Writers Throughput.png'))
    efigure.savefig(path.join(RESULTS_DIR, 'Readers Count - End To End Latency.png'))
    rfigure.savefig(path.join(RESULTS_DIR, 'Readers Count - Readers Latency.png'))


if __name__ == '__main__':
    benchmarks = load_benchmarks()

    print_benchmarks(benchmarks)

    print("Generating images...")
    create_writers_latency_images(benchmarks)
    create_readers_latency_images(benchmarks)
    create_writers_throughput_images(benchmarks)
    create_end_to_end_latency_images(benchmarks)
    create_writers_count_relations_images(benchmarks)
    create_readers_count_relations_images(benchmarks)
    print("Done!")

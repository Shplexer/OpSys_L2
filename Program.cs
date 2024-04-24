using System.Collections;
using System.Collections.Generic;

namespace OpSys_L2 {
    class Program {
        const string eof = "eof";
        const string bad = "bad";
        const string empty = "empty";
        static void Main() {

            List<(object, object)> A =   [(22, 23), (23, 24), (24, 25), (25, eof)];
            List<(object, object)> B =   [(20, 21), (21, 6), (6, 7), (7, eof)];
            List<(object, object)> C =   [(10, 11), (11, 6), (6, 7), (7, eof)];
            List<(object, object)> D =   [(5, 13), (13, 29), (29, 28), (28, eof)];
            List<(object, object)> FAT = [
                (0, empty),  (1, empty),  (2, empty),    (3, bad),    (4, empty),
                (5, 13),     (6, 7),      (7, eof),      (8, empty),  (9, empty),
                (10, 11),    (11, 6),     (12, empty),   (13, 29),    (14, 15),
                (15, 16),    (16, 17),    (17, eof),     (18, empty), (19, empty),
                (20, 21),    (21, 6),     (22, 23),      (23, 24),    (24, 25),
                (25, eof),   (26, empty), (27, bad),     (28, eof),   (29, 28),
                (30, empty),    (31, empty)
            ];
            var lostClusters = FindLostClusters(FAT, A, B, C, D);
            var emptyClusters = FindEmptyClusters(FAT);
            var badClusters = FindBadClusters(FAT);

            var intersections = CheckIntersections(A, B, C, D);

            var notEndingFiles = CheckEndings(A, B, C, D);

            PrintClusters(lostClusters, "Присутствуют потерянные кластеры:", eof);
            PrintClusters(emptyClusters, "Присутствуют пустые кластеры:");
            PrintClusters(badClusters, "Присутствуют дефектные кластеры:");
            PrintIntersections(intersections);
            PrintNotEndingFiles(notEndingFiles);

            ReplaceIntersectingValues(ref A, ref B, ref C, ref D, intersections, ref emptyClusters);
            var newFiles = CreateNewFilesFromClusters(lostClusters);

            Console.WriteLine("==============================");
            Console.WriteLine("Исправленная файловая система:");
            PrintClusters(A, "A:");
            PrintClusters(B, "B:");
            PrintClusters(C, "C:");
            PrintClusters(D, "D:");
            PrintNewFiles(newFiles);
            PrintClusters(emptyClusters, "Пустые кластеры:");
            PrintClusters(badClusters, "Дефектные кластеры:");
        }
        public static void PrintNewFiles(List<List<(object, object)>> newFiles) {
            for (int i = 0; i < newFiles.Count; i++) {
                var tuples = newFiles[i].Select(t => $"[{t.Item1}] => [{t.Item2}]");
                Console.WriteLine($"Новый файл {i + 1}:\n{string.Join("\n", tuples)}");
            }
        }
        private static void ReplaceIntersectingValues(
            ref List<(object, object)> A,
            ref List<(object, object)> B,
            ref List<(object, object)> C,
            ref List<(object, object)> D,
            List<(object Value, List<string> Lists)> intersections,
            ref List<(object, object)> emptyClusters) {

            // Iterate over the intersections
            foreach (var (Value, Lists) in intersections) {
                // Skip the first list to leave its values unchanged
                for (int i = 1; i < Lists.Count; i++) {
                    var listToChange = GetListByName(Lists[i], ref A, ref B, ref C, ref D);
                    var index = listToChange.FindIndex(x => x.Equals(Value));
                    if (index != -1 && emptyClusters.Count >= 2) {
                        // Take two empty clusters to form the new unique pair
                        var newStart = emptyClusters[0].Item1;
                        var newEnd = emptyClusters[1].Item1;
                        
                        if (index == listToChange.Count - 1) {
                            listToChange[index] = (newStart, eof);
                        }
                        else {
                            listToChange[index] = (newStart, newEnd);
                            listToChange[index - 1] = (listToChange[index - 1].Item1, newEnd);
                        }
                        // Replace intersecting value with the new unique value

                        // Update emptyClusters by removing the used clusters and changing 'empty' to the next cluster
                        emptyClusters.RemoveAt(0);
                        emptyClusters[0] = (emptyClusters[0].Item1, eof);
                    }
                }
            }
        }

        private static List<(object, object)> GetListByName(
            string name,
            ref List<(object, object)> A,
            ref List<(object, object)> B,
            ref List<(object, object)> C,
            ref List<(object, object)> D) {

            switch (name) {
                case "A": return A;
                case "B": return B;
                case "C": return C;
                case "D": return D;
                default: throw new ArgumentException("Invalid list name", nameof(name));
            }
        }

        public static List<List<(object, object)>> CreateNewFilesFromClusters(List<(object, object)> clusters) {
            var newFiles = new List<List<(object, object)>>();
            var remainingClusters = new List<(object, object)>(clusters);

            var startingPoints = clusters.Select(e => e.Item1)
                .Except(clusters.Select(e => e.Item2))
                .ToList();

            foreach (var start in startingPoints) {
                var current = start;
                var sequence = new List<(object, object)>();
                var next = remainingClusters.FirstOrDefault(e => e.Item1.Equals(current));

                while (!next.Equals(default((object, object)))) {
                    sequence.Add(next);
                    remainingClusters.Remove(next);
                    current = next.Item2;
                    next = remainingClusters.FirstOrDefault(e => e.Item1.Equals(current));
                }

                if (sequence.Count != 0) {
                    newFiles.Add(sequence);
                }
            }

            newFiles.AddRange(remainingClusters.Where(e => e.Item2.Equals("eof"))
                .Select(e => new List<(object, object)> { e }));

            return newFiles;
        }
        private static bool ListContainsEof(List<(object, object)> list) {
            return list.Any(item => item.Item2?.ToString() == eof);
        }
        private static List<string> CheckEndings(
            List<(object, object)> A,
            List<(object, object)> B,
            List<(object, object)> C,
            List<(object, object)> D) {
            List<string> notEndingFiles = [];

            if (!ListContainsEof(A)) notEndingFiles.Add("A");
            if (!ListContainsEof(B)) notEndingFiles.Add("B");
            if (!ListContainsEof(C)) notEndingFiles.Add("C");
            if (!ListContainsEof(D)) notEndingFiles.Add("D");

            return notEndingFiles;
        }
        private static void PrintNotEndingFiles(List<string> notEndingFiles) {
            if (notEndingFiles.Count != 0) {
                Console.WriteLine("Незавершенные файлы:");
                foreach (var file in notEndingFiles) {
                    Console.WriteLine(file);
                }
            }
        }
        private static List<(object Value, List<string> Lists)> CheckIntersections(
            List<(object, object)> A,
            List<(object, object)> B,
            List<(object, object)> C,
            List<(object, object)> D) {
            var combined = A.Select(x =>
                (Value: x, ListName: "A"))
                .Concat(B.Select(x => (Value: x, ListName: "B")))
                .Concat(C.Select(x => (Value: x, ListName: "C")))
                .Concat(D.Select(x => (Value: x, ListName: "D")));

            var grouped = combined.GroupBy(x => x.Value).Where(g => g.Count() > 1);

            List<(object Value, List<string> Lists)> commonValues = grouped.Select(
                g => {
                    return ((object)g.Key, g.Select(x => x.ListName).ToList());
                }).ToList();

            return commonValues;
        }
        private static void PrintIntersections(List<(object Value, List<string> Lists)> commonValues) {
            if (commonValues.Count != 0) {
                Console.WriteLine("Найдены пересекающиеся кластеры:");

                foreach (var (Value, Lists) in commonValues) {
                    var (Item1, Item2) = ((object, object))Value;
                    Console.WriteLine($"[{Item1}] => [{Item2}] в файлах: {string.Join(", ", Lists)}");
                }
            }
        }

        private static List<(object, object)> FindLostClusters(
            List<(object, object)> FAT,
            List<(object, object)> A,
            List<(object, object)> B,
            List<(object, object)> C,
            List<(object, object)> D) {

            var lostClusters = FAT.Where(cluster =>
                cluster.Item2.ToString() != empty && cluster.Item2.ToString() != bad &&
                !A.Contains(cluster) && !B.Contains(cluster) && !C.Contains(cluster) && !D.Contains(cluster)
                ).ToList();

            return lostClusters;
        }
        private static List<(object, object)> FindBadClusters(List<(object, object)> FAT) {
            var badClusters = FAT.Where(cluster => cluster.Item2.ToString() == bad).ToList();

            return badClusters;
        }
        private static List<(object, object)> FindEmptyClusters(List<(object, object)> FAT) {
            var emptyClusters = FAT.Where(cluster => cluster.Item2.ToString() == empty).ToList();

            return emptyClusters;
        }

        private static void PrintClusters(List<(object ClusterIndex, object ClusterValue)> clusters, string message, string? endOfFileMarker = null) {
            if (clusters.Count != 0) {
                Console.WriteLine(message);
                int lostFileCount = 0;
                foreach (var (ClusterIndex, ClusterValue) in clusters) {
                    Console.WriteLine($"[{ClusterIndex}] => [{ClusterValue}]");
                    if (ClusterValue.ToString() == eof) {
                        lostFileCount++;
                    }
                }
                if (endOfFileMarker != null) {
                    Console.WriteLine($"Количество потерянных файлов: {lostFileCount}");
                }
            }
        }
    }
}
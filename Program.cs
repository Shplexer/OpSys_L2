using System.Collections;
using System.Collections.Generic;

namespace OpSys_L2 {
    class Program {
        const string eof = "eof";
        const string bad = "bad";
        const string empty = "empty";
        static void Main() {

            List<(object, object)> A = [(22, 23), (23, 24), (24, 25), (25, eof)];
            List<(object, object)> B = [(20, 21), (21, 6), (6, 7), (7, eof)];
            List<(object, object)> C = [(10, 11), (11, 6), (6, 7), (7, eof)];
            List<(object, object)> D = [(5, 13), (13, 29), (29, 28), (28, eof)];
            List<(object, object)> FAT = [
                (0, empty), (1, empty), (2, empty), (3, bad), (4, empty),
                (5, 13), (6, 7), (7, eof), (8, empty), (9, empty),
                (10, 11), (11, 6), (12, empty), (13, 29), (14, 15),
                (15, 16), (16, 17), (17, eof), (18, empty), (19, empty),
                (20, 21), (21, 6), (22, 23), (23, 24), (24, 25),
                (25, eof), (26, empty), (27, bad), (28, eof), (29, 28), (30, empty), (31, empty)
            ];
            CheckIndividualClusters(FAT, A, B, C, D);
            
            FindIntersections(A, B, C, D);

            checkEndings(A, B, C, D);


        }
        private static bool ListContainsEof(List<(object, object)> list) {
            return list.Any(item => item.Item2?.ToString() == eof);
        }
        private static void checkEndings(
            List<(object, object)> A,
            List<(object, object)> B,
            List<(object, object)> C,
            List<(object, object)> D) {
            List<string> notEndingFiles = new();
            
            if (!ListContainsEof(A)) notEndingFiles.Add("A");
            if (!ListContainsEof(B)) notEndingFiles.Add("B");
            if (!ListContainsEof(C)) notEndingFiles.Add("C");
            if (!ListContainsEof(D)) notEndingFiles.Add("D");

            if (notEndingFiles.Count != 0) {
                Console.WriteLine("Не завершенные файлы:");
                foreach (var file in notEndingFiles) {
                    Console.WriteLine(file);
                }
            }

        }
        private static void FindIntersections(
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
            if (commonValues.Count != 0) {
                Console.WriteLine("Найдены пересекающиеся кластеры:");

                foreach (var (Value, Lists) in commonValues) {
                    var (Item1, Item2) = ((object, object))Value;
                    Console.WriteLine($"[{Item1}] -> [{Item2}] в файлах: {string.Join(", ", Lists)}");
                }
            }
        }
        private static void CheckIndividualClusters(
            List<(object, object)> FAT,
            List<(object, object)> A,
            List<(object, object)> B,
            List<(object, object)> C,
            List<(object, object)> D) {

            var lostClusters = FAT.Where(cluster =>
                cluster.Item2.ToString() != empty && cluster.Item2.ToString() != bad &&
                !A.Contains(cluster) && !B.Contains(cluster) && !C.Contains(cluster) && !D.Contains(cluster)
            ).ToList();
            var badClusters = FAT.Where(cluster => cluster.Item2.ToString() == bad).ToList();
            var emptyClusters = FAT.Where(cluster => cluster.Item2.ToString() == empty).ToList();

            PrintErrClusters(lostClusters, "Присутствуют потерянные кластеры:", eof);
            PrintErrClusters(badClusters, "Присутствуют дефектные кластеры:");
            PrintErrClusters(emptyClusters, "Присутствуют пустые кластеры:");
        }
        private static void PrintErrClusters(List<(object ClusterIndex, object ClusterValue)> clusters, string message, string? endOfFileMarker = null) {
            if (clusters.Count != 0) {
                Console.WriteLine(message);
                int lostFileCount = 0;
                foreach (var (ClusterIndex, ClusterValue) in clusters) {
                    Console.WriteLine($"[{ClusterIndex}] -> [{ClusterValue}]");
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
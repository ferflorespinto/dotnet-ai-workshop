using System.Numerics.Tensors;
using Microsoft.Extensions.AI;

namespace Embeddings;

public class ZeroShotClassification
{
    public async Task RunAsync()
    {
        string input = string.Empty;
        IList<string> candidateLabels = [];
        while (true)
        {
            Console.Write("\nEnter candidate labels: ");
            input = Console.ReadLine()!;
            if (input == "") break;
            candidateLabels.Add(input);
        }

        while (true)
        {
            Console.Write("\nQuery: ");
            input = Console.ReadLine()!;
            if (input == "") break;

            // Compute embedding of our console input
            string candidate = await ClassifyAsync(input, candidateLabels);
            Console.WriteLine($"Category: {candidate}\n");
        }
    }

    public async Task<string> ClassifyAsync(string text, IEnumerable<string> candidateLabels)
    {
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
            new OllamaEmbeddingGenerator(new Uri("http://127.0.0.1:11434"), modelId: "all-minilm");

        var labelsWithEmbeddings = await embeddingGenerator.GenerateAndZipAsync(candidateLabels);
        var textEmbedding = await embeddingGenerator.GenerateVectorAsync(text);

        var closest =
                from candidate in labelsWithEmbeddings
                let similarity = TensorPrimitives.CosineSimilarity(
                    candidate.Embedding.Vector.Span, textEmbedding.Span)
                orderby similarity descending
                select new { candidate.Value, Similarity = similarity };

        return closest.First().Value.ToString();
    }
}
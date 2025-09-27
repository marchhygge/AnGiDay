namespace AGD.Repositories.VectorModels;

public partial class SignatureFoodEmbedding
{
    public int SignatureFoodId { get; set; }

    public float[] Embedding { get; set; } = Array.Empty<float>();

    public string ModelName { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }
}
using Npgsql;

namespace AGD.Repositories.Repositories
{
    public class EmbeddingRepository
    {
        private readonly string _connectionString;

        public EmbeddingRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<int>> GetExistingRestaurantIdsAsync(CancellationToken ct = default)
        {
            var ids = new List<int>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT restaurant_id FROM restaurant_embeddings";
            await using var rdr = await command.ExecuteReaderAsync(ct);
            while (await rdr.ReadAsync(ct))
            {
                ids.Add(rdr.GetInt32(0));
            }
            return ids;
        }

        public async Task UpsertRestaurantEmbeddingAsync(int restaurantId, float[] embedding, string modelName, CancellationToken ct = default)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);

            await using var command = connection.CreateCommand();
            
            command.CommandText = @"
INSERT INTO restaurant_embeddings (restaurant_id, embedding, model_name, updated_at)
VALUES (@id, @vec::vector, @model, now())
ON CONFLICT (restaurant_id) DO UPDATE
  SET embedding = EXCLUDED.embedding, model_name = EXCLUDED.model_name, updated_at = now();";

            command.Parameters.AddWithValue("id", NpgsqlTypes.NpgsqlDbType.Integer, restaurantId);

            var pVec = command.CreateParameter();
            pVec.ParameterName = "vec";
            pVec.Value = embedding;
            
            command.Parameters.Add(pVec);

            command.Parameters.AddWithValue("model", NpgsqlTypes.NpgsqlDbType.Text, modelName ?? string.Empty);

            await command.ExecuteNonQueryAsync(ct);
        }

        public async Task<List<int>> TopRestaurantIdsByEmbeddingAsync(float[] queryEmbedding, int k, CancellationToken ct = default)
        {
            var ids = new List<int>();
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(ct);
            await using var command = connection.CreateCommand();

            command.CommandText = "SELECT restaurant_id FROM restaurant_embeddings ORDER BY embedding <=> @q::vector LIMIT @k";
            var p = command.CreateParameter();
            p.ParameterName = "q";
            p.Value = queryEmbedding;
            command.Parameters.Add(p);
            command.Parameters.AddWithValue("k", NpgsqlTypes.NpgsqlDbType.Integer, k);

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                ids.Add(reader.GetInt32(0));
            }

            return ids;
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AGD.Repositories.Models;

namespace AGD.Repositories.DBContext.Configurations
{
    public class ConversationConfig : IEntityTypeConfiguration<Conversation>
    {
        public void Configure(EntityTypeBuilder<Conversation> b)
        {
            b.ToTable("conversations");
            b.HasKey(x => x.Id);

            b.Property(x => x.StartedAt).HasColumnName("started_at");
            b.Property(x => x.EndedAt).HasColumnName("ended_at");
            b.Property(x => x.IsDeleted).HasColumnName("is_deleted");

            b.Property(x => x.Title).HasColumnName("title").HasColumnType("text");
            b.Property(x => x.Summary).HasColumnName("summary").HasColumnType("text");
            b.Property(x => x.SummaryTokenCount).HasColumnName("summary_token_count");
        }
    }

    public class MessageConfig : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> b)
        {
            b.ToTable("messages");
            b.HasKey(x => x.Id);

            b.Property(x => x.ConversationId).HasColumnName("conversation_id");
            b.Property(x => x.Sender).HasColumnName("sender").HasMaxLength(10);
            b.Property(x => x.Content).HasColumnName("content").HasColumnType("text");
            b.Property(x => x.CreatedAt).HasColumnName("created_at");
            b.Property(x => x.IsDeleted).HasColumnName("is_deleted");

            b.Property(x => x.ModelName).HasColumnName("model_name").HasColumnType("text");
            b.Property(x => x.TokensIn).HasColumnName("tokens_in");
            b.Property(x => x.TokensOut).HasColumnName("tokens_out");
            b.Property(x => x.Meta).HasColumnName("meta").HasColumnType("jsonb");
        }
    }
}
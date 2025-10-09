FROM redis:7-alpine

# Persist data under /data (mounted by fly volume).
# Redis password is provided via REDIS_PASSWORD secret at runtime.
CMD ["sh", "-c", "redis-server --save 60 1 --appendonly yes --dir /data --requirepass \"$REDIS_PASSWORD\""]
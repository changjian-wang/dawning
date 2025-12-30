# =============================================
# Dawning Admin - Multi-stage Dockerfile
# =============================================

# ===== Build Stage =====
FROM node:20-alpine AS build
WORKDIR /app

# Install pnpm
RUN npm install -g pnpm

# Copy package files first (better layer caching)
COPY apps/admin/package.json apps/admin/pnpm-lock.yaml ./

# Install dependencies
RUN pnpm install --frozen-lockfile

# Copy source code (exclude node_modules by listing specific directories)
COPY apps/admin/src/ ./src/
COPY apps/admin/config/ ./config/
COPY apps/admin/index.html apps/admin/tsconfig.json apps/admin/components.d.ts ./
COPY apps/admin/.env.production ./

# Build the application
RUN pnpm build

# ===== Production Stage =====
FROM nginx:alpine AS production
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy built assets from build stage
COPY --from=build /app/dist .

# Copy nginx configuration
COPY apps/admin/nginx.conf /etc/nginx/conf.d/default.conf

# Create non-root user for security
RUN addgroup -g 1001 -S dawning && \
    adduser -S dawning -G dawning -u 1001 && \
    chown -R dawning:dawning /var/cache/nginx && \
    chown -R dawning:dawning /var/log/nginx && \
    chown -R dawning:dawning /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown -R dawning:dawning /var/run/nginx.pid

# Switch to non-root user
USER dawning

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD wget -q --spider http://localhost:80 || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]

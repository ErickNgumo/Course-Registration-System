# syntax=docker/dockerfile:1

# ───────────────────── build stage ─────────────────────
FROM node:20-alpine AS build
WORKDIR /app

# Install dependencies first for better layer caching.
COPY package.json package-lock.json* ./
RUN npm ci --no-audit --no-fund || npm install --no-audit --no-fund

# Build the static bundle. Vite bundles `VITE_API_BASE_URL` at build time;
# the default "/api/v1" makes the nginx reverse proxy forward to the API.
COPY . .
RUN npm run build

# ───────────────────── runtime stage ──────────────────
FROM nginx:1.27-alpine AS final

# SPA-aware nginx config (history fallback + reverse proxy for /api).
RUN rm /etc/nginx/conf.d/default.conf
COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
HEALTHCHECK --interval=15s --timeout=3s --retries=5 \
  CMD wget -qO- http://127.0.0.1/ >/dev/null || exit 1

CMD ["nginx", "-g", "daemon off;"]

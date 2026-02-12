# Build stage
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm install

COPY . .
# Vite reads env vars at build time, so make API URL configurable via build args
ARG VITE_API_URL=http://localhost:5555
ENV VITE_API_URL=${VITE_API_URL}
ARG VITE_LLM_GATEWAY_URL=http://localhost:8090
ENV VITE_LLM_GATEWAY_URL=${VITE_LLM_GATEWAY_URL}
RUN npm run build

# Serve stage
FROM nginx:1.27-alpine AS serve
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 4173

CMD ["nginx", "-g", "daemon off;"]

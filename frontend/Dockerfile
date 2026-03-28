FROM node:20-alpine AS build
WORKDIR /app

# 依存関係のインストール
COPY package.json package-lock.json ./
RUN npm ci

# ソースコードをコピーしビルド
COPY . .
ENV NEXT_PUBLIC_API_BASE_URL=http://localhost:5150
RUN npm run build

# 実行用イメージ
FROM node:20-alpine
WORKDIR /app

COPY --from=build /app/.next/standalone ./
COPY --from=build /app/.next/static ./.next/static
COPY --from=build /app/public ./public

EXPOSE 3000
ENV PORT=3000
ENV HOSTNAME=0.0.0.0

CMD ["node", "server.js"]

# Dev image: Docker Compose bind-mounts the source over /app and keeps node_modules in a volume.
FROM node:24-alpine

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

EXPOSE 4200

# The dev proxy (/api, /hubs, /audio, /images -> proxy.conf.mjs) is configured in angular.json.
CMD ["npx", "ng", "serve", "--host", "0.0.0.0"]

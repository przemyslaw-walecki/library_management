# Use a Node.js image for building the React app
FROM node:20 AS build
WORKDIR /app

# Install dependencies and build the app
COPY package.json package-lock.json ./
RUN npm install
COPY . ./
RUN npm run build

# Use Nginx to serve the static files
FROM nginx:alpine

# Copy the built React app to the Nginx directory
COPY --from=build /app/build /usr/share/nginx/html

# Copy the custom nginx.conf file
COPY nginx.conf /etc/nginx/nginx.conf

# Expose port 80 (the port for Nginx)
EXPOSE 80

# Default command to run Nginx
CMD ["nginx", "-g", "daemon off;"]

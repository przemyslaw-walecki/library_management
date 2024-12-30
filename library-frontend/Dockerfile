# Use a Node.js image for building the React app
FROM node:20 AS build
WORKDIR /app

# Install dependencies and build the app
COPY package.json package-lock.json ./ 
RUN npm install
COPY . ./ 
RUN npm run build

# Use a minimal Node.js image to serve the app
FROM node:20-alpine

# Install serve globally
RUN npm install -g serve

# Set the working directory to the build folder
WORKDIR /app

# Copy the build folder from the build stage
COPY --from=build /app/build /app/build

# Expose port 5000 (or any other port you prefer)
EXPOSE 3000

# Serve the React app using 'serve'
CMD ["serve", "-s", "build", "-l", "3000"]

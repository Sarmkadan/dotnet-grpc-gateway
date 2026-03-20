## Quick Start with Docker

1. Install Docker on your machine.
2. Pull the dotnet-grpc-gateway image from Docker Hub.
3. Run the container using the docker run command.

## Docker Compose Usage

1. Install Docker Compose on your machine.
2. Create a docker-compose.yml file in the root of your project.
3. Run the containers using the docker-compose up command.

## Environment Variables Reference

* DOTNET_GRPC_GATEWAY_PORT: The port number to use for the gateway.
* DOTNET_GRPC_GATEWAY_HOST: The host IP address to use for the gateway.

## Production Deployment Checklist

1. Configure the production environment variables.
2. Build the Docker image for production.
3. Deploy the container to a cloud platform or a container orchestration tool.

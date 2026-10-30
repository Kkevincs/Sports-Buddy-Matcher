# Sports-Buddy-Matcher

## Deploy with Dokploy

Create a Docker Compose service from this repository on `main`, using `./docker-compose.yml`. Set the variables listed in `dokploy.env.example`, then add an HTTPS domain for service `app` on container port `10000`.

The stack builds the .NET 8 API/frontend, starts SQL Server 2022 Express, and initializes an empty database from `deploy/schema.sql`. Database files and uploaded photos use persistent volumes. Existing local accounts/events/photos are not copied automatically.

See [deployment instructions](docs/dokploy-deployment.md) for configuration, database transfer, and backups. See [matching rules](docs/matching.md) and the [database diagram](docs/database.dbml).

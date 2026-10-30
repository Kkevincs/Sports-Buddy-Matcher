# Dokploy deployment

Use a **Docker Compose** service with this Git repository, branch `main`, repository root as the working directory, and Compose Path `./docker-compose.yml`. Use regular Docker Compose, not Docker Stack/Swarm.

The stack contains the app/frontend, SQL Server 2022 Express, and a one-off schema initializer. The initializer image includes its SQL script, so it does not rely on a repository bind mount that Dokploy can clear during redeployment. It does not require a separately hosted database. The SQL Server image needs an x86-64 server and at least 2 GB memory for SQL Server; allow additional memory for Dokploy and application builds.

## Environment

Copy the variable names from `dokploy.env.example` into Dokploy's Environment tab:

- `ACCEPT_EULA`: review Microsoft's SQL Server license terms, then set `Y` if accepted.
- `MSSQL_SA_PASSWORD`: strong SQL Server password, at least 16 characters with upper/lowercase, digits, and symbols. Do not include double quotes or newlines. It is used by both SQL Server and the app. Keep it stable after database initialization; changing only the environment value does not rotate an existing SQL Server password.
- `JWT_SECRET`: a separate random secret of at least 32 bytes. Keep it stable between deployments.
- `GEOAPIFY_API_KEY`: your existing API key.

No actual credentials are committed. The Compose configuration uses SQL authentication instead of your laptop's Windows authentication. This simple stack uses the SQL `sa` login; use a restricted application login when hardening the deployment.

## Domain

In Dokploy's **Domains** tab, add a domain for service **app**, container port **10000**, path `/`, and enable HTTPS. Dokploy supplies proxy routing. The app and database share the explicit `backend` network; the database has no published host port. Verify Dokploy's Preview Compose retains `backend` on all three services when adding its proxy network.

Production HTTPS is terminated by the reverse proxy. `/health` checks app startup, not database connectivity. Swagger is available only in Development.

## Database and files

On first deployment, the initializer creates `sportsbuddymatcher` and applies `deploy/schema.sql` to an empty database. It skips databases with existing application tables. This initializes the schema only: local accounts, events, and other data are not copied automatically.

To keep your current data, restore a backup/export of the local database into this SQL Server instance. For native SQL backups, ensure the target SQL Server version is at least the source version; otherwise choose a compatible migration/export method. Restore using internal container tooling or an SSH tunnel, rather than publishing SQL Server to the internet. Back up the remote database before replacing anything. Subsequent deployments skip schema initialization; manage later migrations explicitly.

Named volumes preserve SQL data (`sql-data`) and uploaded profile/event photos (`uploaded-photos`). Keep the same Dokploy project/Compose identity so redeployments reuse the volumes. Back up both; do not remove the volumes when redeploying. Existing laptop uploads must be transferred separately. Bundled sports images are already in the app image.

The app runs as UID 1654. Docker named volumes inherit the image upload directory permissions on creation. If Dokploy substitutes a host bind mount, make that directory writable by UID 1654.

## Checks

After deploying, check `/health`, open `/`, register/log in, and exercise matching and photo uploads. A fresh database has no sports catalog or sample users until you import or add them.

Generate a new schema script when migrations change:

```powershell
dotnet ef migrations script --idempotent --project SportsBuddyMatcher/SportsBuddyMatcher.Repository --startup-project SportsBuddyMatcher/SportsBuddyMatcher --output deploy/schema.sql
```

References:

- https://docs.dokploy.com/docs/core/docker-compose/domains
- https://learn.microsoft.com/sql/linux/install-upgrade/quickstart-install-docker

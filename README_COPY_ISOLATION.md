## QLT copy isolation

This copy is configured to run separately from the original project on local ports:

- Frontend: `http://localhost:5174`
- Backend HTTP: `http://localhost:5056`
- Backend HTTPS: `https://localhost:7144`

Cloudinary uploads from this copy are stored in:

- `boarding-house-images-qlt-copy`

Important:

- The database connection in `Backend_Boarding_house_management_system/Backend_Boarding_house_management_system/appsettings.Development.json` now points to a separate PostgreSQL instance for this copy.
- Code changes in this copy do not change the original project's files.
- Runtime data changes in this copy no longer write into the original project's database.

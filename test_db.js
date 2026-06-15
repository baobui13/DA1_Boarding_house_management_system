const { Client } = require('pg');
const client = new Client({
  connectionString: 'postgresql://neondb_owner:npg_Ev0zpRA9nhky@ep-floral-shape-am3eu9dk.c-5.us-east-1.aws.neon.tech/neondb?sslmode=require'
});
client.connect();
client.query('SELECT "Id", "LandlordId", "ModerationStatus" FROM "Properties" LIMIT 5', (err, res) => {
  if (err) throw err;
  console.log(res.rows);
  client.end();
});
